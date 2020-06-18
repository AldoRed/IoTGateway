﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for signature callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SignatureEventHandler(object Sender, SignatureEventArgs e);

	/// <summary>
	/// Event arguments for signature responses
	/// </summary>
	public class SignatureEventArgs : IqResultEventArgs
	{
		private readonly byte[] signature;

        /// <summary>
        /// Event arguments for signature responses
        /// </summary>
        /// <param name="e">IQ response event arguments.</param>
        /// <param name="Signature">Digital signature</param>
        public SignatureEventArgs(IqResultEventArgs e, byte[] Signature)
			: base(e)
		{
			this.signature = Signature;
		}

		/// <summary>
		/// Digital signature
		/// </summary>
		public byte[] Signature => this.signature;
	}
}
