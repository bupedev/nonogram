
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
        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="maxThreads"></param>
        /// <param name="timeout"></param>
        internal ThreadPoolSolver(GameState gameState, int timeout, int maxThreads) 
            : base(gameState, timeout)
        {
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
            terminateEvent.Set();
        }

        /// <summary>
        /// #TODO: Document
        /// </summary>
        /// <param name="state"></param>
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
                        if (ThreadPool.PendingWorkItemCount == 0)
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
