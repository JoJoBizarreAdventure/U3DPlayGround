using System.Collections.Generic;
using System.Text;

namespace PuzzleGame
{
    public class Step
    {
        public readonly int EmptyFrom;
        public readonly int EmptyTo;

        public Step(int i1, int i2)
        {
            EmptyFrom = i1;
            EmptyTo = i2;
        }
    }

    public class State
    {
        public List<int> Chessboard;
        public int EmptyRowIdx, EmptyColumnIdx;

        public State(List<int> chessboard, int r, int c)
        {
            Chessboard = chessboard;
            EmptyRowIdx = r;
            EmptyColumnIdx = c;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var idx in Chessboard)
            {
                sb.Append(idx);
            }

            return sb.ToString();
        }
    }

    public abstract class Search
    {
        protected readonly Dictionary<string, Step> StateCache = new();
        protected List<State> CurrentState = new(), NextState = new();

        protected static int Row, Column;

        public static void SetSize(int row, int column)
        {
            Row = row;
            Column = column;
        }

        protected static int Pair2Index(int r, int c)
        {
            return r * Column + c;
        }

        public static (int, int) Index2Pair(int i)
        {
            return (i / Column, i % Column);
        }

        public virtual bool GetSteps(Queue<Step> steps, State current, string idleKey)
        {
            StateCache.Clear();
            CurrentState.Clear();
            NextState.Clear();

            CurrentState.Add(current);

            return true;
        }

        protected bool TryAddState(int lastIdx, State newState, string targetKey)
        {
            var currentIdx = Pair2Index(newState.EmptyRowIdx, newState.EmptyColumnIdx);
            (newState.Chessboard[lastIdx], newState.Chessboard[currentIdx]) =
                (newState.Chessboard[currentIdx], newState.Chessboard[lastIdx]);
            var key = newState.ToString();

            if (StateCache.ContainsKey(key))
                return false;

            StateCache.Add(key,
                new Step(lastIdx, currentIdx));
            NextState.Add(newState);
            return key == targetKey;
        }
    }

    public class BreathFirstSearch : Search
    {
        private readonly List<Step> _reverseSteps = new();
        private StringBuilder _ptr = new();

        public override bool GetSteps(Queue<Step> steps, State current, string idleKey)
        {
            steps.Clear();

            var currentKey = current.ToString();
            if (currentKey == idleKey)
                return true;

            base.GetSteps(steps, current, idleKey);

            StateCache.Add(currentKey, null);
            var find = false;

            while (CurrentState.Count > 0 && !find)
            {
                foreach (var state in CurrentState)
                {
                    var lastIndex = Pair2Index(state.EmptyRowIdx, state.EmptyColumnIdx);

                    if (state.EmptyRowIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx - 1, state.EmptyColumnIdx);

                        if (TryAddState(lastIndex, probableNext, idleKey))
                        {
                            find = true;
                            break;
                        }
                    }

                    if (state.EmptyRowIdx < Row - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx + 1, state.EmptyColumnIdx);

                        if (TryAddState(lastIndex, probableNext, idleKey))
                        {
                            find = true;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx - 1);

                        if (TryAddState(lastIndex, probableNext, idleKey))
                        {
                            find = true;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx < Column - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx + 1);

                        if (TryAddState(lastIndex, probableNext, idleKey))
                        {
                            find = true;
                            break;
                        }
                    }
                }

                (CurrentState, NextState) = (NextState, CurrentState);
                NextState.Clear();
            }

            if (!find)
                return false;

            _ptr.Clear();
            _ptr.Append(idleKey);
            var ptrStr = idleKey;
            _reverseSteps.Clear();

            while (ptrStr != currentKey)
            {
                var step = StateCache[ptrStr];
                _reverseSteps.Add(step);
                (_ptr[step.EmptyFrom], _ptr[step.EmptyTo]) = (_ptr[step.EmptyTo], _ptr[step.EmptyFrom]);
                ptrStr = _ptr.ToString();
            }

            for (var i = _reverseSteps.Count - 1; i >= 0; i--)
            {
                steps.Enqueue(_reverseSteps[i]);
            }

            return true;
        }
    }
}