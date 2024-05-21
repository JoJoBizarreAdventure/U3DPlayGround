using System;
using System.Collections;
using Algorithm;
using UnityEngine;

namespace RedBlackTree
{
    public class RedBlackTreeVisualize : RedBlackTree<int>
    {
        public IEnumerator OneStepAdd(int key)
        {
            var newNode = Add_Internal(key);
            TreeChange.Invoke();

            while (newNode != null)
            {
                yield return new WaitForSeconds(1);
                newNode = Add_Adjust(newNode);
                TreeChange.Invoke();
            }

            CoroutineEnd.Invoke();
        }

        public IEnumerator OneStepRemove(int key)
        {
            var node = Remove_Find(key);
            if (node == null)
                yield break;

            node = Remove_ReplaceWithChild(node);

            var doubleBlack = Remove_Internal(node);
            TreeChange.Invoke();

            while (doubleBlack != null)
            {
                yield return new WaitForSeconds(1);
                doubleBlack = Remove_Adjust(doubleBlack);
                TreeChange.Invoke();
            }

            CoroutineEnd.Invoke();
        }

        private static int GetDepth_Internal(Node node)
        {
            if (node == null)
                return 0;

            return Math.Max(GetDepth_Internal(node.Left), GetDepth_Internal(node.Right)) + 1;
        }

        public int GetDepth()
        {
            var root = GetRoot();
            return GetDepth_Internal(root);
        }

        public delegate void TreeChangeDelegate();

        public TreeChangeDelegate TreeChange;

        public delegate void CoroutineEndDelegate();

        public CoroutineEndDelegate CoroutineEnd;
    }
}