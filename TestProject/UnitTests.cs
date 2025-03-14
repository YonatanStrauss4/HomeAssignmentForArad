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

        //This method is called once before all tests are run. It sets up shared resources
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // configures the directory for the binary files that the emulator needs using a relative path based on the application’s current base directory
            _emulator.SetBinaryFilesDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Emulator/BinaryFiles/"));
        }

        [SetUp]
        public void Setup()
        {
            _parser = new FlowDataParser(); // Initialize your parser
            _records = new List<FlowRecord>(); // Initialize flow records list
            // subscribes the parser to the DataReceived event of the emulator. When data is received, the HandleRawData method of the parser will be invoked
            _emulator.DataReceived += _parser.HandleRawData;
        }

        // Clears the _records list to ensure the next test starts with an empty collection
        [TearDown]
        public void TearDown()
        {
            _records.Clear();
        }

        // Unsubscribes the HandleRawData method from the emulator’s DataReceived event to clean up any resources used
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _emulator.DataReceived -= _parser.HandleRawData;
        }

        // Main test case to verify all flow patterns
        [TestCase(FlowPattern.Constant)]
        [TestCase(FlowPattern.Pulsing)]
        [TestCase(FlowPattern.Zero)]
        
        public void TestFlowPatterns(FlowPattern pattern)
        {
            // Start emulation with the given flow pattern
            _emulator.Start(pattern);

            // Wait for some time to collect data 
            System.Threading.Thread.Sleep(10000); // Waiting 10 seconds for data collection
            // I would have changed this to signals if I could change the emulator

            // Stop emulation
            _emulator.Stop();

            // Access and analyze the parsed data
            _records = _parser.GetRecords();

            /*// Verify if fields are initialized and contain valid values
            foreach (var record in _records)
            {
                // Validate that fields are not NaN or null
                Assert.IsTrue(!double.IsNaN(record.TofUpE12), "TofUpE12 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.TofDnE12), "TofDnE12 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.AmpUp), "AmpUp should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.AmpDn), "AmpDn should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.PwrUp), "PwrUp should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.PwrDn), "PwrDn should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.PwrMin), "PwrMin should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.PwrMax), "PwrMax should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.VisE14), "VisE14 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.ReynE6), "ReynE6 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.KfE6), "KfE6 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.UcvE6), "UcvE6 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.SosE6), "SosE6 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.FlowE6), "FlowE6 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.StatusWm), "StatusWm should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.TemperatureE1), "TemperatureE1 should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.Fhl), "Fhl should be initialized and contain a valid value.");
                Assert.IsTrue(!double.IsNaN(record.Volume), "Volume should be initialized and contain a valid value.");
            }*/


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

         private void TestZeroPatternStats()
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

        private void TestConstantPatternStats()
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

        private void TestPulsingPatternStats()
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

        // Method to calculate the standard deviation of a list of double values
        private double StandardDeviation(List<double> values)
        {
            // Calculate the mean (average) of the values
            double mean = values.Average();
            
            // Calculate the sum of the squared differences from the mean
            double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            
            // Return the square root of the average squared differences (standard deviation)
            return Math.Sqrt(sumOfSquares / values.Count);
        }

    }
}
