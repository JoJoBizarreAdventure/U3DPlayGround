using System;
using DataStructure;
using UnityEngine;

namespace Algorithm
{
    public class RedBlackTree<TKey> where TKey : IComparable<TKey>
    {
        private Node _root;

        private static void RotateLeft(Node node)
        {
            var right = node.Right;

            if (node.Parent != null)
            {
                if (node.Parent.Left == node)
                    node.Parent.SetLeft(right);
                else
                    node.Parent.SetRight(right);
            }

            node.SetRight(right.Left);
            right.SetLeft(node);
        }

        private static void RotateRight(Node node)
        {
            var left = node.Left;

            if (node.Parent != null)
            {
                if (node.Parent.Left == node)
                    node.Parent.SetLeft(left);
                else
                    node.Parent.SetRight(left);
            }

            node.SetLeft(left.Right);
            left.SetRight(node);
        }

        protected Node Add_Internal(TKey key)
        {
            if (_root == null)
            {
                _root = new Node(key, Node.NodeColor.Black);
                return null;
            }

            var newNode = new Node(key, Node.NodeColor.Red);
            var parentNode = _root;
            while (true)
            {
                if (newNode.Key.CompareTo(parentNode.Key) == 0)
                {
                    Debug.LogWarning($"RBT Key Exist in Add:{newNode.Key}");
                    return null;
                }

                if (newNode.Key.CompareTo(parentNode.Key) < 0)
                {
                    if (parentNode.Left != null)
                    {
                        parentNode = parentNode.Left;
                    }
                    else
                    {
                        parentNode.SetLeft(newNode);
                        break;
                    }
                }
                else
                {
                    if (parentNode.Right != null)
                    {
                        parentNode = parentNode.Right;
                    }
                    else
                    {
                        parentNode.SetRight(newNode);
                        break;
                    }
                }
            }

            return Node.IsBlack(parentNode) ? null : newNode;
        }

        protected Node Add_Adjust(Node newNode)
        {
            var parentNode = newNode.Parent;
            var grandParentNode = parentNode.Parent;
            var uncleNode = parentNode == grandParentNode.Left ? grandParentNode.Right : grandParentNode.Left;

            if (Node.IsRed(uncleNode))
            {
                parentNode.Color = Node.NodeColor.Black;
                uncleNode.Color = Node.NodeColor.Black;
                grandParentNode.Color = Node.NodeColor.Red;

                return grandParentNode.Parent is { Color: Node.NodeColor.Red } ? grandParentNode.Parent : null;
            }

            if (parentNode == grandParentNode.Left)
            {
                if (newNode == parentNode.Right)
                {
                    RotateLeft(parentNode);
                    parentNode = newNode;
                }

                RotateRight(grandParentNode);
            }
            else
            {
                if (newNode == parentNode.Left)
                {
                    RotateRight(parentNode);
                    parentNode = newNode;
                }

                RotateLeft(grandParentNode);
            }

            parentNode.Color = Node.NodeColor.Black;
            grandParentNode.Color = Node.NodeColor.Red;

            if (_root == grandParentNode)
                _root = parentNode;

            return null;
        }

        public void Add(TKey key)
        {
            var newNode = Add_Internal(key);

            while (newNode != null) newNode = Add_Adjust(newNode);
        }

        protected Node Remove_Find(TKey key)
        {
            if (_root == null)
                return null;

            var nodePtr = _root;
            while (true)
            {
                if (key.CompareTo(nodePtr.Key) == 0) break;

                if (key.CompareTo(nodePtr.Key) < 0)
                {
                    if (nodePtr.Left != null)
                    {
                        nodePtr = nodePtr.Left;
                    }
                    else
                    {
                        Debug.LogWarning($"RBT Key Not Exist in Remove:{key}");
                        return null;
                    }
                }
                else
                {
                    if (nodePtr.Right != null)
                    {
                        nodePtr = nodePtr.Right;
                    }
                    else
                    {
                        Debug.LogWarning($"RBT Key Not Exist in Remove:{key}");
                        return null;
                    }
                }
            }

            return nodePtr;
        }

