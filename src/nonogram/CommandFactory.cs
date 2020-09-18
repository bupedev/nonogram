using System;
using System.Collections.Generic;

namespace nonogram
{
    public class CommandFactory
    {
        private readonly IEnumerable<Command> commands;

        internal CommandFactory(IEnumerable<Command> commands)
        {
            this.commands = commands;
        }

        public Command SelectCommand(string commandName)
        {
            throw new NotImplementedException();
        }
    }

    public class UnknownCommandException : Exception
    {
        public UnknownCommandException(string command) 
            : base($"Unknown command \'{command}\'.")
        { }
    }
}   