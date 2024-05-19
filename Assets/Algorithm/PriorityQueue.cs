using System;
using DataStructure;

namespace Algorithm
{
    public class PriorityQueue<TKey> where TKey : IComparable<TKey>
    {
        private Node _top;

        public int Count { get; private set; }

        public void Enqueue(TKey key)
        {
            Count++;
            var newNode = new Node(key);
            if (_top == null)
            {
                _top = newNode;
                return;
            }

            var nodePtr = _top;
            var digitCnt = (int)Math.Floor(Math.Log(Count, 2)) - 1;
            var posPtr = 1 << digitCnt;
            while (posPtr > 1)
            {
                nodePtr = (Count & posPtr) > 0 ? nodePtr.Right : nodePtr.Left;
                posPtr >>= 1;
            }

            if (Count % 2 == 0)
                nodePtr.SetLeft(newNode);
            else
                nodePtr.SetRight(newNode);

            nodePtr = newNode;
            while (nodePtr.Parent != null && nodePtr.Key.CompareTo(nodePtr.Parent.Key) < 0)
                Node.Swap(nodePtr, nodePtr.Parent);

            while (_top.Parent != null) _top = _top.Parent;
        }

        public Node Dequeue()
        {
            if (_top == null)
                return null;

            Node ret;
            if (Count == 1)
            {
                ret = _top;
                _top = null;
                Count = 0;
                return ret;
            }

            var nodePtr = _top;
            var digitCnt = (int)Math.Floor(Math.Log(Count, 2)) - 1;
            var posPtr = 1 << digitCnt;

            while (posPtr > 0)
            {
                nodePtr = (Count & posPtr) > 0 ? nodePtr.Right : nodePtr.Left;
                posPtr >>= 1;
            }

            Node.Swap(_top, nodePtr);

            if (Count % 2 == 0)
                _top.Parent.Left = null;
            else
                _top.Parent.Right = null;

            _top.Parent = null;

            ret = _top;
            _top = nodePtr;

            Count--;

            while (nodePtr.Left != null)
                if (nodePtr.Left.Key.CompareTo(nodePtr.Key) < 0)
                {
                    Node.Swap(nodePtr, nodePtr.Left);
                }
                else
                {
                    if (nodePtr.Right == null || nodePtr.Right.Key.CompareTo(nodePtr.Key) >= 0)
                        break;

                    Node.Swap(nodePtr, nodePtr.Right);
                }

            while (_top.Parent != null) _top = _top.Parent;
            return ret;
        }

        public void Clear()
        {
            Count = 0;
            _top = null;
        }

        public class Node : BinaryTreeNode<Node>
        {
            public TKey Key;

            public Node(TKey key)
            {
                Key = key;
            }

            public static void Swap(Node node1, Node node2)
            {
                if (node1 == null || node2 == null)
                    return;

                if (node1.Parent == node2) (node1, node2) = (node2, node1);

                if (node2.Parent == node1)
                {
                    if (node1.Parent != null)
                    {
                        if (node1.Parent.Left == node1)
                            node1.Parent.SetLeft(node2);
                        else
                            node1.Parent.SetRight(node2);
                    }
                    else
                    {
                        node2.Parent = null;
                    }

                    Node left2 = node2.Left, right2 = node2.Right;
                    if (node1.Left == node2)
                    {
                        node2.SetLeft(node1);
                        node2.SetRight(node1.Right);
                    }
                    else
                    {
                        node2.SetLeft(node1.Left);
                        node2.SetRight(node1);
                    }

                    node1.SetLeft(left2);
                    node1.SetRight(right2);
                }
                else
                {
                    Node left1 = node1.Left, right1 = node1.Right;

                    node1.SetLeft(node2.Left);
                    node1.SetRight(node2.Right);

                    node2.SetLeft(left1);
                    node2.SetRight(right1);

                    if (node1.Parent == null)
                    {
                        if (node2.Parent.Left == node2)
                            node2.Parent.SetLeft(node1);
                        else
                            node2.Parent.SetRight(node1);

                        node2.Parent = null;
                    }
                    else if (node2.Parent == null)
                    {
                        if (node1.Parent.Left == node1)
                            node1.Parent.SetLeft(node2);
                        else
                            node1.Parent.SetRight(node2);

                        node1.Parent = null;
                    }
                    else
                    {
                        var parent1 = node1.Parent;

                        if (node2.Parent.Left == node2)
                            node2.Parent.SetLeft(node1);
                        else
                            node2.Parent.SetRight(node1);

                        if (parent1.Left == node1)
                            parent1.SetLeft(node2);
                        else
                            parent1.SetRight(node2);
                    }
                }
            }
        }
    }
}