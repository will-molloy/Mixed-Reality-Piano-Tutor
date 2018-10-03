using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorVectorInt : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorVectorInt() : base(au_std_vectorVectorInt_new())
            {
            }

            public VectorVectorInt(IntPtr vectorVectorIntPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorVectorIntPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVectorInt_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVectorInt_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVectorInt_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe IntPtr* au_std_vectorVectorInt_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVectorInt_push_back(IntPtr vector, IntPtr value);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorVectorInt_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorVectorInt_delete(CppPtr);
            }

            // Methods

            public VectorInt At(uint pos)
            {
                var exception = new Cv.Exception();
                var element = new VectorInt(au_std_vectorVectorInt_at(CppPtr, pos, exception.CppPtr),
                    Utility.DeleteResponsibility.False);
                exception.Check();
                return element;
            }

            public unsafe VectorInt[] Data()
            {
                var dataPtr = au_std_vectorVectorInt_data(CppPtr);
                var size = Size();

                var data = new VectorInt[size];
                for (var i = 0; i < size; i++) data[i] = new VectorInt(dataPtr[i], Utility.DeleteResponsibility.False);

                return data;
            }

            public void PushBack(VectorInt value)
            {
                au_std_vectorVectorInt_push_back(CppPtr, value.CppPtr);
            }

            public uint Size()
            {
                return au_std_vectorVectorInt_size(CppPtr);
            }
        }
    }
}