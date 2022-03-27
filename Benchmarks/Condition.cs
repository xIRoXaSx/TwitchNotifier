using BenchmarkDotNet.Attributes;

namespace Benchmarks; 

[MemoryDiagnoser]
public class Conditions {
    private const string Contains = ".Contains";

    [Benchmark]
    public void ConditionNestedParenthesesList() => NestedParenthesesList(Program.Value);

    [Benchmark]
    public void ConditionNestedParenthesesEnum() => NestedParenthesesEnum(Program.Value);

    [Benchmark]
    public void ConditionForeachParentheses() => ForeachParentheses(Program.Value);
    
    [Benchmark]
    public void ConditionSplitParentheses() => SplitParentheses(Program.Value);

    [Benchmark]
    public void ConditionStringPartSubstring() => StringPartSubstring(Program.Value);

    [Benchmark]
    public void ConditionStringPartRange() => StringPartRange(Program.Value);
    
    private static void NestedParenthesesEnum(string value) {
        var a = ParenthesesEnum(value).ToList();
    }

    private static void NestedParenthesesList(string value) {
        var a = ParenthesesList(value);
    }
    
    /// <summary>
    /// Get nested parentheses from string
    /// </summary>
    /// <param name="value">The string which contains the condition / parentheses</param>
    /// <returns></returns>
    private static IEnumerable<string> ParenthesesEnum(string value) {
        var brackets = new Stack<int>();
        var containsOpened = false;
        
        if (string.IsNullOrEmpty(value))
            yield break;

        for (var i = 0; i < value.Length; ++i) {
            var currentChar = value[i];

            if (i > 8 && value.Substring(i-Contains.Length, Contains.Length) == Contains)
                containsOpened = true;

            switch (currentChar) {
                // Since contains is handled separately, don't count parentheses in.
                case '(' when !containsOpened:
                    brackets.Push(i);
                    break;
                case ')' when containsOpened:
                    containsOpened = false;
                    break;
                case ')': {
                    var openBracket = brackets.Pop();
                    yield return value.Substring(openBracket + 1, i - openBracket - 1);
                    break;
                }
            }
        }
        yield return value;
    }
    
    private static List<string> ParenthesesList(string value) {
        var returnValue = new List<string>();
        var brackets = new Stack<int>();
        var containsOpened = false;
        
        if (string.IsNullOrEmpty(value))
            return returnValue;

        for (var i = 0; i < value.Length; ++i) {
            var currentChar = value[i];

            if (i > Contains.Length-1 && value.Substring(i-Contains.Length, Contains.Length) == Contains)
                containsOpened = true;

            switch (currentChar) {
                // Since contains is handled separately, don't count parentheses in.
                case '(' when !containsOpened:
                    brackets.Push(i);
                    break;
                case ')' when containsOpened:
                    containsOpened = false;
                    break;
                case ')': {
                    var openBracket = brackets.Pop();
                    returnValue.Add(value.Substring(openBracket + 1, i - openBracket - 1));
                    break;
                }
            }
        }
        return returnValue;
    }

    private static void ForeachParentheses(string value) {
        var opening = 0;
        var closing = 0;
        foreach (var c in value) {
            switch (c) {
                case '(':
                    opening++;
                    break;
                case ')':
                    closing++;
                    break;
            }
        }
    }
    
    private static void SplitParentheses(string value) {
        var opening = value.Split("(").Length - 1;
        var closing = value.Split(")").Length - 1;
    }
    
    private static void StringPartSubstring(string value) {
        var val = value.Substring(0, value.Length - 1);
    }
    
    private static void StringPartRange(string value) {
        var val = value[..^1];
    }
}