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
}