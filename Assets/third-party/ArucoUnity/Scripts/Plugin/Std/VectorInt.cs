using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorInt : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorInt() : base(au_std_vectorInt_new())
            {
            }

            public VectorInt(IntPtr vectorIntPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorIntPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorInt_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorInt_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_std_vectorInt_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe int* au_std_vectorInt_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorInt_push_back(IntPtr vector, int value);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorInt_reserve(IntPtr vector, uint new_cap, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorInt_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorInt_delete(CppPtr);
            }

            // Methods

            public int At(uint pos)
            {
                var exception = new Cv.Exception();
                var element = au_std_vectorInt_at(CppPtr, pos, exception.CppPtr);
                exception.Check();
                return element;
            }

            public unsafe int[] Data()
            {
                var dataPtr = au_std_vectorInt_data(CppPtr);
                var size = Size();

                var data = new int[size];
                for (var i = 0; i < size; i++) data[i] = dataPtr[i];

                return data;
            }

            public void PushBack(int value)
            {
                au_std_vectorInt_push_back(CppPtr, value);
            }

            public void Reserve(uint newCap)
            {
                var exception = new Cv.Exception();
                au_std_vectorInt_reserve(CppPtr, newCap, exception.CppPtr);
                exception.Check();
            }

            public uint Size()
            {
                return au_std_vectorInt_size(CppPtr);
            }
        }
    }
}