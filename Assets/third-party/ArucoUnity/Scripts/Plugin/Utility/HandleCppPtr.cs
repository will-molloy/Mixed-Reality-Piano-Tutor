using System;
using System.Runtime.InteropServices;

namespace ArucoUnity.Plugin
{
    public static class Utility
    {
        public enum DeleteResponsibility
        {
            True,
            False
        }

        public abstract class HandleCppPtr
        {
            // Variables

            private HandleRef handle;
            // Constructors & destructor

            public HandleCppPtr(DeleteResponsibility deleteResponsibility = DeleteResponsibility.True)
            {
                CppPtr = IntPtr.Zero;
                DeleteResponsibility = deleteResponsibility;
            }

            public HandleCppPtr(IntPtr cppPtr, DeleteResponsibility deleteResponsibility = DeleteResponsibility.True)
            {
                CppPtr = cppPtr;
                DeleteResponsibility = deleteResponsibility;
            }

            // Properties

            public DeleteResponsibility DeleteResponsibility { get; set; }

            public IntPtr CppPtr
            {
                get { return handle.Handle; }
                set { handle = new HandleRef(this, value); }
            }

            ~HandleCppPtr()
            {
                if (DeleteResponsibility == DeleteResponsibility.True) DeleteCppPtr();
            }

            // Methods

            protected abstract void DeleteCppPtr();
        }
    }
}