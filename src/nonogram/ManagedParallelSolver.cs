using Nonogram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nonogram
{
    internal class ManagedParallelSolver : Solver
    {
        internal ManagedParallelSolver(GameState gameState) : base(gameState)
        { }

        internal override void Solve()
        {
            base.Solve();
            SolverThreadHandler handler = new SolverThreadHandler(20);
            handler.Start(Board);
            Solutions.Add(handler.Solution);
        }
    }

    internal class SolverThreadHandler
    {
        private GameState solution;
        private SolverThread[] threads;
        private bool[] threadLock;
        private int maxThreads;
        private int availableThreads;
        private int unlockMemory;

        internal GameState Solution => solution;
        internal int AvailableThreads => availableThreads;

        internal SolverThreadHandler(int maxThreads)
        {
            this.threads = new SolverThread[maxThreads];
            this.threadLock = new bool[maxThreads];
            this.maxThreads = maxThreads;
            this.availableThreads = maxThreads;
            this.unlockMemory = 0;

            for (int i = 0; i < maxThreads; i++)
            {
                threads[i] = new SolverThread(this, i);
            }
        }

        internal void Start(GameState state)
        {
            threadLock[0] = true;
            availableThreads++;
            threads[0].Run(state);

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].InnerThread.Join();
            }
        }

        internal bool RequestSolve(GameState state)
        {
            for (int i = unlockMemory; i < unlockMemory + maxThreads; i++)
            {
                if (!threadLock[i % maxThreads])
                {
                    threadLock[i % maxThreads] = true;
                    availableThreads++;
                    threads[i % maxThreads].Run(state);
                    return true;
                }
            }
            return false;
        }

        internal void SetSolution(GameState state, int id)
        {
            solution = state;
            for (int i = 0; i < maxThreads; i++)
            {
                threads[i].InnerThread.Abort();
            }
        }

        internal void MarkComplete(int id)
        {
            threadLock[id] = false;
            availableThreads--;
            unlockMemory = id;
        }
    }

    internal class SolverThread
    {
        SolverThreadHandler handler;
        Thread thread;
        int id;

        internal Thread InnerThread => thread;

        internal SolverThread(SolverThreadHandler handler, int id)
        {
            this.handler = handler;
            this.id = id;
        }

        internal void Run(GameState state) 
        {
            thread = new Thread((state) => { 
                Solve(state); 
                PostCompletion(); 
            }) { IsBackground = true };

            thread.Start(state);
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
                    handler.SetSolution(state, id);
                }
                else
                {
                    IEnumerable<GameState> subStates = Solver.GetSubStates(state, state.TargetRow);
                    state.IncrementRowTarget();

                    foreach (GameState subState in subStates)
                    {
                        if (handler.AvailableThreads > 0)
                        {
                            bool success = handler.RequestSolve(subState);
                            if (!success)
                            { 
                                Solve(subState);
                            }
                        }
                        else 
                        {
                            Solve(subState);
                        }
                    }
                }
            }
        }

        private void PostCompletion()
        {
            handler.MarkComplete(id);
        }

    }


}

