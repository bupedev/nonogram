using System;
using System.IO;
using System.Xml.Serialization;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nonogram
{
    internal enum SolvingMethod
    { 
        Sequential,
        Parallel
    }

    internal enum PuzzleSource
    { 
        Local,
        WebPBN
    }

    class Program
    {
        static int Main(string[] args)
        {
            Option methodOption = new Option<SolvingMethod>(
                new string[] { "--method", "-m" },
                getDefaultValue: () => SolvingMethod.Sequential,
                description: "The method used to solve the nonogram"
            );

            Option methodsOption = new Option<SolvingMethod[]>(
                new string[] { "--methods", "-mx" },
                getDefaultValue: () => new[] { SolvingMethod.Sequential, SolvingMethod.Parallel },
                description: "The methods used to benchmark the nonogram"
            );

            Option sourceOption = new Option<PuzzleSource>(
                new string[] { "--source", "-s" },
                getDefaultValue: () => PuzzleSource.WebPBN,
                description: "The source of the puzzle to be solved"
            );

            Option idOption = new Option<int>(
                new string[] { "--id" },
                getDefaultValue: () => 0,
                description: "The id of the puzzle to be solved"
            );

            Option idSetOption = new Option<string>(
                new string[] { "--idSet", "-ix" },
                getDefaultValue: () => "4",
                description: "The set of puzzle ids to benchmark (comma delimited)"
            );

            Option trialsOption = new Option<int>(
                new string[] { "--trials", "-t" },
                getDefaultValue: () => 5,
                description: "The number of trials to time per benchmark"
            );

            Option timeoutOption = new Option<int>(
                new string[] { "--timeout", "-to" },
                getDefaultValue: () => 10000,
                description: "The maximum number of milliseconds to wait for a solution"
            );

            Option directoryOption = new Option<DirectoryInfo>(
                new string[] { "--directory", "-d" },
                getDefaultValue: () => new DirectoryInfo(@"./"),
                description: "The working root directory for all I/O"
            );

            Option verboseOption = new Option<bool>(
                new string[] { "--verbose", "-v" },
                getDefaultValue: () => false,
                description: "Make program report all processes"
            );

            Option threadsOption = new Option<int>(
                new string[] { "--threads", "-t" },
                getDefaultValue: () => 2,
                description: "Sets number of threads to use for parallel solvers"
            );

            // Root command
            RootCommand rootCommand = new RootCommand();

            rootCommand.AddGlobalOption(directoryOption);
            rootCommand.AddGlobalOption(verboseOption);
            rootCommand.AddGlobalOption(timeoutOption);
            rootCommand.AddGlobalOption(sourceOption);

            // Solve command
            Command solveCommand = new Command("solve");

            solveCommand.AddOption(methodOption);
            
            solveCommand.AddOption(idOption);
            solveCommand.AddOption(threadsOption);

            solveCommand.Handler = 
                CommandHandler.Create<PuzzleSource, SolvingMethod, DirectoryInfo, int, int, int, bool>(Solve);

            // Play command
            Command playCommand = new Command("play");

            playCommand.AddOption(idOption);

            playCommand.Handler = 
                CommandHandler.Create<PuzzleSource, DirectoryInfo, int, bool>(Play);

            Command benchmarkCommand = new Command("benchmark");

            benchmarkCommand.AddOption(methodsOption);
            benchmarkCommand.AddOption(idSetOption);
            benchmarkCommand.AddOption(trialsOption);
            

            benchmarkCommand.Handler = 
                CommandHandler.Create<PuzzleSource, SolvingMethod[], DirectoryInfo, string, int, int, bool>(Benchmark);

            rootCommand.AddCommand(benchmarkCommand);
            rootCommand.AddCommand(solveCommand);
            rootCommand.AddCommand(playCommand);

            return rootCommand.InvokeAsync(args).Result;
        }

        internal static void Play(PuzzleSource source, DirectoryInfo directory, int id, bool verbose)
        {
            try 
            {
                GameState gameState = GetPuzzle(source, directory, id, verbose);

                throw new NotImplementedException();
            }
            catch (MissingDataException exception)
            {
                Logging.Error(exception.Message);
            }
            catch (NotImplementedException)
            {
                Logging.Error("Feature not implemented.");
            }
            catch(Exception exception)
            {
                #if DEBUG
                    Logging.Error("Unexpected exception");
                    Console.WriteLine(exception);
                #else
                    Logging.Error("Unknown");
                #endif
            }
        }

        internal static void Solve(PuzzleSource source, SolvingMethod method, DirectoryInfo directory, int id, int threads, int timeout, bool verbose)
        {
            try
            {
                GameState gameState = GetPuzzle(source, directory, id, verbose);

                Solver solver;
                double timeElapsed;

                switch (method)
                {
                    case SolvingMethod.Sequential:
                        solver = new SequentialSolver(gameState, timeout);
                        Logging.Message($"Attempting to solve using {method} solver...", verbose);
                        break;
                    case SolvingMethod.Parallel:
                        solver = new ThreadPoolSolver(gameState, timeout, threads);
                        Logging.Message($"Attempting to solve puzzle #{id} using {method} solver ({threads} threads)...");
                        break;
                    default:
                        throw new NotImplementedException();
                }

                timeElapsed = TimeSolverAverage(solver, 1);

                Logging.Message($"{solver.Solutions.Count( x => true )} solution(s) found in {timeElapsed:0.00E+00} seconds:");

                foreach (GameState solution in solver.Solutions)
                {
                    Console.WriteLine();
                    solution.Print();
                }

                Console.WriteLine();
            }
            catch (NonogramException exception)
            {
                Logging.Error(exception.Message);
            }
            catch (TimeoutException exception)
            {
                Logging.Error(exception.Message);
            }
            catch (NotImplementedException)
            {
                Logging.Error("Feature not implemented.");
            }
            catch(System.Exception exception)
            {
                #if DEBUG
                    Logging.Error("Unexpected exception");
                    Console.WriteLine(exception);
                #else
                    Logging.Error("Unknown");
                #endif
            }
        }

        internal static void Benchmark(PuzzleSource source, SolvingMethod[] methods, DirectoryInfo directory, string idSet, int trials, int timeout, bool verbose)
        {
            List<BenchmarkData> benchmarkData = new List<BenchmarkData>();

            foreach(int id in ParseID(idSet))
            {
                GameState gameState;

                try
                {
                    gameState = GetPuzzle(source, directory, id, verbose);

                    foreach (SolvingMethod method in methods)
                    {
                        Solver solver;
                        double time;

                        switch (method)
                        {
                            case SolvingMethod.Sequential:
                                solver = new SequentialSolver(gameState, timeout);

                                Logging.Message($"Attempting to solve puzzle #{id} using {method} solver...");

                                time = TimeSolverAverage(solver, trials);
                                benchmarkData.Add(new BenchmarkData(id, gameState.Width, gameState.Height, method, 1, time));
                                break;
                            case SolvingMethod.Parallel:
                                for (int threads = 1; threads <= Environment.ProcessorCount; threads++)
                                {
                                    solver = new SequentialSolver(gameState, timeout);

                                    Logging.Message($"Attempting to solve puzzle #{id} using {method} solver ({threads} threads)...");

                                    time = TimeSolverAverage(solver, trials);
                                    benchmarkData.Add(new BenchmarkData(id, gameState.Width, gameState.Height, method, threads, time));
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
                catch (NonogramException exception)
                {
                    Logging.Error(exception.Message);
                }
                catch (TimeoutException exception)
                {
                    Logging.Error(exception.Message);
                }
                catch (NotImplementedException)
                {
                    Logging.Error("Feature not implemented.");
                }
                catch (System.Exception exception)
                {
                    #if DEBUG
                        Logging.Error("Unexpected exception");
                        Console.WriteLine(exception);
                    #else
                        Logging.Error("Unknown");
                    #endif
                }
            }

            DateTime dateTime = DateTime.Now;

            FileInfo benchmarkFile = new FileInfo($"{directory.FullName}benchmark_{DateTime.Now:dd-MM-yyyy_hh-mm}.csv");

            Logging.Message($"Saving benchmark data to file: {benchmarkFile.Name}");

            using (StreamWriter writer = new StreamWriter(benchmarkFile.FullName))
            {
                writer.WriteLine(BenchmarkData.HeaderString(trials));
                foreach (BenchmarkData data in benchmarkData)
                {
                    writer.WriteLine(data);
                }
            }
        }

        private static IEnumerable<int> ParseID(string idSet)
        {
            string[] idStrings = idSet.Split(",");
            for (int i = 0; i < idStrings.Length; i++)
            {
                yield return int.Parse(idStrings[i]);
            }
        }

        private static double TimeSolverAverage(Solver solver, int trials)
        {
            double[] times = TimeSolverComplete(solver, trials);

            double totalTime = 0;
            foreach (double time in times)
            {
                totalTime += time;
            }
            return totalTime / trials;
        }

        private static ManualResetEvent timeoutEvent;

        private static double[] TimeSolverComplete(Solver solver, int trials)
        {
            Stopwatch stopwatch = new Stopwatch();            

            double[] times = new double[trials];
            for (int i = 0; i < trials; i++)
            {
                WaitThreads(Environment.ProcessorCount);

                stopwatch.Restart();
                solver.Solve();
                stopwatch.Stop();

                times[i] = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            }

            return times;
        }

        private static void WaitThreads(int threads)
        {
            ThreadPool.SetMinThreads(threads, threads);
            ThreadPool.SetMaxThreads(threads, threads);
            while (true)
            {
                ThreadPool.GetMaxThreads(out int maxWThreads, out int maxCPThreads);
                ThreadPool.GetAvailableThreads(out int wThreads, out int cpThreads);
                if (wThreads >= maxWThreads && cpThreads >= maxCPThreads) break;
            }
        }

        private static GameState GetPuzzle(PuzzleSource source, DirectoryInfo directory, int id, bool verbose)
        {
            InitializeDirectories(directory, new string[] { $"resources/{source.ToString().ToLower()}" }, verbose);

            FileInfo puzzleFile = new FileInfo($"{directory.FullName}resources/{source.ToString().ToLower()}/{id}.xml");

            FetchPuzzle(source, id, puzzleFile, verbose);

            return LoadPuzzle(puzzleFile, verbose);
        }

        private static GameState LoadPuzzle(FileInfo puzzleFile, bool verbose)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PuzzleSet));
            PuzzleSet puzzle = null;

            Logging.Message($"Loading puzzle from {puzzleFile.FullName}...", verbose);

            try
            {
                using (StreamReader reader = new StreamReader(puzzleFile.FullName))
                {
                    puzzle = (PuzzleSet)serializer.Deserialize(reader);
                }
                if (puzzle.Puzzle.PuzzleColours.Length > 2)
                    throw new IncompatiblePuzzleException($"Puzzle has more colours " +
                        $"({puzzle.Puzzle.PuzzleColours.Length}) than current colour limit (2)");
            }
            catch (InvalidOperationException)
            {
                throw new MissingDataException($"{puzzleFile.FullName} has missing or corrupt data.");
            }

            return new GameState(puzzle.Puzzle);
        }

        private static void FetchPuzzle(PuzzleSource source, int id, FileInfo fileStore, bool verbose)
        {
            Scrapper scrapper = new Scrapper();

            Logging.Message($"Fetching puzzle from source ({source})...", verbose);

            if (!fileStore.Exists)
            {
                if (source.Equals(PuzzleSource.Local))
                {
                    throw new MissingDataException($"File not found: {fileStore.FullName}\'");
                }

                using (StreamWriter writer = new StreamWriter(fileStore.FullName))
                {
                    scrapper.GetFromSource(source, id, writer);
                }
            }
        }

        private static void InitializeDirectories(DirectoryInfo directory, string[] subDirectories, bool verbose)
        {
            Logging.Message("Intialzing directories...", verbose);

            directory.Refresh();

            if (!directory.Exists)
            {
                Logging.Message($"Creating {directory.FullName}...", verbose);
                directory.Create();
            }

            if (!directory.Exists)
            {
                Logging.Message($"Creating {directory.FullName}...", verbose);
                directory.Create();
            }

            if (subDirectories != null)
            { 
                foreach (string subDirectory in subDirectories)
                {
                    if (!File.Exists($"{directory.FullName}{subDirectory}"))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo($"{directory.FullName}{subDirectory}");
                        Logging.Message($"Creating {directoryInfo.FullName}...", verbose);
                        directoryInfo.Create();
                    }
                }
            }
        }
    }
}
