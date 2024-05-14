using System;
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
        private class State
        {
            public List<int> Chessboard;
            public int EmptyRowIdx, EmptyColumnIdx;

            public State(List<int> chessboard, int r, int c)
            {
                Chessboard = chessboard;
                EmptyRowIdx = r;
                EmptyColumnIdx = c;
            }
        }

        private readonly int _row, _column;
        private readonly PuzzleUI _ui;
        private readonly State _current;
        private readonly List<int> _idle;

        private int Pair2Index(int r, int c)
        {
            return r * _column + c;
        }

        private (int, int) Index2Pair(int i)
        {
            return (i / _column, i % _column);
        }

        public PuzzleControl(int row, int column, PuzzleUI ui)
        {
            _row = row;
            _column = column;
            _ui = ui;
            var total = _row * _column;
            _idle = new List<int>();
            for (var i = 0; i < total; i++)
            {
                _idle.Add(i);
            }

            _current = new State(_idle, _row - 1, _column - 1);
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

        private class Step
        {
            public readonly int EmptyFrom;
            public readonly int EmptyTo;


            public Step(int i1, int i2)
            {
                EmptyFrom = i1;
                EmptyTo = i2;
            }
        }

        private Queue<Step> _steps = new();

        private static string StateToString(List<int> state)
        {
            var sb = new StringBuilder();
            foreach (var idx in state)
            {
                sb.Append(idx);
            }

            return sb.ToString();
        }

        private void BreathFirstSearch()
        {
            var stopwatch = new Stopwatch();
            var stateCache = new Dictionary<string, Step>();
            var currentState = new List<State> { _current };
            var nextState = new List<State>();
            var sb = new StringBuilder();
            for (var i = 0; i < _current.Chessboard.Count; i++)
            {
                sb.Append(i);
            }

            var idleKey = StateToString(_idle);
            var currentKey = StateToString(_current.Chessboard);
            if (idleKey == currentKey)
                return;

            stopwatch.Start();
            stateCache.Add(currentKey, null);
            var find = false;


            while (currentState.Count > 0 && !find)
            {
                foreach (var state in currentState)
                {
                    var lastIndex = Pair2Index(state.EmptyRowIdx, state.EmptyColumnIdx);
                    if (state.EmptyRowIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx - 1, state.EmptyColumnIdx);

                        if (JudgeState(lastIndex, probableNext))
                        {
                            find = true;
                            break;
                        }
                    }

                    if (state.EmptyRowIdx < _row - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx + 1, state.EmptyColumnIdx);

                        if (JudgeState(lastIndex, probableNext))
                        {
                            find = true;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx - 1);

                        if (JudgeState(lastIndex, probableNext))
                        {
                            find = true;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx < _column - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx + 1);

                        if (JudgeState(lastIndex, probableNext))
                        {
                            find = true;
                            break;
                        }
                    }
                }

                (currentState, nextState) = (nextState, currentState);
                nextState.Clear();
            }

            stopwatch.Stop();

            AddLog($"BFS time cost: {stopwatch.Elapsed.ToString()}");
            if (!stateCache.ContainsKey(idleKey))
            {
                AddLog("BFS no solution");
                return;
            }

            _steps.Clear();
            var ptr = new StringBuilder(idleKey);
            var ptrStr = idleKey;
            var steps = new List<Step>();
            while (ptrStr != currentKey)
            {
                var step = stateCache[ptrStr];
                steps.Add(step);
                (ptr[step.EmptyFrom], ptr[step.EmptyTo]) = (ptr[step.EmptyTo], ptr[step.EmptyFrom]);
                ptrStr = ptr.ToString();
            }

            for (var i = steps.Count - 1; i >= 0; i--)
            {
                _steps.Enqueue(steps[i]);
            }

            return;

            bool JudgeState(int lastIdx, State newState)
            {
                var currentIdx = Pair2Index(newState.EmptyRowIdx, newState.EmptyColumnIdx);
                (newState.Chessboard[lastIdx], newState.Chessboard[currentIdx]) =
                    (newState.Chessboard[currentIdx], newState.Chessboard[lastIdx]);
                var key = StateToString(newState.Chessboard);

                if (stateCache.ContainsKey(key))
                    return false;

                stateCache.Add(key,
                    new Step(lastIdx, currentIdx));
                nextState.Add(newState);
                return key == idleKey;
            }
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
                _current.Chessboard = _current.Chessboard.OrderBy(x => Random.value).ToList();

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
                AddLog($"One Step: Swap {Index2Pair(step.EmptyFrom)} and {Index2Pair(step.EmptyTo)}");
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
    }
}