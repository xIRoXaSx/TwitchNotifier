using BenchmarkDotNet.Attributes;

namespace Benchmarks; 

[MemoryDiagnoser]
public class Placeholder {
    // [Benchmark]
    public void PlaceholderLinqWhereCount() => LinqWhereCount();
    
    // [Benchmark]
    public void PlaceholderLinqCount() => LinqCount();

    // [Benchmark]
    public void PlaceholderForEachSeparateLoopIf() => ForEachSeparateLoopIf();
    
    // [Benchmark]
    public void PlaceholderForSameLoopIf() => ForSameLoopIf(); 
    
    // [Benchmark]
    public void PlaceholderForSameLoopIfVar() => ForSameLoopIfVar();
    
    // [Benchmark]
    public void PlaceholderForEachSameLoopIf() => ForEachSameLoopIf();
    
    // [Benchmark]
    public void PlaceholderForEachSameLoopSwitch() => ForEachSameLoopSwitch();

    // [Benchmark]
    public void PlaceholderLinqSkipAndTake() => LinqSkipAndTake();

    // [Benchmark]
    public void PlaceholderSubstring() => Substring();
    
    private void LinqWhereCount() {
        var oCount = Program.Value.Where(x => x == '(').Count();
        var cCount = Program.Value.Where(x => x == ')').Count();
    }

    private void LinqCount() {
        var oCount = Program.Value.Count(x => x == '(');
        var cCount = Program.Value.Count(x => x == ')');
    }

    private void ForEachSeparateLoopIf() {
        var oCount = 0;
        var cCount = 0;

        foreach (var c in Program.Value) {
            if (c == '(')
                oCount++;
        }
        
        foreach (var c in Program.Value) {
            if (c == ')')
                cCount++;
        }
    }

    private void ForSameLoopIf() {
        var oCount = 0;
        var cCount = 0;

        for (var i = 0; i < Program.Value.Length; i++) {
            if (Program.Value[i] == '(')
                oCount++;
            if (Program.Value[i] == ')')
                cCount++;
        }
    }

    private void ForSameLoopIfVar() {
        var oCount = 0;
        var cCount = 0;

        for (var i = 0; i < Program.Value.Length; i++) {
            var val = Program.Value[i];
            if (val == '(')
                oCount++;
            if (val == ')')
                cCount++;
        }
    }

    private void ForEachSameLoopIf() {
        var oCount = 0;
        var cCount = 0;

        foreach (var c in Program.Value) {
            if (c == '(')
                oCount++;
            if (c == ')')
                cCount++;
        }
    }
    
    private void ForEachSameLoopSwitch() {
        var oCount = 0;
        var cCount = 0;

        foreach (var c in Program.Value) {
            switch (c) {
                case '(':
                    oCount++;
                    break;
                case ')':
                    cCount++;
                    break;
            }
        }
    }

    private void LinqSkipAndTake() {
        string.Join("", Program.Value.Skip(37).Take(3));
    }
    
    private void Substring() {
        Program.Value.Substring(37, 3);
    }
}