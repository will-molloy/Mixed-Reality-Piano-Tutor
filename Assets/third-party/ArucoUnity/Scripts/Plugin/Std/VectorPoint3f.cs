using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorPoint3f : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorPoint3f() : base(au_std_vectorPoint3f_new())
            {
            }

            public VectorPoint3f(IntPtr vectorPoint3fPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorPoint3fPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorPoint3f_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorPoint3f_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorPoint3f_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe IntPtr* au_std_vectorPoint3f_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorPoint3f_push_back(IntPtr vector, IntPtr value);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorPoint3f_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorPoint3f_delete(CppPtr);
            }

            // Methods

            public Cv.Point3f At(uint pos)
            {
                var exception = new Cv.Exception();
                var element = new Cv.Point3f(au_std_vectorPoint3f_at(CppPtr, pos, exception.CppPtr),
                    Utility.DeleteResponsibility.False);
                exception.Check();
                return element;
            }

            public unsafe Cv.Point3f[] Data()
            {
                var dataPtr = au_std_vectorPoint3f_data(CppPtr);
                var size = Size();

                var data = new Cv.Point3f[size];
                for (var i = 0; i < size; i++) data[i] = new Cv.Point3f(dataPtr[i], Utility.DeleteResponsibility.False);

                return data;
            }

            public void PushBack(Cv.Point3f value)
            {
                au_std_vectorPoint3f_push_back(CppPtr, value.CppPtr);
            }

            public uint Size()
            {
                return au_std_vectorPoint3f_size(CppPtr);
            }
        }
    }
}