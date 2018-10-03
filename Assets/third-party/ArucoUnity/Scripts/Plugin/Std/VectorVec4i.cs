using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorVec4i : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorVec4i() : base(au_std_vectorVec4i_new())
            {
            }

            public VectorVec4i(IntPtr vectorVec4iPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorVec4iPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVec4i_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVec4i_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVec4i_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe IntPtr* au_std_vectorVec4i_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVec4i_push_back(IntPtr vector, IntPtr value);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorVec4i_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorVec4i_delete(CppPtr);
            }

            // Methods

            public Cv.Vec4i At(uint pos)
            {
                var exception = new Cv.Exception();
                var element = new Cv.Vec4i(au_std_vectorVec4i_at(CppPtr, pos, exception.CppPtr),
                    Utility.DeleteResponsibility.False);
                exception.Check();
                return element;
            }

            public unsafe Cv.Vec4i[] Data()
            {
                var dataPtr = au_std_vectorVec4i_data(CppPtr);
                var size = Size();

                var data = new Cv.Vec4i[size];
                for (var i = 0; i < size; i++) data[i] = new Cv.Vec4i(dataPtr[i], Utility.DeleteResponsibility.False);

                return data;
            }

            public void PushBack(Cv.Vec4i value)
            {
                au_std_vectorVec4i_push_back(CppPtr, value.CppPtr);
            }

            public uint Size()
            {
                return au_std_vectorVec4i_size(CppPtr);
            }
        }
    }
}