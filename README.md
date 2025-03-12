# Flow Data Parser Implementation Assignment

## Overview

In this assignment, you will implement a parser for binary data emitted by a Bluetooth Low Energy (BLE) flow meter device. You have been provided with a BLE device emulator DLL that simulates the device's behavior by sending binary data packets. Your task is to parse these binary packets into structured data records and perform basic analysis on the collected data.

## Provided Materials

1. **BLEEmulator.dll**: A compiled .NET library that simulates a BLE flow meter device
2. **IFlowDataParser<TRecord> interface**: Generic interface defining the required parser functionality

## Assignment Tasks

### 1. Create a FlowRecord Class

Implement a class that represents the structured data from the flow meter. Each binary packet contains the following fields in this exact order:

- `TofUpE12` (uint): Time of flight upstream value (multiplied by 10^12)
- `TofDnE12` (uint): Time of flight downstream value (multiplied by 10^12)
- `AmpUp` (short): Amplitude upstream value
- `AmpDn` (short): Amplitude downstream value
- `PwrUp` (byte): Power upstream value
- `PwrDn` (byte): Power downstream value
- `PwrMin` (byte): Minimum power value
- `PwrMax` (byte): Maximum power value
- `VisE14` (ulong): Viscosity value (multiplied by 10^14)
- `ReynE6` (ulong): Reynolds number (multiplied by 10^6)
- `KfE6` (uint): K-factor value (multiplied by 10^6)
- `UcvE6` (int): UCV value (multiplied by 10^6)
- `SosE6` (uint): Speed of sound value (multiplied by 10^6)
- `FlowE6` (int): Flow rate value (multiplied by 10^6)
- `FlowCalculated` (float): Calculated flow rate
- `StatusWm` (uint): Status value
- `TemperatureE1` (short): Temperature value (multiplied by 10)
- `Fhl` (byte): Flow history level
- `Volume` (double): Cumulative volume
- `ArrayLength` (ushort): Number of items in the **variable-length** array

After these fixed fields, there is a variable-length array of short values. The length of this array is specified by the `ArrayLength` field.
Due to "Firmware bug", the `ArrayLength` field is not reliabe. Think of a way to parse it correctly.

Additionally, implement a calculated property:
- `TemperatureCelsius` (float): Temperature in Celsius (TemperatureE1 / 10.0f)

### 2. Implement the Flow Data Parser

Create a class that implements the provided `IFlowDataParser<TRecord>` interface using your FlowRecord class as the type parameter. The parser should:

1. Subscribe to the BLE emulator's data events
2. Parse the binary data into FlowRecord objects
3. Store the parsed records for analysis
4. Implement all methods defined in the interface:
   - `HandleRawData`: Process raw binary data from the emulator
   - `ClearRecords`: Clear all stored records
   - `GetRecords`: Return all parsed records
   -  Additional methods you want.

### 3. Implement the testing code

Write comprehensive tests that verify that the emulated embedded devices works correctly:
1. Verify correct(to a possible extend) parsing of all fields in the FlowRecord
2. Test the parser with all three flow patterns (Zero, Constant, and Pulsing)
3. Confirm the following statistical requirements are met:
   - For Zero flow pattern: Mean volume should be -0.0000235 within += 0.00002 volume units, with standard deviation not more than ±9.3 × 10⁻⁶
   - For Constant flow pattern: Mean flow should be within 12 ±5 flow units, with standard deviation less than 9
   - For Pulsing flow pattern: Mean flow should be within 15 ± 1 flow units, with standard deviation between 7 and 9
     Utilize Nunit framework, to impement tests, which comfirm the said requirements.
4. Verify that the status field is 0 for at least 98% of the records (status 1 is allowed for up to 2% of the records)
5. Steps 3 and 4 might fail, but this does not necessarily indicate an error in your solution. Some tests might fail because the emulated data does not fully comply with the requirements

## Implementation Guidelines

1. Use `BinaryReader` to parse the binary data efficiently
2. Handle errors gracefully and log parsing issues
3. Ensure all interface methods are correctly implemented
4. Follow C# coding conventions and practice good object-oriented design
5. Follow the testing code concepts and paradigmas.
6. Include appropriate documentation for your code


## Using the BLE Emulator

The emulator can be used as follows:

```csharp
// Create an emulator instance
IBleDeviceEmulator emulator = BleDeviceEmulatorFactory.Create(intervalMs: 500);

// Create your parser
IFlowDataParser<YourFlowRecord> parser = new YourFlowDataParser();

// Subscribe to data events
emulator.DataReceived += parser.HandleRawData;

// Start emulation with desired pattern
emulator.Start(FlowPattern.Constant);

// Wait for some time to collect data

// Stop emulation
emulator.Stop();

// Access and analyze the parsed data
var records = parser.GetRecords();
```

## Deliverables

1. Source code for your FlowRecord class
2. Source code for your parser implementation
3. Test project demonstrating your implementation works correctly
4. Brief documentation explaining your approach and any design decisions

## Evaluation Criteria

Your implementation will be evaluated based on:

1. Correctness of the binary data parsing
2. Compliance with the provided interface
3. Accuracy of the data analysis
4. Code quality and adherence to best practices
5. Test coverage and quality

Good luck!
