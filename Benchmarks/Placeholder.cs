using BenchmarkDotNet.Attributes;

namespace Benchmarks; 

[MemoryDiagnoser]
public class Placeholder {
    private const string Value = "(This is (a simple string (with some '('))";
    private void LinqWhereCount() {
        var oCount = Value.Where(x => x == '(').Count();
        var cCount = Value.Where(x => x == ')').Count();
    }

    private void LinqCount() {
        var oCount = Value.Count(x => x == '(');
        var cCount = Value.Count(x => x == ')');
    }

    private void ForEachSeparateLoopIf() {
        var oCount = 0;
        var cCount = 0;

        foreach (var c in Value) {
            if (c == '(')
                oCount++;
        }
        
        foreach (var c in Value) {
            if (c == ')')
                cCount++;
        }
    }

    private void ForSameLoopIf() {
        var oCount = 0;
        var cCount = 0;

        for (var i = 0; i < Value.Length; i++) {
            if (Value[i] == '(')
                oCount++;
            if (Value[i] == ')')
                cCount++;
        }
    }

    private void ForSameLoopIfVar() {
        var oCount = 0;
        var cCount = 0;

        for (var i = 0; i < Value.Length; i++) {
            var val = Value[i];
            if (val == '(')
                oCount++;
            if (val == ')')
                cCount++;
        }
    }

    private void ForEachSameLoopIf() {
        var oCount = 0;
        var cCount = 0;

        foreach (var c in Value) {
            if (c == '(')
                oCount++;
            if (c == ')')
                cCount++;
        }
    }
    
    private void ForEachSameLoopSwitch() {
        var oCount = 0;
        var cCount = 0;

        foreach (var c in Value) {
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

    [Benchmark]
    public void BenchLinqWhereCount() => LinqWhereCount();
    
    [Benchmark]
    public void BenchLinqCount() => LinqCount();

    [Benchmark]
    public void BenchForEachSeparateLoopIf() => ForEachSeparateLoopIf();
    
    [Benchmark]
    public void BenchForSameLoopIf() => ForSameLoopIf(); 
    
    [Benchmark]
    public void BenchForSameLoopIfVar() => ForSameLoopIfVar();
    
    [Benchmark]
    public void BenchForEachSameLoopIf() => ForEachSameLoopIf();
    
    [Benchmark]
    public void BenchForEachSameLoopSwitch() => ForEachSameLoopSwitch();
}