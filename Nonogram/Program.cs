using System;
using System.Collections.Generic;
using System.IO;

namespace Nonogram
{
    class Program
    {
        static int counter = 0;
        static void Main(string[] args)
        {
            GameState board = ReadFromFile(@"C:\Users\b7lew\OneDrive - Queensland University of Technology (1)\Documents\Coursework\2020\S2\CAB401\Project\Nonogram64\Nonogram\Puzzles\fishEater.nono");

            Solver solver = new Solver(board);

            solver.Solve();
        }

        private static GameState ReadFromFile(string filename)
        {
            List<Hint> rowHints = new List<Hint>();
            List<Hint> columnHints = new List<Hint>();

            using (StreamReader reader = new StreamReader(filename))
            {
                
                string line;
                bool rowSection = false;
                bool columnSection = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!(rowSection || columnSection || line.Equals(": rows") || line.Equals(": columns")))
                    {
                        continue;
                    }

                    if (line.Equals(": rows"))
                    {
                        rowSection = true;
                        columnSection = false;
                        continue;
                    }

                    if (line.Equals(": columns"))
                    {
                        columnSection = true;
                        rowSection = false;
                        continue;
                    }

                    string[] elements = line.Split(' ');

                    int[] hintNumbers = new int[elements.Length];

                    for (int i = 0; i < elements.Length; ++i)
                    {
                        string substr = elements[i].Substring(0, elements[i].Length - 1);
                        hintNumbers[i] = int.Parse(substr);
                    }

                    Hint hint = new Hint(hintNumbers);

                    if (rowSection)
                        rowHints.Add(hint);

                    if (columnSection)
                        columnHints.Add(hint);
                }
            }

            int N = rowHints.Count, M = columnHints.Count;
            CellState[][] cellStates = new CellState[N][];
            for (int i = 0; i < N; ++i)
            {
                cellStates[i] = new CellState[M];
                for (int j = 0; j < M; ++j)
                {
                    cellStates[i][j] = CellState.Blank;
                }
            }

            return new GameState(cellStates, new HintSet(rowHints.ToArray()), new HintSet(columnHints.ToArray()));
        }
    }
}
