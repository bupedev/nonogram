using System;
using System.Collections.Generic;
using System.Text;

namespace nonogram
{
    internal abstract class Command : Argument
    {
        protected RuntimeOptions options;

        internal abstract ArgumentFactory<Option> OptionFactory { get; }
        internal abstract void HelpMessage();
        internal virtual void Execute(string[] args = null)
        {
            if (args == null) return;

            int index = 0;
            while (index < args.Length)
            {
                Option option = OptionFactory.SelectArgument(args[index++]);
                string[] subargs = ArgumentProcessor.ExtractArguments(args, index, option.ArgCount);
                option.Process(args, options);
                index += subargs.Length;
            }
        }
    }

    internal class HelpCommand : Command
    {
        internal override ArgumentFactory<Option> OptionFactory => new ArgumentFactory<Option>(
            new Option[] { 
                
            }
        );

        internal override string Keyword => "help";

        internal override void Execute(string[] args = null)
        {
            base.Execute(args);
            HelpOptions helpOptions = (HelpOptions)options;

            if (args == null || args.Length == 0)
            {
                HelpMessage();
            }
            else
            {
                try
                {
                    ArgumentFactory<Command> commandFactory = new ArgumentFactory<Command>(
                        new Command[]
                        {
                            new SolveCommand(),
                            new BenchmarkCommand(),
                            new PlayCommand(),
                        }
                    );
                    string commandName = args[0];
                    Command command = commandFactory.SelectArgument(commandName);
                    command.HelpMessage();
                }
                catch (UnknownArgumentException<Command> exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        internal override void HelpMessage()
        {
            Console.WriteLine("Full help goes here...");
        }
    }

    internal class SolveCommand : Command
    {
        internal override ArgumentFactory<Option> OptionFactory => new ArgumentFactory<Option>(
            new Option[] {

            }
        );

        internal override string Keyword => "solve";

        internal override void Execute(string[] args)
        {
            base.Execute(args);
            SolveOptions solveOptions = (SolveOptions)options;

            Console.WriteLine("solve would go here...");
        }

        internal override void HelpMessage()
        {
            Console.WriteLine("help for solve goes here...");
        }
    }

    internal class BenchmarkCommand : Command
    {
        internal override ArgumentFactory<Option> OptionFactory => new ArgumentFactory<Option>(
            new Option[] {

            }
        );

        internal override string Keyword => "benchmark";

        internal override void Execute(string[] args)
        {
            base.Execute(args);
            BenchmarkOptions benchmarkOptions = (BenchmarkOptions)options;

            Console.WriteLine("benchmark would go here...");
        }

        internal override void HelpMessage()
        {
            Console.WriteLine("help for benchmark goes here...");
        }
    }

    internal class PlayCommand : Command
    {
        internal override ArgumentFactory<Option> OptionFactory => new ArgumentFactory<Option>(
            new Option[] {

            }
        );

        internal override string Keyword => "play";

        internal override void Execute(string[] args)
        {
            base.Execute(args);
            PlayOptions playOptions = (PlayOptions)options;

            Console.WriteLine("play would go here...");
        }

        internal override void HelpMessage()
        {
            Console.WriteLine("play for benchmark goes here...");
        }
    }
}
