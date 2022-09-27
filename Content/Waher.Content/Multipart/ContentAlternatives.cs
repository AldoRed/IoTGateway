﻿namespace Waher.Content.Multipart
{
	/// <summary>
	/// Represents alternative versions of the same content, encoded with 
	/// multipart/alternative
	/// </summary>
	public class ContentAlternatives : MultipartContent
	{
		/// <summary>
		/// Represents mixed content, encoded with multipart/mixed
		/// </summary>
		/// <param name="Content">Embedded content.</param>
		public ContentAlternatives(EmbeddedContent[] Content)
			: base(Content)
		{
		}
	}
}
