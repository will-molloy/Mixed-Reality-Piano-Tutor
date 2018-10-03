using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Cv
    {
        public class Vec4i : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public Vec4i() : base(au_cv_Vec4i_new())
            {
            }

            public Vec4i(IntPtr vec4iPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vec4iPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_Vec4i_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Vec4i_delete(IntPtr vec4i);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Vec4i_get(IntPtr vec4i, int i, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Vec4i_set(IntPtr vec4i, int i, int value, IntPtr exception);

            protected override void DeleteCppPtr()
            {
                au_cv_Vec4i_delete(CppPtr);
            }

            // Methods

            public int Get(int i)
            {
                var exception = new Exception();
                var value = au_cv_Vec4i_get(CppPtr, i, exception.CppPtr);
                exception.Check();
                return value;
            }

            public void Set(int i, int value)
            {
                var exception = new Exception();
                au_cv_Vec4i_set(CppPtr, i, value, exception.CppPtr);
                exception.Check();
            }
        }
    }
}