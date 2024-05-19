using System;
using System.Collections.Generic;
using Algorithm;

namespace PuzzleGame
{
    public class AStarSearch : Search
    {
        private readonly StateBuilder _ptr = new();
        private readonly List<Step> _reverseSteps = new();

        private readonly PriorityQueue<StateKey, State> _stateQueue = new();

        private static int EvaluateStateH(State state)
        {
            var h = 0;
            var index = 0;
            for (var r = 0; r < Row; r++)
            for (var c = 0; c < Column; c++)
            {
                if (r == state.EmptyRowIdx && c == state.EmptyColumnIdx)
                    continue;

                var targetRow = state.Chessboard[index] / Column;
                var targetColumn = state.Chessboard[index] % Column;

                h += Math.Abs(targetRow - r) + Math.Abs(targetColumn - c);

                index++;
            }

            return h;
        }

        private bool TryAddState(int lastIdx, int gn, State newState, string targetKey)
        {
            var currentIdx = Pair2Index(newState.EmptyRowIdx, newState.EmptyColumnIdx);
            (newState.Chessboard[lastIdx], newState.Chessboard[currentIdx]) =
                (newState.Chessboard[currentIdx], newState.Chessboard[lastIdx]);
            var key = newState.ToString();

            if (StateCache.ContainsKey(key))
                return false;

            StateCache.Add(key, new Step(lastIdx, currentIdx));
            var hn = EvaluateStateH(newState);
            _stateQueue.Enqueue(new StateKey(hn * 10, gn), newState);
            return key == targetKey;
        }

        public override (bool, int) GetSteps(Queue<Step> steps, State current, State idle)
        {
            steps.Clear();

            var currentKey = current.ToString();
            var idleKey = idle.ToString();
            if (currentKey == idleKey)
                return (true, 1);

            _stateQueue.Clear();
            _stateQueue.Enqueue(new StateKey(0, 0), current);
            StateCache.Clear();
            StateCache.Add(currentKey, null);

            while (_stateQueue.Count > 0)
            {
                var lastNode = _stateQueue.Dequeue();
                var state = lastNode.Value;
                var gn = lastNode.Key.Gn + 1;
                var lastIndex = Pair2Index(state.EmptyRowIdx, state.EmptyColumnIdx);

                if (state.EmptyRowIdx > 0)
                {
                    var chessboardCopy = new List<int>(state.Chessboard);
                    var probableNext = new State(chessboardCopy, state.EmptyRowIdx - 1, state.EmptyColumnIdx);

                    if (TryAddState(lastIndex, gn, probableNext, idleKey)) break;
                }

                if (state.EmptyRowIdx < Row - 1)
                {
                    var chessboardCopy = new List<int>(state.Chessboard);
                    var probableNext = new State(chessboardCopy, state.EmptyRowIdx + 1, state.EmptyColumnIdx);

                    if (TryAddState(lastIndex, gn, probableNext, idleKey)) break;
                }

                if (state.EmptyColumnIdx > 0)
                {
                    var chessboardCopy = new List<int>(state.Chessboard);
                    var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx - 1);

                    if (TryAddState(lastIndex, gn, probableNext, idleKey)) break;
                }

                if (state.EmptyColumnIdx < Column - 1)
                {
                    var chessboardCopy = new List<int>(state.Chessboard);
                    var probableNext = new State(chessboardCopy, state.EmptyRowIdx, state.EmptyColumnIdx + 1);

                    if (TryAddState(lastIndex, gn, probableNext, idleKey)) break;
                }
            }

            if (!StateCache.ContainsKey(idleKey)) return (false, StateCache.Count);

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

            for (var i = _reverseSteps.Count - 1; i >= 0; i--) steps.Enqueue(_reverseSteps[i]);

            return (true, StateCache.Count);
        }

        private readonly struct StateKey : IComparable<StateKey>
        {
            public readonly int Gn;
            private readonly int _hn;

            public int CompareTo(StateKey other)
            {
                return Gn + _hn - other.Gn - other._hn;
            }

            public StateKey(int h, int g)
            {
                _hn = h;
                Gn = g;
            }
        }
    }
}