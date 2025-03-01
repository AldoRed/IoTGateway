﻿using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.WebSocket
{
	/// <summary>
	/// Implements a Web-socket XMPP protocol, as defined in RFC 7395.
	/// https://tools.ietf.org/html/rfc7395
	/// </summary>
	public class WebSocketBinding : AlternativeTransport
	{
		/// <summary>
		/// urn:ietf:params:xml:ns:xmpp-framing
		/// </summary>
		public const string FramingNamespace = "urn:ietf:params:xml:ns:xmpp-framing";

		private readonly LinkedList<KeyValuePair<string, EventHandlerAsync<DeliveryEventArgs>>> queue = new LinkedList<KeyValuePair<string, EventHandlerAsync<DeliveryEventArgs>>>();
		private XmppClient xmppClient;
		private XmppBindingInterface bindingInterface;
		private ClientWebSocket webSocketClient;
		private ArraySegment<byte> inputBuffer;
		private Uri url;
		private string to;
		private string from;
		private string language;
		private double version = 0;
		private bool terminated = false;
		private bool writing = false;
		private bool closeSent = false;
		private bool disposed = false;
		private bool reading = false;

		/// <summary>
		/// Implements a Web-socket XMPP protocol, as defined in RFC 7395.
		/// https://tools.ietf.org/html/rfc7395
		/// </summary>
		public WebSocketBinding()
		{
		}

		/// <summary>
		/// If the alternative binding mechanism handles heartbeats.
		/// </summary>
		public override bool HandlesHeartbeats => true;

		/// <summary>
		/// How well the alternative transport handles the XMPP credentials provided.
		/// </summary>
		/// <param name="URI">URI defining endpoint.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Uri URI)
		{
			switch (URI.Scheme.ToLower())
			{
				case "ws":
				case "wss":
					return Grade.Ok;

				default:
					return Grade.NotAtAll;
			}
		}

		/// <summary>
		/// Instantiates a new alternative connections.
		/// </summary>
		/// <param name="URI">URI defining endpoint.</param>
		/// <param name="Client">XMPP Client</param>
		/// <param name="BindingInterface">Inteface to internal properties of the <see cref="XmppClient"/>.</param>
		/// <returns>Instantiated binding.</returns>
		public override IAlternativeTransport Instantiate(Uri URI, XmppClient Client, XmppBindingInterface BindingInterface)
		{
			return new WebSocketBinding()
			{
				bindingInterface = BindingInterface,
				url = URI,
				xmppClient = Client,
				inputBuffer = new ArraySegment<byte>(new byte[65536])
			};
		}

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public override event TextEventHandler OnSent;

		/// <summary>
		/// Event received when text data has been received.
		/// </summary>
		public override event TextEventHandler OnReceived;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			this.disposed = true;
			this.terminated = true;
			this.xmppClient = null;

			this.webSocketClient?.Dispose();
			this.webSocketClient = null;
		}

		private async Task<bool> RaiseOnSent(string Payload)
		{
			TextEventHandler h = this.OnSent;
			bool Result = true;

			if (!(h is null))
			{
				try
				{
					Result = await h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Result;
		}

		private async Task<bool> RaiseOnReceived(string Payload)
		{
			TextEventHandler h = this.OnReceived;
			bool Result = true;

			if (!(h is null))
			{
				try
				{
					Result = await h(this, Payload);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return Result;
		}

		/// <summary>
		/// Creates a Web-socket session.
		/// </summary>
		public override async void CreateSession()
		{
			try
			{
				lock (this.queue)
				{
					this.terminated = false;
					this.writing = false;
					this.closeSent = false;
					this.queue.Clear();

					this.webSocketClient?.Dispose();
					this.webSocketClient = null;

					this.webSocketClient = new ClientWebSocket();
					this.webSocketClient.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
				}

				await this.webSocketClient.ConnectAsync(this.url, CancellationToken.None);

				// TODO: this.xmppClient.TrustServer

				if (this.xmppClient.HasSniffers)
					await this.xmppClient.Information("Initiating session.");

				await this.SendAsync("<?");

				string XmlResponse = await this.ReadText();

				XmlDocument ResponseXml = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				ResponseXml.LoadXml(XmlResponse);

				XmlElement Open;

				if ((Open = ResponseXml.DocumentElement) is null || Open.LocalName != "open" ||
					Open.NamespaceURI != FramingNamespace)
				{
					throw new Exception("Unexpected response returned.");
				}

				string StreamPrefix = "stream";
				LinkedList<KeyValuePair<string, string>> Namespaces = null;

				this.to = null;
				this.from = null;
				this.version = 0;
				this.language = null;

				foreach (XmlAttribute Attr in Open.Attributes)
				{
					switch (Attr.Name)
					{
						case "version":
							if (!CommonTypes.TryParse(Attr.Value, out this.version))
								throw new Exception("Invalid version number.");
							break;

						case "to":
							this.to = Attr.Value;
							break;

						case "from":
							this.from = Attr.Value;
							break;

						case "xml:lang":
							this.language = Attr.Value;
							break;

						default:
							if (Attr.Prefix == "xmlns")
							{
								if (Attr.Value == XmppClient.NamespaceStream)
									StreamPrefix = Attr.LocalName;
								else
								{
									if (Namespaces is null)
										Namespaces = new LinkedList<KeyValuePair<string, string>>();

									Namespaces.AddLast(new KeyValuePair<string, string>(Attr.Prefix, Attr.Value));
								}
							}
							break;
					}
				}

				StringBuilder sb = new StringBuilder();

				sb.Append('<');
				sb.Append(StreamPrefix);
				sb.Append(":stream xmlns:");
				sb.Append(StreamPrefix);
				sb.Append("='");
				sb.Append(XmppClient.NamespaceStream);

				if (!(this.to is null))
				{
					sb.Append("' to='");
					sb.Append(XML.Encode(this.to));
				}

				if (!(this.from is null))
				{
					sb.Append("' from='");
					sb.Append(XML.Encode(this.from));
				}

				if (this.version > 0)
				{
					sb.Append("' version='");
					sb.Append(CommonTypes.Encode(this.version));
				}

				if (!(this.language is null))
				{
					sb.Append("' xml:lang='");
					sb.Append(XML.Encode(this.language));
				}

				sb.Append("' xmlns='");
				sb.Append(XmppClient.NamespaceClient);

				if (!(Namespaces is null))
				{
					foreach (KeyValuePair<string, string> P in Namespaces)
					{
						sb.Append("' xmlns:");
						sb.Append(P.Key);
						sb.Append("='");
						sb.Append(XML.Encode(P.Value));
					}
				}

				sb.Append("'>");

				this.bindingInterface.StreamHeader = sb.ToString();

				sb.Clear();
				sb.Append("</");
				sb.Append(StreamPrefix);
				sb.Append(":stream>");

				this.bindingInterface.StreamFooter = sb.ToString();

				this.StartReading();
			}
			catch (Exception ex)
			{
				await this.bindingInterface.ConnectionError(ex);
			}
		}

		private async void StartReading()
		{
			if (this.reading)
				throw new InvalidOperationException("Already in a reading state.");

			this.reading = true;

			try
			{
				while (!this.terminated)
				{
					string Xml = await this.ReadText();

					if (!await this.FragmentReceived(Xml))
						break;
				}
			}
			catch (Exception ex)
			{
				await this.bindingInterface.ConnectionError(ex);
			}
			finally
			{
				this.reading = false;
			}
		}

		/// <summary>
		/// If reading has been paused.
		/// </summary>
		public override bool Paused => !this.reading && !this.disposed;

		/// <summary>
		/// Continues a paused connection.
		/// </summary>
		public override void Continue()
		{
			this.StartReading();
		}

		/// <summary>
		/// Closes a session.
		/// </summary>
		public override void CloseSession()
		{
			if (this.webSocketClient is null)
				this.terminated = true;
			else
			{
				try
				{
					if (!this.closeSent && this.webSocketClient.State == WebSocketState.Open)
					{
						this.closeSent = true;

						Task _ = this.SendAsync("<close xmlns=\"urn:ietf:params:xml:ns:xmpp-framing\"/>", async (Sender, e) =>
						{
							this.terminated = true;

							try
							{
								if (this.webSocketClient.State == WebSocketState.Open)
									await this.webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

								this.webSocketClient.Dispose();
								this.webSocketClient = null;
							}
							catch (Exception)
							{
								this.webSocketClient = null;
							}
						}, null);
					}
					else
					{
						this.terminated = true;

						this.webSocketClient.Dispose();
						this.webSocketClient = null;
					}
				}
				catch (Exception)
				{
					// Ignore.
				}
			}
		}

		private async Task<string> ReadText()
		{
			if (this.webSocketClient is null)
				throw new Exception("No web socket client available.");

			WebSocketReceiveResult Response = await this.webSocketClient.ReceiveAsync(this.inputBuffer, CancellationToken.None);
			if (Response is null)
				return string.Empty;

			this.AssureText(Response);

			int Count = Response.Count;
			if (Count == 0)
				return string.Empty;

			string s = Encoding.UTF8.GetString(this.inputBuffer.Array, 0, Count);

			if (this.xmppClient.HasSniffers)
				await this.xmppClient.ReceiveText(s);

			if (Response.EndOfMessage)
				return s;

			StringBuilder sb = new StringBuilder(s);

			do
			{
				Response = await this.webSocketClient.ReceiveAsync(this.inputBuffer, CancellationToken.None);
				this.AssureText(Response);

				Count = Response.Count;
				s = Encoding.UTF8.GetString(this.inputBuffer.Array, 0, Count);
				sb.Append(s);

				if (this.xmppClient.HasSniffers)
					await this.xmppClient.ReceiveText(s);
			}
			while (!Response.EndOfMessage && !this.disposed);

			return sb.ToString();
		}

		private void AssureText(WebSocketReceiveResult Response)
		{
			if (Response.CloseStatus.HasValue)
				throw new Exception("Web-socket connection closed. Code: " + Response.CloseStatus.Value.ToString() + ", Description: " + Response.CloseStatusDescription);

			if (Response.MessageType != WebSocketMessageType.Text)
				throw new Exception("Expected text.");
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		public override Task<bool> SendAsync(string Packet)
		{
			return this.SendAsync(Packet, null, null);
		}

		/// <summary>
		/// Sends a text packet.
		/// </summary>
		/// <param name="Packet">Text packet.</param>
		/// <param name="DeliveryCallback">Optional method to call when packet has been delivered.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public override async Task<bool> SendAsync(string Packet, EventHandlerAsync<DeliveryEventArgs> DeliveryCallback, object State)
		{
			if (this.terminated)
				return false;

			if (Packet is null)
				throw new ArgumentException("Null payloads not allowed.", nameof(Packet));

			if (Packet.StartsWith("<?"))
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<open xmlns=\"");
				Xml.Append(FramingNamespace);
				Xml.Append("\" to=\"");
				Xml.Append(this.xmppClient.Domain);
				Xml.Append("\" xml:lang=\"en\" version=\"1.0\"/>");

				Packet = Xml.ToString();
			}

			lock (this.queue)
			{
				if (this.writing)
				{
					if (this.xmppClient?.HasSniffers ?? false)
					{
						Task.Run(() =>
						{
							return this.xmppClient.Information("Outbound stanza queued.");
						});
					}

					this.queue.AddLast(new KeyValuePair<string, EventHandlerAsync<DeliveryEventArgs>>(Packet, DeliveryCallback));
					return true;
				}
				else
					this.writing = true;
			}

			try
			{
				bool HasSniffers = this.xmppClient.HasSniffers;

				while (!(Packet is null) && !this.disposed)
				{
					if (HasSniffers && !(this.xmppClient is null))
						await this.xmppClient.TransmitText(Packet);

					ArraySegment<byte> Buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(Packet));

					if (this.webSocketClient is null)
						throw new Exception("No web socket client available.");

					await this.webSocketClient.SendAsync(Buffer, WebSocketMessageType.Text, true, CancellationToken.None);

					this.bindingInterface.NextPing = DateTime.Now.AddMinutes(1);

					await this.RaiseOnSent(Packet);

					await DeliveryCallback.Raise(this.xmppClient, new DeliveryEventArgs(State, true));

					lock (this.queue)
					{
						if (!(this.queue.First is null))
						{
							LinkedListNode<KeyValuePair<string, EventHandlerAsync<DeliveryEventArgs>>> Node = this.queue.First;
							Packet = Node.Value.Key;
							DeliveryCallback = Node.Value.Value;
							this.queue.RemoveFirst();
						}
						else
						{
							Packet = null;
							DeliveryCallback = null;
							this.writing = false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				lock (this.queue)
				{
					this.writing = false;
					this.queue.Clear();
				}

				await this.bindingInterface.ConnectionError(ex);
			}

			return true;
		}

		private Task<bool> FragmentReceived(string Xml)
		{
			if (this.terminated)
				return Task.FromResult(false);

			if (Xml.StartsWith("<close"))
			{
				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.LoadXml(Xml);

				if (!(Doc.DocumentElement is null) && Doc.DocumentElement.LocalName == "close" && Doc.DocumentElement.NamespaceURI == FramingNamespace)
				{
					if (!this.closeSent)
						this.CloseSession();

					return Task.FromResult(false);
				}
			}

			return this.RaiseOnReceived(Xml);
		}
	}
}
