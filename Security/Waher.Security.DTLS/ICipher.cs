﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Interface for ciphers recognized by the DTLS class library.
	/// </summary>
    public interface ICipher : IDisposable
    {
		/// <summary>
		/// Cipher name.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// IANA cipher suite code:
		/// https://www.iana.org/assignments/tls-parameters/tls-parameters.xml#tls-parameters-4
		/// </summary>
		ushort IanaCipherSuite
		{
			get;
		}

		/// <summary>
		/// Priority. The higher the number, the higher priority.
		/// </summary>
		int Priority
		{
			get;
		}

		/// <summary>
		/// Master secret.
		/// </summary>
		byte[] MasterSecret
		{
			get;
			set;
		}

		/// <summary>
		/// Client random
		/// </summary>
		byte[] ClientRandom
		{
			get;
			set;
		}

		/// <summary>
		/// Server random
		/// </summary>
		byte[] ServerRandom
		{
			get;
			set;
		}

		/// <summary>
		/// If the endpoint where the cipher is being used, is a client endpoint (true),
		/// or a server endpoint (false).
		/// </summary>
		bool IsClient
		{
			get;
			set;
		}

		/// <summary>
		/// If the cipher can be used by the endpoint.
		/// </summary>
		/// <param name="State">Endpoint state.</param>
		/// <returns>If the cipher can be used.</returns>
		bool CanBeUsed(EndpointState State);

		/// <summary>
		/// Sends the Client Key Exchange message.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="State">Endpoint state.</param>
		void SendClientKeyExchange(DtlsEndpoint Endpoint, EndpointState State);

		/// <summary>
		/// Pseudo-random function for the cipher, as defined in §5 of RFC 5246:
		/// https://tools.ietf.org/html/rfc5246#section-5
		/// </summary>
		/// <param name="Secret">Secret</param>
		/// <param name="Label">Label</param>
		/// <param name="Seed">Seed</param>
		/// <param name="NrBytes">Number of bytes to generate.</param>
		byte[] PRF(byte[] Secret, string Label, byte[] Seed, uint NrBytes);

		/// <summary>
		/// Finishes the handshake.
		/// </summary>
		/// <param name="Endpoint">Endpoint.</param>
		/// <param name="Client">If the client acts as a client (true), or a server (false).</param>
		/// <param name="Handshake">Entire handshake communication.</param>
		/// <param name="State">Endpoint state.</param>
		void SendFinished(DtlsEndpoint Endpoint, bool Client, byte[] Handshake, EndpointState State);

		/// <summary>
		/// Encrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to encrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <returns>Encrypted data.</returns>
		byte[] Encrypt(byte[] Data, byte[] Header);

		/// <summary>
		/// Decrypts data according to the cipher settings.
		/// </summary>
		/// <param name="Data">Data to decrypt.</param>
		/// <param name="Header">Record header.</param>
		/// <returns>Decrypted data, or null if authentication failed.</returns>
		byte[] Decrypt(byte[] Data, byte[] Header);

		/// <summary>
		/// Allows the cipher to process any server key information sent by the DTLS server.
		/// </summary>
		/// <param name="Data">Binary data.</param>
		/// <param name="Offset">Offset where data begins.</param>
		void ServerKeyExchange(byte[] Data, ref int Offset);
	}
}
