using System;
using System.Linq;
using System.Xml.Serialization;

namespace Nonogram
{

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("puzzleset")]
    public class PuzzleSet
    {
        [XmlElement("source")]
        public string Source { get; set; }

        [XmlElement("puzzle",typeof(Puzzle))]
        public Puzzle Puzzle { get; set; }
    }

    public class Puzzle
    {
        [XmlAttribute("type")]
        public string Type {get; set;}

        [XmlAttribute("defaultcolor")]
        public string DefaultColour {get; set;}

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("author")]
        public string Author { get; set; }
        
        [XmlElement("authorid")]
        public string AuthorID { get; set; }

        [XmlElement("copyright")]
        public string Copyright { get; set; }

        [XmlElement("id")]
        public string ID { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("note")]
        public string Notes { get; set; }

        [XmlElement("color")]
        public PuzzleColour[] PuzzleColours {get; set;}

        [XmlElement("clues")]
        public HintSet[] HintSets {get; set;}

        public override string ToString()
        {
            return $"Puzzle(Type = {Type}, Default Colour = {DefaultColour})\n " + 
                    "{\n" + 
                        $"\tTitle: {Title}\n" + 
                        $"\tAuthor: {Author}\n" + 
                        $"\tAuthorID: {AuthorID}\n" + 
                        $"\tCopyright: {Copyright}\n" + 
                        $"\tID: {ID}\n" + 
                        $"\tDescription: {Description}\n" + 
                        $"\tNotes: {Notes}\n" +
                        $"\tColours: \n\t{{\n\t\t{string.Join("\n\t\t", (from colour in PuzzleColours select colour.ToString()).ToArray())}\n\t}}\n" +
                        $"\tClues (Columns): \n\t{{\n\t\t{string.Join("\n\t\t", (from line in HintSets[0].Hints select line.ToString()).ToArray())}\n\t}}\n" +
                        $"\tClues (Rows): \n\t{{\n\t\t{string.Join("\n\t\t", (from line in HintSets[1].Hints select line.ToString()).ToArray())}\n\t}}\n" +
                    "}";
        }
    }

    public class PuzzleColour
    {
        [XmlAttribute("name")]
        public string Name {get; set;}

        [XmlAttribute("char")]
        public string Character {get; set;}

        [XmlText]
        public string HexCode {get; set;}

        public override string ToString()
        {
            return $"{Name} {{Character: '{Character}', Hex: {HexCode}}}";
        }
    }

    [Serializable]
    public class HintSet
    {
        [XmlElement("line")]
        public Hint[] Hints {get; set;}

        public int Length => Hints.Length; 

        public Hint this[int i]
        {
            get
            {
                return Hints[i];
            }
        }

        public HintSet()
        {
            this.Hints = new Hint[0];
        }

        public HintSet(Hint[] hintArray)
        {
            this.Hints = hintArray;
        }
    }

    [Serializable]
    public class Hint
    {
        [XmlElement("count")]
        public int[] Counts {get; set;}

        public int Length 
        {
            get 
            {
                return Counts.Length;
            }
        }

        public int this[int i]
        {
            get
            {
                return Counts[i];
            }
        }

        public Hint()
        {
            Counts = new int[0];
        }

        public Hint(int[] counts)
        {
            Counts = counts;
        }

        public int Occupation(int p = 0)
        {
            int sum = 0;
            for (int i = p; i < Counts.Length; i++)
            {
                sum += Counts[i];
            }
            return sum + (Counts.Length - 1 - p);
        }

        public override string ToString()
        {
            return string.Join(" ", Counts);
        }
    }
}