using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TwitchNotifier; 

internal class Condition {
    private string _condition; 
    private const string LogicAnd = "&&";
    private const string LogicOr = "||";
    private static readonly string[] LogicalOperator = {
        "==",        // Equals
        "!=",        // NotEquals
        ">=",        // GreaterEquals
        "<=",        // LessEquals
        ">",         // GreaterThan
        "<",         // LessThan
        ".contains", // Contains
        "true",      // True
        "false"      // False
    };

    internal Condition(string value) {
        _condition = value;
    }

    /// <summary>
    /// Evaluates a condition parsed via the constructor's string.  
    /// </summary>
    /// <returns>
    /// <c>True</c> - If condition was satisfied, is null or empty. 
    /// <c>False</c> - If condition was unsatisfied, or parentheses are unbalanced. 
    /// </returns>
    internal bool Evaluate() {
        if (string.IsNullOrEmpty(_condition))
            return true;
        var opening = 0;
        var closing = 0;
        foreach (var c in _condition) {
            switch (c) {
                case '(':
                    opening++;
                    break;
                case ')':
                    closing++;
                    break;
            }
        }
        
        if (opening != closing) {
            Logging.Error("Unbalanced condition parentheses.");
            return false;
        }

        // Split value by logical operators and replace all simple conditions with their corresponding values.
        var replacedConditions = Regex.Split(_condition, @"&&|\|\|");
        foreach (var replaced in replacedConditions) {
            var matched = Regex.Match(replaced, @"[\s\(]*(.*?)(?:\)|$)");
            if (!matched.Success)
                continue;
            var matchedVal = matched.Groups[1].Value;
            
            // Append a closing parentheses if condition included the "contains()" clause.
            if (Regex.Match(replaced, @"\.contains\(.*\)", RegexOptions.IgnoreCase).Success)
                matchedVal += ")";
            var partEval = EvaluateSingleCondition(matchedVal);
            _condition = _condition.Replace(matchedVal, partEval.ToString());
        }

        // Abbreviate boolean strings.
        var conditions = EvaluateParenthesesOrder(_condition);
        var remainder = conditions[^1];
        while (remainder.ToLower() != "true" && remainder.ToLower() != "false") {
            foreach (var cond in conditions) {
                // Replace encapsulated booleans.
                var replacedCond = Regex.Replace(cond, @"\(" + false + @"\)", false.ToString());
                replacedCond = Regex.Replace(replacedCond, @"\(" + true + @"\)", true.ToString());
                var logicalOp = Regex.Match(cond, @"(&&|\|\|)");
                
                if (!logicalOp.Success)
                    continue;
                var tmpEval = EvaluateOperation(replacedCond, logicalOp.Groups[1].Value);
                remainder = Regex.Replace(remainder, Regex.Escape(cond), tmpEval.ToString());
            }
            conditions = EvaluateParenthesesOrder("(" + remainder + ")");
        }

        bool.TryParse(remainder, out var eval);
        return eval;
    }

    /// <summary>
    /// Evaluates a single condition.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to evaluate.</param>
    /// <returns>
    /// <c>True</c> - If the evaluation returned true.<br/>
    /// <c>False</c> - If the evaluation returned false.<br/>
    /// </returns>
    private bool EvaluateSingleCondition(string value) {
        var returnValue = false;
        if (string.IsNullOrEmpty(value))
            return true;
        
        for (var i = 0; i < LogicalOperator.Length; i++) {
            var op = LogicalOperator[i];
            var separated = value.ToLower().Split(op, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (separated.Length != 2 && separated.Length != 0)
                continue;

            int int1, int2;
            switch (i) {
                case 0: // Equals (==).
                    returnValue = separated[0] == separated[1];
                    break;
                case 1: // Not equal to (!=).
                    returnValue = separated[0] != separated[1];
                    break;
                case 2: // Greater than or equal to (>=).
                    if (int.TryParse(separated[0], out int1) && int.TryParse(separated[1], out int2))
                        returnValue = int1 >= int2;
                    break;
                case 3: // Less than or equal to (<=).
                    if (int.TryParse(separated[0], out int1) && int.TryParse(separated[1], out int2))
                        returnValue = int1 <= int2;
                    break;
                case 4: // Greater than (>).
                    if (int.TryParse(separated[0], out int1) && int.TryParse(separated[1], out int2))
                        returnValue = int1 > int2;
                    break;
                case 5: // Less than (<).
                    if (int.TryParse(separated[0], out int1) && int.TryParse(separated[1], out int2))
                        returnValue = int1 < int2;
                    break;
                case 6: // Contains (.contains).
                    var match = Regex.Match(separated[1], @"\((.*)\)");
                    if (match.Success)
                        returnValue = separated[0].Contains(match.Groups[1].Value);
                    break;
                case 7: // True.
                    returnValue = true;
                    break;
                case 8: // False.
                    break;
            }
            break;
        }
        return returnValue;
    }

    /// <summary>
    /// Evaluates a logical operation listed in <c>LogicalOperator</c> with booleans.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to operate with.</param>
    /// <param name="logicalOperator"><c>String</c> - The logical operator to concatenate with.</param>
    /// <returns>
    /// <c>True</c> - If the evaluation result returned true.<br/>
    /// <c>False</c> - If the evaluation result returned false.
    /// </returns>
    private bool EvaluateOperation(string value, string logicalOperator) {
        var conditions = value.Split(logicalOperator, StringSplitOptions.TrimEntries);
        var eval = false;
        var parsed = new bool[conditions.Length];
        for (var i = 0; i < conditions.Length; i++) {
            var cond = conditions[i];
            if (!IsLogicalOperator(cond))
                return false;
            parsed[i] = EvaluateSingleCondition(cond);
            // Set the initial eval value.
            if (i == 0)
                eval = parsed[i];
        }

        if (logicalOperator == LogicAnd) {
            for (var i = 0; i < parsed.Length; ++i) {
                eval &= parsed[i];
            }
            return eval;
        }

        for (var i = 0; i < parsed.Length; ++i) {
            eval |= parsed[i];
        }
        return eval;
    }
    
    /// <summary>
    /// Get an ordered List of all parentheses from the given value.
    /// </summary>
    /// <param name="value">The string which contains the condition / parentheses</param>
    /// <returns></returns>
    private List<string> EvaluateParenthesesOrder(string value) {
        var returnValue = new List<string>();
        var brackets = new Stack<int>();
        var containsOpened = false;
        var containsOp = LogicalOperator[6];
        
        if (string.IsNullOrEmpty(value))
            return returnValue;

        for (var i = 0; i < value.Length; ++i) {
            var currentChar = value[i];

            if (i > containsOp.Length-1 && value.Substring(i-containsOp.Length, containsOp.Length).ToLower() == containsOp)
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

    /// <summary>
    /// Check if the given value is a logical operator, boolean or known function.
    /// </summary>
    /// <param name="value"><c>String</c> - The value to check.</param>
    /// <returns>
    /// <c>True</c> - If value is either logical operator, boolean or known function.<br/>
    /// <c>False</c> - Otherwise.
    /// </returns>
    private bool IsLogicalOperator(string value) {
        for (var i = 0; i < LogicalOperator.Length; i++) {
            if (LogicalOperator[i] == value.ToLower())
                return true;
        }
        return false;
    }
}