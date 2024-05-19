using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly State _current;
        private readonly State _idle;
        private readonly int _row, _column;
        private readonly PuzzleUI _ui;

        public PuzzleControl(int row, int column, PuzzleUI ui)
        {
            _row = row;
            _column = column;
            _ui = ui;
            var total = row * column;
            var idleBoard = new List<int>();
            var sb = new StringBuilder();
            for (var i = 0; i < total; i++)
            {
                idleBoard.Add(i);
                sb.Append(i);
            }

            _idle = new State(idleBoard, row - 1, column - 1);
            _current = new State(new List<int>(_idle.Chessboard), row - 1, column - 1);

            Search.SetSize(row, column);
        }

        #region Search

        private readonly Queue<Step> _steps = new();

        private readonly BreathFirstSearch _bfs = new();

        private void BreathFirstSearch()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            bool find;
            int states;
            (find, states) = _bfs.GetSteps(_steps, _current, _idle);
            stopwatch.Stop();

            AddLog($"BFS time cost: {stopwatch.Elapsed.ToString()}");
            AddLog($"BFS states cnt: {states}");
            if (find) return;
            AddLog("BFS no solution");
        }

        private readonly BidirectionalBreathFirstSearch _bbfs = new();

        private void BidirectionalBreathFirstSearch()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            bool find;
            int states;
            (find, states) = _bbfs.GetSteps(_steps, _current, _idle);
            stopwatch.Stop();

            AddLog($"B-BFS time cost: {stopwatch.Elapsed.ToString()}");
            AddLog($"B-BFS states cnt: {states}");
            if (find) return;
            AddLog("B-BFS no solution");
        }

        private readonly AStarSearch _ass = new();

        private void AStarSearch()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            bool find;
            int states;
            (find, states) = _ass.GetSteps(_steps, _current, _idle);
            stopwatch.Stop();

            AddLog($"AStar time cost: {stopwatch.Elapsed.ToString()}");
            AddLog($"AStar states cnt: {states}");
            if (find) return;
            AddLog("AStar no solution");
        }

        #endregion

        #region Log

        private TextMeshProUGUI _logText;
        private readonly Queue<string> _logCache = new();

        private void AddLog(string logText)
        {
            _logCache.Enqueue(logText);

            while (_logCache.Count >= 9) _logCache.Dequeue();

            var sb = new StringBuilder();
            foreach (var line in _logCache) sb.Append(line).Append('\n');

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
            text.fontSize = 30;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.autoSizeTextContainer = true;

            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
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
                _current.Chessboard.Clear();
                _current.Chessboard.AddRange(_idle.Chessboard);
                _current.EmptyRowIdx = _idle.EmptyRowIdx;
                _current.EmptyColumnIdx = _idle.EmptyColumnIdx;

                _ui.ApplyIndexes(_current.Chessboard);
            });
            CreateButton(verticalLayoutGroupGameObject, "Shuffle", () =>
            {
                for (var i = 0; i < 2 * _row * _column;)
                {
                    var op = Convert.ToInt32(Random.value * 3.99);
                    switch (op)
                    {
                        case 0 when _current.EmptyRowIdx > 0:
                        {
                            var lastIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);
                            _current.EmptyRowIdx--;
                            var currentIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);

                            (_current.Chessboard[lastIdx], _current.Chessboard[currentIdx]) = (
                                _current.Chessboard[currentIdx], _current.Chessboard[lastIdx]);

                            i++;
                            break;
                        }
                        case 1 when _current.EmptyRowIdx < _row - 1:
                        {
                            var lastIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);
                            _current.EmptyRowIdx++;
                            var currentIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);

                            (_current.Chessboard[lastIdx], _current.Chessboard[currentIdx]) = (
                                _current.Chessboard[currentIdx], _current.Chessboard[lastIdx]);

                            i++;
                            break;
                        }
                        case 2 when _current.EmptyColumnIdx > 0:
                        {
                            var lastIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);
                            _current.EmptyColumnIdx--;
                            var currentIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);

                            (_current.Chessboard[lastIdx], _current.Chessboard[currentIdx]) = (
                                _current.Chessboard[currentIdx], _current.Chessboard[lastIdx]);

                            i++;
                            break;
                        }
                        case 3 when _current.EmptyColumnIdx < _column - 1:
                        {
                            var lastIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);
                            _current.EmptyColumnIdx++;
                            var currentIdx = Search.Pair2Index(_current.EmptyRowIdx, _current.EmptyColumnIdx);

                            (_current.Chessboard[lastIdx], _current.Chessboard[currentIdx]) = (
                                _current.Chessboard[currentIdx], _current.Chessboard[lastIdx]);

                            i++;
                            break;
                        }
                    }
                }

                _ui.ApplyIndexes(_current.Chessboard);
            });
            CreateButton(verticalLayoutGroupGameObject, "BFS", BreathFirstSearch);
            CreateButton(verticalLayoutGroupGameObject, "BidirectionalBFS", BidirectionalBreathFirstSearch);
            CreateButton(verticalLayoutGroupGameObject, "AStarSearch", AStarSearch);
            CreateButton(verticalLayoutGroupGameObject, "OneStep", () =>
            {
                if (_steps.Count == 0)
                {
                    AddLog("One Step: Empty Steps");
                    return;
                }


                var step = _steps.Dequeue();
                (_current.Chessboard[step.EmptyFrom], _current.Chessboard[step.EmptyTo]) =
                    (_current.Chessboard[step.EmptyTo], _current.Chessboard[step.EmptyFrom]);
                (_current.EmptyRowIdx, _current.EmptyColumnIdx) = Search.Index2Pair(step.EmptyTo);

                _ui.ApplyIndexes(_current.Chessboard);
                AddLog($"One Step: Swap {Search.Index2Pair(step.EmptyFrom)} and {Search.Index2Pair(step.EmptyTo)}");
                if (_steps.Count == 0) AddLog("One Step: All Steps Complete");
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