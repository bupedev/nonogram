using System;
using System.Collections.Generic;
using System.Text;

namespace nonogram
{
    public abstract class ArgumentProcessor
    {
        private ArgumentFactory<Command> commandFactory;

        public ArgumentProcessor() : base()
        {
            commandFactory = new ArgumentFactory<Command>(
                new Command[]
                {
                    new HelpCommand(),
                    new SolveCommand(),
                    new BenchmarkCommand(),
                    new PlayCommand(),
                }
            );
        }

        public void Process(string[] args)
        {
            if (args.Length < 1)
            {
                // TODO: turn this into an exit prompt.
                Console.WriteLine("Warning: No command provided.");
                Environment.Exit(0);
            }

            string commandName = args[0];
            Command command = commandFactory.SelectArgument(commandName);
            string[] subargs = ExtractArguments(args, 1, args.Length - 1);
            command.Execute(subargs);
        }

        public static string[] ExtractArguments(string[] args, int startIndex, int argCount)
        {
            string[] extract = new string[argCount];
            for (int i = startIndex; i < argCount && i < args.Length; i++)
            {
                extract[i - startIndex] = args[i];
            }
            return extract;
        }
    }
}
