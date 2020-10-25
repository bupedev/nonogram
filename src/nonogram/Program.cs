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

namespace Nonogram
{
    internal enum SolvingMethod
    { 
        Sequential,
        Parallel,
        Async,
        ManagedParallel,
        ThreadPool
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
                getDefaultValue: () => new[] { SolvingMethod.Sequential, SolvingMethod.ThreadPool },
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
                getDefaultValue: () => 5000,
                description: "The maximum number of milliseconds to wait for a solution"
            );

            Option outputOption = new Option<DirectoryInfo>(
                new string[] { "--output", "-o" },
                getDefaultValue: () => new DirectoryInfo(@"./"),
                description: "The directory where output files are stored"
            );

            Option inputOption = new Option<DirectoryInfo>(
                new string[] { "--input", "-i" },
                getDefaultValue: () => new DirectoryInfo(@"./"),
                description: "The directory where input files are stored"
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

            rootCommand.AddGlobalOption(outputOption);
            rootCommand.AddGlobalOption(inputOption);
            rootCommand.AddGlobalOption(verboseOption);
            rootCommand.AddGlobalOption(timeoutOption);

            // Solve command
            Command solveCommand = new Command("solve");

            solveCommand.AddOption(methodOption);
            solveCommand.AddOption(sourceOption);
            solveCommand.AddOption(idOption);
            solveCommand.AddOption(threadsOption);

            solveCommand.Handler = CommandHandler.Create<SolvingMethod, DirectoryInfo, DirectoryInfo, int, int, int, bool>(Solve);

            // Play command
            Command playCommand = new Command("play");

            playCommand.AddOption(sourceOption);
            playCommand.AddOption(idOption);

            playCommand.Handler = CommandHandler.Create<PuzzleSource, DirectoryInfo, DirectoryInfo, int, bool>(Play);

            Command benchmarkCommand = new Command("benchmark");

            benchmarkCommand.AddOption(methodsOption);
            benchmarkCommand.AddOption(idSetOption);
            benchmarkCommand.AddOption(trialsOption);

            benchmarkCommand.Handler = CommandHandler.Create<SolvingMethod[], DirectoryInfo, DirectoryInfo, string, int, int, bool> (Benchmark);

            rootCommand.AddCommand(benchmarkCommand);
            rootCommand.AddCommand(solveCommand);
            rootCommand.AddCommand(playCommand);

            //Solve(SolvingMethod.Sequential, new DirectoryInfo(".\\"), new DirectoryInfo(".\\"), 48, 1, 100000, true);
            //Solve(SolvingMethod.ManagedParallel, new DirectoryInfo(".\\"), new DirectoryInfo(".\\"), 48, 8, 100000, true);

            //return 0;

            return rootCommand.InvokeAsync(args).Result;
        }

