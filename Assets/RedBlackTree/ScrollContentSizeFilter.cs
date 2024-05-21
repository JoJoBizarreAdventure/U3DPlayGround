using UnityEngine;

namespace RedBlackTree
{
    public class ScrollContentSizeFilter : MonoBehaviour
    {
        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Resize(int width, int height)
        {
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}