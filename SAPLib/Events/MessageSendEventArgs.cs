using System;
using System.Collections.Generic;
using System.Text;

namespace SAPLib.Events
{
    public class MessageSendEventArgs: EventArgs
    {
        public SapPacket Packet { get; set; }
    }
}
