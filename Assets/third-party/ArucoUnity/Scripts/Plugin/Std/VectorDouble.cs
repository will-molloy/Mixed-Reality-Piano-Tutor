using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorDouble : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorDouble() : base(au_std_vectorDouble_new())
            {
            }

            public VectorDouble(IntPtr vectorDoublePtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorDoublePtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorDouble_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorDouble_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_std_vectorDouble_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe double* au_std_vectorDouble_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorDouble_push_back(IntPtr vector, double value);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorDouble_reserve(IntPtr vector, uint new_cap, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorDouble_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorDouble_delete(CppPtr);
            }

            // Methods

            public double At(uint pos)
            {
                var exception = new Cv.Exception();
                double element = au_std_vectorDouble_at(CppPtr, pos, exception.CppPtr);
                exception.Check();
                return element;
            }

            public unsafe double[] Data()
            {
                var dataPtr = au_std_vectorDouble_data(CppPtr);
                var size = Size();

                var data = new double[size];
                for (var i = 0; i < size; i++) data[i] = dataPtr[i];

                return data;
            }

            public void PushBack(double value)
            {
                au_std_vectorDouble_push_back(CppPtr, value);
            }

            public void Reserve(uint newCap)
            {
                var exception = new Cv.Exception();
                au_std_vectorDouble_reserve(CppPtr, newCap, exception.CppPtr);
                exception.Check();
            }

            public uint Size()
            {
                return au_std_vectorDouble_size(CppPtr);
            }
        }
    }
}