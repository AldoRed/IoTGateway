﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers.Model
{
    /// <summary>
    /// Represents a sniffer binary reception event.
    /// </summary>
    public class SnifferRxBinary : SnifferBinaryEvent
    {
        /// <summary>
        /// Represents a sniffer binary reception event.
        /// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
        /// <param name="Data">Data.</param>
        public SnifferRxBinary(DateTime Timestamp, byte[] Data)
            : base(Timestamp, Data)
        {
        }

        /// <summary>
        /// Replays the event to a given sniffer.
        /// </summary>
        /// <param name="Sniffer">Sniffer.</param>
        public override Task Replay(ISniffer Sniffer)
        {
            return Sniffer.ReceiveBinary(this.Timestamp, this.Data);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            int Len = this.Data?.Length ?? 0;

            if (Len == 1)
                return "RX: (1 byte)";
            else
                return "RX: (" + Len.ToString() + " bytes)";
        }
    }
}
