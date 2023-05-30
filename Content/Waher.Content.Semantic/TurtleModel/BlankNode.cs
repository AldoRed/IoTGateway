﻿namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a blank node
	/// </summary>
	public class BlankNode : ISemanticElement
	{
		/// <summary>
		/// Represents a blank node
		/// </summary>
		/// <param name="NodeIndex">Blank-node Node Index in document.</param>
		public BlankNode(int NodeIndex)
			: this("n" + NodeIndex.ToString())
		{
		}

		/// <summary>
		/// Represents a blank node
		/// </summary>
		/// <param name="NodeId">Blank-node Node ID in document.</param>
		public BlankNode(string NodeId)
		{
			this.NodeId = NodeId;
		}

		/// <summary>
		/// Blank node Node ID.
		/// </summary>
		public string NodeId { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return "_:" + this.NodeId.ToString();
		}
	}
}
