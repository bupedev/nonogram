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
        static async Task Main(string[] args)
        {

            PuzzleSet puzzle = null;
            string path = "../../resources/test.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(PuzzleSet));

            StreamReader reader = new StreamReader(path);
            puzzle = (PuzzleSet)serializer.Deserialize(reader);
            reader.Close();

            Console.WriteLine(puzzle.Puzzle);

            // HttpWebRequest request =
            // (HttpWebRequest)WebRequest.Create("https://webpbn.com/export.cgi");

            // request.Credentials = CredentialCache.DefaultCredentials;

            // request.Method = "POST";

            // // Create POST data and convert it to a byte array.
            // string postData = "fmt=xml&go=1&id=2040";
            // byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // // Set the ContentType property of the WebRequest.
            // request.ContentType = "application/x-www-form-urlencoded";
            // // Set the ContentLength property of the WebRequest.
            // request.ContentLength = byteArray.Length;

            // // Get the request stream.
            // Stream dataStream = request.GetRequestStream();
            // // Write the data to the request stream.
            // dataStream.Write(byteArray, 0, byteArray.Length);
            // // Close the Stream object.
            // dataStream.Close();

            // // Get the response.
            // WebResponse response = request.GetResponse();
            // // Display the status.
            // Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            // // Get the stream containing content returned by the server.
            // // The using block ensures the stream is automatically closed.
            // using (dataStream = response.GetResponseStream())
            // {
            //     // Open the stream using a StreamReader for easy access.
            //     StreamReader reader = new StreamReader(dataStream);
            //     // Read the content.
            //     string responseFromServer = reader.ReadToEnd();
            //     // Display the content.
            //     Console.WriteLine(responseFromServer);
            // }

            // // Close the response.
            // response.Close();

            // GameState board = ReadFromFile(@"C:\Users\b7lew\OneDrive - Queensland University of Technology (1)\Documents\Coursework\2020\S2\CAB401\Project\Nonogram64\Nonogram\Puzzles\fishEater.nono");

            // Solver solver = new Solver(board);

            // solver.Solve();
        }

        private static GameState ReadFromFile(string filename)
        {
            List<Hint> rowHints = new List<Hint>();
            List<Hint> columnHints = new List<Hint>();

            using (StreamReader reader = new StreamReader(filename))
            {
                
                string line;
                bool rowSection = false;
                bool columnSection = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!(rowSection || columnSection || line.Equals(": rows") || line.Equals(": columns")))
                    {
                        continue;
                    }

                    if (line.Equals(": rows"))
                    {
                        rowSection = true;
                        columnSection = false;
                        continue;
                    }

                    if (line.Equals(": columns"))
                    {
                        columnSection = true;
                        rowSection = false;
                        continue;
                    }

                    string[] elements = line.Split(' ');

                    int[] hintNumbers = new int[elements.Length];

                    for (int i = 0; i < elements.Length; ++i)
                    {
                        string substr = elements[i].Substring(0, elements[i].Length - 1);
                        hintNumbers[i] = int.Parse(substr);
                    }

                    Hint hint = new Hint(hintNumbers);

                    if (rowSection)
                        rowHints.Add(hint);

                    if (columnSection)
                        columnHints.Add(hint);
                }
            }

            int N = rowHints.Count, M = columnHints.Count;
            CellState[][] cellStates = new CellState[N][];
            for (int i = 0; i < N; ++i)
            {
                cellStates[i] = new CellState[M];
                for (int j = 0; j < M; ++j)
                {
                    cellStates[i][j] = CellState.Blank;
                }
            }

            return new GameState(cellStates, new HintSet(rowHints.ToArray()), new HintSet(columnHints.ToArray()));
        }
    }
}
