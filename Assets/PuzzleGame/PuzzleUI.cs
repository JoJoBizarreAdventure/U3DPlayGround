using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame
{
    public class PuzzleUI
    {
        private readonly int _row, _column, _cellSize;
        private readonly Texture _origin;

        public PuzzleUI(int row, int column, int cellSize, Texture origin)
        {
            _row = row;
            _column = column;
            _cellSize = cellSize;
            _origin = origin;
        }

        private readonly List<RawImage> _splits = new();

        private void CreateChild(GameObject parent, int c, int r)
        {
            var child = new GameObject($"piece({r},{c})");
            var split = child.AddComponent<RawImage>();
            child.transform.SetParent(parent.transform);

            split.texture = _origin;
            var columnUnit = 1.0f / _column;
            var rowUnit = 1.0f / _row;
            r = _row - r - 1;
            split.uvRect = new Rect(c * columnUnit, r * rowUnit, columnUnit, rowUnit);
            split.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _cellSize);
            split.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _cellSize);
            split.rectTransform.pivot = Vector2.up;

            _splits.Add(split);
        }

        private readonly List<Vector2> _positions = new();

        public void ApplyIndexes(List<int> indexes)
        {
            for (var i = 0; i < _splits.Count; i++)
            {
                _splits[indexes[i]].rectTransform.anchoredPosition = _positions[i];
            }
        }

        public void CreateGrid(GameObject parent)
        {
            var gridLayoutGroupGameObject = new GameObject("grid", typeof(RectTransform));
            gridLayoutGroupGameObject.transform.SetParent(parent.transform);

            var spaceSize = _cellSize / 50f;
            var totalWidth = _cellSize * _column + spaceSize * (_column - 1);
            var totalHeight = _cellSize * _row + spaceSize * (_row - 1);
            var startX = -totalWidth / 2.0f;
            var startY = totalHeight / 2.0f;

            for (var r = 0; r < _row; r++)
            {
                for (var c = 0; c < _column; c++)
                {
                    _positions.Add(
                        new Vector2(startX + c * (_cellSize + spaceSize), startY - r * (_cellSize + spaceSize)));
                }
            }

            for (var r = 0; r < _row; r++)
            {
                for (var c = 0; c < _column; c++)
                {
                    CreateChild(gridLayoutGroupGameObject, c, r);
                }
            }

            _splits[^1].uvRect = new Rect(0, 0, 0, 0);

            var parentWidth = parent.GetComponent<RectTransform>().sizeDelta.x;
            var rectTransform = gridLayoutGroupGameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(-0.2f * parentWidth, 0);

            var defaultIndexes = new List<int>();
            for (var i = 0; i < _splits.Count; i++)
            {
                defaultIndexes.Add(i);
            }

            ApplyIndexes(defaultIndexes);
        }
    }
}