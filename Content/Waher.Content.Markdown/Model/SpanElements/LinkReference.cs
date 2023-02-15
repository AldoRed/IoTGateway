﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Link reference
	/// </summary>
	public class LinkReference : MarkdownElementChildren
	{
		private readonly string label;

		/// <summary>
		/// Link reference
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Label">Link label.</param>
		public LinkReference(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements, string Label)
			: base(Document, ChildElements)
		{
			this.label = Label;
		}

		/// <summary>
		/// Link label
		/// </summary>
		public string Label => this.label;

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override async Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append('[');
			await base.GenerateMarkdown(Output);
			Output.Append("][");
			Output.Append(this.label);
			Output.Append(']');
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			Multimedia Multimedia = this.Document.GetReference(this.label);

			if (!(Multimedia is null))
				await Link.GenerateHTML(Output, Multimedia.Items[0].Url, Multimedia.Items[0].Title, this.Children, this.Document);
			else
			{
				foreach (MarkdownElement E in this.Children)
					await E.GenerateHTML(Output);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.label;
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			Multimedia Multimedia = this.Document.GetReference(this.label);

			if (!(Multimedia is null))
			{
				await Link.GenerateXAML(Output, TextAlignment, Multimedia.Items[0].Url, Multimedia.Items[0].Title, this.Children,
					this.Document);
			}
			else
			{
				foreach (MarkdownElement E in this.Children)
					await E.GenerateXAML(Output, TextAlignment);
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			Multimedia Multimedia = this.Document.GetReference(this.label);

			string Bak = State.Hyperlink;

			if (!(Multimedia is null))
				State.Hyperlink = Multimedia.Items[0].Url;

			foreach (MarkdownElement E in this.Children)
				await E.GenerateXamarinForms(Output, State);

			State.Hyperlink = Bak;
		}

		/// <summary>
		/// Generates LaTeX for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		public override async Task GenerateLaTeX(StringBuilder Output)
		{
			foreach (MarkdownElement E in this.Children)
				await E.GenerateLaTeX(Output);
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
			Output.WriteStartElement("LinkReference");
			Output.WriteAttributeString("label", this.label);
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
			return new LinkReference(Document, Children, this.label);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is LinkReference x &&
				x.label == this.label &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is LinkReference x &&
				this.label == x.label &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.label?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Multimedia Multimedia = this.Document.GetReference(this.label);

			if (!(Multimedia is null))
			{
				if (Statistics.IntUrlHyperlinks is null)
					Statistics.IntUrlHyperlinks = new List<string>();

				string Url = Multimedia.Items[0].Url;

				if (!Statistics.IntUrlHyperlinks.Contains(Url))
					Statistics.IntUrlHyperlinks.Add(Url);
			}

			Statistics.NrHyperLinks++;
			Statistics.NrUrlHyperLinks++;
		}

	}
}
