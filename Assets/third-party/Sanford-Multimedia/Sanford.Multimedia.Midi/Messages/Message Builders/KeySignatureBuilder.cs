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

namespace Sanford.Multimedia.Midi
{
    /// <summary>
    ///     Builds key signature MetaMessages.
    /// </summary>
    public class KeySignatureBuilder : IMessageBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the KeySignatureBuilder class.
        /// </summary>
        public KeySignatureBuilder()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the KeySignatureBuilder class with
        ///     the specified key signature MetaMessage.
        /// </summary>
        /// <param name="message">
        ///     The key signature MetaMessage to use for initializing the
        ///     KeySignatureBuilder class.
        /// </param>
        public KeySignatureBuilder(MetaMessage message)
        {
            Initialize(message);
        }

        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        public Key Key { get; set; } = Key.CMajor;

        /// <summary>
        ///     The build key signature MetaMessage.
        /// </summary>
        public MetaMessage Result { get; private set; }

        #region IMessageBuilder Members

        /// <summary>
        ///     Builds the key signature MetaMessage.
        /// </summary>
        public void Build()
        {
            var data = new byte[MetaMessage.KeySigLength];

            unchecked
            {
                switch (Key)
                {
                    case Key.CFlatMajor:
                        data[0] = (byte) -7;
                        data[1] = 0;
                        break;

                    case Key.GFlatMajor:
                        data[0] = (byte) -6;
                        data[1] = 0;
                        break;

                    case Key.DFlatMajor:
                        data[0] = (byte) -5;
                        data[1] = 0;
                        break;

                    case Key.AFlatMajor:
                        data[0] = (byte) -4;
                        data[1] = 0;
                        break;

                    case Key.EFlatMajor:
                        data[0] = (byte) -3;
                        data[1] = 0;
                        break;

                    case Key.BFlatMajor:
                        data[0] = (byte) -2;
                        data[1] = 0;
                        break;

                    case Key.FMajor:
                        data[0] = (byte) -1;
                        data[1] = 0;
                        break;

                    case Key.CMajor:
                        data[0] = 0;
                        data[1] = 0;
                        break;

                    case Key.GMajor:
                        data[0] = 1;
                        data[1] = 0;
                        break;

                    case Key.DMajor:
                        data[0] = 2;
                        data[1] = 0;
                        break;

                    case Key.AMajor:
                        data[0] = 3;
                        data[1] = 0;
                        break;

                    case Key.EMajor:
                        data[0] = 4;
                        data[1] = 0;
                        break;

                    case Key.BMajor:
                        data[0] = 5;
                        data[1] = 0;
                        break;

                    case Key.FSharpMajor:
                        data[0] = 6;
                        data[1] = 0;
                        break;

                    case Key.CSharpMajor:
                        data[0] = 7;
                        data[1] = 0;
                        break;

                    case Key.AFlatMinor:
                        data[0] = (byte) -7;
                        data[1] = 1;
                        break;

                    case Key.EFlatMinor:
                        data[0] = (byte) -6;
                        data[1] = 1;
                        break;

                    case Key.BFlatMinor:
                        data[0] = (byte) -5;
                        data[1] = 1;
                        break;

                    case Key.FMinor:
                        data[0] = (byte) -4;
                        data[1] = 1;
                        break;

                    case Key.CMinor:
                        data[0] = (byte) -3;
                        data[1] = 1;
                        break;

                    case Key.GMinor:
                        data[0] = (byte) -2;
                        data[1] = 1;
                        break;

                    case Key.DMinor:
                        data[0] = (byte) -1;
                        data[1] = 1;
                        break;

                    case Key.AMinor:
                        data[0] = 1;
                        data[1] = 0;
                        break;

                    case Key.EMinor:
                        data[0] = 1;
                        data[1] = 1;
                        break;

                    case Key.BMinor:
                        data[0] = 2;
                        data[1] = 1;
                        break;

                    case Key.FSharpMinor:
                        data[0] = 3;
                        data[1] = 1;
                        break;

                    case Key.CSharpMinor:
                        data[0] = 4;
                        data[1] = 1;
                        break;

                    case Key.GSharpMinor:
                        data[0] = 5;
                        data[1] = 1;
                        break;

                    case Key.DSharpMinor:
                        data[0] = 6;
                        data[1] = 1;
                        break;

                    case Key.ASharpMinor:
                        data[0] = 7;
                        data[1] = 1;
                        break;
                }
            }

            Result = new MetaMessage(MetaType.KeySignature, data);
        }

        #endregion

        /// <summary>
        ///     Initializes the KeySignatureBuilder with the specified MetaMessage.
        /// </summary>
        /// <param name="message">
        ///     The key signature MetaMessage to use for initializing the
        ///     KeySignatureBuilder.
        /// </param>
        public void Initialize(MetaMessage message)
        {
            #region Require

            if (message == null)
                throw new ArgumentNullException("message");
            if (message.MetaType != MetaType.KeySignature)
                throw new ArgumentException("Wrong meta event type.", "messaege");

            #endregion

            var b = (sbyte) message[0];

            // If the key is major.
            if (message[1] == 0)
                switch (b)
                {
                    case -7:
                        Key = Key.CFlatMajor;
                        break;

                    case -6:
                        Key = Key.GFlatMajor;
                        break;

                    case -5:
                        Key = Key.DFlatMajor;
                        break;

                    case -4:
                        Key = Key.AFlatMajor;
                        break;

                    case -3:
                        Key = Key.EFlatMajor;
                        break;

                    case -2:
                        Key = Key.BFlatMajor;
                        break;

                    case -1:
                        Key = Key.FMajor;
                        break;

                    case 0:
                        Key = Key.CMajor;
                        break;

                    case 1:
                        Key = Key.GMajor;
                        break;

                    case 2:
                        Key = Key.DMajor;
                        break;

                    case 3:
                        Key = Key.AMajor;
                        break;

                    case 4:
                        Key = Key.EMajor;
                        break;

                    case 5:
                        Key = Key.BMajor;
                        break;

                    case 6:
                        Key = Key.FSharpMajor;
                        break;

                    case 7:
                        Key = Key.CSharpMajor;
                        break;
                }
            // Else the key is minor.
            else
                switch (b)
                {
                    case -7:
                        Key = Key.AFlatMinor;
                        break;

                    case -6:
                        Key = Key.EFlatMinor;
                        break;

                    case -5:
                        Key = Key.BFlatMinor;
                        break;

                    case -4:
                        Key = Key.FMinor;
                        break;

                    case -3:
                        Key = Key.CMinor;
                        break;

                    case -2:
                        Key = Key.GMinor;
                        break;

                    case -1:
                        Key = Key.DMinor;
                        break;

                    case 0:
                        Key = Key.AMinor;
                        break;

                    case 1:
                        Key = Key.EMinor;
                        break;

                    case 2:
                        Key = Key.BMinor;
                        break;

                    case 3:
                        Key = Key.FSharpMinor;
                        break;

                    case 4:
                        Key = Key.CSharpMinor;
                        break;

                    case 5:
                        Key = Key.GSharpMinor;
                        break;

                    case 6:
                        Key = Key.DSharpMinor;
                        break;

                    case 7:
                        Key = Key.ASharpMinor;
                        break;
                }
        }
    }
}