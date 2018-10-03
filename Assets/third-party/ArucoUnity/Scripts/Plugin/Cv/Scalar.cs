using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ArucoUnity.Plugin
{
    public static partial class Cv
    {
        public class Scalar : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public Scalar(double v0, double v1, double v2) : base(au_cv_Scalar_new(v0, v1, v2))
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_Scalar_new(double v0, double v1, double v2);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Scalar_delete(IntPtr scalar);

            public static implicit operator Scalar(Color color)
            {
                return new Scalar(color.r, color.g, color.b);
            }

            protected override void DeleteCppPtr()
            {
                au_cv_Scalar_delete(CppPtr);
            }
        }
    }
}