using System;

namespace nonogram
{
    internal abstract class Option : Argument
    {
        internal abstract int ArgCount { get; }

        internal abstract void Process(string[] args, RuntimeOptions options);
    }

    internal class SolveOption : Option
    {
        internal override string Keyword => "solve";

        internal override int ArgCount => 0;

        internal override void Process(string[] args, RuntimeOptions options)
        {
            HelpOptions helpOptions = (HelpOptions)options;

            helpOptions
        }
    }
}