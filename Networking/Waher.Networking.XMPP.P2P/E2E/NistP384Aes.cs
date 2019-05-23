﻿using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// NIST P-384 Curve
	/// </summary>
	public class NistP384Aes : EcAes256
	{
		/// <summary>
		/// NIST P-384 Curve
		/// </summary>
		public NistP384Aes()
			: this(new NistP384())
		{
		}

		/// <summary>
		/// NIST P-384 Curve
		/// </summary>
		/// <param name="Curve">Curve instance</param>
		public NistP384Aes(NistP384 Curve)
			: base(Curve)
		{
		}

        /// <summary>
        /// NIST P-384 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public NistP384Aes(byte[] PublicKey)
            : base(PublicKey, new NistP384())
        {
        }

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public override string LocalName => "p384";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 192;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new NistP384Aes();
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePrivate(byte[] Secret)
        {
            return new NistP384Aes(new NistP384(Secret));
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePublic(byte[] PublicKey)
        {
            return new NistP384Aes(PublicKey);
        }
    }
}
