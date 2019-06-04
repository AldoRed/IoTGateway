﻿using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Curve448 Montgomery Curve
	/// </summary>
	public class Curve448Endpoint : EllipticCurveEndpoint
    {
        /// <summary>
        /// Curve448 Montgomery Curve
        /// </summary>
        public Curve448Endpoint()
			: this(new Curve448())
		{
		}

        /// <summary>
        /// Curve448 Montgomery Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public Curve448Endpoint(Curve448 Curve)
			: base(Curve)
		{
		}

        /// <summary>
        /// Curve448 Montgomery Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public Curve448Endpoint(byte[] PublicKey)
			: base(PublicKey, new Curve448())
		{
		}

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public override string LocalName => "x448";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 224;

        /// <summary>
        /// If signatures are supported.
        /// </summary>
        public override bool SupportsSignatures => false;

        /// <summary>
        /// Creates a new key.
        /// </summary>
        /// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
        /// <returns>New E2E endpoint.</returns>
        public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new Curve448Endpoint();
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
		{
			return new Curve448Endpoint(new Curve448(Secret));
		}

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
		{
			return new Curve448Endpoint(PublicKey);
		}
	}
}
