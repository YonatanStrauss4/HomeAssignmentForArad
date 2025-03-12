using Assignment;

namespace TestProject;

public class Tests
{
    private readonly IBleDeviceEmulator _emulator = BleDeviceEmulatorFactory.Create(intervalMs: 500);

    private FlowDataParser parser;
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _emulator.SetBinaryFilesDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Emulator/BinaryFiles/"));
        parser = new FlowDataParser();
    }
   
    [SetUp]
    public void Setup()
    {
        _emulator.DataReceived += parser.HandleRawData;
    }

    [TearDown]
    public void TearDown()
    {
        parser.ClearRecords();
        _emulator.DataReceived -= parser.HandleRawData;
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _emulator.Dispose();
    }

    [TestCase(FlowPattern.Constant)]
    [TestCase(FlowPattern.Pulsing)]
    [TestCase(FlowPattern.Zero)]
    public void Test1(FlowPattern pattern)
    {
        _emulator.Start(pattern);
        var records = parser.GetRecords();
        var mean = records.Average(r => r.Volume);
        var stdDev = Math.Sqrt(records.Average(r => Math.Pow(r.Volume - mean, 2)));
        Console.WriteLine($"Type: {pattern}; Mean: {mean}, StdDev: {stdDev}");
        _emulator.Stop();
        
    }
    
}