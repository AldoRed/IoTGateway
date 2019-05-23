﻿using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Edwards25519 Twisted Edwards Curve
	/// </summary>
	public class Edwards25519Aes : EcAes256
    {
        /// <summary>
        /// Edwards25519 Twisted Edwards Curve
        /// </summary>
        public Edwards25519Aes()
			: this(new Edwards25519())
		{
		}

        /// <summary>
        /// Edwards25519 Twisted Edwards Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public Edwards25519Aes(Edwards25519 Curve)
			: base(Curve)
		{
		}

        /// <summary>
        /// Edwards25519 Twisted Edwards Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public Edwards25519Aes(byte[] PublicKey)
			: base(PublicKey, new Edwards25519())
		{
		}

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public override string LocalName => "ed25519";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 128;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new Edwards25519Aes();
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePrivate(byte[] Secret)
		{
			return new Edwards25519Aes(new Edwards25519(Secret));
		}

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePublic(byte[] PublicKey)
		{
			return new Edwards25519Aes(PublicKey);
		}
	}
}
