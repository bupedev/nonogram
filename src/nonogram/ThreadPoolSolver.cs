using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nonogram
{
    internal class ThreadPoolSolver : Solver
    {
        public ThreadPoolSolver(GameState board) : base(board)
        {
        }

        internal override void Solve()
        {
            base.Solve();
            Solve(Board, 0);
        }

        internal void Solve(GameState gameState, int row)
        {
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
                    }
                    else
                    {
                        Solve(newGameState, row + 1);
                    }
                }
            }
        }
    }

    internal class SolverThread
    {
        private ManualResetEvent doneEvent;

        internal SolverThread(GameState gameState, int row)
        { 
            
        }

        internal void ThreadPoolCallback(Object threadContext)
        { 
            
        }
    }

    internal struct ThreadContext
    {
        GameState gameState;
        int targetRow;
        int threadNumber;
    }
}
