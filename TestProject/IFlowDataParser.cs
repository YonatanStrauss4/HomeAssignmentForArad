using Assignment;

namespace TestProject
{
    /// <summary>
    /// Interface for a flow data parser that handles BLE device data using a custom record type
    /// </summary>
    /// <typeparam name="TRecord">The flow record type to be created by the implementer</typeparam>
    public interface IFlowDataParser<TRecord>
    {
        /// <summary>
        /// Handles raw data received from the BLE device
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Event arguments containing the raw data</param>
        void HandleRawData(object? sender, RawDataEventArgs e);

        /// <summary>
        /// Clears all parsed records
        /// </summary>
        void ClearRecords();

        /// <summary>
        /// Gets all parsed records
        /// </summary>
        /// <returns>List of flow records</returns>
        List<TRecord> GetRecords();
    }
}