using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphametikSolver
{
    public enum Operator
    {
        Addition = 1,
        Subtraction = 2,

        Multiplication = 3,
        Division = 4
    }
    public class Alphametik
    {
        public string Result
        { get; set; }

        public string[] Words
        { get; set; }

        public int MaximumWordLength
        { get; set; }

        public Operator Operator
        { get; set; }

        public HashSet<char> SymbolTable
        { get; private set; }

        public Alphametik(string result, Operator _operator, params string[] words)
        {
            this.Words = words;
            this.Result = result.ToUpper().Trim();
            this.Operator = _operator;
            this.SymbolTable = new HashSet<char>();

            foreach (string word in words)
                foreach (char c in word) 
                    this.SymbolTable.Add(c);
            foreach (char c in this.Result) this.SymbolTable.Add(c);

            this.MaximumWordLength = this.Words[0].Length;
            for (int i = 1; i < this.Words.Length; i++)
                this.MaximumWordLength = Math.Max(this.MaximumWordLength, this.Words[i].Length);
        }

        
        public static int Convert(string str, SortedList<char, int> assignment)
        {
            foreach (KeyValuePair<char, int> assign in assignment)
                str = str.Replace(assign.Key.ToString(), assign.Value.ToString());
            return int.Parse(str);
        }
    
        public SortedList<char, int> Solve()
        {
            if (this.SymbolTable.Count > 10)
                throw new Exception("Too many letters.");

            SortedList<char, int> assignment = new SortedList<char, int>();
            switch (this.Operator)
            {
                case Operator.Addition:
                    if (this.Result.Length == this.MaximumWordLength + 1)
                        assignment[this.Result[0]] = 1; // this HAS to be 1, for mathematical background see documentation
                    else if (this.Result.Length != this.MaximumWordLength)
                        throw new Exception("The result has to few or to many letters.");
                    break;

                case Operator.Multiplication:
                    int wordSum = this.Words.Sum((s) => s.Length);
                    if (this.Result.Length != wordSum) // as every word has to start with non-zero the result must be of that length
                        throw new Exception("The result has to few or to many letters.");
                    break;

                case Operator.Subtraction: // change alphametik to get same results solving a additive/multiplicative alphametik
                case Operator.Division:
                    {
                        string[] newWords = new string[this.Words.Length];
                        Array.Copy(this.Words, newWords, this.Words.Length);
                        newWords[0] = this.Result;

                        Alphametik alphametik = new Alphametik(this.Words[0], (Operator)((int)this.Operator - 1), newWords);
                        return alphametik.Solve();
                    }
            }

            // Start recursive search for solutions
            if (SearchRecursive(0, 0, assignment, 0))
                return assignment;
            else
                return null;
        }

        private bool SearchRecursive(int alphaIndex, int wordIndex, SortedList<char, int> assignment, int div)
        {
            // Check if we already gone through all word letters
            // if so, set assignments according to result (this is much easier than the recursive search)
            // and check if this assignment fits for the result
            if (alphaIndex >= this.MaximumWordLength)
            {
                if (this.Result.Length > this.MaximumWordLength)
                    return CheckResult(assignment, div);
                else
                    return assignment.Count == this.SymbolTable.Count && div == 0;  // if result is not longer than the longest word, there shall be no div
            }


            if (wordIndex < this.Words.Length) // Do the recursion over words
            {
                string word = this.Words[wordIndex];
                if (alphaIndex >= word.Length) // if this word has not enough letters, just skip to the next word
                    return SearchRecursive(alphaIndex, wordIndex + 1, assignment, div);

                char symb = word[word.Length - 1 - alphaIndex];

                int assign;
                if (!assignment.TryGetValue(symb, out assign))
                {
                    // The current symbol is not assigned, so try all possible assignments through recursion

                    int start = (alphaIndex == word.Length - 1) ? 1 : 0; // exclude words starting with 0
                    for (int i = start; i <= 9; i++)
                    {
                        if (!assignment.ContainsValue(i))
                        {
                            assignment[symb] = i;
                            if (SearchRecursive(alphaIndex, wordIndex + 1, assignment, div))
                                return true;
                        }
                    }
                    assignment.Remove(symb); // there was no assignment in this iteration => remove the assignment - it was unassigned before and we want to restore the previous state
                    return false;
                }
                else if (assign == 0 && alphaIndex == word.Length - 1) // the symbol is assigned to 0 but it is the beginning of a word
                    return false;
                else // symbol is already assigned, so just continue to the next word (no work to do here)
                    return SearchRecursive(alphaIndex, wordIndex + 1, assignment, div);
            }
            else // Do recursion over letters
            {
                // Check if the assignment of the current letter index is correct
                // by calculating the result value and checking if the calculated value
                // is equal to the actual value

                int chk = GetResultValue(assignment, alphaIndex, ref div);
                char res = this.Result[this.Result.Length - 1 - alphaIndex];

                int assign;
                if (!assignment.TryGetValue(res, out assign)) // the result symbol is not yet assigned
                {
                    if (assignment.ContainsValue(chk)) // the calculated value is already assigned to another symbol, so this assignment is incorrect
                        return false;
                    else
                    {
                        // Calculated value is not yet assigned - assign it to the symbol in the result
                        // and continue recursion to the next letter index

                        assignment[res] = chk;
                        if (!SearchRecursive(alphaIndex + 1, 0, assignment, div))
                        {
                            // no assignment found in this recursion -> remove the assignment from assignments

                            assignment.Remove(res);
                            return false;
                        }
                        else 
                            return true;
                    }
                }
                else if (assign == chk) // the result symbol is already correctly assigned -> continue recursion to the next letter
                    return SearchRecursive(alphaIndex + 1, 0, assignment, div);
                else // the result symbol is assigned to an incorrect value -> this assignment is incorrect
                    return false;
            }
        }
    
        private int GetResultValue(SortedList<char, int> assignment, int alphaIndex, ref int div)
        {
            // Calculate expected value in result -> see documention for mathematic backgrounds

            if (this.Operator == Operator.Addition)
            {
                int sum = this.Words.Where((s) => s.Length - 1 - alphaIndex >= 0).Select((s) => s[s.Length - 1 - alphaIndex]).Sum((c) => assignment[c]);
                int chk = (sum + div) % 10;
                div = (sum + div) / 10;
                return chk;
            }
            else // if (this.Operator == Operator.Multiplication)
            {
                int sum = GetMultiplicativeDigitSum(assignment, alphaIndex, 0);
                int chk = (sum + div) % 10;
                div = (sum + div) / 10;
                return chk;
            }
        }
    
        private bool CheckResult(SortedList<char, int> assignment, int div)
        {
            if (this.Operator == Operator.Addition)
            {
                // Result can only be one letter longer than the maximum
                // so we only need to check the first letter

                int assign;
                if (assignment.TryGetValue(this.Result[0], out assign)) // symbol is already assigned, so check if it fits to div
                    return assign == div && assignment.Count == this.SymbolTable.Count;
                else
                {
                    // symbol is not yet assigned -> check if calculated value can be assigned

                    if (assignment.ContainsValue(div))
                        return false;
                    assignment[this.Result[0]] = div;

                    // all symbols must be assigned now
                    return assignment.Count == this.SymbolTable.Count;
                }
            }
            else // if (this.Operator == Operator.Multiplication)
            {
                // The same as for Addition - just more complex
                // Go through all remaining letters in result and check and assign (if necessary) the
                // expected symbol assignment

                for (int i = this.MaximumWordLength; i < this.Result.Length; i++)
                {
                    int sum = GetMultiplicativeDigitSum(assignment, i, 0);
                    int chk = (sum + div) % 10;
                    div = (sum + div) / 10;

                    int assign;
                    if (assignment.TryGetValue(this.Result[this.Result.Length - 1 - i], out assign))
                    {
                        if (assign != chk) 
                            return false;
                    }
                    else
                    {
                        if (assignment.ContainsValue(chk))
                            return false;
                        else if (i == this.Result.Length - 1 && chk == 0)
                            return false;
                        assignment[this.Result[this.Result.Length - 1 - i]] = chk;
                    }
                }

                // div must be 0 now, otherwise result is no long enough
                // all symbols must be assigned now
                return assignment.Count == this.SymbolTable.Count && div == 0;
            }
        }

        private int GetMultiplicativeDigitSum(SortedList<char, int> assignment, int alphaIndex, int wordStart)
        {
            // Calculate a sum from which a specific digit and the remainder for the next digit
            // can be calculated
            // do this through recursion, as we have multiple words and multiplication is more complex
            // than addition

            if (wordStart == this.Words.Length - 1)
            {
                if (this.Words[wordStart].Length > alphaIndex)
                    return assignment[this.Words[wordStart][this.Words[wordStart].Length - 1 - alphaIndex]];
                else return 0;
            }
            
            int sum = 0;
            for (int i = 0; i <= alphaIndex; i++)
            {
                if (this.Words[wordStart].Length > i)
                {
                    sum += assignment[this.Words[wordStart][this.Words[wordStart].Length - 1 - i]] *
                        GetMultiplicativeDigitSum(assignment, alphaIndex - i, wordStart + 1);
                }
            }

            return sum;
        }
    }
}
