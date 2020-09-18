using System;
using System.Collections.Generic;
using System.Text;

namespace nonogram
{
    abstract class Command
    {
        public abstract string Keyword { get; }
        public abstract void Execute(string[] args);
        public abstract void HelpMessage();
    }

    class Help : Command
    {
        public override string Keyword => "help";

        public override void Execute(string[] args = null)
        {
            if (args == null || args.Length == 0)
            {
                HelpMessage();
            }
            else
            {
                try
                {
                    CommandFactory commandFactory = new CommandFactory(
                        new Command[]
                        {
                            new Solve(),
                            new Benchmark(),
                            new Play(),
                        }
                    );
                    string commandName = args[0];
                    Command command = commandFactory.SelectCommand(commandName);
                    command.HelpMessage();
                }
                catch (UnknownCommandException exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        public override void HelpMessage()
        {
            
        }
    }

    class Solve : Command
    {
        public override string Keyword => "solve";

        public override void Execute(string[] args)
        {
            Console.WriteLine("solving would go here...");
        }

        public override void HelpMessage()
        {
            Console.WriteLine("help for solve goes here");
        }
    }

    class Benchmark : Command
    {
        public override string Keyword => "benchmark";

        public override void Execute(string[] args)
        {
            Console.WriteLine("benchmark would go here...");
        }

        public override void HelpMessage()
        {
            Console.WriteLine("help for benchmark goes here");
        }
    }

    class Play : Command
    {
        public override string Keyword => "play";

        public override void Execute(string[] args)
        {
            Console.WriteLine("playing would go here...");
        }

        public override void HelpMessage()
        {
            Console.WriteLine("help for play goes here");
        }
    }
}
