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

        public void Resize()
        {
            float minX = 0, maxX = 0, minY = 0;

            foreach (var rect in GetComponentsInChildren<RectTransform>())
            {
                var childWidth = rect.rect.width;
                var childHeight = rect.rect.height;
                ;
                var anchorPos = rect.anchoredPosition;

                minX = Mathf.Min(anchorPos.x - childWidth * 0.5f, minX);
                maxX = Mathf.Max(anchorPos.x + childWidth * 0.5f, maxX);

                minY = Mathf.Min(anchorPos.y - childHeight, minY);
            }

            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(minY));
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                Mathf.Max(Mathf.Abs(minX), maxX) * 2.0f);
        }
    }
}