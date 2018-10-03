using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static partial class Cv
    {
        public class TermCriteria : Utility.HandleCppPtr
        {
            // Enums

            public enum Type
            {
                Count = 0,
                MaxIter = Count,
                Eps = 2
            }

            // Constructors & destructor

            public TermCriteria() : base(au_cv_TermCriteria_new1())
            {
            }

            public TermCriteria(Type type, int maxCount, double epsilon) : base(
                au_cv_TermCriteria_new2((int) type, maxCount, epsilon))
            {
            }

            // Properties

            public double Epsilon
            {
                get { return au_cv_TermCriteria_getEpsilon(CppPtr); }
                set { au_cv_TermCriteria_setEpsilon(CppPtr, value); }
            }

            public int MaxCount
            {
                get { return au_cv_TermCriteria_getMaxCount(CppPtr); }
                set { au_cv_TermCriteria_setMaxCount(CppPtr, value); }
            }

            public int TypeValue
            {
                get { return au_cv_TermCriteria_getType(CppPtr); }
                set { au_cv_TermCriteria_setType(CppPtr, value); }
            }

            // Native functions

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_TermCriteria_new1();

            [DllImport("ArucoUnityPlugin")]
            private static extern IntPtr au_cv_TermCriteria_new2(int type, int maxCount, double epsilon);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_TermCriteria_delete(IntPtr termCriteria);

            [DllImport("ArucoUnityPlugin")]
            private static extern double au_cv_TermCriteria_getEpsilon(IntPtr termCriteria);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_TermCriteria_setEpsilon(IntPtr termCriteria, double epsilon);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_TermCriteria_getMaxCount(IntPtr termCriteria);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_TermCriteria_setMaxCount(IntPtr termCriteria, int maxCount);

            [DllImport("ArucoUnityPlugin")]
            private static extern int au_cv_TermCriteria_getType(IntPtr termCriteria);

            [DllImport("ArucoUnityPlugin")]
            private static extern void au_cv_TermCriteria_setType(IntPtr termCriteria, int type);

            protected override void DeleteCppPtr()
            {
                au_cv_TermCriteria_delete(CppPtr);
            }
        }
    }
}