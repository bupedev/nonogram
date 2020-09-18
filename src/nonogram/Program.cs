using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace Nonogram
{
    class Program
    {
        static void Main(string[] args)
        {
            Scrapper scrapper = new Scrapper();
            StringWriter xmlWritter = new StringWriter();

            int id = 2;

            Directory.CreateDirectory("resources");

            using (StreamWriter writer = new StreamWriter($"resources/{id}.xml"))
            {
                scrapper.GetFromSource(Source.WebPBN, id, writer);
            }

            PuzzleSet puzzle = null;

            XmlSerializer serializer = new XmlSerializer(typeof(PuzzleSet));

            string response = xmlWritter.ToString();

            using (StreamReader reader = new StreamReader($"resources/{id}.xml"))
            {
                puzzle = (PuzzleSet)serializer.Deserialize(reader);
            }

            Console.WriteLine(puzzle.Puzzle);

            GameState gameState = new GameState(puzzle.Puzzle);

            Solver solver = new Solver(gameState);

            solver.Solve();
        }
    }
}
