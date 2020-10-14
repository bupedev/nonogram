using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Nonogram
{
    internal abstract class Solver
    {
        internal GameState Board { get; }
        internal List<GameState> Solutions { get; }

        internal Solver(GameState board)
        {
            Board = board;
            Solutions = new List<GameState>();
        }

        internal abstract void Solve();
    }
}
