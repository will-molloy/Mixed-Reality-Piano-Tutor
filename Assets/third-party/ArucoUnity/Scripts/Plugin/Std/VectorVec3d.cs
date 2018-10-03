using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Std
    {
        public class VectorVec3d : Utility.HandleCppPtr
        {
            // Constructors & destructor

            public VectorVec3d() : base(au_std_vectorVec3d_new())
            {
            }

            public VectorVec3d(IntPtr vectorVec3dPtr,
                Utility.DeleteResponsibility deleteResponsibility = Utility.DeleteResponsibility.True)
                : base(vectorVec3dPtr, deleteResponsibility)
            {
            }
            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVec3d_new();

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVec3d_delete(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_std_vectorVec3d_at(IntPtr vector, uint pos, IntPtr exception);

            [DllImport("ArucoUnityPlugin")]
            private static extern unsafe IntPtr* au_std_vectorVec3d_data(IntPtr vector);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_std_vectorVec3d_push_back(IntPtr vector, IntPtr value);

            [DllImport("ArucoUnityPlugin")]
            private static extern uint au_std_vectorVec3d_size(IntPtr vector);

            protected override void DeleteCppPtr()
            {
                au_std_vectorVec3d_delete(CppPtr);
            }

            // Methods

            public Cv.Vec3d At(uint pos)
            {
                var exception = new Cv.Exception();
                var element = new Cv.Vec3d(au_std_vectorVec3d_at(CppPtr, pos, exception.CppPtr),
                    Utility.DeleteResponsibility.False);
                exception.Check();
                return element;
            }

            public unsafe Cv.Vec3d[] Data()
            {
                var dataPtr = au_std_vectorVec3d_data(CppPtr);
                var size = Size();

                var data = new Cv.Vec3d[size];
                for (var i = 0; i < size; i++) data[i] = new Cv.Vec3d(dataPtr[i], Utility.DeleteResponsibility.False);

                return data;
            }

            public void PushBack(Cv.Vec3d value)
            {
                au_std_vectorVec3d_push_back(CppPtr, value.CppPtr);
            }

            public uint Size()
            {
                return au_std_vectorVec3d_size(CppPtr);
            }
        }
    }
}