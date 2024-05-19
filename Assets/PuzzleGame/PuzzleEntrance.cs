using UnityEngine;

namespace PuzzleGame
{
    public class PuzzleEntrance : MonoBehaviour
    {
        public int row, column, cellSize;

        public Texture origin;
        private PuzzleControl _control;

        private PuzzleUI _ui;

        private void Start()
        {
            row = row > 0 ? row : 1;
            column = column > 0 ? column : 1;

            _ui = new PuzzleUI(row, column, cellSize, origin);
            _ui.CreateGrid(gameObject);

            _control = new PuzzleControl(row, column, _ui);
            _control.CreateControl(gameObject);
        }
    }
}