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

        private int _nodeCnt;
        private Node _top;

        public void Enqueue(TKey key, TValue value)
        {
            _nodeCnt++;
            var newNode = new Node(key, value);
            if (_top == null)
            {
                _top = newNode;
                return;
            }

            var nodePtr = _top;
            var digitCnt = (int)Math.Floor(Math.Log(_nodeCnt, 2)) - 1;
            var posPtr = 1 << digitCnt;
            while (posPtr > 1)
            {
                nodePtr = (_nodeCnt & posPtr) > 0 ? nodePtr.Right : nodePtr.Left;
                posPtr >>= 1;
            }

            if (_nodeCnt % 2 == 0)
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

        public void Dequeue()
        {
            if (_top == null)
                return;

            if (_nodeCnt == 1)
            {
                _top = null;
                _nodeCnt = 0;
                return;
            }

            var nodePtr = _top;
            var digitCnt = (int)Math.Floor(Math.Log(_nodeCnt, 2)) - 1;
            var posPtr = 1 << digitCnt;

            while (posPtr > 0)
            {
                nodePtr = (_nodeCnt & posPtr) > 0 ? nodePtr.Right : nodePtr.Left;
                posPtr >>= 1;
            }

            _top.Swap(nodePtr);

            if (_nodeCnt % 2 == 0)
            {
                nodePtr.Parent.Left = null;
            }
            else
            {
                nodePtr.Parent.Right = null;
            }
            
            _nodeCnt--;

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
        }

        public Node GetTop()
        {
            return _top;
        }
    }
}