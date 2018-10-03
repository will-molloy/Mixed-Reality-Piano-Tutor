#region License

/* Copyright (c) 2006 Leslie Sanford
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

#endregion

#region Contact

/*
 * Leslie Sanford
 * Email: jabberdabber@hotmail.com
 */

#endregion

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Sanford.Threading;

namespace Sanford.Multimedia.Midi
{
    public abstract class OutputDeviceBase : MidiDevice
    {
        protected const int MOM_OPEN = 0x3C7;
        protected const int MOM_CLOSE = 0x3C8;
        protected const int MOM_DONE = 0x3C9;

        protected readonly object lockObject = new object();

        // The number of buffers still in the queue.
        protected int bufferCount;

        // For releasing buffers.
        protected DelegateQueue delegateQueue = new DelegateQueue();

        // The device handle.
        protected IntPtr handle = IntPtr.Zero;

        // Builds MidiHeader structures for sending system exclusive messages.
        private readonly MidiHeaderBuilder headerBuilder = new MidiHeaderBuilder();

        public OutputDeviceBase(int deviceID) : base(deviceID)
        {
        }

        public override IntPtr Handle => handle;

        public static int DeviceCount => midiOutGetNumDevs();

        [DllImport("winmm.dll")]
        protected static extern int midiOutReset(IntPtr handle);

        [DllImport("winmm.dll")]
        protected static extern int midiOutShortMsg(IntPtr handle, int message);

        [DllImport("winmm.dll")]
        protected static extern int midiOutPrepareHeader(IntPtr handle,
            IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        protected static extern int midiOutUnprepareHeader(IntPtr handle,
            IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        protected static extern int midiOutLongMsg(IntPtr handle,
            IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        protected static extern int midiOutGetDevCaps(IntPtr deviceID,
            ref MidiOutCaps caps, int sizeOfMidiOutCaps);

        [DllImport("winmm.dll")]
        protected static extern int midiOutGetNumDevs();

        ~OutputDeviceBase()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) delegateQueue.Dispose();

            base.Dispose(disposing);
        }

        public virtual void Send(ChannelMessage message)
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            Send(message.Message);
        }

        public virtual void SendShort(int message)
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            Send(message);
        }

        public virtual void Send(SysExMessage message)
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            lock (lockObject)
            {
                headerBuilder.InitializeBuffer(message);
                headerBuilder.Build();

                // Prepare system exclusive buffer.
                var result = midiOutPrepareHeader(Handle, headerBuilder.Result, SizeOfMidiHeader);

                // If the system exclusive buffer was prepared successfully.
                if (result == DeviceException.MMSYSERR_NOERROR)
                {
                    bufferCount++;

                    // Send system exclusive message.
                    result = midiOutLongMsg(Handle, headerBuilder.Result, SizeOfMidiHeader);

                    // If the system exclusive message could not be sent.
                    if (result != DeviceException.MMSYSERR_NOERROR)
                    {
                        midiOutUnprepareHeader(Handle, headerBuilder.Result, SizeOfMidiHeader);
                        bufferCount--;
                        headerBuilder.Destroy();

                        // Throw an exception.
                        throw new OutputDeviceException(result);
                    }
                }
                // Else the system exclusive buffer could not be prepared.
                else
                {
                    // Destroy system exclusive buffer.
                    headerBuilder.Destroy();

                    // Throw an exception.
                    throw new OutputDeviceException(result);
                }
            }
        }

        public virtual void Send(SysCommonMessage message)
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            Send(message.Message);
        }

        public virtual void Send(SysRealtimeMessage message)
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            Send(message.Message);
        }

        public override void Reset()
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            lock (lockObject)
            {
                // Reset the OutputDevice.
                var result = midiOutReset(Handle);

                if (result == DeviceException.MMSYSERR_NOERROR)
                    while (bufferCount > 0)
                        Monitor.Wait(lockObject);
                else
                    throw new OutputDeviceException(result);
            }
        }

        protected void Send(int message)
        {
            lock (lockObject)
            {
                var result = midiOutShortMsg(Handle, message);

                if (result != DeviceException.MMSYSERR_NOERROR) throw new OutputDeviceException(result);
            }
        }

        public static MidiOutCaps GetDeviceCapabilities(int deviceID)
        {
            var caps = new MidiOutCaps();

            // Get the device's capabilities.
            var devId = (IntPtr) deviceID;
            var result = midiOutGetDevCaps(devId, ref caps, Marshal.SizeOf(caps));

            // If the capabilities could not be retrieved.
            if (result != DeviceException.MMSYSERR_NOERROR) throw new OutputDeviceException(result);

            return caps;
        }

        // Handles Windows messages.
        protected virtual void HandleMessage(IntPtr hnd, int msg, IntPtr instance, IntPtr param1, IntPtr param2)
        {
            if (msg == MOM_OPEN)
            {
            }
            else if (msg == MOM_CLOSE)
            {
            }
            else if (msg == MOM_DONE)
            {
                delegateQueue.Post(ReleaseBuffer, param1);
            }
        }

        // Releases buffers.
        private void ReleaseBuffer(object state)
        {
            lock (lockObject)
            {
                var headerPtr = (IntPtr) state;

                // Unprepare the buffer.
                var result = midiOutUnprepareHeader(Handle, headerPtr, SizeOfMidiHeader);

                if (result != DeviceException.MMSYSERR_NOERROR)
                {
                    Exception ex = new OutputDeviceException(result);

                    OnError(new ErrorEventArgs(ex));
                }

                // Release the buffer resources.
                headerBuilder.Destroy(headerPtr);

                bufferCount--;

                Monitor.Pulse(lockObject);

                Debug.Assert(bufferCount >= 0);
            }
        }

        public override void Dispose()
        {
            #region Guard

            if (IsDisposed) return;

            #endregion

            lock (lockObject)
            {
                Close();
            }
        }

        protected delegate void GenericDelegate<T>(T args);

        // Represents the method that handles messages from Windows.
        protected delegate void MidiOutProc(IntPtr hnd, int msg, IntPtr instance, IntPtr param1, IntPtr param2);
    }
}