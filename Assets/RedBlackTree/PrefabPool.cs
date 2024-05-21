using System.Collections.Generic;
using UnityEngine;

namespace RedBlackTree
{
    public enum PrefabType
    {
        Node,
        Line
    }

    public class PrefabPool
    {
        private readonly GameObject _nodePrefab, _linePrefab;
        private readonly Dictionary<PrefabType, Stack<GameObject>> _pool = new();

        public PrefabPool(GameObject nodePrefab, GameObject linePrefab)
        {
            _nodePrefab = nodePrefab;
            _linePrefab = linePrefab;

            _pool[PrefabType.Node] = new Stack<GameObject>();
            _pool[PrefabType.Line] = new Stack<GameObject>();
        }

        public GameObject CreatObject(Transform parent, PrefabType type)
        {
            var stack = _pool[type];
            if (stack.Count <= 0)
                return Object.Instantiate(type == PrefabType.Node ? _nodePrefab : _linePrefab, parent);
            var obj = stack.Pop();
            obj.SetActive(true);
            obj.transform.SetParent(parent);
            return obj;
        }

        public void RecycleObject(Transform transform, PrefabType type)
        {
            transform.gameObject.SetActive(false);
            transform.SetParent(null);
            _pool[type].Push(transform.gameObject);
        }
    }
}