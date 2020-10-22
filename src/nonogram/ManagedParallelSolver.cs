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
            SolverThreadHandler handler = new SolverThreadHandler(2);
        }
    }

    internal class SolverThreadHandler
    {
        SolverThread[] threads;
        int maxThreads;
        int availableThreads;

        internal SolverThreadHandler(int maxThreads)
        {
            this.threads = new SolverThread[maxThreads];
            this.maxThreads = maxThreads;
            this.availableThreads = maxThreads;
        }
    }

    internal class SolverThread
    {
        SolverThreadHandler handler;
        Thread thread;
        int id;
    }
}

