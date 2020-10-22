using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Concurrent;

namespace Nonogram
{
    internal abstract class Solver
    {
        internal GameState Board { get; }
        internal ConcurrentBag<GameState> Solutions { get; }

        internal Solver(GameState board)
        {
            Board = board;
            Solutions = new ConcurrentBag<GameState>();
        }

        internal virtual void Solve()
        {
            Board.Clear();
            Solutions.Clear();
        }

        protected static bool ValidatePermutation(GameState gameState)
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

        protected static void GenerateLinePermutations(out List<CellState[]> permutations, Hint hint, int lineLength)
        {
            permutations = new List<CellState[]>();
            GenerateLinePermutations(permutations, hint, new CellState[lineLength], 0, 0);
        }

        protected static void GenerateLinePermutations(List<CellState[]> permutations, Hint hint, CellState[] states, int hintIdx, int posIdx)
        {
            if (hintIdx >= hint.Length)
            {
                permutations.Add(states);
                return;
            }
            int k = states.Length - (hint.Occupation(hintIdx + 1) + hint[hintIdx]) - posIdx;
            for (int i = posIdx; i < posIdx + k; i++)
            {
                CellState[] newState = new CellState[states.Length];
                states.CopyTo(newState, 0);
                for (int j = 0; j < hint[hintIdx]; ++j)
                {
                    newState[i + j] = CellState.Fill;
                }
                GenerateLinePermutations(permutations, hint, newState, hintIdx + 1, i + hint[hintIdx] + 1);
            }
        }
    }
}
