#region License

/* Copyright (c) 2005 Leslie Sanford
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
using System.Collections.Generic;
using Sanford.Threading;

namespace Sanford.Multimedia.Midi
{
    public partial class InputDevice
    {
        private readonly object lockObject = new object();

        private volatile int bufferCount;

        private readonly ChannelMessageBuilder cmBuilder = new ChannelMessageBuilder();

        private readonly DelegateQueue delegateQueue;

        private readonly IntPtr handle;

        private readonly MidiHeaderBuilder headerBuilder = new MidiHeaderBuilder();

        private readonly MidiInProc midiInProc;

        private bool recording;

        private volatile bool resetting;

        private readonly SysCommonMessageBuilder scBuilder = new SysCommonMessageBuilder();

        private int sysExBufferSize = 4096;

        private readonly List<byte> sysExData = new List<byte>();

        private delegate void GenericDelegate<T>(T args);
    }
}