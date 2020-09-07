using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Nonogram
{
    class Solver
    {
        private GameState board;
        private List<GameState> solutions;

        public Solver(GameState board)
        {
            this.board = board;
            solutions = new List<GameState>();
        }

        public void Solve()
        {
            Solve(board, 0);
        }

        public void Solve(GameState gameState, int row)
        {
            Solver.GenerateLinePermutations(out List<CellState[]> permutations, board.RowHints[row], board.Width);
            foreach (CellState[] permutation in permutations)
            {
                GameState newGameState = gameState.DeepClone();
                newGameState[row] = permutation;
                bool validPermutation = true;
                for (int j = 0; j < board.Width; ++j)
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
                    if (row == board.Height - 1)
                    {
                        Console.WriteLine("Valid Solution");
                        newGameState.Print();
                        Console.WriteLine();
                        solutions.Add(newGameState);
                        return;
                    }
                    else 
                    { 
                        Solve(newGameState, row + 1);
                    }
                }
            }
        }

        public static void GenerateLinePermutations(out List<CellState[]> permutations, Hint hint, int lineLength)
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
