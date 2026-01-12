using System;
using System.Collections.Generic;
using System.Text;

namespace Astral_ServerChecker.Classes;

// Define a simple class to hold server info from JSON
public class Server {
    public required string name { get; set; }
    public required string url { get; set; }
}
