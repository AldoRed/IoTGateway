﻿using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for angle elements.
	/// </summary>
	public abstract class Angle : LayoutElement
	{
		private FloatAttribute degrees;

		/// <summary>
		/// Abstract base class for distance elements.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Angle(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Degrees
		/// </summary>
		public FloatAttribute DegreesAttribute
		{
			get => this.degrees;
			set => this.degrees = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.degrees = new FloatAttribute(Input, "degrees");
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.degrees?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Angle Dest)
				Dest.degrees = this.degrees?.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			EvaluationResult<float> Degrees = await this.degrees.TryEvaluate(State.Session);
			this.angle = Degrees.Result;
			this.defined &= Degrees.Ok;
		}

		/// <summary>
		/// Measured distance
		/// </summary>
		protected float angle;

	}
}
