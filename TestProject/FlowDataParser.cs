using Assignment;
using System;
using System.Collections.Generic;

namespace TestProject
{
    public class FlowDataParser : IFlowDataParser<FlowRecord>
    {
        // A list to store the floe records
        private List<FlowRecord> _records = new List<FlowRecord>();

        // Implement HandleRawData
        public void HandleRawData(object? sender, RawDataEventArgs e)
        {
            // Parse the raw data to create FlowRecord instances
            FlowRecord record = FlowRecord.FromBytes(e.RawData);
            _records.Add(record);  // Add the parsed record to the list
        }

        // Implement ClearRecords
        public void ClearRecords()
        {
            _records.Clear();  // Clear the list of records
        }

        // Implement GetRecords
        public List<FlowRecord> GetRecords()
        {
            return new List<FlowRecord>(_records);  // Return a copy of the list
        }
    }
}