﻿using System;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Curve448 (Goldilocks), as defined in RFC 7748:
    /// https://tools.ietf.org/html/rfc7748
    /// </summary>
    public class Curve448 : MontgomeryCurve
    {
        private static readonly BigInteger p0 = BigInteger.Pow(2, 448) - BigInteger.Pow(2, 224) - 1;
        private static readonly BigInteger A0 = 156326;
        private static readonly BigInteger A24 = (A0 - 2) / 4;
        private static readonly BigInteger n0 = BigInteger.Pow(2, 446) - BigInteger.Parse("008335dc163bb124b65129c96fde933d8d723a70aadc873d6d54a7bb0d", NumberStyles.HexNumber);
        private static readonly BigInteger BasePointU = 5;
        private static readonly BigInteger BasePointV = BigInteger.Parse("355293926785568175264127502063783334808976399387714271831880898435169088786967410002932673765864550910142774147268105838985595290606362");

        /// <summary>
        /// Curve448 (Goldilocks), as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        public Curve448()
            : base(p0, new PointOnCurve(BasePointU, BasePointV), n0, 4)
        {
        }

        /// <summary>
        /// Curve448 (Goldilocks), as defined in RFC 7748:
        /// https://tools.ietf.org/html/rfc7748
        /// </summary>
        /// <param name="Secret">Secret.</param>
        public Curve448(byte[] Secret)
            : base(p0, new PointOnCurve(BasePointU, BasePointV), n0, 4, Secret)
        {
        }

        /// <summary>
        /// Name of curve.
        /// </summary>
        public override string CurveName => "Curve448";

        /// <summary>
        /// a Coefficient in the definition of the curve E:	v²=u³+A*u²+u
        /// </summary>
        protected override BigInteger A => A0;

        /// <summary>
        /// Converts a pair of (U,V) coordinates to a pair of (X,Y) coordinates
        /// in the birational Edwards curve.
        /// </summary>
        /// <param name="UV">(U,V) coordinates.</param>
        /// <returns>(X,Y) coordinates.</returns>
        public override PointOnCurve ToXY(PointOnCurve UV)
        {
            BigInteger U2 = this.modP.Multiply(UV.X, UV.X);
            BigInteger U3 = this.modP.Multiply(U2, UV.X);
            BigInteger U4 = this.modP.Multiply(U2, U2);
            BigInteger U5 = this.modP.Multiply(U3, U2);
            BigInteger V2 = this.modP.Multiply(UV.Y, UV.Y);

            BigInteger TwoU2 = U2 << 1;
            BigInteger TwoU3 = U3 << 1;

            BigInteger X = this.modP.Divide(this.modP.Multiply(UV.Y << 2, U2 - 1),
                (U4 - TwoU2 + (V2 << 2) + BigInteger.One));

            BigInteger Y = this.modP.Divide(-(U5 - TwoU3 -
                this.modP.Multiply((UV.X << 2), V2) + UV.X),
               (U5 - this.modP.Multiply(TwoU2, V2) - TwoU3 - (V2 << 1) + UV.X));

            if (X.Sign < 0)
                X += this.p;

            if (Y.Sign < 0)
                Y += this.p;

            return new PointOnCurve(X, Y);
        }

        /// <summary>
        /// Converts a pair of (X,Y) coordinates for the birational Edwards curve
        /// to a pair of (U,V) coordinates.
        /// </summary>
        /// <param name="XY">(X,Y) coordinates.</param>
        /// <returns>(U,V) coordinates.</returns>
        public override PointOnCurve ToUV(PointOnCurve XY)
        {
            BigInteger X2 = this.modP.Multiply(XY.X, XY.X);
            BigInteger Y2 = this.modP.Multiply(XY.Y, XY.Y);
            BigInteger U = this.modP.Divide(Y2, X2);
            BigInteger X3 = this.modP.Multiply(XY.X, X2);
            BigInteger V = this.modP.Divide(this.modP.Multiply(Two - X2 - Y2, XY.Y), X3);

            if (U.Sign < 0)
                U += this.p;

            if (V.Sign < 0)
                V += this.p;

            return new PointOnCurve(U, V);
        }

        /// <summary>
        /// Performs the scalar multiplication of <paramref name="N"/>*<paramref name="U"/>.
        /// </summary>
        /// <param name="N">Scalar</param>
        /// <param name="U">U-coordinate of point</param>
        /// <returns><paramref name="N"/>*<paramref name="U"/></returns>
        public override BigInteger ScalarMultiplication(byte[] N, BigInteger U)
        {
            return XFunction(N, U, A24, this.p, 448);
        }

        /// <summary>
        /// Calculates a private key from a secret.
        /// </summary>
        /// <param name="Secret">Binary secret.</param>
        /// <returns>Private key</returns>
        public override Tuple<byte[], byte[]> CalculatePrivateKey(byte[] Secret)
        {
            byte[] Bin = Secret;

            switch (Bin.Length)
            {
                case 56:
                    Array.Resize(ref Bin, 57);
                    break;

                case 57:
                    break;

                default:
                    Bin = Hashes.ComputeSHA512Hash(Secret);
                    Array.Resize(ref Bin, 57);
                    break;
            }

            Bin[0] &= 0xfc;
            Bin[55] |= 0x80;
            Bin[56] |= 0;

            return new Tuple<byte[], byte[]>(Bin, null);
        }

        /// <summary>
        /// Creates the Edwards Curve pair.
        /// </summary>
        /// <returns>Edwards curve.</returns>
        public override EdwardsCurveBase CreatePair()
        {
            PointOnCurve PublicKeyUV = this.PublicKeyPoint;
            PointOnCurve PublicKeyXY = this.ToXY(PublicKeyUV);

            Edwards448 Candidate = new Edwards448(this.PrivateKey, false);
            PointOnCurve PublicKeyXY2 = Candidate.PublicKeyPoint;
            PublicKeyXY2 = Candidate.ScalarMultiplication(4, PublicKeyXY2, true);

            if (PublicKeyXY.Y.Equals(PublicKeyXY2.Y))
                return Candidate;
            else
                throw new InvalidOperationException("Unable to create pair curve.");
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the XEdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(byte[] Data)
        {
            throw new NotSupportedException("Signatures not supported.");
            //return XEdDSA.Sign(Data, this.PrivateKey, Hashes.ComputeSHA512Hash, this);
        }

        /// <summary>
        /// Creates a signature of <paramref name="Data"/> using the XEdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <returns>Signature.</returns>
        public override byte[] Sign(Stream Data)
        {
            throw new NotSupportedException("Signatures not supported.");
            //return XEdDSA.Sign(Data, this.PrivateKey, Hashes.ComputeSHA512Hash, this);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(byte[] Data, byte[] PublicKey, byte[] Signature)
        {
            throw new NotSupportedException("Signatures not supported.");
            //return XEdDSA.Verify(Data, PublicKey, Hashes.ComputeSHA512Hash, this,
            //    Signature, 448, 446);
        }

        /// <summary>
        /// Verifies a signature of <paramref name="Data"/> made by the EdDSA algorithm.
        /// </summary>
        /// <param name="Data">Payload to sign.</param>
        /// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
        /// <param name="Signature">Signature</param>
        /// <returns>If the signature is valid.</returns>
        public override bool Verify(Stream Data, byte[] PublicKey, byte[] Signature)
        {
            throw new NotSupportedException("Signatures not supported.");
            //return XEdDSA.Verify(Data, PublicKey, Hashes.ComputeSHA512Hash, this,
            //    Signature, 448, 446);
        }

    }
}
