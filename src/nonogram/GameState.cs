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
    class GameState
    {
        public void Print()
        {
            foreach (CellState[] row in cells)
            {
                PrintRow(row);
            }
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
                        Console.Write(" ");
                        break;
                    case CellState.Fill:
                        Console.Write("\u25A0");
                        break;
                    case CellState.Void:
                        Console.Write(".");
                        break;
                }
            }
            Console.WriteLine();
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

        public GameState(CellState[][] cells, HintSet rowHints, HintSet columnHints)
        {
            this.cells = cells;
            this.rowHints = rowHints;
            this.columnHints = columnHints;
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

            int voidCountLimit = Height - columnHints[j].Occupation(x);

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
    }
}
