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

        public static int Pair2Index(int r, int c)
        {
            return r * Column + c;
        }

        public static (int, int) Index2Pair(int i)
        {
            return (i / Column, i % Column);
        }

        public virtual (bool, int) GetSteps(Queue<Step> steps, State current, State idle)
        {
            StateCache.Clear();
            CurrentState.Clear();
            NextState.Clear();

            CurrentState.Add(current);

            return (true, 0);
        }

        protected class StateBuilder
        {
            private readonly List<int> _builder = new();
            private readonly StringBuilder _sb = new();

            public int this[int i]
            {
                get => _builder[i];
                set => _builder[i] = value;
            }

            public override string ToString()
            {
                _sb.Clear();
                foreach (var num in _builder)
                {
                    _sb.Append(num);
                }

                return _sb.ToString();
            }

            public void Reset(List<int> target)
            {
                _builder.Clear();
                _builder.AddRange(target);
            }
        }
    }

    public class BreathFirstSearch : Search
    {
        private readonly List<Step> _reverseSteps = new();
        private readonly StateBuilder _ptr = new();

        private bool TryAddState(int lastIdx, State newState, string targetKey)
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

        public override (bool, int) GetSteps(Queue<Step> steps, State current, State idle)
        {
            steps.Clear();

            var currentKey = current.ToString();
            var idleKey = idle.ToString();
            if (currentKey == idleKey)
                return (true, 1);

            base.GetSteps(steps, current, idle);

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
                return (false, StateCache.Count);

            _ptr.Reset(idle.Chessboard);
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

            return (true, StateCache.Count);
        }
    }

    public class BidirectionalBreathFirstSearch : Search
    {
        private readonly Dictionary<string, Step> _reverseStateCache = new();
        private List<State> _reverseCurrentState = new(), _reverseNextState = new();

        private readonly List<Step> _reverseSteps = new();
        private readonly StateBuilder _ptr = new();

        private bool TryAddState(int lastIdx, State newState)
        {
            var currentIdx = Pair2Index(newState.EmptyRowIdx, newState.EmptyColumnIdx);
            (newState.Chessboard[lastIdx], newState.Chessboard[currentIdx]) =
                (newState.Chessboard[currentIdx], newState.Chessboard[lastIdx]);
            var key = newState.ToString();

            if (StateCache.ContainsKey(key))
                return false;

            StateCache.Add(key, new Step(lastIdx, currentIdx));
            NextState.Add(newState);

            return _reverseStateCache.ContainsKey(key);
        }

        private bool TryAddReverseState(int lastIdx, State newState)
        {
            var currentIdx = Pair2Index(newState.EmptyRowIdx, newState.EmptyColumnIdx);
            (newState.Chessboard[lastIdx], newState.Chessboard[currentIdx]) =
                (newState.Chessboard[currentIdx], newState.Chessboard[lastIdx]);
            var key = newState.ToString();

            if (_reverseStateCache.ContainsKey(key))
                return false;

            _reverseStateCache.Add(key, new Step(lastIdx, currentIdx));
            _reverseNextState.Add(newState);

            return StateCache.ContainsKey(key);
        }


        public override (bool, int) GetSteps(Queue<Step> steps, State current, State idle)
        {
            steps.Clear();

            var currentKey = current.ToString();
            var idleKey = idle.ToString();
            if (currentKey == idleKey)
                return (true, 1);

            base.GetSteps(steps, current, idle);

            StateCache.Add(currentKey, null);

            _reverseStateCache.Clear();
            _reverseCurrentState.Clear();
            _reverseNextState.Clear();

            _reverseStateCache.Add(idleKey, null);
            _reverseCurrentState.Add(idle);

            List<int> bothState = null;

            while (CurrentState.Count > 0 && bothState == null)
            {
                foreach (var state in CurrentState)
                {
                    var lastIndex = Pair2Index(state.EmptyRowIdx, state.EmptyColumnIdx);

                    if (state.EmptyRowIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx - 1, state.EmptyColumnIdx);

                        if (TryAddState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }

                    if (state.EmptyRowIdx < Row - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx + 1, state.EmptyColumnIdx);

                        if (TryAddState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx - 1);

                        if (TryAddState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx < Column - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx + 1);

                        if (TryAddState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }
                }

                (CurrentState, NextState) = (NextState, CurrentState);
                NextState.Clear();

                if (bothState != null)
                    break;

                foreach (var state in _reverseCurrentState)
                {
                    var lastIndex = Pair2Index(state.EmptyRowIdx, state.EmptyColumnIdx);

                    if (state.EmptyRowIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx - 1, state.EmptyColumnIdx);

                        if (TryAddReverseState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }

                    if (state.EmptyRowIdx < Row - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx + 1, state.EmptyColumnIdx);

                        if (TryAddReverseState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx > 0)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx - 1);

                        if (TryAddReverseState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }

                    if (state.EmptyColumnIdx < Column - 1)
                    {
                        var chessboardCopy = new List<int>(state.Chessboard);
                        var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx + 1);

                        if (TryAddReverseState(lastIndex, probableNext))
                        {
                            bothState = probableNext.Chessboard;
                            break;
                        }
                    }
                }

                (_reverseCurrentState, _reverseNextState) = (_reverseNextState, _reverseCurrentState);
                _reverseNextState.Clear();
            }

            if (bothState == null)
            {
                return (false, StateCache.Count + _reverseStateCache.Count);
            }


            _ptr.Reset(bothState);
            var ptrStr = _ptr.ToString();
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

            _ptr.Reset(bothState);
            ptrStr = _ptr.ToString();

            while (ptrStr != idleKey)
            {
                var step = _reverseStateCache[ptrStr];
                steps.Enqueue(step);
                (_ptr[step.EmptyFrom], _ptr[step.EmptyTo]) = (_ptr[step.EmptyTo], _ptr[step.EmptyFrom]);
                ptrStr = _ptr.ToString();
            }

            return (true, StateCache.Count + _reverseStateCache.Count - 1);
        }
    }
}