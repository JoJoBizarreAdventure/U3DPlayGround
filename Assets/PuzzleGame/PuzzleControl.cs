using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PuzzleGame
{
    public class PuzzleControl
    {
        private List<int> _current;
        private readonly int _row, _column;
        private readonly PuzzleUI _ui;

        public PuzzleControl(int row, int column, PuzzleUI ui)
        {
            _row = row;
            _column = column;
            _ui = ui;
            var total = _row * _column;
            _current = new List<int>(total);
            for (var i = 0; i < total; i++)
            {
                _current.Add(i);
            }
        }

        private static void CreateButton(GameObject parent, string buttonName, UnityAction action)
        {
            var buttonObject = new GameObject(buttonName);
            buttonObject.transform.SetParent(parent.transform);

            var bgImage = buttonObject.AddComponent<RawImage>();
            bgImage.color = Color.white;

            var hintTextObject = new GameObject("ButtonText");
            hintTextObject.transform.SetParent(buttonObject.transform);
            var text = hintTextObject.AddComponent<TextMeshProUGUI>();
            text.text = buttonName;
            text.fontSize = 50;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.autoSizeTextContainer = true;

            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
            rectTransform.anchoredPosition = Vector2.zero;

            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(action);
        }

        public void CreateControl(GameObject parent)
        {
            var verticalLayoutGroupGameObject = new GameObject("control");
            verticalLayoutGroupGameObject.transform.SetParent(parent.transform);

            var verticalLayoutGroup = verticalLayoutGroupGameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = 200;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

            var rectTransform = verticalLayoutGroupGameObject.GetComponent<RectTransform>();
            var parentWidth = parent.GetComponent<RectTransform>().sizeDelta.x;
            rectTransform.anchoredPosition = new Vector2(0.3f * parentWidth, 0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);

            CreateButton(verticalLayoutGroupGameObject, "Reset", () =>
            {
                _current.Clear();
                var total = _row * _column;
                for (var i = 0; i < total; i++)
                {
                    _current.Add(i);
                }

                _ui.ApplyIndexes(_current);
            });
            CreateButton(verticalLayoutGroupGameObject, "Random", () =>
            {
                _current = _current.OrderBy(x => Random.value).ToList();
                _ui.ApplyIndexes(_current);
            });
            CreateButton(verticalLayoutGroupGameObject, "BFS", () => { Debug.Log("BFS"); });
        }
    }
}