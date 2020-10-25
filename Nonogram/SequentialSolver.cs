using System;
using System.Collections.Generic;
using System.Text;

namespace Nonogram
{
    /// <summary>
    /// #TODO: Document
    /// </summary>
    internal class SequentialSolver : Solver
    {
        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="board"></param>
        internal SequentialSolver(GameState board, bool solveAll) : base(board, solveAll)
        {
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        internal override void Solve()
        {
            base.Solve();
            Solve(initialState);
        }

        /// <summary>
        /// #TODO: Document, Clean
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="row"></param>
        private void Solve(GameState state)
        {
            if (!solveAll && solutions.Count > 0) return;

            foreach (GameState subState in StatePermutations(state))
            {
                if (ValidatePermutation(subState))
                {
                    if (subState.IsFinal())
                    {
                        solutions.Add(subState);
                        return;
                    }
                    else
                    {
                        Solve(subState);
                    }
                }
            }
        }
    }
}
