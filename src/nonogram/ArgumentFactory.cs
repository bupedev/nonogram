using System;
using System.Collections.Generic;

namespace nonogram
{
    public class ArgumentFactory<T> where T : Argument
    {
        private readonly IEnumerable<T> arguments;

        internal ArgumentFactory(IEnumerable<T> arguments)
        {
            this.arguments = arguments;
        }

        internal T SelectArgument(string argumentName)
        {
            T argument = FindArgument(argumentName);

            if (argument == null)
            {
                throw new UnknownArgumentException<T>(argumentName);
            }

            return argument;
        }

        private T FindArgument(string argumentName)
        {
            foreach (T argument in arguments)
            {
                if (argument.Keyword.Equals(argumentName))
                {
                    return argument;
                }
            }
            return null;
        }
    }

    public class UnknownArgumentException<T> : Exception where T : Argument
    {
        public UnknownArgumentException(string command) 
            : base($"Unknown {typeof(T).Name.ToLower()} \'{command}\'.")
        { }
    }
}   