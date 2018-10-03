using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ArucoUnity.Plugin
{
    public static partial class Cv
    {
        public class Exception : Utility.HandleCppPtr
        {
            // Constants

            private const int WhatLength = 1024;

            // Variables

            private readonly StringBuilder sb;

            // Constructor & Destructor

            public Exception() : base(au_cv_Exception_new())
            {
                sb = new StringBuilder(WhatLength);
            }

            // Properties

            public int Code => au_cv_Exception_getCode(CppPtr);

            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_Exception_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Exception_delete(IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Exception_what(IntPtr exception, StringBuilder sb, int sbLength);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Exception_getCode(IntPtr exception);

            protected override void DeleteCppPtr()
            {
                //au_cv_Exception_delete(cvPtr); // TODO: fix
            }

            // Methods

            public string What()
            {
                au_cv_Exception_what(CppPtr, sb, WhatLength);
                return sb.ToString();
            }

            public void Check()
            {
                if (Code != 0) throw new System.Exception(What());
            }
        }
    }
}