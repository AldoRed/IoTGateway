﻿using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Backgrounds
{
	/// <summary>
	/// A solid background
	/// </summary>
	public class SolidBackground : Background 
	{
		private ColorAttribute color;

		/// <summary>
		/// A solid background
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public SolidBackground(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "SolidBackground";

		/// <summary>
		/// Color
		/// </summary>
		public ColorAttribute Color
		{
			get => this.color;
			set => this.color = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.color = new ColorAttribute(Input, "color");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.color.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new SolidBackground(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is SolidBackground Dest)
				Dest.color = this.color.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (this.paint is null &&
				this.color.TryEvaluate(State.Session, out SKColor Color))
			{
				this.paint = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					Color = Color
				};

				this.defined = true;
			}
		}

	}
}
