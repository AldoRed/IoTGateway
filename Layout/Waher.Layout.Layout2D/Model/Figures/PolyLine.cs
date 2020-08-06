﻿using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A poly-line
	/// </summary>
	public class PolyLine : Vertices
	{
		/// <summary>
		/// A poly-line
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public PolyLine(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "PolyLine";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new PolyLine(Document, Parent);
		}
	}
}
