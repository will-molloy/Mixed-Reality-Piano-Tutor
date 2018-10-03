using System;

namespace Sanford.Collections.Generic
{
    public partial class Deque<T>
    {
        #region Node Class

        // Represents a node in the deque.
        [Serializable]
        private class Node
        {
            private Node next;

            private Node previous;

            public Node(T value)
            {
                Value = value;
            }

            public T Value { get; }

            public Node Previous
            {
                get { return previous; }
                set { previous = value; }
            }

            public Node Next
            {
                get { return next; }
                set { next = value; }
            }
        }

        #endregion
    }
}