using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PuzzleGame
{
    public class PuzzleControl
    {
        private readonly int _row, _column;
        private readonly PuzzleUI _ui;
        private readonly List<int> _idle;
        private readonly string _idleKey;
        private readonly State _current;

        public PuzzleControl(int row, int column, PuzzleUI ui)
        {
            _row = row;
            _column = column;
            _ui = ui;
            var total = row * column;
            _idle = new List<int>();
            var sb = new StringBuilder();
            for (var i = 0; i < total; i++)
            {
                _idle.Add(i);
                sb.Append(i);
            }

            _idleKey = sb.ToString();
            _current = new State(_idle, row - 1, column - 1);

            Search.SetSize(row, column);
            _bfs = new BreathFirstSearch();
        }

        #region Search

        private readonly Queue<Step> _steps = new();

        private readonly BreathFirstSearch _bfs;

        private void BreathFirstSearch()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var find = _bfs.GetSteps(_steps, _current, _idleKey);

            stopwatch.Stop();

            AddLog($"BFS time cost: {stopwatch.Elapsed.ToString()}");
            if (find) return;
            AddLog("BFS no solution");
        }

        #endregion

        #region Log

        private TextMeshProUGUI _logText;
        private readonly Queue<string> _logCache = new();

        private void AddLog(string logText)
        {
            _logCache.Enqueue(logText);

            while (_logCache.Count >= 9)
            {
                _logCache.Dequeue();
            }

            var sb = new StringBuilder();
            foreach (var line in _logCache)
            {
                sb.Append(line).Append('\n');
            }

            _logText.text = sb.ToString();
        }

        #endregion

        #region UI

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
            verticalLayoutGroup.spacing = 0;
            verticalLayoutGroup.childControlHeight = false;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;

            var rectTransform = verticalLayoutGroupGameObject.GetComponent<RectTransform>();
            var parentWidth = parent.GetComponent<RectTransform>().sizeDelta.x;
            rectTransform.anchoredPosition = new Vector2(0.3f * parentWidth, 0);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);

            CreateButton(verticalLayoutGroupGameObject, "Reset", () =>
            {
                _current.Chessboard = _idle;
                _current.EmptyRowIdx = _row - 1;
                _current.EmptyColumnIdx = _column - 1;

                _ui.ApplyIndexes(_current.Chessboard);
            });
            CreateButton(verticalLayoutGroupGameObject, "Random", () =>
            {
                _current.Chessboard = _current.Chessboard.OrderBy(_ => Random.value).ToList();

                var emptyIdx = _current.Chessboard.IndexOf(_current.Chessboard.Count - 1);
                _current.EmptyRowIdx = emptyIdx / _column;
                _current.EmptyColumnIdx = emptyIdx % _column;
                _ui.ApplyIndexes(_current.Chessboard);
            });
            CreateButton(verticalLayoutGroupGameObject, "BFS", BreathFirstSearch);
            CreateButton(verticalLayoutGroupGameObject, "OneStep", () =>
            {
                if (_steps.Count == 0)
                {
                    AddLog($"One Step: Empty Steps");
                    return;
                }


                var step = _steps.Dequeue();
                (_current.Chessboard[step.EmptyFrom], _current.Chessboard[step.EmptyTo]) =
                    (_current.Chessboard[step.EmptyTo], _current.Chessboard[step.EmptyFrom]);
                _ui.ApplyIndexes(_current.Chessboard);
                AddLog($"One Step: Swap {Search.Index2Pair(step.EmptyFrom)} and {Search.Index2Pair(step.EmptyTo)}");
                if (_steps.Count == 0)
                {
                    AddLog($"One Step: All Steps Complete");
                }
            });

            var logBgImageObject = new GameObject("LogBg");
            logBgImageObject.transform.SetParent(verticalLayoutGroupGameObject.transform);
            var logBgImage = logBgImageObject.AddComponent<RawImage>();
            logBgImage.color = Color.white;
            logBgImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
            logBgImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 280);

            var logObject = new GameObject("LogText");
            logObject.transform.SetParent(logBgImageObject.transform);
            _logText = logObject.AddComponent<TextMeshProUGUI>();
            _logText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500);
            _logText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 280);
            _logText.color = Color.black;
            _logText.fontSize = 30;
        }

        #endregion
    }
}