﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Base class of Elliptic curves over a prime field.
	/// </summary>
	public abstract class CurvePrimeField : ModulusP
	{
		/// <summary>
		/// http://waher.se/Schema/EllipticCurves.xsd
		/// </summary>
		public const string Namespace = "http://waher.se/Schema/EllipticCurves.xsd";

		/// <summary>
		/// "EllipticCurve"
		/// </summary>
		public const string ElementName = "EllipticCurve";

		/// <summary>
		/// Random number generator
		/// </summary>
		protected static readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		private readonly ModulusP modN;
		private readonly PointOnCurve g;
		private readonly BigInteger a;
		private readonly BigInteger n;
		private readonly BigInteger maxScalarExclusive;
		private BigInteger d;
		private PointOnCurve publicKey;
		private readonly int scalarBits;
		private readonly int scalarBytes;
		private readonly byte msbMask;
		private readonly byte lsbMask;

		/// <summary>
		/// Base class of Elliptic curves over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">a Coefficient in the definition of the curve E:	y^2=x^3+a*x+b</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="OrderBits">Number of bits used to encode order.</param>
		public CurvePrimeField(BigInteger Prime, PointOnCurve BasePoint, BigInteger A,
			BigInteger Order, int OrderBits)
			: this(Prime, BasePoint, A, Order, OrderBits, null)
		{
		}

		/// <summary>
		/// Base class of Elliptic curves over a prime field.
		/// </summary>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="A">a Coefficient in the definition of the curve E:	y^2=x^3+a*x+b</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="D">Private key.</param>
		public CurvePrimeField(BigInteger Prime, PointOnCurve BasePoint, BigInteger A,
			BigInteger Order, int Cofactor, BigInteger? D)
			: base(Prime)
		{
			if (Prime <= BigInteger.One)
				throw new ArgumentException("Invalid prime base.", nameof(Prime));

			this.g = BasePoint;
			this.a = A;
			this.n = Order;
			this.maxScalarExclusive = Order * Cofactor;
			this.scalarBits = CalcBits(this.maxScalarExclusive);
			this.scalarBytes = (this.scalarBits + 7) >> 3;
			this.modN = new ModulusP(Order);

			this.msbMask = this.lsbMask = 0xff;
			int MaskBits = (8 - this.scalarBits) & 7;
			if (MaskBits == 0)
			{
				this.scalarBytes++;
				this.msbMask = 0;
			}
			else
				this.msbMask >>= MaskBits;

			int i = Cofactor;
			while (i > 1)
			{
				this.lsbMask <<= 1;
				i >>= 1;
			}

			if (D.HasValue)
			{
				this.publicKey = this.ScalarMultiplication(D.Value, this.g);
				this.d = D.Value;
			}
			else
				this.GenerateKeys();
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public abstract string CurveName
		{
			get;
		}

		/// <summary>
		/// Order of curve.
		/// </summary>
		public BigInteger Order => this.n;

		/// <summary>
		/// Base-point of curve.
		/// </summary>
		public PointOnCurve BasePoint => this.g;

		/// <summary>
		/// Prime of curve.
		/// </summary>
		public BigInteger Prime => this.p;

		/// <summary>
		/// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="P"/>.
		/// </summary>
		/// <param name="N">Scalar</param>
		/// <param name="P">Point</param>
		/// <returns><paramref name="N"/>*<paramref name="P"/></returns>
		public virtual PointOnCurve ScalarMultiplication(BigInteger N, PointOnCurve P)
		{
			PointOnCurve Result = PointOnCurve.Zero;
			byte[] Bin = N.ToByteArray();
			int i;
			int c = Math.Min(this.scalarBits, Bin.Length << 3);

			for (i = 0; i < c; i++)
			{
				if ((Bin[i >> 3] & (1 << (i & 7))) != 0)
					this.AddTo(ref Result, P);

				this.Double(ref P);
			}

			return Result;
		}

		/// <summary>
		/// Negates a point on the curve.
		/// </summary>
		/// <param name="P">Point</param>
		public void Negate(ref PointOnCurve P)
		{
			P.Y = this.p - P.Y;
		}

		/// <summary>
		/// Adds <paramref name="Q"/> to <paramref name="P"/>.
		/// </summary>
		/// <param name="P">Point 1.</param>
		/// <param name="Q">Point 2.</param>
		/// <returns>P+Q</returns>
		public void AddTo(ref PointOnCurve P, PointOnCurve Q)
		{
			if (P.IsZero)
			{
				P.X = Q.X;
				P.Y = Q.Y;
				P.IsZero = Q.IsZero;
			}
			else if (!Q.IsZero)
			{
				BigInteger sDividend = this.Subtract(P.Y, Q.Y);
				BigInteger sDivisor = this.Subtract(P.X, Q.X);
				BigInteger s, xR, yR;

				if (sDivisor.IsZero)
				{
					if (sDividend.IsZero)   // P=Q
						this.Double(ref P);
					else
					{
						P.X = BigInteger.Zero;
						P.Y = BigInteger.Zero;
						P.IsZero = true;
					}
				}
				else
				{
					s = this.Divide(sDividend, sDivisor);
					xR = this.Subtract(this.Multiply(s, s), this.Add(P.X, Q.X));
					yR = this.Add(P.Y, this.Multiply(s, this.Subtract(xR, P.X)));

					P.X = xR;
					P.Y = this.p - yR;
				}
			}
		}

		/// <summary>
		/// Doubles a point on the curve.
		/// </summary>
		/// <param name="P">Point</param>
		public void Double(ref PointOnCurve P)
		{
			if (!P.IsZero)
			{
				BigInteger sDividend = this.Add(3 * this.Multiply(P.X, P.X), this.a);
				BigInteger sDivisor = this.Multiply(Two, P.Y);

				BigInteger s = this.Divide(sDividend, sDivisor);
				BigInteger xR = this.Subtract(this.Multiply(s, s), this.Add(P.X, P.X));
				BigInteger yR = this.Add(P.Y, this.Multiply(s, this.Subtract(xR, P.X)));

				P.X = xR;
				P.Y = this.p - yR;
			}
		}

		/// <summary>
		/// Generates a new Private Key.
		/// </summary>
		public void GenerateKeys()
		{
			BigInteger D = this.NextRandomNumber();
			this.publicKey = this.ScalarMultiplication(D, this.g);
			this.d = D;
		}

		/// <summary>
		/// Sets the private key (and therefore also the public key) of the curve.
		/// </summary>
		/// <param name="D">Private key.</param>
		public void SetPrivateKey(BigInteger D)
		{
			if (D <= BigInteger.Zero || D >= this.n)
				throw new ArgumentException("Invalid private key.", nameof(D));

			this.publicKey = this.ScalarMultiplication(D, this.g);
			this.d = D;
		}

		/// <summary>
		/// Returns the next random number, in the range [1, n-1].
		/// </summary>
		/// <returns>Random number.</returns>
		public virtual BigInteger NextRandomNumber()
		{
			byte[] B = new byte[this.scalarBytes];
			BigInteger D;

			do
			{
				lock (rnd)
				{
					rnd.GetBytes(B);
				}

				B[this.scalarBytes - 1] &= this.msbMask;
				B[0] &= this.lsbMask;

				D = new BigInteger(B);
			}
			while (D.IsZero || D >= this.maxScalarExclusive);

			return D;
		}

		/// <summary>
		/// Converts a sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>.
		/// </summary>
		/// <param name="BigEndianDWords">Sequence of unsigned 32-bit integers to a <see cref="BigInteger"/>, most significant word first.</param>
		/// <returns><see cref="BigInteger"/> value.</returns>
		protected static BigInteger ToBigInteger(uint[] BigEndianDWords)
		{
			int i, c = BigEndianDWords.Length;
			int j = c << 2;
			byte[] B = new byte[((BigEndianDWords[0] & 0x80000000) != 0) ? j + 1 : j];
			uint k;

			for (i = 0; i < c; i++)
			{
				k = BigEndianDWords[i];

				B[j - 4] = (byte)k;
				k >>= 8;
				B[j - 3] = (byte)k;
				k >>= 8;
				B[j - 2] = (byte)k;
				k >>= 8;
				B[j - 1] = (byte)k;

				j -= 4;
			}

			return new BigInteger(B);
		}

		/// <summary>
		/// Public key.
		/// </summary>
		public PointOnCurve PublicKey => this.publicKey;

		/// <summary>
		/// Gets a shared key using the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
		/// </summary>
		/// <param name="RemotePublicKey">Public key of the remote party.</param>
		/// <param name="HashFunction">A Hash function is applied to the derived key to generate the shared secret.
		/// The derived key, as a byte array of equal size as the order of the prime field, ordered by most significant byte first,
		/// is passed on to the hash function before being returned as the shared key.</param>
		/// <returns>Shared secret.</returns>
		public byte[] GetSharedKey(PointOnCurve RemotePublicKey, HashFunction HashFunction)
		{
			PointOnCurve P = this.ScalarMultiplication(this.d, RemotePublicKey);
			byte[] B = P.X.ToByteArray();

			if (B.Length != this.scalarBytes)
				Array.Resize<byte>(ref B, this.scalarBytes);

			Array.Reverse(B);   // Most significant byte first.

			return Hashes.ComputeHash(HashFunction, B);
		}

		/// <summary>
		/// Creates a signature of <paramref name="Data"/> using the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="HashFunction">Hash function to use.</param>
		/// <returns>Signature.</returns>
		public KeyValuePair<BigInteger, BigInteger> Sign(byte[] Data, HashFunction HashFunction)
		{
			BigInteger e = this.CalcE(Data, HashFunction);
			BigInteger r, k, s;
			PointOnCurve P1;

			do
			{
				do
				{
					k = this.NextRandomNumber();
					P1 = this.ScalarMultiplication(k, this.g);
				}
				while (P1.IsXZero);

				r = BigInteger.Remainder(P1.X, this.n);
				s = this.modN.Divide(this.modN.Add(e, this.modN.Multiply(r, this.d)), k);
			}
			while (s.IsZero);

			return new KeyValuePair<BigInteger, BigInteger>(r, s);
		}

		private BigInteger CalcE(byte[] Data, HashFunction HashFunction)
		{
			byte[] Hash = Hashes.ComputeHash(HashFunction, Data);
			int c = Hash.Length;

			if (c != this.scalarBytes)
				Array.Resize<byte>(ref Hash, this.scalarBytes);

			Hash[this.scalarBytes - 1] &= this.msbMask;

			return new BigInteger(Hash);
		}

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/> made by the ECDSA algorithm.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="HashFunction">Hash function to use.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		public bool Verify(byte[] Data, PointOnCurve PublicKey, HashFunction HashFunction,
			KeyValuePair<BigInteger, BigInteger> Signature)
		{
			if (PublicKey.IsZero ||
				Signature.Key.IsZero ||
				Signature.Value.IsZero ||
				Signature.Key >= this.n ||
				Signature.Value >= this.n)
			{
				return false;
			}

			BigInteger e = this.CalcE(Data, HashFunction);
			BigInteger r = Signature.Key;
			BigInteger s = Signature.Value;
			BigInteger w = this.modN.Invert(s);
			BigInteger u1 = this.modN.Multiply(e, w);
			BigInteger u2 = this.modN.Multiply(r, w);
			PointOnCurve P2 = this.ScalarMultiplication(u1, this.g);
			PointOnCurve P3 = this.ScalarMultiplication(u2, PublicKey);
			this.AddTo(ref P2, P3);

			if (P2.IsZero)
				return false;

			return BigInteger.Remainder(P2.X, this.n) == r;
		}

		/// <summary>
		/// Exports the curve parameters to XML.
		/// </summary>
		/// <param name="Output">Output</param>
		public virtual void Export(XmlWriter Output)
		{
			Output.WriteStartElement("EllipticCurve", Namespace);
			Output.WriteAttributeString("type", this.GetType().FullName);
			Output.WriteAttributeString("d", this.d.ToString());
			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports the curve parameters to an XML string.
		/// </summary>
		public string Export()
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Indent = false,
				OmitXmlDeclaration = true
			};
			StringBuilder sb = new StringBuilder();
			using (XmlWriter w = XmlWriter.Create(sb, Settings))
			{
				this.Export(w);
				w.Flush();
			}

			return sb.ToString();
		}

	}
}
