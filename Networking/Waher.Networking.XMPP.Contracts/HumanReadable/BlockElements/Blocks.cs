﻿using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Abstract base class for sets of blocks.
	/// </summary>
	public abstract class Blocks : BlockElement
	{
		private BlockElement[] body;

		/// <summary>
		/// Body elements
		/// </summary>
		public BlockElement[] Body
		{
			get => this.body;
			set => this.body = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				if (this.body is null)
					return false;

				foreach (BlockElement E in this.body)
				{
					if (E is null || !E.IsWellDefined)
						return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override void GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			if (!(this.body is null))
			{
				foreach (HumanReadableElement E in this.body)
					E.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			}
		}
	}
}