        internal static void Play(PuzzleSource source, DirectoryInfo output, DirectoryInfo input, int id, bool verbose)
        {
            try 
            {
                GameState gameState = GetPuzzle(source, output, input, id, verbose);

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

        internal static void Solve(SolvingMethod method, DirectoryInfo output, DirectoryInfo input, int id, int threads, int timeout, bool verbose)
        {
            PuzzleSource source = PuzzleSource.WebPBN;

            try
            {
                GameState gameState = GetPuzzle(source, output, input, id, verbose);

                Solver solver;

                switch (method)
                {
                    case SolvingMethod.Sequential:
                        solver = new SequentialSolver(gameState);
                        break;
                    case SolvingMethod.Parallel:
                        solver = new ParallelSolver(gameState);
                        break;
                    case SolvingMethod.ManagedParallel:
                        solver = new ManagedParallelSolver(gameState);
                        break;
                    case SolvingMethod.ThreadPool:
                        solver = new ThreadPoolSolver(gameState, threads, timeout);
                        break;
                    case SolvingMethod.Async:
                        solver = new AsyncSolver(gameState);
                        break;
                    default:
                        solver = new SequentialSolver(gameState);
                        break;
                }

                Logging.Message($"Attempting to solve using {method} solver...", verbose);

                double timeElapsed = 0;

                if (method.Equals(SolvingMethod.Sequential))
                {
                    timeElapsed = TimeSolverAverage(solver, 1, Environment.ProcessorCount, true, timeout);
                }
                else
                {
                    timeElapsed = TimeSolverAverage(solver, 1);
                }

                Logging.Message($"{solver.Solutions.Count} solution(s) found in {timeElapsed:0.00E+00} seconds:");

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

        internal static void Benchmark(SolvingMethod[] methods, DirectoryInfo output, DirectoryInfo input, string idSet, int trials, int timeout, bool verbose)
        {
            PuzzleSource source = PuzzleSource.WebPBN;

            List<BenchmarkData> benchmarkData = new List<BenchmarkData>();
            int idIndex = 0;
            int successCounter = 0;

            string[] idStrings = idSet.Split(",");
            int[] idArray = new int[idStrings.Length];
            for (int i = 0; i < idArray.Length; i++)
            {
                idArray[i] = int.Parse(idStrings[i]);
            }

            foreach(int id in idArray)
            {
                GameState gameState;

                try
                {
                    gameState = GetPuzzle(source, output, input, id, verbose);

                    //Solver timeoutSolver = new ManagedThreadPoolSolver(gameState, Environment.ProcessorCount, timeout);

                    //double time = TimeSolverAverage(timeoutSolver, 1, Environment.ProcessorCount);
                    //if (timeoutSolver.Solutions.TryPeek(out GameState solution))
                    //{ 
                    //    if (solution == null)
                    //    {
                    //        throw new NonogramException("Puzzle solution doesn't exist or timed out!");
                    //    }

                    //}

                    foreach (SolvingMethod method in methods)
                    {
                        if (method.Equals(SolvingMethod.Sequential))
                        {
                            Solver solver = new SequentialSolver(gameState);

                            Logging.Message($"Attempting to solve puzzle #{id} using {method} solver...");

                            double time = TimeSolverAverage(solver, trials, 1, false);

                            benchmarkData.Add(new BenchmarkData(id, gameState.Width, gameState.Height, method, 1, time));
                        }
                        else
                        {
                            int maxThreads = Environment.ProcessorCount;
                            for (int threadCount = 1; threadCount <= maxThreads; threadCount++)
                            {
                                Solver solver;

                                switch (method)
                                {
                                    // others can go in here once they have the appropriate interface (gameState, threadCount, timeout)
                                    case SolvingMethod.ThreadPool:
                                        solver = new ThreadPoolSolver(gameState, threadCount, timeout);
                                        break;
                                    default:
                                        solver = new ThreadPoolSolver(gameState, threadCount, timeout);
                                        break;
                                }

                                Logging.Message($"Attempting to solve puzzle #{id} using {method} solver ({threadCount} threads)...");

                                double times = TimeSolverAverage(solver, trials, threadCount);

                                benchmarkData.Add(new BenchmarkData(id, gameState.Width, gameState.Height, method, threadCount, times));
                            }
                        }
                        successCounter++;
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

                idIndex++;
            }

            DateTime dateTime = DateTime.Now;

            FileInfo benchmarkFile = new FileInfo($"{output.FullName}benchmark_{DateTime.Now:dd-MM-yyyy_hh-mm}.csv");

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

        private static double TimeSolverAverage(Solver solver, int trials, int threads = 2, bool useTimer = false, int timeout = 10000)
        {
            double[] times = TimeSolverComplete(solver, trials, threads, useTimer, timeout);

            double totalTime = 0;
            foreach (double time in times)
            {
                totalTime += time;
            }
            return totalTime / trials;
        }

        private static ManualResetEvent timeoutEvent;

        private static double[] TimeSolverComplete(Solver solver, int trials, int threads = 2, bool useTimer = false, int timeout = 5000)
        {
            

            double[] times = new double[trials];
            for (int i = 0; i < trials; i++)
            {
                WaitThreads(Environment.ProcessorCount);
                timeoutEvent = new ManualResetEvent(false);
                Stopwatch watch = new Stopwatch();

                WaitCallback waitCallback = (obj) =>
                {
                    if (timeoutEvent.WaitOne(0)) return;
                    watch.Restart();
                    solver.Solve();
                    watch.Stop();
                    timeoutEvent.Set();
                };

                if (useTimer)
                {
                    ThreadPool.QueueUserWorkItem(waitCallback);
                    timeoutEvent.WaitOne(timeout);
                }
                else
                {
                    waitCallback(null);  
                }


                times[i] = (double) watch.ElapsedTicks / Stopwatch.Frequency;
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

        private static GameState GetPuzzle(PuzzleSource source, DirectoryInfo output, DirectoryInfo input, int id, bool verbose)
        {
            InitializeDirectories(input, output, new string[] { $"resources/{source.ToString().ToLower()}" }, null, verbose);

            FileInfo puzzleFile = new FileInfo($"{input.FullName}resources/{source.ToString().ToLower()}/{id}.xml");

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

        private static void InitializeDirectories(DirectoryInfo input, DirectoryInfo output, string[] inputSubDirectories, string[] outputSubDirectories, bool verbose)
        {
            Logging.Message("Intialzing directories...", verbose);

            input.Refresh();
            output.Refresh();

            if (!input.Exists)
            {
                Logging.Message($"Creating {input.FullName}...", verbose);
                input.Create();
            }

            if (!output.Exists)
            {
                Logging.Message($"Creating {output.FullName}...", verbose);
                output.Create();
            }

            if (inputSubDirectories != null)
            { 
                foreach (string subdirectory in inputSubDirectories)
                {
                    if (!File.Exists($"{input.FullName}{subdirectory}"))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo($"{input.FullName}{subdirectory}");
                        Logging.Message($"Creating {directoryInfo.FullName}...", verbose);
                        directoryInfo.Create();
                    }
                }
            }

            if (outputSubDirectories != null)
            { 
                foreach (string subDirectory in outputSubDirectories)
                {
                    if (!File.Exists($"{output.FullName}{subDirectory}"))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo($"{output.FullName}{subDirectory}");
                        Logging.Message($"Creating {directoryInfo}...", verbose);
                        directoryInfo.Create();
                    }
                }
            }
        }
    }
}
