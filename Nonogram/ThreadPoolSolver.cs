
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nonogram
{
    /// <summary>
    /// #TODO: Document
    /// </summary>
    internal class ThreadPoolSolver : Solver
    {
        private ManualResetEvent terminateEvent = new ManualResetEvent(false);

        private int maxThreads;
        private long maxThreadQueue;
        private int timeout;

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="maxThreads"></param>
        /// <param name="timeout"></param>
        internal ThreadPoolSolver(GameState gameState, bool solveAll, int maxThreads, int timeout, long maxThreadQueue = 0) 
            : base(gameState, solveAll)
        {
            this.maxThreads = maxThreads;
            this.maxThreadQueue = maxThreadQueue;
            this.timeout = timeout;

            ThreadPool.SetMinThreads(maxThreads, maxThreads);
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        internal override void Solve()
        {
            base.Solve();

            ThreadPool.QueueUserWorkItem(SolveCallback, initialState);
            terminateEvent.WaitOne(timeout);
            terminateEvent.Set();
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="state"></param>
        private void SolveCallback(object obj)
        {
            GameState state = obj as GameState;

            if (!solveAll && terminateEvent.WaitOne(0))
                return;

            IEnumerable<GameState> subStates = StatePermutations(state);

            foreach (GameState subState in subStates)
            {
                if (ValidatePermutation(subState))
                {
                    if (subState.IsFinal())
                    {
                        solutions.Add(subState);
                        if (!solveAll)
                        {
                            terminateEvent.Set();
                        }
                        return;
                    }
                    else
                    {
                        if (ThreadPool.PendingWorkItemCount <= maxThreadQueue)
                        {
                            ThreadPool.QueueUserWorkItem(SolveCallback, subState);
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
}
