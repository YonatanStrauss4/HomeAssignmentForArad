using Assignment;

namespace TestProject;

public class Tests
{
    private readonly IBleDeviceEmulator _emulator = BleDeviceEmulatorFactory.Create(intervalMs: 500);
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _emulator.SetBinaryFilesDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Emulator/BinaryFiles/"));
    }
   
    [SetUp]
    public void Setup()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }

    [TestCase(FlowPattern.Constant)]
    [TestCase(FlowPattern.Pulsing)]
    [TestCase(FlowPattern.Zero)]
    public void Test1(FlowPattern pattern)
    {
        
    }
    
}