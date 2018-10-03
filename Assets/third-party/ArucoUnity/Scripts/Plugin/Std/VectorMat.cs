using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorMat : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorMat() : base(au_std_vectorMat_new())
            {
            }

            public VectorMat(IntPtr vectorMatPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorMatPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorMat_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorMat_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorMat_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe IntPtr* au_std_vectorMat_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorMat_push_back(IntPtr vector, IntPtr value);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorMat_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorMat_delete(CppPtr);
            }

            // Methods

            public Cv.Mat At(uint pos)
            {
                var exception = new Cv.Exception();
                var element = new Cv.Mat(au_std_vectorMat_at(CppPtr, pos, exception.CppPtr),
                    Utility.DeleteResponsibility.False);
                exception.Check();
                return element;
            }

            public unsafe Cv.Mat[] Data()
            {
                var dataPtr = au_std_vectorMat_data(CppPtr);
                var size = Size();

                var data = new Cv.Mat[size];
                for (var i = 0; i < size; i++) data[i] = new Cv.Mat(dataPtr[i], Utility.DeleteResponsibility.False);

                return data;
            }

            public void PushBack(Cv.Mat value)
            {
                au_std_vectorMat_push_back(CppPtr, value.CppPtr);
            }

            public uint Size()
            {
                return au_std_vectorMat_size(CppPtr);
            }
        }
    }
}