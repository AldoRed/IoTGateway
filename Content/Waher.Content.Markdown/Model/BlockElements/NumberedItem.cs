﻿using System;
using System.Text;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a numbered item in an ordered list.
	/// </summary>
	public class NumberedItem : BlockElementSingleChild
	{
		private readonly int number;

		/// <summary>
		/// Represents a numbered item in an ordered list.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Number">Number associated with item.</param>
		/// <param name="Child">Child element.</param>
		public NumberedItem(MarkdownDocument Document, int Number, MarkdownElement Child)
			: base(Document, Child)
		{
			this.number = Number;
		}

		/// <summary>
		/// Number associated with item.
		/// </summary>
		public int Number
		{
			get { return this.number; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			PrefixedBlock(Output, this.Child, "#.\t", "\t");
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			Output.Append("<li value=\"");
			Output.Append(this.number.ToString());
			Output.Append("\">");
			
			this.Child.GenerateHTML(Output);

			Output.AppendLine("</li>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			Output.Append(this.number.ToString());
			Output.Append(". ");

			StringBuilder sb = new StringBuilder();
			this.Child.GeneratePlainText(sb);

			string s = sb.ToString();

			Output.Append(s);

			if (!s.EndsWith("\n"))
				Output.AppendLine();
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			this.Child.GenerateXAML(Output, TextAlignment);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
			this.Child.GenerateXamarinForms(Output, TextAlignment);
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="Level">Current section level.</param>
		public override void GenerateSmartContractXml(XmlWriter Output, ref int Level)
		{
			Output.WriteStartElement("item");
			this.Child.GenerateSmartContractXml(Output, ref Level);
			Output.WriteEndElement();
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get
			{
				return this.Child.InlineSpanElement;
			}
		}

		/// <summary>
		/// Gets margins for content.
		/// </summary>
		/// <param name="TopMargin">Top margin.</param>
		/// <param name="BottomMargin">Bottom margin.</param>
		internal override void GetMargins(out int TopMargin, out int BottomMargin)
		{
			this.Child.GetMargins(out TopMargin, out BottomMargin);
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("NumberedItem");
			Output.WriteAttributeString("number", this.number.ToString());
			this.ExportChild(Output);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Child"/>.
		/// </summary>
		/// <param name="Child">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementSingleChild Create(MarkdownElement Child, MarkdownDocument Document)
		{
			return new NumberedItem(Document, this.number, Child);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is NumberedItem x &&
				x.number == this.number &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is NumberedItem x &&
				this.number == x.number &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.number.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
