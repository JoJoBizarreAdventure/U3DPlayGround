using System.Collections.Generic;

namespace PuzzleGame
{
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
}