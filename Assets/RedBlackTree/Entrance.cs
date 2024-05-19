using UnityEngine;

namespace RedBlackTree
{
    public class Entrance : MonoBehaviour
    {
        public GameObject treeNodePrefab;
        private ScrollContentSizeFilter _scsf;

        private void Start()
        {
            _scsf = GetComponent<ScrollContentSizeFilter>();

            var obj1 = Instantiate(treeNodePrefab, gameObject.transform);

            var obj2 = Instantiate(treeNodePrefab, gameObject.transform);
            var rect = obj2.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(2000, -2000);

            _scsf.Resize();
        }
    }
}