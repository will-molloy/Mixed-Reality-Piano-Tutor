/*
 * Created by: Leslie Sanford 
 * 
 * Last modified: 02/23/2005
 * 
 * Contact: jabberdabber@hotmail.com
 */

using System.ComponentModel;
using System.Diagnostics;

namespace Sanford.Collections.Immutable
{
    /// <summary>
    ///     Represents subtree nodes within random access lists.
    /// </summary>
    [ImmutableObject(true)]
    internal class RalTreeNode
    {
        #region RalTreeNode Members

        #region Instance Fields

        // The value represented by this node.

        // The number of nodes in the tree.

        // Left and right children.

        #endregion

        #region Construction

        /// <summary>
        ///     Initializes an instance of the RandomAccessListNode with the
        ///     specified value, left child, and right child.
        /// </summary>
        /// <param name="value">
        ///     The value to store in the node.
        /// </param>
        /// <param name="leftChild">
        ///     The left child.
        /// </param>
        /// <param name="rightChild">
        ///     The right child.
        /// </param>
        public RalTreeNode(
            object value,
            RalTreeNode leftChild,
            RalTreeNode rightChild)
        {
            Value = value;
            LeftChild = leftChild;
            RightChild = rightChild;

            Count = 1;

            if (leftChild != null)
            {
                Count += leftChild.Count * 2;

                Debug.Assert(rightChild != null);
                Debug.Assert(Count == 1 + leftChild.Count + rightChild.Count);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the value at the specified element in the random access list
        ///     subtree.
        /// </summary>
        /// <param name="index">
        ///     An integer that represents the position of the random access list
        ///     subtree element to get.
        /// </param>
        /// <returns>
        ///     The value at the specified position in the random access list
        ///     subtree.
        /// </returns>
        public object GetValue(int index)
        {
            Debug.Assert(index < Count);

            return GetValue(index, this);
        }

        // Recursive method for getting the value at the specified position.
        private object GetValue(int index, RalTreeNode node)
        {
            object result;

            // If the position of the value to get has been reached.
            if (index == 0)
            {
                // Get the value.
                result = node.Value;
            }
            // Else the position of the value to get has not been reached.
            else
            {
                var n = node.Count / 2;

                // If the value is in the left subtree.
                if (index <= n)
                {
                    Debug.Assert(node.LeftChild != null);

                    // Descend into the left subtree.
                    result = GetValue(index - 1, node.LeftChild);
                }
                // Else the value is in the right subtree.
                else
                {
                    Debug.Assert(node.RightChild != null);

                    // Descend into the right subtree.
                    result = GetValue(index - 1 - n, node.RightChild);
                }
            }

            return result;
        }

        /// <summary>
        ///     Sets the specified element in the current random access list
        ///     subtree to the specified value.
        /// </summary>
        /// <param name="value">
        ///     The new value for the specified element.
        /// </param>
        /// <param name="index">
        ///     An integer that represents the position of the random access list
        ///     subtree element to set.
        /// </param>
        /// <returns>
        ///     A new random access list tree node with the element at the specified
        ///     position set to the specified value.
        /// </returns>
        public RalTreeNode SetValue(object value, int index)
        {
            return SetValue(value, index, this);
        }

        // Recursive method for setting the value at the specified position.
        private RalTreeNode SetValue(object value, int index, RalTreeNode node)
        {
            RalTreeNode result;

            // If the position of the value to set has been reached.
            if (index == 0)
            {
                // Set the value.
                result = new RalTreeNode(
                    value,
                    node.LeftChild,
                    node.RightChild);
            }
            // Else if the position of the value to set has not been reached.
            else
            {
                Debug.Assert(node.LeftChild != null);

                var n = Count / 2;

                // If the value is in the left subtree.
                if (index <= n)
                {
                    // Descend into the left subtree.
                    result = new RalTreeNode(
                        node.Value,
                        node.LeftChild.SetValue(value, index - 1),
                        node.RightChild);
                }
                // Else if the value is in the right subtree.
                else
                {
                    Debug.Assert(node.RightChild != null);

                    // Descend into the right subtree.
                    result = new RalTreeNode(
                        node.Value,
                        node.LeftChild,
                        node.RightChild.SetValue(value, index - 1 - n));
                }
            }

            return result;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the number of nodes in the tree.
        /// </summary>
        public int Count { get; }

        /// <summary>
        ///     Gets the left child.
        /// </summary>
        public RalTreeNode LeftChild { get; }

        /// <summary>
        ///     Gets the right child.
        /// </summary>
        public RalTreeNode RightChild { get; }

        /// <summary>
        ///     Gets the value represented by this node.
        /// </summary>
        public object Value { get; }

        #endregion

        #endregion
    }
}