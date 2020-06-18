﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for Schema References callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SchemaReferencesEventHandler(object Sender, SchemaReferencesEventArgs e);

	/// <summary>
	/// Event arguments for Schema References responses
	/// </summary>
	public class SchemaReferencesEventArgs : IqResultEventArgs
	{
		private readonly SchemaReference[] references;

		/// <summary>
		/// Event arguments for Schema References responses
		/// </summary>
		/// <param name="e">IQ response event arguments.</param>
		/// <param name="References">Schema References.</param>
		public SchemaReferencesEventArgs(IqResultEventArgs e, SchemaReference[] References)
			: base(e)
		{
			this.references = References;
		}

		/// <summary>
		/// Schema References
		/// </summary>
		public SchemaReference[] References => this.references;
	}
}
