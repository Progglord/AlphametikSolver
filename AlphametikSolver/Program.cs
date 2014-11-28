using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AlphametikSolver
{
    class Program
    {
        
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("--------------");
                Alphametik alphametik = ReadAlphametik();
                if (alphametik == null) continue;

                try
                {
                    SortedList<char, int> assignment = alphametik.Solve();
                    if (assignment == null)
                        Console.WriteLine("There is no solution for this alphametik.");
                    else
                        PrintAssignment(alphametik, assignment);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static Alphametik ReadAlphametik()
        {
            Console.WriteLine("Enter an alphametik: ");
            Match match = Regex.Match(Console.ReadLine(), @"([A-Z]+)\s*([\+|\-|\*|/])\s*([A-Z]+\s*)(([\+|\-|\*|/]\s*[A-Z]+\s*)*)=\s*([A-Z]+)", RegexOptions.None);

            if (match.Success)
            {
                string op = match.Groups[2].Value;
                List<string> words = new List<string>();
                words.Add(match.Groups[1].Value);
                words.Add(match.Groups[3].Value);

                if (!ReadAlphametikRecursive(words, match.Groups[4].Value, op))
                {
                    Console.WriteLine("Input was no valid alphametik.");
                    return null;
                }

                switch (op)
                {
                    case "+": return new Alphametik(match.Groups[6].Value, Operator.Addition, words.ToArray());
                    case "-": return new Alphametik(match.Groups[6].Value, Operator.Subtraction, words.ToArray());
                    case "*": return new Alphametik(match.Groups[6].Value, Operator.Multiplication, words.ToArray());
                    case "/": return new Alphametik(match.Groups[6].Value, Operator.Division, words.ToArray());
                    default: Console.WriteLine("Input was no valid alphametik."); return null;
                }
                
            }
            else
                Console.WriteLine("Input was no valid alphametik.");

            return null;
        }

        private static bool ReadAlphametikRecursive(List<string> words, string lastMatch, string _operator)
        {
            if (string.IsNullOrEmpty(lastMatch))
                return true;

            Match match = Regex.Match(lastMatch, @"([\+|\-|\*|/])\s*([A-Z]+\s*)(([\+|\-|\*|/]\s*[A-Z]+\s*)*)", RegexOptions.None);
            if (match.Success)
            {
                if (match.Groups[1].Value != _operator)
                    return false;
                words.Add(match.Groups[2].Value);

                return ReadAlphametikRecursive(words, match.Groups[4].Value, _operator);
            }
            else
                return false;
        }

        private static void PrintAssignment(Alphametik alphametik, SortedList<char, int> assignment)
        {
            Console.WriteLine("The following assinment solves the alphametik: ");
            assignment.All(kvp => { Console.WriteLine("{0} -> {1}", kvp.Key, kvp.Value); return true; });

            string op = "";
            switch (alphametik.Operator)
            {
                case Operator.Addition: op = "+"; break;
                case Operator.Subtraction: op = "-"; break;
                case Operator.Multiplication: op = "*"; break;
                case Operator.Division: op = "/"; break;
            }

            Console.Write(String.Join(" " + op + " ", alphametik.Words.Select((w) => Alphametik.Convert(w, assignment))));
            Console.WriteLine(" = {0}", Alphametik.Convert(alphametik.Result, assignment));
        }
    
    }
}
