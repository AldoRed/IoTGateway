﻿using System;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Interface for consolidators.
	/// </summary>
	public interface IConsolidator : IDisposable
	{
		/// <summary>
		/// Consolidated sources.
		/// </summary>
		Task<string[]> GetSources();

		/// <summary>
		/// External tag object that can be tagged to the object by its owner.
		/// </summary>
		object Tag
		{
			get;
			set;
		}

		/// <summary>
		/// Number of sources that have reported content.
		/// </summary>
		Task<int> GetNrReportedSources();

		/// <summary>
		/// Adds incoming markdown information.
		/// </summary>
		/// <param name="Source">Source of information.</param>
		/// <param name="Markdown">Markdown document.</param>
		/// <returns>If the source is new.</returns>
		Task<bool> Add(string Source, MarkdownDocument Markdown);

		/// <summary>
		/// Adds incoming markdown information.
		/// </summary>
		/// <param name="Source">Source of information.</param>
		/// <param name="Markdown">Markdown document.</param>
		/// <param name="Id">Optional ID of document.</param>
		/// <returns>If the source is new.</returns>
		Task<bool> Add(string Source, MarkdownDocument Markdown, string Id);

		/// <summary>
		/// Adds incoming markdown information.
		/// </summary>
		/// <param name="Source">Source of information.</param>
		/// <param name="Text">Text input.</param>
		/// <returns>If the source is new.</returns>
		Task<bool> Add(string Source, string Text);

		/// <summary>
		/// Adds incoming markdown information.
		/// </summary>
		/// <param name="Source">Source of information.</param>
		/// <param name="Text">Text input.</param>
		/// <param name="Id">Optional ID of document.</param>
		/// <returns>If the source is new.</returns>
		Task<bool> Add(string Source, string Text, string Id);

		/// <summary>
		/// Updates incoming markdown information.
		/// </summary>
		/// <param name="Source">Source of information.</param>
		/// <param name="Markdown">Markdown document.</param>
		/// <param name="Id">Optional ID of document.</param>
		/// <returns>If the source is new.</returns>
		Task<bool> Update(string Source, MarkdownDocument Markdown, string Id);

		/// <summary>
		/// Updates incoming markdown information.
		/// </summary>
		/// <param name="Source">Source of information.</param>
		/// <param name="Text">Text input.</param>
		/// <param name="Id">Optional ID of document.</param>
		/// <returns>If the source is new.</returns>
		Task<bool> Update(string Source, string Text, string Id);

		/// <summary>
		/// Event raised when content from a source has been added.
		/// </summary>
		event SourceEventHandler Added;

		/// <summary>
		/// Event raised when content from a source has been updated.
		/// </summary>
		event SourceEventHandler Updated;

		/// <summary>
		/// Event raised when consolidator has been disposed.
		/// </summary>
		event EventHandler Disposed;
	}
}
