using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Cv
    {
        public class Size : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public Size() : base(au_cv_Size_new1())
            {
            }

            public Size(int width, int height) : base(au_cv_Size_new2(width, height))
            {
            }

            public Size(IntPtr sizePtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(sizePtr, deleteResponsibility)
            {
            }

            // Properties

            public int Height
            {
                get { return au_cv_Size_getHeight(CppPtr); }
                set { au_cv_Size_setHeight(CppPtr, value); }
            }

            public int Width
            {
                get { return au_cv_Size_getWidth(CppPtr); }
                set { au_cv_Size_setWidth(CppPtr, value); }
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_Size_new1();

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_Size_new2(int width, int height);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Size_delete(IntPtr size);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Size_area(IntPtr size);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Size_getHeight(IntPtr size);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Size_setHeight(IntPtr size, int height);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Size_getWidth(IntPtr size);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Size_setWidth(IntPtr size, int width);

            protected override void DeleteCppPtr()
            {
                au_cv_Size_delete(CppPtr);
            }

            // Methods

            public int Area()
            {
                return au_cv_Size_area(CppPtr);
            }
        }
    }
}