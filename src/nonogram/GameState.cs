using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Nonogram
{
    enum CellState 
    { 
        Blank = -1,
        Fill = 1,
        Void = 0
    }

    [Serializable]
    class GameState : ICloneable
    {
        private int targetRow;

        internal int TargetRow => targetRow;
        internal Hint TargetRowHint => rowHints[targetRow];

        internal void SetTargetRow(CellState[] row)
        {
            cells[targetRow] = row;
        }

        internal void IncrementRowTarget()
        {
            targetRow++;
        }

        internal bool IsFinal()
        {
            return targetRow == Height;
        }

        CellState[][] cells;
        HintSet rowHints, columnHints;

        public CellState[][] Cells => cells;

        public CellState[] this[int row]
        {
            get
            {
                return cells[row];
            }
            set
            {
                cells[row] = value;
            }
        }

        public CellState this[int row, int column]
        {
            get 
            {
                return cells[row][column];
            }
            set
            {
                cells[row][column] = value;
            }
        }

        public int Height => RowHints.Length;

        public int Width => ColumnHints.Length;

        public HintSet RowHints => rowHints;

        public HintSet ColumnHints => columnHints;

        public GameState(CellState[][] cells, HintSet rowHints, HintSet columnHints, int targetRow = 0)
        {
            this.cells = cells;
            this.rowHints = rowHints;
            this.columnHints = columnHints;
            this.targetRow = targetRow;
        }

        public GameState(Puzzle puzzle)
        {
            this.rowHints = puzzle.HintSets[1];
            this.columnHints = puzzle.HintSets[0];

            this.cells = new CellState[rowHints.Length][];
            
            for (int i = 0; i < rowHints.Length; i++)
            {
                cells[i] = new CellState[columnHints.Length];
                for (int j = 0; j < columnHints.Length; j++)
                {
                    cells[i][j] = CellState.Blank;
                }
            }
        }

        public bool IsBlank()
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                for (int j = 0; j < cells[0].Length; ++j)
                {
                    if (cells[i][j] != CellState.Blank)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsSolved()
        {
            for (int i = 0; i < cells.Length; ++i)
            {
                if (!IsRowSolved(i))
                {
                    return false;
                }
            }

            for (int j = 0; j < cells[0].Length; ++j)
            {
                if (!IsColumnSolved(j))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsRowSolved(int i)
        {
            int x = 0;
            int count = 0;
            bool block = false;

            for (int j = 0; j < cells[0].Length; ++j)
            {
                switch (cells[i][j])
                {
                    case CellState.Blank:
                        return false;
                    case CellState.Fill:
                        if (!block)
                        {
                            block = true;
                        }
                        ++count;
                        break;
                    case CellState.Void:
                        if (block)
                        {
                            if (rowHints[i][x] != count)
                            {
                                return false;
                            }
                            ++x;
                            if (x > rowHints[i].Length)
                            {
                                return false;
                            }
                            block = false;
                            count = 0;
                        }
                        break;
                }
            }
            if (x < rowHints[i].Length && rowHints[i][x] != count)
            {
                return false;
            }
            return true;
        }

        public bool IsColumnSolved(int j)
        {
            int x = 0;
            int count = 0;
            bool block = false;

            for (int i = 0; i < cells.Length; ++i)
            {
                switch (cells[i][j])
                {
                    case CellState.Blank:
                        return false;
                    case CellState.Fill:
                        if (!block)
                        {
                            block = true;
                        }
                        ++count;
                        break;
                    case CellState.Void:
                        if (block)
                        {
                            if (columnHints[j][x] != count)
                            {
                                return false;
                            }
                            ++x;
                            if (x > columnHints[j].Length)
                            {
                                return false;
                            }
                            block = false;
                            count = 0;
                        }
                        break;
                }
            }
            if (x < columnHints[j].Length && columnHints[j][x] != count)
            {
                return false;
            }
            return true;
        }

        // Cummulative validity
        public bool IsRowValid(int i)
        {
            int x = 0;
            int blockVount = 0;
            int voidCount = 0;
            bool blockLock = false;
            bool voidLock = false;

            int voidCountLimit  = Width - rowHints[i].Occupation(x);

            for (int j = 0; j < cells[0].Length; ++j)
            {
                switch (cells[i][j])
                {
                    case CellState.Fill:
                        if (!voidLock)
                            voidLock = true;
                        if (!blockLock) 
                            blockLock = true;

                        ++blockVount;
                        break;
                    case CellState.Blank:
                        if (blockLock)
                        {
                            if (rowHints[i][x] != blockVount)
                                return false;
                            ++x;
                            if (x > rowHints[i].Length)
                                return false;

                            blockLock = false;
                            blockVount = 0;

                            voidLock = false;
                            voidCount = 0;

                            voidCountLimit = Width - rowHints[i].Occupation(x) - j;
                        }
                        ++voidCount;
                        break;
                    case CellState.Void:
                        if (!voidLock && voidCount >= voidCountLimit)
                            return false;
                        if (blockLock)
                        {
                            if (rowHints[i][x] != blockVount)
                                return false;
                            ++x;
                            if (x > rowHints[i].Length)
                                return false;

                            blockLock = false;
                            blockVount = 0;

                            voidLock = false;
                            voidCount = 0;

                            voidCountLimit = Width - rowHints[i].Occupation(x) - j;
                        }
                        ++voidCount;
                        break;
                }
            }
            if (x > rowHints[i].Length)
                return false;
            if (x < rowHints[i].Length && rowHints[i][x] < blockVount)
                return false;
            return true;
        }

        public bool IsColumnValid(int j)
         {
            int x = 0;
            int blockCount = 0;
            int voidCount = 0;
            bool blockLock = false;
            bool voidLock = false;

            // #TODO: Find a better way of dealing with blank lines
            int voidCountLimit = Height - (columnHints[j].Length == 0 ? 0 :columnHints[j].Occupation(x));

            for (int i = 0; i < cells.Length; ++i)
            {
                switch (cells[i][j])
                {
                    case CellState.Fill:
                        if (!voidLock)
                            voidLock = true;
                        if (!blockLock)
                            blockLock = true;

                        ++blockCount;
                        break;
                    case CellState.Blank:
                        if (blockLock)
                        {
                            if (x >= columnHints[j].Length)
                                return false;
                            if (columnHints[j][x] < blockCount)
                                return false;
                            ++x;
                            

                            blockLock = false;
                            blockCount = 0;

                            voidLock = false;
                            voidCount = 0;

                            voidCountLimit = Height - columnHints[j].Occupation(x) - i;
                        }
                        ++voidCount;
                        break;
                    case CellState.Void:
                        if (!voidLock && voidCount >= voidCountLimit)
                            return false;
                        if (blockLock)
                        {
                            if (columnHints[j][x] != blockCount)
                                return false;
                            ++x;
                            if (x > columnHints[j].Length)
                                return false;

                            blockLock = false;
                            blockCount = 0;

                            voidLock = false;
                            voidCount = 0;

                            voidCountLimit = Height - columnHints[j].Occupation(x) - i;
                        }
                        ++voidCount;
                        break;
                }
            }
            if (x > columnHints[j].Length)
                return false;
            if (x < columnHints[j].Length && columnHints[j][x] < blockCount)
                return false;
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < cells.Length; i++)
            { 
                for (int j = 0; j < cells[0].Length; ++j)
                {
                    cells[i][j] = CellState.Blank;
                }
            }
        }

        public void Print()
        {
            const int columnWidth = 2;

            int rowHintBuffer = CalculateRowHintBuffer();
            int columnHintBuffer = CalculateColumnHintBuffer();

            for (int i = columnHintBuffer; i > 0; i--)
            {
                Console.Write(new string(' ', rowHintBuffer));
                for (int j = 0; j < columnHints.Length; j++)
                {
                    if (j % 2 == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    //Console.WriteLine($"{columnHints[j].Length}, {i}, {columnHintBuffer - i}");
                    if (columnHints[j].Length >= i)
                    {

                        Console.Write(columnHints[j][columnHints[j].Length - i].ToString().PadLeft(columnWidth));
                    }
                    else
                    {
                        Console.Write(new string(' ', columnWidth));
                    }
                }
                Console.WriteLine();
            }

            for (int i = 0; i < rowHints.Length; i++)
            {
                Console.Write(new string(' ', rowHintBuffer - columnWidth * rowHints[i].Length));
                for (int k = 0; k < rowHints[i].Length; k++)
                {
                    if (k % 2 == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    Console.Write(rowHints[i].Counts[k].ToString().PadLeft(columnWidth));
                }
                Console.ForegroundColor = ConsoleColor.White;
                PrintRow(cells[i]);
            }
        }

        private int CalculateRowHintBuffer()
        {
            int max = 0;
            for (int i = 0; i < rowHints.Length; i++)
            {
                int buffer = 3 * rowHints[i].Length - 1;
                max = buffer > max ? buffer : max;
            }
            return max;
        }

        private int CalculateColumnHintBuffer()
        {
            int max = 0;
            for (int j = 0; j < columnHints.Length; j++)
            {
                int buffer = columnHints[j].Length;
                max = buffer > max ? buffer : max;
            }
            return max;
        }

        public static void PrintRow(CellState[] states)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            foreach (CellState state in states)
            {
                Console.Write(" ");
                switch (state)
                {
                    case CellState.Blank:
                        Console.Write(".");
                        break;
                    case CellState.Fill:
                        Console.Write("\u25A0");
                        break;
                    case CellState.Void:
                        Console.Write("x");
                        break;
                }
            }
            Console.WriteLine();
        }

        public void Write(string filePath)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(@filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, this);
            }
        }

        public static GameState Read(string filePath)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(@filePath, FileMode.Open, FileAccess.Read))
            {
                return (GameState)formatter.Deserialize(stream);
            }
        }

        public object Clone()
        {
            return new GameState(cells.Clone() as CellState[][], rowHints, columnHints, targetRow);
        }
    }
}
