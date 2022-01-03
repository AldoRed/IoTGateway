﻿using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Filters
{
	/// <summary>
	/// Shadow filter
	/// </summary>
	public class Shadow : LayoutContainer
	{
		private LengthAttribute dX;
		private LengthAttribute dY;
		private FloatAttribute sigmaX;
		private FloatAttribute sigmaY;
		private ColorAttribute color;

		/// <summary>
		/// Shadow filter
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Shadow(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Shadow";

		/// <summary>
		/// DX
		/// </summary>
		public LengthAttribute DXAttribute
		{
			get => this.dX;
			set => this.dX = value;
		}

		/// <summary>
		/// DY
		/// </summary>
		public LengthAttribute DYAttribute
		{
			get => this.dY;
			set => this.dY = value;
		}

		/// <summary>
		/// Sigma X
		/// </summary>
		public FloatAttribute SigmaXAttribute
		{
			get => this.sigmaX;
			set => this.sigmaX = value;
		}

		/// <summary>
		/// Sigma Y
		/// </summary>
		public FloatAttribute SigmaYAttribute
		{
			get => this.sigmaY;
			set => this.sigmaY = value;
		}

		/// <summary>
		/// Color
		/// </summary>
		public ColorAttribute ColorAttribute
		{
			get => this.color;
			set => this.color = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.dX = new LengthAttribute(Input, "dX");
			this.dY = new LengthAttribute(Input, "dY");
			this.sigmaX = new FloatAttribute(Input, "sigmaX");
			this.sigmaY = new FloatAttribute(Input, "sigmaY");
			this.color = new ColorAttribute(Input, "color");

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.dX?.Export(Output);
			this.dY?.Export(Output);
			this.sigmaX?.Export(Output);
			this.sigmaY?.Export(Output);
			this.color?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Shadow(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Shadow Dest)
			{
				Dest.dX = this.dX?.CopyIfNotPreset();
				Dest.dY = this.dY?.CopyIfNotPreset();
				Dest.sigmaX = this.sigmaX?.CopyIfNotPreset();
				Dest.sigmaY = this.sigmaY?.CopyIfNotPreset();
				Dest.color = this.color?.CopyIfNotPreset();
			}
		}
	}
}
