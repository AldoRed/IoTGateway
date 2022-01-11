﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents inserted blocks in a markdown document.
	/// </summary>
	public class DeleteBlocks : BlockElementChildren
	{
		/// <summary>
		/// Represents inserted blocks in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Children">Child elements.</param>
		public DeleteBlocks(MarkdownDocument Document, IEnumerable<MarkdownElement> Children)
			: base(Document, Children)
		{
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			await PrefixedBlock(Output, this.Children, "->\t");
			Output.AppendLine();
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			Output.AppendLine("<blockquote class=\"deleted\">");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateHTML(Output);

			Output.AppendLine("</blockquote>");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			StringBuilder sb = new StringBuilder();

			foreach (MarkdownElement E in this.Children)
				await E.GeneratePlainText(sb);

			string s = sb.ToString().Trim();
			s = s.Replace("\r\n", "\n").Replace("\r", "\n");

			foreach (string Row in s.Split('\n'))
			{
				Output.Append("-> ");
				Output.AppendLine(Row);
			}

			Output.AppendLine();
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;

			Output.WriteStartElement("Border");
			Output.WriteAttributeString("BorderThickness", Settings.BlockQuoteBorderThickness.ToString() + ",0,0,0");
			Output.WriteAttributeString("Margin", Settings.BlockQuoteMargin.ToString() + "," + Settings.ParagraphMarginTop.ToString() + ",0," +
				Settings.ParagraphMarginBottom.ToString());
			Output.WriteAttributeString("Padding", Settings.BlockQuotePadding.ToString() + ",0,0,0");
			Output.WriteAttributeString("BorderBrush", Settings.DeletedBlockQuoteBorderColor);
			Output.WriteStartElement("StackPanel");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateXAML(Output, TextAlignment);

			Output.WriteEndElement();
			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			XamlSettings Settings = this.Document.Settings.XamlSettings;

			Output.WriteStartElement("ContentView");
			Output.WriteAttributeString("Padding", Settings.BlockQuoteMargin.ToString() + "," + Settings.ParagraphMarginTop.ToString() + ",0," +
				Settings.ParagraphMarginBottom.ToString());

			Output.WriteStartElement("Frame");
			Output.WriteAttributeString("Padding", Settings.BlockQuotePadding.ToString() +
				",0," + Settings.BlockQuotePadding.ToString() + ",0");
			Output.WriteAttributeString("BorderColor", Settings.DeletedBlockQuoteBorderColor);
			// TODO: Border thickness

			Output.WriteStartElement("StackLayout");
			Output.WriteAttributeString("Orientation", "Vertical");

			foreach (MarkdownElement E in this.Children)
				await E.GenerateXamarinForms(Output, State);

			Output.WriteEndElement();
			Output.WriteEndElement();
			Output.WriteEndElement();
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => false;

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			this.Export(Output, "DeleteBlocks");
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new DeleteBlocks(Document, Children);
		}
	}
}