        protected static Node Remove_ReplaceWithChild(Node node)
        {
            if (node.Left == null || node.Right == null)
                return node;

            var leftMax = node.Left;
            var leftDepth = 0;
            while (leftMax.Right != null)
            {
                leftMax = leftMax.Right;
                leftDepth++;
            }

            var rightMin = node.Right;
            var rightDepth = 0;
            while (rightMin.Left != null)
            {
                rightMin = rightMin.Left;
                rightDepth++;
            }

            // prefer a red node directly
            if (Node.IsRed(leftMax))
            {
                node.Key = leftMax.Key;
                return leftMax;
            }

            if (Node.IsRed(rightMin))
            {
                node.Key = rightMin.Key;
                return rightMin;
            }

            // prefer a node with a red child
            if (Node.IsRed(leftMax.Left))
            {
                node.Key = leftMax.Key;
                return leftMax;
            }

            if (Node.IsRed(rightMin.Right))
            {
                node.Key = rightMin.Key;
                return rightMin;
            }

            // prefer a deeper node
            if (leftDepth > rightDepth)
            {
                node.Key = leftMax.Key;
                return leftMax;
            }

            node.Key = rightMin.Key;
            return rightMin;
        }

        protected Node Remove_Internal(Node node)
        {
            if (node == _root)
            {
                if (node.Left != null)
                {
                    _root = node.Left;
                    _root.Parent = null;
                    _root.Color = Node.NodeColor.Black;
                }
                else if (node.Right != null)
                {
                    _root = node.Right;
                    _root.Parent = null;
                    _root.Color = Node.NodeColor.Black;
                }
                else
                {
                    _root = null;
                }

                return null;
            }

            var parent = node.Parent;
            Action<Node> parentSetChild;
            if (node == parent.Left)
            {
                parentSetChild = child => { parent.SetLeft(child); };
            }
            else
            {
                parentSetChild = child => { parent.SetRight(child); };
            }

            // simple case - node is red
            if (Node.IsRed(node))
            {
                var child = node.Left ?? node.Right;
                parentSetChild(child);
                return null;
            }

            // simple case - node child is red
            if (Node.IsRed(node.Left))
            {
                parentSetChild(node.Left);
                node.Left.Color = Node.NodeColor.Black;
                return null;
            }

            if (Node.IsRed(node.Right))
            {
                parentSetChild(node.Right);
                node.Right.Color = Node.NodeColor.Black;
                return null;
            }

            // complex case - double black
            var doubleBlack = node.Left ?? node.Right;
            parentSetChild(doubleBlack);
            return doubleBlack;
        }

        protected Node Remove_Adjust(Node node)
        {
            var parent = node.Parent;
            Action<Node> rotateLeft, rotateRight;
            Node sister;
            if (parent.Left == node)
            {
                sister = parent.Right;
                rotateLeft = RotateLeft;
                rotateRight = RotateRight;
            }
            else
            {
                sister = parent.Left;
                rotateLeft = RotateRight;
                rotateRight = RotateLeft;
            }

            if (Node.IsRed(sister))
            {
                rotateRight(parent);
                parent.Color = Node.NodeColor.Red;
                sister.Color = Node.NodeColor.Black;

                if (_root == parent)
                {
                    _root = sister;
                }

                return sister;
            }

            if (Node.IsRed(parent))
            {
                sister.Color = Node.NodeColor.Red;
                parent.Color = Node.NodeColor.Black;
                return null;
            }

            if (Node.IsBlack(sister.Left) && Node.IsBlack(sister.Right))
            {
                sister.Color = Node.NodeColor.Red;
                return parent;
            }

            if (Node.IsRed(sister.Left))
            {
                var sisterLeft = sister.Left;
                rotateRight(sister);
                sisterLeft.Color = Node.NodeColor.Black;
                sister.Color = Node.NodeColor.Red;
                sister = sister.Left;
            }

            rotateLeft(parent);
            sister.Right.Color = Node.NodeColor.Black;
            if (_root == parent)
            {
                _root = sister;
            }

            return null;
        }

        public void Remove(TKey key)
        {
            var node = Remove_Find(key);

            if (node == null)
                return;

            node = Remove_ReplaceWithChild(node);

            var doubleBlack = Remove_Internal(node);

            while (doubleBlack != null)
            {
                doubleBlack = Remove_Adjust(doubleBlack);
            }
        }

        protected Node GetRoot()
        {
            return _root;
        }

        public class Node : BinaryTreeNode<Node>
        {
            public enum NodeColor
            {
                Red,
                Black
            }

            public NodeColor Color;

            public TKey Key;

            public Node(TKey key, NodeColor color)
            {
                Key = key;
                Color = color;
            }

            public static bool IsBlack(Node node)
            {
                return node == null || node.Color == NodeColor.Black;
            }

            public static bool IsRed(Node node)
            {
                return node is { Color: NodeColor.Red };
            }
        }
    }
}