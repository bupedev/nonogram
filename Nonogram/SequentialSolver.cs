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
        internal SequentialSolver(GameState board, int timeout) : base(board, timeout)
        {
        }

        /// <summary>
        /// #TODO: Document, Clean
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="row"></param>
        protected override void SolveCallback(object obj)
        {
            GameState state = obj as GameState;

            if (terminateEvent.WaitOne(0))
                return;

            foreach (GameState subState in StatePermutations(state))
            {
                if (ValidatePermutation(subState))
                {
                    if (subState.IsFinal())
                    {
                        solutions.Add(subState);
                        terminateEvent.Set();
                        return;
                    }
                    else
                    {
                        SolveCallback(subState);
                    }
                }
            }
        }
    }
}
