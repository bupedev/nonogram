using System;
using System.Runtime.Serialization;

namespace Nonogram
{
    [Serializable]
    internal class NonogramException : Exception
    {
        public NonogramException()
        {
        }

        public NonogramException(string message) : base(message)
        {
        }

        public NonogramException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NonogramException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class IncompatiblePuzzleException : NonogramException
    {
        public IncompatiblePuzzleException()
        {
        }

        public IncompatiblePuzzleException(string message) : base(message)
        {
        }

        public IncompatiblePuzzleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IncompatiblePuzzleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class MissingDataException : NonogramException
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