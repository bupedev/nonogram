using System;
using System.Collections.Generic;
using System.Text;

namespace Nonogram
{
    internal class SequentialSolver : Solver
    {
        internal SequentialSolver(GameState board) : base(board)
        {
        }

        internal override void Solve()
        {
            Solve(Board, 0);
        }

        internal void Solve(GameState gameState, int row)
        {
            GenerateLinePermutations(out List<CellState[]> permutations, Board.RowHints[row], Board.Width);
            foreach (CellState[] permutation in permutations)
            {
                GameState newGameState = gameState.DeepClone();
                newGameState[row] = permutation;
                bool validPermutation = true;
                for (int j = 0; j < Board.Width; ++j)
                {
                    if (!newGameState.IsColumnValid(j))
                    {
                        //Console.WriteLine("False Permutation");
                        //Console.WriteLine(newGameState);
                        //Console.WriteLine();
                        validPermutation = false;
                        break;
                    }
                }
                if (validPermutation)
                {
                    if (row == Board.Height - 1)
                    {
                        //Console.WriteLine("Valid Solution");
                        //newGameState.Print();
                        //Console.WriteLine();
                        Solutions.Add(newGameState);
                    }
                    else
                    {
                        Solve(newGameState, row + 1);
                    }
                }
            }
        }

        internal static void GenerateLinePermutations(out List<CellState[]> permutations, Hint hint, int lineLength)
        {
            permutations = new List<CellState[]>();
            GenerateLinePermutations(permutations, hint, new CellState[lineLength], 0, 0);
        }

        private static void GenerateLinePermutations(List<CellState[]> permutations, Hint hint, CellState[] states, int hintIdx, int posIdx)
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
