using System;
using System.Collections.Generic;
using System.Text;

namespace nonogram
{
    public abstract class ArgumentProcessor
    {
        public abstract void Process(string[] args);

        protected string[] ExtractArguments(string[] args, int startIndex, int argCount)
        {
            string[] extract = new string[argCount];
            for (int i = startIndex; i < argCount && i < args.Length; i++)
            {
                extract[i - startIndex] = args[i];
            }
            return extract;
        }
    }

    public class CommandProcessor : ArgumentProcessor
    {
        private CommandFactory commandFactory;

        public CommandProcessor() : base()
        {
            commandFactory = new CommandFactory(
                new Command[]
                {
                    new Help(),
                    new Solve(),
                    new Benchmark(),
                    new Play(),
                }
            );
        }

        public override void Process(string[] args)
        {
            if (args.Length < 1)
            {
                // TODO: turn this into an exit prompt.
                Console.WriteLine("Warning: No command provided.");
                Environment.Exit(0);
            }

            string commandName = args[0];
            Command command = commandFactory.SelectCommand(commandName);
            string[] subargs = ExtractArguments(args, 0, args.Length - 1);
            command.Execute(subargs);
        }
    }

    public class OptionProcessor : ArgumentProcessor
    {
        private OptionFactory optionFactory;

        public OptionProcessor() : base()
        {
            optionFactory = new OptionFactory();
        }

        public override void Process(string[] args)
        {
            //Option option = optionFactory.SelectCommand();
        }
    }
}
