﻿using System;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for CanControl callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CanControlCallback(object Sender, CanControlResponseEventArgs e);

	/// <summary>
	/// Event arguments for CanControl callback event arguments.
	/// </summary>
	public class CanControlResponseEventArgs : NodesEventArgs
	{
		private readonly bool canControl;
		private readonly string[] parameterNames;

		internal CanControlResponseEventArgs(IqResultEventArgs e, object State, string JID, bool CanControl, 
			IThingReference[] Nodes, string[] ParameterNames)
			: base(e, State, JID, Nodes)
		{
			this.canControl = CanControl;
			this.parameterNames = ParameterNames;
		}

		/// <summary>
		/// If the control operation can be performed.
		/// </summary>
		public bool CanControl => this.canControl;

		/// <summary>
		/// Parameter names allowed to be processed. If null, no parameter restrictions exist.
		/// </summary>
		public string[] ParameterNames => this.parameterNames;
	}
}
