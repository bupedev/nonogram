using System;
using System.IO;
using System.Xml.Serialization;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Runtime.Serialization;
using System.Diagnostics;

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

            // Root command
            RootCommand rootCommand = new RootCommand();

            rootCommand.AddGlobalOption(outputOption);
            rootCommand.AddGlobalOption(inputOption);
            rootCommand.AddGlobalOption(verboseOption);

            // Solve command
            Command solveCommand = new Command("solve");

            solveCommand.AddOption(methodOption);
            solveCommand.AddOption(sourceOption);
            solveCommand.AddOption(idOption);

            solveCommand.Handler = CommandHandler.Create<SolvingMethod, PuzzleSource, DirectoryInfo, DirectoryInfo, int, bool>(Solve);

            // Play command
            Command playCommand = new Command("play");

            playCommand.AddOption(sourceOption);
            playCommand.AddOption(idOption);

            playCommand.Handler = CommandHandler.Create<PuzzleSource, DirectoryInfo, DirectoryInfo, int, bool>(Play);

            rootCommand.AddCommand(solveCommand);
            rootCommand.AddCommand(playCommand);

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

        // #TODO:
        //  - Document
        //  - Implement solving method switch
        //  - Command output
        internal static void Solve(SolvingMethod method, PuzzleSource source, DirectoryInfo output, DirectoryInfo input, int id, bool verbose)
        {
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
                    default:
                        solver = new SequentialSolver(gameState);
                        break;
                }

                Logging.Message($"Attempting to solve using {method} solver...", verbose);

                decimal timeElapsed = TimeSolverAverage(solver, 1);

                Logging.Message($"{solver.Solutions.Count} solution(s) found in {timeElapsed:0.00E+00} seconds:");

                foreach (GameState solution in solver.Solutions)
                {
                    Console.WriteLine();
                    solution.Print();
                }

                Console.WriteLine();
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

        private static decimal TimeSolverAverage(Solver solver, int trials)
        {
            decimal[] times = TimeSolverComplete(solver, trials);

            decimal totalTime = 0;
            foreach (decimal time in times)
            {
                totalTime += time;
            }
            return totalTime / trials;
        }

        private static decimal[] TimeSolverComplete(Solver solver, int trials)
        {
            Stopwatch watch = new Stopwatch();

            decimal[] times = new decimal[trials];
            for (int i = 0; i < trials; i++)
            {
                watch.Start();
                solver.Solve();
                watch.Stop();

                times[i] = (decimal) watch.ElapsedTicks / Stopwatch.Frequency;
            }

            return times;
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

    [Serializable]
    internal class MissingDataException : Exception
    {
        public MissingDataException()
        {
        }

        public MissingDataException(string message) : base(message)
        {
        }

        public MissingDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
