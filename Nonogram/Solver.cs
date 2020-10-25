using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Concurrent;

namespace Nonogram
{
    /// <summary>
    /// #TODO: Document
    /// </summary>
    internal abstract class Solver
    {
        protected ConcurrentBag<GameState> solutions;
        protected GameState initialState;
        protected bool solveAll;

        /// <summary>
        /// #TODO: Document
        /// </summary>
        internal GameState InitialState => initialState;
        /// <summary>
        /// #TODO: Document
        /// </summary>
        internal IEnumerable<GameState> Solutions => solutions;

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="initialState"></param>
        internal Solver(GameState initialState, bool solveAll)
        {
            this.solutions = new ConcurrentBag<GameState>();

            this.initialState = initialState;
            this.solveAll = solveAll;
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        internal virtual void Solve()
        {
            initialState.Clear();
            solutions.Clear();
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="gameState"></param>
        /// <returns></returns>
        internal static bool ValidatePermutation(GameState gameState)
        {
            for (int j = 0; j < gameState.Width; ++j)
            {
                if (!gameState.IsColumnValid(j))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        internal static IEnumerable<GameState> StatePermutations(GameState gameState)
        {
            foreach (CellState[] permutation in RowPermutations(gameState.RowHints[gameState.TargetRow], gameState.Width))
            {
                GameState newState = gameState.Clone() as GameState;
                newState.SetTargetRow(permutation);
                yield return newState;
            }
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="hint"></param>
        /// <param name="lineLength"></param>
        /// <returns></returns>
        public static IEnumerable<CellState[]> RowPermutations(Hint hint, int lineLength)
        {
            return GenerateLinePermutations(hint, new CellState[lineLength], 0, 0);
        }

        /// <summary>
        /// #TODO: Document, Clean
        /// </summary>
        /// <param name="hint"></param>
        /// <param name="progressiveState"></param>
        /// <param name="hintIndex"></param>
        /// <param name="positionIndex"></param>
        /// <returns></returns>
        protected static IEnumerable<CellState[]> GenerateLinePermutations(Hint hint, CellState[] progressiveState, int hintIndex, int positionIndex)
        {
            if (hintIndex >= hint.Length)
            {
                yield return progressiveState;
            }
            else
            { 
                int k = progressiveState.Length - (hint.Occupation(hintIndex + 1) + hint[hintIndex]) - positionIndex;
                for (int i = positionIndex; i < positionIndex + k; i++)
                {
                    CellState[] newState = new CellState[progressiveState.Length];
                    progressiveState.CopyTo(newState, 0);
                    for (int j = 0; j < hint[hintIndex]; ++j)
                    {
                        newState[i + j] = CellState.Fill;
                    }
                    foreach (var t in GenerateLinePermutations(hint, newState, hintIndex + 1, i + hint[hintIndex] + 1))
                    { 
                        yield return t;
                    }
                }
            }
        }
    }
}
