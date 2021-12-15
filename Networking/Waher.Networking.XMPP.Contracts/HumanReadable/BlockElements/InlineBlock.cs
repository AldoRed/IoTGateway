﻿using System;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Abstract base class of blocks with inline elements.
	/// </summary>
	public abstract class InlineBlock : BlockElement
	{
		private InlineElement[] elements;

		/// <summary>
		/// Inline elements
		/// </summary>
		public InlineElement[] Elements
		{
			get => this.elements;
			set => this.elements = value;
		}
	
		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				foreach (InlineElement E in this.elements)
				{
					if (E is null || !E.IsWellDefined)
						return false;
				}

				return true;
			}
		}
	}
}
