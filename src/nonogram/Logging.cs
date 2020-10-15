using System;
using System.Collections.Generic;
using System.Text;

namespace Nonogram
{
    static class Logging
    {
        private static string FormattedTime
        {
            get { return DateTime.Now.ToString("[hh:mm:ss:fff]"); }
        }

        public static void Success(string message, bool verbose = true)
        {
            Message(message, verbose, "Success", ConsoleColor.Green);
        }

        public static void Warning(string message, bool verbose = true)
        {
            Message(message, verbose, "Warning", ConsoleColor.Yellow);
        }

        public static void Error(string message, bool verbose = true)
        {
            Message(message, verbose, "Error", ConsoleColor.Red);
        }

        public static void Message(string message, bool verbose = true, string prefix = null,
            ConsoleColor color = ConsoleColor.White)
        {
            if (verbose)
            {
                ConsoleColor formerColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine($"{FormattedTime}{(prefix != null ? $" {prefix}: " : " ")}{message}");
                Console.ForegroundColor = formerColor;
            }
        }
    }
}
