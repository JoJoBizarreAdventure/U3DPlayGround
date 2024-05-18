using System;
using DataStructure;

namespace Algorithm
{
    public class PriorityQueue<TKey, TValue> where TKey : IComparable<TKey>
    {
        public class Node : BinaryTreeNode<Node>
        {
            public TKey Key;
            public TValue Value;

            public Node(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }

            public void Swap(Node node)
            {
                (Key, node.Key) = (node.Key, Key);
                (Value, node.Value) = (node.Value, Value);
            }
        }

        private Node _top;

        public void Enqueue(TKey key, TValue value)
        {
            Count++;
            var newNode = new Node(key, value);
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
            {
                nodePtr.SetLeft(newNode);
            }
            else
            {
                nodePtr.SetRight(newNode);
            }

            nodePtr = newNode;
            while (nodePtr != _top && nodePtr.Key.CompareTo(nodePtr.Parent.Key) < 0)
            {
                nodePtr.Swap(nodePtr.Parent);
                nodePtr = nodePtr.Parent;
            }
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

            _top.Swap(nodePtr);

            if (Count % 2 == 0)
            {
                nodePtr.Parent.Left = null;
            }
            else
            {
                nodePtr.Parent.Right = null;
            }

            nodePtr.Parent = null;

            ret = nodePtr;

            Count--;

            nodePtr = _top;
            while (nodePtr.Left != null)
            {
                if (nodePtr.Left.Key.CompareTo(nodePtr.Key) < 0)
                {
                    nodePtr.Swap(nodePtr.Left);
                    nodePtr = nodePtr.Left;
                }
                else
                {
                    if (nodePtr.Right == null || nodePtr.Right.Key.CompareTo(nodePtr.Key) >= 0)
                        break;

                    nodePtr.Swap(nodePtr.Right);
                    nodePtr = nodePtr.Right;
                }
            }

            return ret;
        }

        public int Count { get; private set; }

        public void Clear()
        {
            Count = 0;
            _top = null;
        }
    }
}