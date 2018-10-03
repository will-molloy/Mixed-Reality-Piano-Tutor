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
using System.Runtime.InteropServices;
using System.Text;

namespace Sanford.Multimedia.Midi
{
    /// <summary>
    ///     Represents a device capable of sending MIDI messages.
    /// </summary>
    public sealed class OutputDevice : OutputDeviceBase
    {
        private readonly MidiOutProc midiOutProc;

        private int runningStatus;

        private bool runningStatusEnabled;

        #region Construction

        /// <summary>
        ///     Initializes a new instance of the OutputDevice class.
        /// </summary>
        public OutputDevice(int deviceID) : base(deviceID)
        {
            midiOutProc = HandleMessage;

            var result = midiOutOpen(out handle, deviceID, midiOutProc, IntPtr.Zero, CALLBACK_FUNCTION);

            if (result != DeviceException.MMSYSERR_NOERROR) throw new OutputDeviceException(result);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether the OutputDevice uses
        ///     a running status.
        /// </summary>
        public bool RunningStatusEnabled
        {
            get { return runningStatusEnabled; }
            set
            {
                runningStatusEnabled = value;

                // Reset running status.
                runningStatus = 0;
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (lockObject)
                {
                    Reset();

                    // Close the OutputDevice.
                    var result = midiOutClose(Handle);

                    if (result != DeviceException.MMSYSERR_NOERROR) throw new OutputDeviceException(result);
                }
            }
            else
            {
                midiOutReset(Handle);
                midiOutClose(Handle);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Closes the OutputDevice.
        /// </summary>
        /// <exception cref="OutputDeviceException">
        ///     If an error occurred while closing the OutputDevice.
        /// </exception>
        public override void Close()
        {
            #region Guard

            if (IsDisposed) return;

            #endregion

            Dispose(true);
        }

        /// <summary>
        ///     Resets the OutputDevice.
        /// </summary>
        public override void Reset()
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            runningStatus = 0;

            base.Reset();
        }

        public override void Send(ChannelMessage message)
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            lock (lockObject)
            {
                // If running status is enabled.
                if (runningStatusEnabled)
                {
                    // If the message's status value matches the running status.
                    if (message.Status == runningStatus)
                    {
                        // Send only the two data bytes without the status byte.
                        Send(message.Message >> 8);
                    }
                    // Else the message's status value does not match the running
                    // status.
                    else
                    {
                        // Send complete message with status byte.
                        Send(message.Message);

                        // Update running status.
                        runningStatus = message.Status;
                    }
                }
                // Else running status has not been enabled.
                else
                {
                    Send(message.Message);
                }
            }
        }

        public override void Send(SysExMessage message)
        {
            // System exclusive cancels running status.
            runningStatus = 0;

            base.Send(message);
        }

        public override void Send(SysCommonMessage message)
        {
            #region Require

            if (IsDisposed) throw new ObjectDisposedException(GetType().Name);

            #endregion

            // System common cancels running status.
            runningStatus = 0;

            base.Send(message);
        }

        #region Win32 Midi Output Functions and Constants

        [DllImport("winmm.dll")]
        private static extern int midiOutOpen(out IntPtr handle, int deviceID,
            MidiOutProc proc, IntPtr instance, int flags);

        [DllImport("winmm.dll")]
        private static extern int midiOutClose(IntPtr handle);

        #endregion
    }

    /// <summary>
    ///     The exception that is thrown when a error occurs with the OutputDevice
    ///     class.
    /// </summary>
    public class OutputDeviceException : MidiDeviceException
    {
        #region OutputDeviceException Members

        #region Win32 Midi Output Error Function

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern int midiOutGetErrorText(int errCode,
            StringBuilder message, int sizeOfMessage);

        #endregion

        #region Fields

        // The error message.
        private readonly StringBuilder message = new StringBuilder(128);

        #endregion

        #region Construction

        /// <summary>
        ///     Initializes a new instance of the OutputDeviceException class with
        ///     the specified error code.
        /// </summary>
        /// <param name="errCode">
        ///     The error code.
        /// </param>
        public OutputDeviceException(int errCode) : base(errCode)
        {
            // Get error message.
            midiOutGetErrorText(errCode, message, message.Capacity);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a message that describes the current exception.
        /// </summary>
        public override string Message => message.ToString();

        #endregion

        #endregion
    }
}