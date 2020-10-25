using System.Collections.Generic;

namespace Nonogram
{
    internal struct BenchmarkData
    {
        private int puzzleID;
        private int width;
        private int height;
        private SolvingMethod method;
        private int threadCount;
        private double times;

        internal BenchmarkData(int puzzleID, int width, int height, SolvingMethod method, int threadCount, double times) {
            this.puzzleID = puzzleID;
            this.width = width;
            this.height = height;
            this.method = method;
            this.threadCount = threadCount;
            this.times = times;
        }

        public override string ToString()
        {
            return $"{puzzleID},{width},{height},{method},{threadCount},{times/*string.Join(",", times)*/}";
        }

        public static string HeaderString(int trials)
        {
            //string[] timesHeaders = new string[trials];
            //for (int t = 0; t < trials; t++)
            //{
            //    timesHeaders[t] = $"T{t + 1}";
            //}

            return $"PuzzleID,Width,Height,Method,ThreadCount,{"AverageTime"/*string.Join(",", timesHeaders)*/}";
        }
    }
}