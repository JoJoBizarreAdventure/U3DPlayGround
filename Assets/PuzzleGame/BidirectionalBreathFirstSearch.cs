using System.Collections.Generic;

namespace PuzzleGame
{
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