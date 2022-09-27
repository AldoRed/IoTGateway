﻿namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
	/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
	/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
	/// (Not Found) can be used instead. 
	/// </summary>
	public class ForbiddenException : HttpException
	{
		/// <summary>
		/// 403
		/// </summary>
		public const int Code = 403;

		/// <summary>
		/// Forbidden
		/// </summary>
		public const string StatusMessage = "Forbidden";

		/// <summary>
		/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
		/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
		/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
		/// (Not Found) can be used instead. 
		/// </summary>
		public ForbiddenException()
			: base(Code, StatusMessage)
		{
		}

		/// <summary>
		/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
		/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
		/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
		/// (Not Found) can be used instead. 
		/// </summary>
		/// <param name="ContentObject">Any content object to return. The object will be encoded before being sent.</param>
		public ForbiddenException(object ContentObject)
			: base(Code, StatusMessage, ContentObject)
		{
		}

		/// <summary>
		/// The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated. 
		/// If the request method was not HEAD and the server wishes to make public why the request has not been fulfilled, it SHOULD describe the 
		/// reason for the refusal in the entity. If the server does not wish to make this information available to the client, the status code 404 
		/// (Not Found) can be used instead. 
		/// </summary>
		/// <param name="Content">Any encoded content to return.</param>
		/// <param name="ContentType">The content type of <paramref name="Content"/>, if provided.</param>
		public ForbiddenException(byte[] Content, string ContentType)
			: base(Code, StatusMessage, Content, ContentType)
		{
		}
	}
}
