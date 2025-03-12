using Assignment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestProject
{
    // The parsed data structure
    public class FlowRecord
    {
        // Fixed header fields
        public uint TofUpE12 { get; set; }
        public uint TofDnE12 { get; set; }
        public short AmpUp { get; set; }
        public short AmpDn { get; set; }
        public byte PwrUp { get; set; }
        public byte PwrDn { get; set; }
        public byte PwrMin { get; set; }
        public byte PwrMax { get; set; }
        public ulong VisE14 { get; set; }
        public ulong ReynE6 { get; set; }
        public uint KfE6 { get; set; }
        public int UcvE6 { get; set; }
        public uint SosE6 { get; set; }
        public int FlowE6 { get; set; }
        public float FlowCalculated { get; set; }
        public uint StatusWm { get; set; }
        public short TemperatureE1 { get; set; }
        public byte Fhl { get; set; }
        public double Volume { get; set; }
        public ushort ArrayLength { get; set; }

        // Variable-length array
        public short[] DataArray { get; set; }

        // Additional properties for analysis
        public float TemperatureCelsius => TemperatureE1 / 10.0f;

        public override string ToString()
        {
            return $"Flow={FlowE6} e-6, Status={StatusWm}, Temp={TemperatureCelsius:F1}°C, Array={ArrayLength} items";
        }
    }


    // Parser for the raw binary data, now implementing the generic interface
    public class FlowDataParser : IFlowDataParser<FlowRecord>
    {
        private readonly List<FlowRecord> _records = new List<FlowRecord>();

        // Handler for the raw data event
        public void HandleRawData(object? sender, RawDataEventArgs e)
        {
            // Parse the raw binary data
            var record = ParseRawData(e.RawData);

            if (record != null)
            {
                _records.Add(record);
                Console.WriteLine($"Parsed record #{_records.Count}: {record}");
            }
        }

        // Parse the raw binary data into a FlowRecord
        private FlowRecord ParseRawData(byte[] rawData)
        {
            try
            {
                using var stream = new MemoryStream(rawData);
                using var reader = new BinaryReader(stream);
                var record = new FlowRecord
                {
                    TofUpE12 = reader.ReadUInt32(),
                    TofDnE12 = reader.ReadUInt32(),
                    AmpUp = reader.ReadInt16(),
                    AmpDn = reader.ReadInt16(),
                    PwrUp = reader.ReadByte(),
                    PwrDn = reader.ReadByte(),
                    PwrMin = reader.ReadByte(),
                    PwrMax = reader.ReadByte(),
                    VisE14 = reader.ReadUInt64(),
                    ReynE6 = reader.ReadUInt64(),
                    KfE6 = reader.ReadUInt32(),
                    UcvE6 = reader.ReadInt32(),
                    SosE6 = reader.ReadUInt32(),
                    FlowE6 = reader.ReadInt32(),
                    FlowCalculated = reader.ReadSingle(),
                    StatusWm = reader.ReadUInt32(),
                    TemperatureE1 = reader.ReadInt16(),
                    Fhl = reader.ReadByte(),
                    Volume = reader.ReadDouble(),
                    ArrayLength = reader.ReadUInt16()
                };

                // Read the variable-length array
                record.DataArray = new short[record.ArrayLength];
                
                var dataArray = new List<short>();
                while (stream.Position < stream.Length)
                {
                    dataArray.Add(reader.ReadInt16());
                }
                record.DataArray = dataArray.ToArray();

                return record;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing data: {ex.Message}");
                return null;
            }
        }

        // Clear all records
        public void ClearRecords()
        {
            _records.Clear();
        }

        // Get all records
        public List<FlowRecord> GetRecords()
        {
            return _records;
        }

        // Implementing the remaining interface methods
        
        // Reconstruct the sine wave from all records
        public short[] ReconstructSineWave()
        {
            // Calculate total length
            int totalLength = _records.Sum(r => r.DataArray.Length);
            short[] sineWave = new short[totalLength];

            int position = 0;
            foreach (var record in _records)
            {
                Array.Copy(record.DataArray, 0, sineWave, position, record.DataArray.Length);
                position += record.DataArray.Length;
            }

            return sineWave;
        }

        // Save the sine wave to a CSV file
        public void SaveSineWaveToCSV(string filePath)
        {
            short[] sineWave = ReconstructSineWave();

            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Index,Value");
                    for (int i = 0; i < sineWave.Length; i++)
                    {
                        writer.WriteLine($"{i},{sineWave[i]}");
                    }
                }

                Console.WriteLine($"Sine wave saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving sine wave to CSV: {ex.Message}");
            }
        }

        // Calculate basic statistics for the flow rate
        public (double Min, double Max, double Avg) CalculateFlowStatistics()
        {
            if (_records.Count == 0)
                return (0, 0, 0);

            double min = _records.Min(r => r.FlowE6);
            double max = _records.Max(r => r.FlowE6);
            double avg = _records.Average(r => r.FlowE6);

            return (min, max, avg);
        }

        // Detect status changes
        public List<int> DetectStatusChanges()
        {
            var changes = new List<int>();

            for (int i = 1; i < _records.Count; i++)
            {
                if (_records[i].StatusWm != _records[i - 1].StatusWm)
                {
                    changes.Add(i);
                }
            }

            return changes;
        }
    }
}