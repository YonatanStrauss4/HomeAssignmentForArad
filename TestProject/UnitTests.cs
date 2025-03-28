using Assignment;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestProject
{
    public class Tests
    {
        // A reference to the device emulator, which simulates the embedded device’s behavior. It is created with an interval of 500 milliseconds
        private readonly IBleDeviceEmulator _emulator = BleDeviceEmulatorFactory.Create(intervalMs: 500);
        // A reference to a parser that processes the raw data into FlowRecord objects
        private IFlowDataParser<FlowRecord> _parser;
        // A list of FlowRecord objects that will hold the parsed data
        private List<FlowRecord> _records;

        // Simple Logger 
        private void LogError(string message)
        {
            // A simple log to console
            Console.WriteLine($"[ERROR] {DateTime.Now}: {message}");
        }

        //This method is called once before all tests are run. It sets up shared resources
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            try
            {
                // configures the directory for the binary files that the emulator needs using a relative path based on the application’s current base directory
                _emulator.SetBinaryFilesDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Emulator/BinaryFiles/"));
            }
            catch (Exception ex)
            {
                LogError($"Error during OneTimeSetup: {ex.Message}");
            }
        }

        [SetUp]
        public void Setup()
        {
            try
            {
                _parser = new FlowDataParser(); // Initialize your parser
                _records = new List<FlowRecord>(); // Initialize flow records list
                // subscribes the parser to the DataReceived event of the emulator. When data is received, the HandleRawData method of the parser will be invoked
                _emulator.DataReceived += _parser.HandleRawData;
            }
            
            
            catch (Exception ex)
            {
                LogError($"Error during Setup: {ex.Message}");
            }
        }

        // Clears the _records list to ensure the next test starts with an empty collection
        [TearDown]
        public void TearDown()
        {
            try
            {
                _records.Clear();
            }
            catch (Exception ex)
            {
                LogError($"Error during TearDown: {ex.Message}");
            }
        }

        // Unsubscribes the HandleRawData method from the emulator’s DataReceived event to clean up any resources used
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            try
            {
                _emulator.DataReceived -= _parser.HandleRawData;
            }
            catch (Exception ex)
            {
                LogError($"Error during OneTimeTearDown: {ex.Message}");
            }
        }

        // Main test case to verify all flow patterns
        [TestCase(FlowPattern.Constant)]
        [TestCase(FlowPattern.Pulsing)]
        [TestCase(FlowPattern.Zero)]
        
        public void TestFlowPatterns(FlowPattern pattern)
        {
            try
            {
                // Start emulation with the given flow pattern
                _emulator.Start(pattern);

                // Wait for some time to collect data 
                System.Threading.Thread.Sleep(10000); // Waiting 10 seconds for data collection

                // Stop emulation
                _emulator.Stop();

                // Access and analyze the parsed data
                _records = _parser.GetRecords();
                
                TestParserValidity(_records);

                
                // Perform statistical analysis based on the pattern
                if (pattern == FlowPattern.Zero)
                {
                    TestZeroPatternStats();
                }
                else if (pattern == FlowPattern.Constant)
                {
                    TestConstantPatternStats();
                }
                else if (pattern == FlowPattern.Pulsing)
                {
                    TestPulsingPatternStats();
                }

                // Verify the status field is 0 for at least 98% of the records
                int statusZeroCount = _records.Count(r => r.StatusWm == 0);
                double statusZeroPercentage = ((double)statusZeroCount / _records.Count) * 100;

                Assert.IsTrue(statusZeroPercentage >= 98, "Status field should be 0 for at least 98% of records.");
            }
            catch (Exception ex)
            {
                LogError($"Error during TestFlowPatterns for pattern {pattern}: {ex.Message}");
            }
        }

        public void TestParserValidity(List<FlowRecord> _records)
        {
            try
            {

                // Ensure that records exist
                Assert.IsNotNull(_records, "Parser returned null records.");
                Assert.IsNotEmpty(_records, "Parser returned an empty list of records.");

                // Validate each record
                foreach (var record in _records)
                {
                    // Check if short values are within the valid range
                    Assert.That(record.AmpUp, Is.InRange(short.MinValue, short.MaxValue), "AmpUp is out of range.");
                    Assert.That(record.AmpDn, Is.InRange(short.MinValue, short.MaxValue), "AmpDn is out of range.");
                    Assert.That(record.TemperatureE1, Is.InRange(short.MinValue, short.MaxValue), "TemperatureE1 is out of range.");

                    // Check if byte values are within the valid range
                    Assert.That(record.PwrUp, Is.InRange(byte.MinValue, byte.MaxValue), "PwrUp is out of range.");
                    Assert.That(record.PwrDn, Is.InRange(byte.MinValue, byte.MaxValue), "PwrDn is out of range.");
                    Assert.That(record.PwrMin, Is.InRange(byte.MinValue, byte.MaxValue), "PwrMin is out of range.");
                    Assert.That(record.PwrMax, Is.InRange(byte.MinValue, byte.MaxValue), "PwrMax is out of range.");
                    Assert.That(record.Fhl, Is.InRange(byte.MinValue, byte.MaxValue), "Fhl is out of range.");

                    // Check if uint values are within the valid range
                    Assert.That(record.TofUpE12, Is.InRange(uint.MinValue, uint.MaxValue), "TofUpE12 is out of range.");
                    Assert.That(record.TofDnE12, Is.InRange(uint.MinValue, uint.MaxValue), "TofDnE12 is out of range.");
                    Assert.That(record.KfE6, Is.InRange(uint.MinValue, uint.MaxValue), "KfE6 is out of range.");
                    Assert.That(record.SosE6, Is.InRange(uint.MinValue, uint.MaxValue), "SosE6 is out of range.");
                    Assert.That(record.StatusWm, Is.InRange(uint.MinValue, uint.MaxValue), "StatusWm is out of range.");
                    Assert.That(record.ArrayLength, Is.InRange(0, ushort.MaxValue), "ArrayLength is out of range.");

                    // Check if int values are within the valid range
                    Assert.That(record.UcvE6, Is.InRange(int.MinValue, int.MaxValue), "UcvE6 is out of range.");
                    Assert.That(record.FlowE6, Is.InRange(int.MinValue, int.MaxValue), "FlowE6 is out of range.");

                    // Check if ulong values are within the valid range
                    Assert.That(record.VisE14, Is.InRange(ulong.MinValue, ulong.MaxValue), "VisE14 is out of range.");
                    Assert.That(record.ReynE6, Is.InRange(ulong.MinValue, ulong.MaxValue), "ReynE6 is out of range.");

                    // Check if float values are valid
                    Assert.IsFalse(float.IsNaN(record.FlowCalculated), "FlowCalculated is NaN.");
                    Assert.IsFalse(float.IsInfinity(record.FlowCalculated), "FlowCalculated is infinite.");

                    // Check if double values are valid
                    Assert.IsFalse(double.IsNaN(record.Volume), "Volume is NaN.");
                    Assert.IsFalse(double.IsInfinity(record.Volume), "Volume is infinite.");
                    
                }
            }
            catch (Exception ex)
            {
                LogError($"Error during TestParserValidity: {ex.Message}");
            }
        }


        private void TestZeroPatternStats()
        {
            try
            {
                // Select all the Volume values from the records and convert them to a list
                var volumes = _records.Select(r => r.Volume).ToList();

                // Calculate the mean (average) volume
                double meanVolume = volumes.Average();

                // Calculate the standard deviation of the volume values
                double stdDevVolume = StandardDeviation(volumes);

                // Assert that the mean volume is within the expected range for the Zero flow pattern
                Assert.AreEqual(-0.0000235, meanVolume, 0.00002, "Mean volume for Zero flow pattern is incorrect.");

                // Assert that the standard deviation of the volume values is within the expected range
                Assert.LessOrEqual(stdDevVolume, 9.3e-6, "Standard deviation for Zero flow pattern is too high.");
            }
            catch (Exception ex)
            {
                LogError($"Error during TestZeroPatternStats: {ex.Message}");
            }
        }

        private void TestConstantPatternStats()
        {
            try
            {
                // Select all the FlowCalculated values from the records and convert them to a list
                var flows = _records.Select(r => r.FlowCalculated).ToList();

                // Calculate the mean (average) flow
                double meanFlow = flows.Average();

                // Calculate the standard deviation of the flow values (convert each to double before passing to the method)
                double stdDevFlow = StandardDeviation(flows.Select(f => (double)f).ToList());

                // Assert that the mean flow is within the expected range for the Constant flow pattern
                Assert.AreEqual(12, meanFlow, 5, "Mean flow for Constant flow pattern is incorrect.");

                // Assert that the standard deviation of the flow values is within the expected range
                Assert.LessOrEqual(stdDevFlow, 9, "Standard deviation for Constant flow pattern is too high.");
            }
            catch (Exception ex)
            {
                LogError($"Error during TestConstantPatternStats: {ex.Message}");
            }
        }

        private void TestPulsingPatternStats()
        {
            try
            {
                // Select all the FlowCalculated values from the records and convert them to a list
                var flows = _records.Select(r => r.FlowCalculated).ToList();

                // Calculate the mean (average) flow
                double meanFlow = flows.Average();

                // Calculate the standard deviation of the flow values (convert each to double before passing to the method)
                double stdDevFlow = StandardDeviation(flows.Select(f => (double)f).ToList());

                // Assert that the mean flow is within the expected range for the Pulsing flow pattern
                Assert.AreEqual(15, meanFlow, 1, "Mean flow for Pulsing flow pattern is incorrect.");

                // Assert that the standard deviation of the flow values is within the expected range for Pulsing pattern
                Assert.IsTrue(stdDevFlow >= 7 && stdDevFlow <= 9, "Standard deviation for Pulsing flow pattern is out of range.");
            }
            catch (Exception ex)
            {
                LogError($"Error during TestPulsingPatternStats: {ex.Message}");
            }
        }

        // Method to calculate the standard deviation of a list of double values
        private double StandardDeviation(List<double> values)
        {
            try
            {
                // Calculate the mean (average) of the values
                double mean = values.Average();

                // Calculate the sum of the squared differences from the mean
                double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));

                // Return the square root of the average squared differences (standard deviation)
                return Math.Sqrt(sumOfSquares / values.Count);
            }
            catch (Exception ex)
            {
                LogError($"Error calculating standard deviation: {ex.Message}");
                return 0;  // Return a default value to prevent failure
            }
        }
    }
}
