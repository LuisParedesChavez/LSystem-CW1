using System;
using System.Collections.Generic;

public class LSystemGenerator
{
    private readonly Dictionary<char, string> rules;

    public LSystemGenerator(Dictionary<char, string> rules)
    {
        this.rules = rules;
    }

    public void Generate(string axiom, int iterations, Action<char> processSymbol)
    {
        foreach (char symbol in axiom)
        {
            Expand(symbol, iterations, processSymbol);
        }
    }

    private void Expand(char symbol, int depth, Action<char> processSymbol)
    {
        if (depth > 0 && rules.TryGetValue(symbol, out string replacement))
        {
            foreach (char nextSymbol in replacement)
            {
                Expand(nextSymbol, depth - 1, processSymbol);
            }
        }
        else
        {
            processSymbol(symbol);
        }
    }
}