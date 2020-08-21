﻿using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Layout.Layout2D.Model.Pens;

namespace Waher.Layout.Layout2D.Model.Backgrounds
{
	/// <summary>
	/// Root node for two-dimensional layouts
	/// </summary>
	public class Layout2D : LayoutContainer
	{
		private StringAttribute font;
		private StringAttribute pen;
		private StringAttribute background;
		private ColorAttribute textColor;

		/// <summary>
		/// Root node for two-dimensional layouts
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Layout2D(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Layout2D";

		/// <summary>
		/// Font ID
		/// </summary>
		public StringAttribute FontIdAttribute
		{
			get => this.font;
			set => this.font = value;
		}

		/// <summary>
		/// Pen ID
		/// </summary>
		public StringAttribute PenIdAttribute
		{
			get => this.pen;
			set => this.pen = value;
		}

		/// <summary>
		/// Background ID
		/// </summary>
		public StringAttribute BackgroundIdAttribute
		{
			get => this.background;
			set => this.background = value;
		}

		/// <summary>
		/// Text Color
		/// </summary>
		public ColorAttribute TextColorAttribute
		{
			get => this.textColor;
			set => this.textColor = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.font = new StringAttribute(Input, "font");
			this.pen = new StringAttribute(Input, "pen");
			this.background = new StringAttribute(Input, "background");
			this.textColor = new ColorAttribute(Input, "textColor");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.font.Export(Output);
			this.pen.Export(Output);
			this.background.Export(Output);
			this.textColor.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Layout2D(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Layout2D Dest)
			{
				Dest.font = this.font.CopyIfNotPreset();
				Dest.pen = this.pen.CopyIfNotPreset();
				Dest.background = this.background.CopyIfNotPreset();
				Dest.textColor = this.textColor.CopyIfNotPreset();
			}
		}

		private Font fontDef = null;

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			SKFont FontBak = null;
			SKPaint TextBak = null;

			if (this.font.TryEvaluate(State.Session, out string FontId) &&
				this.Document.TryGetElement(FontId, out ILayoutElement Element) &&
				Element is Font Font)
			{
				this.fontDef = Font;
				Font.MeasureDimensions(State);	// Not measured before.
				
				FontBak = State.Font;
				State.Font = this.fontDef.FontDef;

				TextBak = State.Text;
				State.Text = this.fontDef.Text;
			}

			if (this.pen.TryEvaluate(State.Session, out string PenId) &&
				this.Document.TryGetElement(PenId, out Element) &&
				Element is Pen Pen)
			{
				// TODO
			}

			if (this.background.TryEvaluate(State.Session, out string BackgroundId) &&
				this.Document.TryGetElement(BackgroundId, out Element) &&
				Element is Background Background)
			{
				// TODO
			}

			base.MeasureDimensions(State);

			if (!(FontBak is null))
				State.Font = FontBak;

			if (!(TextBak is null))
				State.Text = TextBak;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			SKFont FontBak = null;
			SKPaint TextBak = null;

			if (!(this.fontDef is null))
			{
				FontBak = State.Font;
				State.Font = this.fontDef.FontDef;

				TextBak = State.Text;
				State.Text = this.fontDef.Text;
			}

			base.MeasurePositions(State);

			if (!(FontBak is null))
				State.Font = FontBak;

			if (!(TextBak is null))
				State.Text = TextBak;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			SKFont FontBak = null;
			SKPaint TextBak = null;

			if (!(this.fontDef is null))
			{
				FontBak = State.Font;
				State.Font = this.fontDef.FontDef;

				TextBak = State.Text;
				State.Text = this.fontDef.Text;
			}

			base.Draw(State);

			if (!(FontBak is null))
				State.Font = FontBak;

			if (!(TextBak is null))
				State.Text = TextBak;
		}
	}
}
