﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class of dynamic layout elements (i.e. elements that can
	/// generate child elements dynamically).
	/// </summary>
	public abstract class DynamicElement : LayoutElement, IDynamicChildren
	{
		/// <summary>
		/// Abstract base class of dynamic layout elements (i.e. elements that can
		/// generate child elements dynamically).
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public DynamicElement(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Dynamic array of children
		/// </summary>
		public abstract ILayoutElement[] DynamicChildren
		{
			get;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			ILayoutElement[] Children = this.DynamicChildren;

			if (!(Children is null))
			{
				foreach (ILayoutElement E in Children)
				{
					if (E.IsVisible)
						E.Draw(State);
				}
			}

		}
	}
}
