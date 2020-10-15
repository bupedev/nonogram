using System;
using System.Collections.Generic;
using System.Text;

namespace Nonogram
{
    internal class ParallelSolver : Solver
    {
        internal ParallelSolver(GameState gameState) : base(gameState)
        { }

        internal override void Solve()
        {
            base.Solve();
            throw new NotImplementedException();
        }
    }
}
