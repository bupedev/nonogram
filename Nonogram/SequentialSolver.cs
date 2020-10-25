using System;
using System.Collections.Generic;
using System.Text;

namespace Nonogram
{
    internal class SequentialSolver : Solver
    {
        internal SequentialSolver(GameState board) : base(board)
        {
        }

        internal override void Solve()
        {
            base.Solve();
            Solve(Board, 0);
        }

        internal void Solve(GameState gameState, int row)
        {
            if (Solutions.Count > 0) return;

            GenerateLinePermutations(out List<CellState[]> permutations, Board.RowHints[row], Board.Width);
            foreach (CellState[] permutation in permutations)
            {
                GameState newGameState = gameState.Clone() as GameState;
                newGameState[row] = permutation;
                
                if (ValidatePermutation(newGameState))
                {
                    if (row == Board.Height - 1)
                    {
                        Solutions.Add(newGameState);
                        return;
                    }
                    else
                    {
                        Solve(newGameState, row + 1);
                    }
                }
            }
        }
    }
}
