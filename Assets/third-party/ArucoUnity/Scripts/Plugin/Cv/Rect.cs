using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Cv
    {
        public class Rect : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public Rect() : base(au_cv_Rect_new1())
            {
            }

            public Rect(int x, int y, int width, int height) : base(au_cv_Rect_new2(x, y, width, height))
            {
            }

            public Rect(IntPtr RectPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(RectPtr, deleteResponsibility)
            {
            }

            // Properties

            public int X
            {
                get { return au_cv_Rect_getX(CppPtr); }
                set { au_cv_Rect_setX(CppPtr, value); }
            }

            public int Y
            {
                get { return au_cv_Rect_getY(CppPtr); }
                set { au_cv_Rect_setY(CppPtr, value); }
            }

            public int Width
            {
                get { return au_cv_Rect_getWidth(CppPtr); }
                set { au_cv_Rect_setWidth(CppPtr, value); }
            }

            public int Height
            {
                get { return au_cv_Rect_getHeight(CppPtr); }
                set { au_cv_Rect_setHeight(CppPtr, value); }
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_Rect_new1();

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_Rect_new2(int x, int y, int width, int height);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Rect_delete(IntPtr Rect);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Rect_getX(IntPtr Rect);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Rect_setX(IntPtr Rect, int x);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Rect_getY(IntPtr Rect);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Rect_setY(IntPtr Rect, int y);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Rect_getWidth(IntPtr Rect);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Rect_setWidth(IntPtr Rect, int width);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_Rect_getHeight(IntPtr Rect);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_Rect_setHeight(IntPtr Rect, int height);

            protected override void DeleteCppPtr()
            {
                au_cv_Rect_delete(CppPtr);
            }
        }
    }
}