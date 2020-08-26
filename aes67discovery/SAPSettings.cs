using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace aes67discovery
{
    public class SAPSettings
    {
        public int Interval { get; set; } = 30000;
        public string NetInterface { get; set; }
        public List<string> SDPFiles { get; set; } = new List<string>();

    }
}
