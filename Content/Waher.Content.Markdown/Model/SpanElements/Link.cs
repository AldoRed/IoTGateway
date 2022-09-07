﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Link
	/// </summary>
	public class Link : MarkdownElementChildren
	{
		private readonly string url;
		private readonly string title;

		/// <summary>
		/// Link
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		public Link(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements, string Url, string Title)
			: base(Document, ChildElements)
		{
			this.url = Url;
			this.title = Title;
		}

		/// <summary>
		/// URL
		/// </summary>
		public string Url => this.url;

		/// <summary>
		/// Optional Link title.
		/// </summary>
		public string Title => this.title;

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('[');
			await base.GenerateMarkdown(Output);
			Output.Append("](");
			Output.Append(this.url);

			if (!string.IsNullOrEmpty(this.title))
			{
				Output.Append(" \"");
				Output.Append(this.title.Replace("\"", "\\\""));
				Output.Append('"');
			}

			Output.Append(')');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override Task GenerateHTML(StringBuilder Output)
		{
			return GenerateHTML(Output, this.url, this.title, this.Children, this.Document);
		}

		/// <summary>
		/// Generates HTML for a link.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="Document">Markdown document.</param>
		public static async Task GenerateHTML(StringBuilder Output, string Url, string Title, IEnumerable<MarkdownElement> ChildNodes,
			MarkdownDocument Document)
		{
			bool IsRelative = Url.IndexOf(':') < 0;

			if (!IsRelative && Url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
			{
				Output.Append("<a href=\"#\" onclick=\"");
				Output.Append(XML.HtmlAttributeEncode(System.Net.WebUtility.UrlDecode(Url.Substring(11))));
				IsRelative = true;
			}
			else
			{
				Output.Append("<a href=\"");
				Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Url, null)));
			}

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append("\" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Title));
			}

			if (!IsRelative)
				Output.Append("\" target=\"_blank");

			Output.Append("\">");

			foreach (MarkdownElement E in ChildNodes)
				await E.GenerateHTML(Output);

			Output.Append("</a>");
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.url;
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			return GenerateXAML(Output, TextAlignment, this.url, this.title, this.Children, this.Document);
		}

		/// <summary>
		/// Generates XAML for a link.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="Document">Markdown document.</param>
		public static async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, string Url, string Title,
			IEnumerable<MarkdownElement> ChildNodes, MarkdownDocument Document)
		{
			Output.WriteStartElement("Hyperlink");
			Output.WriteAttributeString("NavigateUri", Document.CheckURL(Url, null));

			if (!string.IsNullOrEmpty(Title))
				Output.WriteAttributeString("ToolTip", Title);

			foreach (MarkdownElement E in ChildNodes)
				await E.GenerateXAML(Output, TextAlignment);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			string Bak = State.Hyperlink;
			State.Hyperlink = this.url;

			foreach (MarkdownElement E in this.Children)
				await E.GenerateXamarinForms(Output, State);

			State.Hyperlink = Bak;
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => true;

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("Link");
			Output.WriteAttributeString("url", this.url);
			Output.WriteAttributeString("title", this.title);
			this.ExportChildren(Output);
			Output.WriteEndElement();
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
			return new Link(Document, Children, this.url, this.title);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Link x &&
				x.url == this.url &&
				x.title == this.title &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Link x &&
				this.url == x.url &&
				this.title == x.title &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.url?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.title?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrHyperLinks++;
			Statistics.NrUrlHyperLinks++;
		}

	}
}
