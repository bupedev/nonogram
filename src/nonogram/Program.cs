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

            Command solveCommand = new Command("solve");

            solveCommand.AddOption(methodOption);
            solveCommand.AddOption(sourceOption);
            solveCommand.AddOption(idOption);

            solveCommand.Handler = CommandHandler.Create<SolvingMethod, PuzzleSource, DirectoryInfo, DirectoryInfo, int, bool>(Solve);

            RootCommand rootCommand = new RootCommand();

            rootCommand.AddGlobalOption(outputOption);
            rootCommand.AddGlobalOption(inputOption);
            rootCommand.AddGlobalOption(verboseOption);

            rootCommand.AddCommand(solveCommand);

            return rootCommand.InvokeAsync(args).Result;
            
        }

        // #TODO:
        //  - Document
        //  - Implement solving method switch
        //  - Command output
        //  - Verbose mode
        internal static void Solve(SolvingMethod method, PuzzleSource source, DirectoryInfo output, DirectoryInfo input, int id, bool verbose)
        {
            try
            {
                if (verbose)
                {
                    Console.WriteLine("Intialzing directories...");
                }

                input.Create();
                output.Create();

                input.CreateSubdirectory($"resources/{source.ToString().ToLower()}");

                Scrapper scrapper = new Scrapper();
                StringWriter xmlWritter = new StringWriter();

                FileInfo puzzleFile = new FileInfo($"{input.FullName}resources/{source.ToString().ToLower()}/{id}.xml");

                if (verbose)
                {
                    Console.WriteLine("Fetching puzzle from source...");
                }

                if (!puzzleFile.Exists)
                {
                    if (source.Equals(PuzzleSource.Local))
                    {
                        throw new MissingDataException($"Could not find file \'{puzzleFile.FullName}\'");
                    }

                    using (StreamWriter writer = new StreamWriter(puzzleFile.FullName))
                    {
                        scrapper.GetFromSource(source, id, writer);
                    }
                }

                PuzzleSet puzzle = null;

                XmlSerializer serializer = new XmlSerializer(typeof(PuzzleSet));

                if (verbose)
                {
                    Console.WriteLine("Loading puzzle...");
                }

                string response = xmlWritter.ToString();
                try 
                { 
                    using (StreamReader reader = new StreamReader(puzzleFile.FullName))
                    {
                        puzzle = (PuzzleSet)serializer.Deserialize(reader);
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new MissingDataException($"Puzzle {id} from \'{source}\' has missing or corrupt data");
                }

                GameState gameState = new GameState(puzzle.Puzzle);

                Solver solver = new SequentialSolver(gameState);

                if (verbose)
                {
                    Console.WriteLine("Solving...");
                }

                Stopwatch watch = new Stopwatch();

                watch.Start();
                solver.Solve();
                watch.Stop();

                Console.WriteLine($"{solver.Solutions.Count} solutions found in {((decimal)watch.ElapsedTicks/Stopwatch.Frequency):0.00E+00} seconds.");

                foreach (GameState solution in solver.Solutions)
                {
                    Console.WriteLine();
                    solution.Print();
                }

                Console.WriteLine();

            }
            catch (MissingDataException exception)
            {
                Console.WriteLine($"Error: {exception.Message}");
            }
            catch
            {
                Console.WriteLine($"Unknown Error");
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
