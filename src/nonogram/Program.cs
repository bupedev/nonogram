using System;
using System.IO;
using System.Xml.Serialization;
using System.CommandLine;
using System.CommandLine.Invocation;

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

            Option idOption = new Option<PuzzleSource>(
                new string[] { "--id" },
                getDefaultValue: () => 0,
                description: "The id of the puzzle to be solved"
            );

            Option outputOption = new Option<string>(
                new string[] { "--output", "-o" },
                getDefaultValue: () => @"./",
                description: "The directory where output files are stored"
            );

            Option inputOption = new Option<string>(
                new string[] { "--input", "-i" },
                getDefaultValue: () => @"./",
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

            solveCommand.Handler = CommandHandler.Create<SolvingMethod, PuzzleSource, int, string, string, bool>(Solve);

            RootCommand rootCommand = new RootCommand();

            rootCommand.AddGlobalOption(outputOption);
            rootCommand.AddGlobalOption(inputOption);
            rootCommand.AddGlobalOption(verboseOption);

            rootCommand.AddCommand(solveCommand);
            
            return  rootCommand.InvokeAsync(args).Result;
        }

        internal static void Solve(SolvingMethod method, PuzzleSource source, int id, string output, string input, bool verbose)
        {
            // VALIDATE OUTPUT/INPUT DIRECTORY
            // TODO: Change to DirectoryInfo...?

            Scrapper scrapper = new Scrapper();
            StringWriter xmlWritter = new StringWriter();

            Directory.CreateDirectory($"{input}/resources");

            using (StreamWriter writer = new StreamWriter($"resources/{id}.xml"))
            {
                scrapper.GetFromSource(PuzzleSource.WebPBN, id, writer);
            }

            PuzzleSet puzzle = null;

            XmlSerializer serializer = new XmlSerializer(typeof(PuzzleSet));

            string response = xmlWritter.ToString();

            using (StreamReader reader = new StreamReader($"resources/{id}.xml"))
            {
                puzzle = (PuzzleSet)serializer.Deserialize(reader);
            }

            Console.WriteLine(puzzle.Puzzle);

            GameState gameState = new GameState(puzzle.Puzzle);

            Solver solver = new Solver(gameState);

            solver.Solve();
        }
    }
}
