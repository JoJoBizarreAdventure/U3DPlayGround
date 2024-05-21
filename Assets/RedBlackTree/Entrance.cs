using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RedBlackTree
{
    public class Entrance : MonoBehaviour
    {
        public GameObject treeNodePrefab;
        public GameObject linePrefab;

        private string _key = "";
        private PrefabPool _pool;
        private ScrollContentSizeFilter _scsf;

        private void Start()
        {
            _scsf = GetComponent<ScrollContentSizeFilter>();
            _pool = new PrefabPool(treeNodePrefab, linePrefab);

            _rbt.TreeChange = TreeChangeImpl;
            _rbt.CoroutineEnd = () => { _coroutine = null; };
        }

        private Coroutine _coroutine;
        private readonly RedBlackTreeVisualize _rbt = new();
        private Transform _root;

        private void RecycleRecursive(Transform node)
        {
            while (node.childCount > 2)
            {
                RecycleRecursive(node.GetChild(2));
            }

            _pool.RecycleObject(node, PrefabType.Node);
        }

        private const int CellSize = 200;
        public Material BlackCircle, RedCircle;

        private Transform CreateRecursive(Transform parent, RedBlackTreeVisualize.Node node, int size, int x, int y)
        {
            var nodeObject = _pool.CreatObject(parent, PrefabType.Node);
            var rectTransform = nodeObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(x, y);
            var text = nodeObject.GetComponentInChildren<TextMeshProUGUI>();
            nodeObject.name = text.text = node != null ? $"{node.Key}" : "null";
            var image = nodeObject.GetComponentInChildren<RawImage>();
            image.material = RedBlackTreeVisualize.Node.IsBlack(node) ? BlackCircle : RedCircle;

            if (node != null)
            {
                var half = size / 2;
                CreateRecursive(nodeObject.transform, node.Left, half, -half, -CellSize);
                CreateRecursive(nodeObject.transform, node.Right, half, half, -CellSize);
            }

            return nodeObject.transform;
        }

        private void TreeChangeImpl()
        {
            if (_root)
            {
                RecycleRecursive(_root);
                _root = null;
            }

            var root = _rbt.GetRoot();
            if (root == null)
                return;

            var maxDepth = _rbt.GetDepth();
            var width = (1 << maxDepth) * CellSize;
            _root = CreateRecursive(transform, root, width / 2, 0, 0);
            _scsf.Resize(width, maxDepth * CellSize);
        }

        private void OnGUI()
        {
            var startX = Screen.width - 550;
            var startY = 50;
            const int boxWidth = 500, boxHeight = 280, gap = 10;
            GUI.skin.box.fontSize = 50;
            GUI.Box(new Rect(startX, startY, boxWidth, boxHeight), "Control");
            startY += gap + 60;

            const int fontSize = 40;
            const int lineHeight = 60;
            GUI.skin.label.fontSize = fontSize;
            const int labelWidth = 80;
            GUI.Label(new Rect(startX + gap, startY, labelWidth, lineHeight), "Key:");
            GUI.skin.textField.fontSize = fontSize;
            _key = GUI.TextField(
                new Rect(startX + labelWidth + gap * 2, startY, boxWidth - labelWidth - gap * 3, lineHeight), _key);
            startY += lineHeight + gap;

            GUI.skin.button.fontSize = fontSize;
            if (GUI.Button(new Rect(startX + gap, startY, boxWidth - gap * 2, lineHeight), "Add") && _coroutine == null)
            {
                int key;
                try
                {
                    key = Convert.ToInt32(_key);
                    Console.WriteLine($"Add Key:{key}");
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                _coroutine = StartCoroutine(_rbt.OneStepAdd(key));
            }

            startY += lineHeight + gap;

            if (GUI.Button(new Rect(startX + gap, startY, boxWidth - gap * 2, lineHeight), "Remove") &&
                _coroutine == null)
            {
                int key;
                try
                {
                    key = Convert.ToInt32(_key);
                    Console.WriteLine($"Remove Key:{key}");
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                _coroutine = StartCoroutine(_rbt.OneStepRemove(key));
            }
        }

        private float _scale = 1.0f;
        private const float ScaleDelta = 0.1f;

        private void Update()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 1e-4)
                return;
            _scale += Input.GetAxis("Mouse ScrollWheel") * ScaleDelta;
            _scale = Mathf.Clamp(_scale, 0.01f, 1.0f);
            transform.localScale = new Vector3(_scale, _scale, 1.0f);
        }
    }
}