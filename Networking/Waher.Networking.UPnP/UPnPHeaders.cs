﻿using System;
using System.Collections.Generic;
using Waher.Content;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Direction of message
	/// </summary>
	public enum HttpDirection
	{
		/// <summary>
		/// Message is a request message.
		/// </summary>
		Request,

		/// <summary>
		/// Message is a response message.
		/// </summary>
		Response,

		/// <summary>
		/// Unknown HTTP message format.
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Class managing any HTTP headers in a UPnP UDP request/response.
	/// </summary>
	public class UPnPHeaders : IEnumerable<KeyValuePair<string, string>>, IHostReference
	{
		private readonly Dictionary<string, string> headers = new Dictionary<string, string>();
		private readonly HttpDirection direction = HttpDirection.Unknown;
		private readonly string[] rows;
		private readonly string searchTarget = string.Empty;
		private readonly string server = string.Empty;
		private readonly string location = string.Empty;
		private readonly string uniqueServiceName = string.Empty;
		private readonly string verb = string.Empty;
		private readonly string parameter = string.Empty;
		private readonly string responseMessage = string.Empty;
		private readonly string host = string.Empty;
		private readonly string cacheControl = string.Empty;
		private readonly string notificationType = string.Empty;
		private readonly string notificationSubType = string.Empty;
		private readonly double httpVersion = 0;
		private readonly int responseCode = 0;

		internal UPnPHeaders(string Header)
		{
			string s, Key, Value;
			int i, j, c;

			this.rows = Header.Split(CRLF, StringSplitOptions.RemoveEmptyEntries);
			c = this.rows.Length;

			if (c > 0)
			{
				string[] P = this.rows[0].Split(' ');
				if (P.Length == 3 && P[2].StartsWith("HTTP/"))
				{
					this.direction = HttpDirection.Request;
					this.verb = P[0];
					this.parameter = P[1];
					
					if (!double.TryParse(P[2].Substring(5).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out this.httpVersion))
						this.httpVersion = 0;
				}
				else if (P.Length >= 3 && P[0].StartsWith("HTTP/"))
				{
					this.direction = HttpDirection.Response;

					if (!double.TryParse(P[0].Substring(5).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out this.httpVersion))
						this.httpVersion = 0;

					if (!int.TryParse(P[1], out this.responseCode))
						this.responseCode = 0;

					this.responseMessage = null;
					for (i = 2; i < P.Length; i++)
					{
						if (this.responseMessage is null)
							this.responseMessage = P[i];
						else
							this.responseMessage += " " + P[i];
					}
				}
			}

			for (i = 1; i < c; i++)
			{
				s = rows[i];
				j = s.IndexOf(':');

				Key = s.Substring(0, j).ToUpper();
				Value = s.Substring(j + 1).TrimStart();

				this.headers[Key] = Value;

				switch (Key)
				{
					case "ST":
						this.searchTarget = Value;
						break;

					case "SERVER":
						this.server = Value;
						break;

					case "LOCATION":
						this.location = Value;
						break;

					case "USN":
						this.uniqueServiceName = Value;
						break;

					case "HOST":
						this.host = Value;
						break;

					case "CACHE-CONTROL":
						this.cacheControl = Value;
						break;

					case "NT":
						this.notificationType = Value;
						break;

					case "NTS":
						this.notificationSubType = Value;
						break;
				}
			}
		}

		/// <summary>
		/// CR or LF characters.
		/// </summary>
		internal static readonly char[] CRLF = new char[] { '\r', '\n' };
		
		/// <summary>
		/// Gets the value of the corresponding key. If the key is not found, the empty string is returned.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <returns>Value</returns>
		public string this[string Key]
		{
			get
			{
				if (this.headers.TryGetValue(Key, out string Value))
					return Value;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Message direction.
		/// </summary>
		public HttpDirection Direction => this.direction;

		/// <summary>
		/// HTTP Verb
		/// </summary>
		public string Verb => this.verb;

		/// <summary>
		/// HTTP Parameter
		/// </summary>
		public string Parameter => this.parameter;

		/// <summary>
		/// HTTP Version
		/// </summary>
		public double HttpVersion => this.httpVersion;

		/// <summary>
		/// Search Target header
		/// </summary>
		public string SearchTarget => this.searchTarget;

		/// <summary>
		/// Server header
		/// </summary>
		public string Server => this.server;

		/// <summary>
		/// Location header
		/// </summary>
		public string Location => this.location;

		/// <summary>
		/// Unique Service Name (USN) header
		/// </summary>
		public string UniqueServiceName => this.uniqueServiceName;

		/// <summary>
		/// Response message
		/// </summary>
		public string ResponseMessage => this.responseMessage;

		/// <summary>
		/// Host
		/// </summary>
		public string Host => this.host;

		/// <summary>
		/// Cache Control
		/// </summary>
		public string CacheControl => this.cacheControl;

		/// <summary>
		/// Notification Type
		/// </summary>
		public string NotificationType => this.notificationType;

		/// <summary>
		/// Notification Sub-type
		/// </summary>
		public string NotificationSubType => this.notificationSubType;

		/// <summary>
		/// Response Code
		/// </summary>
		public int ResponseCode => this.responseCode;

		/// <summary>
		/// Gets an enumerator, enumerating all headers.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return this.headers.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator, enumerating all headers.
		/// </summary>
		/// <returns>Enumerator</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.headers.GetEnumerator();
		}
	}
}
