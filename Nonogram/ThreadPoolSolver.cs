
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nonogram
{
    internal class ThreadPoolSolver : Solver
    {
        private int maxThreads;
        private int timeout;

        internal ThreadPoolSolver(GameState gameState, int maxThreads, int timeout) : base(gameState)
        {
            this.maxThreads = maxThreads;
            this.timeout = timeout;
        }

        internal override void Solve()
        {
            base.Solve();
            SolverThreadPoolHandler handler = new SolverThreadPoolHandler(maxThreads, 8, timeout);
            handler.Start(Board);
            Solutions.Add(handler.Solution);
        }
    }

    internal class SolverThreadPoolHandler
    {
        private ManualResetEvent terminateEvent = new ManualResetEvent(false);

        private GameState solution;
        private int maxThreads;
        private long maxThreadQueue;
        private int timeout;

        internal GameState Solution => solution;

        internal SolverThreadPoolHandler(int maxThreads, int maxThreadQueue, int timeout)
        {
            this.maxThreads = maxThreads;
            this.maxThreadQueue = maxThreadQueue;
            this.timeout = timeout;

            ThreadPool.SetMinThreads(maxThreads, maxThreads);
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);
        }

        internal void Start(GameState state)
        {
            ThreadPool.QueueUserWorkItem(Solve, state);
            terminateEvent.WaitOne(timeout);
            terminateEvent.Set();
        }

        private void Solve(object state)
        {
            Solve((GameState)state);
        }

        private void Solve(GameState state)
        {
            if(terminateEvent.WaitOne(0))
                return;

            IEnumerable<GameState> subStates = Solver.GetSubStates(state, state.TargetRow);
            state.IncrementRowTarget();

            foreach (GameState subState in subStates)
            {
                if (Solver.ValidatePermutation(subState))
                {
                    if (subState.IsFinal())
                    {
                        solution = subState;
                        terminateEvent.Set();
                        return;
                    }
                    else
                    {
                        if (ThreadPool.PendingWorkItemCount <= maxThreadQueue)
                        {
                            ThreadPool.QueueUserWorkItem(Solve, subState);
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
}
