﻿using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The response to the request can be found under a different URI and SHOULD be retrieved using a GET method on that resource. This method exists 
	/// primarily to allow the output of a POST-activated script to redirect the user agent to a selected resource. The new URI is not a substitute 
	/// reference for the originally requested resource. The 303 response MUST NOT be cached, but the response to the second (redirected) request might 
	/// be cacheable. 
	/// </summary>
	public class SeeOtherException : HttpException
	{
		/// <summary>
		/// 303
		/// </summary>
		public const int Code = 303;

		/// <summary>
		/// See Other
		/// </summary>
		public const string StatusMessage = "See Other";

		/// <summary>
		/// The response to the request can be found under a different URI and SHOULD be retrieved using a GET method on that resource. This method exists 
		/// primarily to allow the output of a POST-activated script to redirect the user agent to a selected resource. The new URI is not a substitute 
		/// reference for the originally requested resource. The 303 response MUST NOT be cached, but the response to the second (redirected) request might 
		/// be cacheable. 
		/// </summary>
		/// <param name="Location">Location.</param>
		public SeeOtherException(string Location)
			: base(Code, StatusMessage, new KeyValuePair<string, string>("Location", Location))
		{
		}

		/// <summary>
		/// The response to the request can be found under a different URI and SHOULD be retrieved using a GET method on that resource. This method exists 
		/// primarily to allow the output of a POST-activated script to redirect the user agent to a selected resource. The new URI is not a substitute 
		/// reference for the originally requested resource. The 303 response MUST NOT be cached, but the response to the second (redirected) request might 
		/// be cacheable. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public SeeOtherException(string Location, object ContentObject)
			: base(Code, StatusMessage, ContentObject, new KeyValuePair<string, string>("Location", Location))
		{
		}

		/// <summary>
		/// The response to the request can be found under a different URI and SHOULD be retrieved using a GET method on that resource. This method exists 
		/// primarily to allow the output of a POST-activated script to redirect the user agent to a selected resource. The new URI is not a substitute 
		/// reference for the originally requested resource. The 303 response MUST NOT be cached, but the response to the second (redirected) request might 
		/// be cacheable. 
		/// </summary>
		/// <param name="Location">Location.</param>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public SeeOtherException(string Location, byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType, new KeyValuePair<string, string>("Location", Location))
		{
		}
	}
}
