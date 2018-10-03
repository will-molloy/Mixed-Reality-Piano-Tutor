using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorVectorVectorPoint2f : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorVectorVectorPoint2f() : base(au_std_vectorVectorVectorPoint2f_new())
            {
            }

            public VectorVectorVectorPoint2f(IntPtr vectorVectorVectorPoint2fPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorVectorVectorPoint2fPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVectorVectorPoint2f_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVectorVectorPoint2f_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVectorVectorPoint2f_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe IntPtr* au_std_vectorVectorVectorPoint2f_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVectorVectorPoint2f_push_back(IntPtr vector, IntPtr value);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorVectorVectorPoint2f_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorVectorVectorPoint2f_delete(CppPtr);
            }

            // Methods

            public VectorVectorPoint2f At(uint pos)
            {
                var exception = new Cv.Exception();
                var element = new VectorVectorPoint2f(
                    au_std_vectorVectorVectorPoint2f_at(CppPtr, pos, exception.CppPtr),
                    Utility.DeleteResponsibility.False);
                exception.Check();
                return element;
            }

            public unsafe VectorVectorPoint2f[] Data()
            {
                var dataPtr = au_std_vectorVectorVectorPoint2f_data(CppPtr);
                var size = Size();

                var data = new VectorVectorPoint2f[size];
                for (var i = 0; i < size; i++)
                    data[i] = new VectorVectorPoint2f(dataPtr[i], Utility.DeleteResponsibility.False);

                return data;
            }

            public void PushBack(VectorVectorPoint2f value)
            {
                au_std_vectorVectorVectorPoint2f_push_back(CppPtr, value.CppPtr);
            }

            public uint Size()
            {
                return au_std_vectorVectorVectorPoint2f_size(CppPtr);
            }
        }
    }
}