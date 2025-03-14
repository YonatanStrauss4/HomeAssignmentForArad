using System;
using System.IO;

namespace TestProject
{
    public class FlowRecord
    {
        // Fixed fields
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

        // Variable-length array of shorts
        public short[] VariableArray { get; set; }

        // Calculated property to convert TemperatureE1 to Celsius
        public float TemperatureCelsius => TemperatureE1 / 10.0f;

        // Method to parse the binary data into a FlowRecord
        public static FlowRecord FromBytes(byte[] rawData)
        {
            using (var reader = new BinaryReader(new MemoryStream(rawData)))
            {
                FlowRecord record = new FlowRecord();

                // Read the fixed fields
                record.TofUpE12 = reader.ReadUInt32(); // 4 bytes
                record.TofDnE12 = reader.ReadUInt32(); // 4 bytes
                record.AmpUp = reader.ReadInt16(); // 2 bytes
                record.AmpDn = reader.ReadInt16(); // 2 bytes
                record.PwrUp = reader.ReadByte(); // 1 byte
                record.PwrDn = reader.ReadByte(); // 1 byte
                record.PwrMin = reader.ReadByte(); // 1 byte
                record.PwrMax = reader.ReadByte(); // 1 byte
                record.VisE14 = reader.ReadUInt64(); // 8 bytes
                record.ReynE6 = reader.ReadUInt64(); // 8 bytes
                record.KfE6 = reader.ReadUInt32(); // 4 bytes
                record.UcvE6 = reader.ReadInt32(); // 4 bytes
                record.SosE6 = reader.ReadUInt32(); // 4 bytes
                record.FlowE6 = reader.ReadInt32(); // 4 bytes
                record.FlowCalculated = reader.ReadSingle(); // 4 bytes
                record.StatusWm = reader.ReadUInt32(); // 4 bytes
                record.TemperatureE1 = reader.ReadInt16(); // 2 bytes
                record.Fhl = reader.ReadByte(); // 1 byte
                record.Volume = reader.ReadDouble(); // 8 bytes
                record.ArrayLength = reader.ReadUInt16(); // 2 bytes

                // Handle the "firmware bug" in ArrayLength
                // The ArrayLength field might be unreliable, so we calculate it based on the remaining bytes.
                int remainingBytes = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                int estimatedLength = remainingBytes / 2; // Since each short is 2 bytes

                // Read the variable-length array (shorts)
                record.VariableArray = new short[estimatedLength];
                for (int i = 0; i < estimatedLength; i++)
                {
                    record.VariableArray[i] = reader.ReadInt16();
                }

                return record;
            }
        }
    }
}

