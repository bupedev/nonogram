using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Nonogram
{
    /// <summary>
    /// #TODO: Document
    /// </summary>
    internal abstract class Solver
    {
        protected ManualResetEvent terminateEvent = new ManualResetEvent(false);

        protected ConcurrentBag<GameState> solutions;
        protected GameState initialState;
        protected int timeout;

        /// <summary>
        /// #TODO: Document
        /// </summary>
        internal IEnumerable<GameState> Solutions => solutions;

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="initialState"></param>
        internal Solver(GameState initialState, int timeout)
        {
            this.solutions = new ConcurrentBag<GameState>();

            this.initialState = initialState;
            this.timeout = timeout;
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        internal virtual void Solve()
        {
            initialState.Clear();
            solutions.Clear();
            ThreadPool.QueueUserWorkItem(SolveCallback, initialState);
            terminateEvent.WaitOne(timeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        protected abstract void SolveCallback(object obj);

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="gameState"></param>
        /// <returns></returns>
        internal static bool ValidatePermutation(GameState gameState)
        {
            for (int i = 0; i < gameState.Width; i++)
            {
                if (!gameState.ValidateColumn(i))
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
