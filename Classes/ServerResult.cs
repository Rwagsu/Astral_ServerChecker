using System;
using System.Collections.Generic;
using System.Text;

namespace Astral_ServerChecker.Classes;

// Define a class to hold test results for each server
public class ServerResult {
    public required Server Server { get; set; }
    public double AverageLatency { get; set; }  // In milliseconds, -1 if failed
    public double StdDev { get; set; }  // Standard deviation for stability
}