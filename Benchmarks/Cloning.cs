using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace Benchmarks; 

class Sample {
    public string field1  { get; set; } = "";
    public string field2  { get; set; } = "";
    public string field3  { get; set; } = "";
    public string field4  { get; set; } = "";
    public string field5  { get; set; } = "";
}

[MemoryDiagnoser]
public class Cloning {
    [Benchmark]
    public void SerializedCloning() => SerializedClone(new Sample());

    [Benchmark]
    public void ConstructoredCloning() => ConstructoredClone(new Sample());
    
    private void SerializedClone(Sample s) {
        var serialized = JsonConvert.SerializeObject(s);
        var settings = new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Replace};
        JsonConvert.DeserializeObject<Sample>(serialized,settings);
    }
    
    private void ConstructoredClone(Sample s) {
        new Sample().field1 = s.field1;
        new Sample().field2 = s.field2;
        new Sample().field3 = s.field3;
        new Sample().field4 = s.field4;
        new Sample().field5 = s.field5;
    }
}