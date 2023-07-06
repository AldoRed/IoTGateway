﻿using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
	/// </summary>
	public class MisdirectedRequestException : HttpException
	{
		/// <summary>
		/// 421
		/// </summary>
		public const int Code = 421;

		/// <summary>
		/// Misdirected Request
		/// </summary>
		public const string StatusMessage = "Misdirected Request";

		/// <summary>
		/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
		/// </summary>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MisdirectedRequestException(params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, HeaderFields)
		{
		}

		/// <summary>
		/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MisdirectedRequestException(object ContentObject, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, ContentObject, HeaderFields)
		{
		}

		/// <summary>
		/// The request was directed at a server that is not able to produce a response (for example because a connection reuse).
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		/// <param name="HeaderFields">HTTP Header fields to include in the response.</param>
		public MisdirectedRequestException(byte[] Content, string ContentType, params KeyValuePair<string, string>[] HeaderFields)
			: base(Code, StatusMessage, Content, ContentType, HeaderFields)
		{
		}
	}
}
