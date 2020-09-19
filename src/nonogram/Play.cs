using System;

namespace nonogram
{
    internal class PlayCommand : Command
    {
        internal override OptionFactory OptionFactory => new OptionFactory(
            new Option[] {

            }
        );

        internal override string Keyword => "play";

        internal override void Execute(string[] args)
        {
            Console.WriteLine("playing would go here...");
        }

        internal override void HelpMessage()
        {
            Console.WriteLine("help for play goes here...");
        }
    }
}
