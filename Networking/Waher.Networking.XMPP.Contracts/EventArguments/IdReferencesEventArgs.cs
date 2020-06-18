﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for ID References callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task IdReferencesEventHandler(object Sender, IdReferencesEventArgs e);

	/// <summary>
	/// Event arguments for ID References responses
	/// </summary>
	public class IdReferencesEventArgs : IqResultEventArgs
	{
		private readonly string[] references;

		/// <summary>
		/// Event arguments for ID References responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="References">ID References.</param>
		public IdReferencesEventArgs(IqResultEventArgs e, string[] References)
			: base(e)
		{
			this.references = References;
		}

		/// <summary>
		/// ID References
		/// </summary>
		public string[] References => this.references;
	}
}
