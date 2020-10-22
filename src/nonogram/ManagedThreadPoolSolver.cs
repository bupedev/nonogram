
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Nonogram
{
    internal class ManagedThreadPoolSolver : Solver
    {
        internal ManagedThreadPoolSolver(GameState gameState) : base(gameState)
        { }

        internal override void Solve()
        {
            base.Solve();
            SolverThreadPoolHandler handler = new SolverThreadPoolHandler(8, 0);
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

        internal GameState Solution => solution;

        internal SolverThreadPoolHandler(int maxThreads, int maxThreadQueue)
        {
            this.maxThreads = maxThreads;
            this.maxThreadQueue = maxThreadQueue;

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);
        }

        internal void Start(GameState state)
        {
            ThreadPool.QueueUserWorkItem(Solve, state);
            terminateEvent.WaitOne();
        }

        private void Solve(object state)
        {
            Solve((GameState)state);
        }

        private void Solve(GameState state)
        {
            if (Solver.ValidatePermutation(state))
            {
                if (state.IsFinal())
                {
                    solution = state;
                    terminateEvent.Set();
                }
                else
                {
                    IEnumerable<GameState> subStates = Solver.GetSubStates(state, state.TargetRow);
                    state.IncrementRowTarget();

                    foreach (GameState subState in subStates)
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
