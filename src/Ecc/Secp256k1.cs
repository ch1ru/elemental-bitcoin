using System.Globalization;
using System.Numerics;

namespace LBitcoin.Ecc {

	/// <summary>
	/// Creates the elliptic curve y = x^3 + 7 mod P.
	/// See <see href="https://en.bitcoin.it/wiki/Secp256k1">Secp256k1</see> for more info.
	/// </summary>
	class Secp256k1 {

		Point256 G_;
		public static BigInteger N = BigInteger.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494337", NumberStyles.Integer);
		
		public Secp256k1() {

			BigInteger gx_ = BigInteger.Parse("55066263022277343669578718895168534326250603453777594175500187360389116729240", NumberStyles.Integer);
			BigInteger gy_ = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424", NumberStyles.Integer);

			Sha256Field x1_ = new Sha256Field(gx_);
			Sha256Field y1_ = new Sha256Field(gy_);

			G_ = new Point256(x1_, y1_);
		}

		/// <summary>
		/// Gets the point that is the public key.
		/// </summary>
		/// <param name="k">Scalar as <see cref="System.Numerics.BigInteger"/>.</param>
		public Point256 getPublicKeyPoint(BigInteger k) {
			Point t = G_ * k;
			return new Point256(t);
		}

		/// <summary>
		/// Gets the base point for the elliptic curve.
		/// </summary>
		public Point256 getGeneratorPoint() {
			return G_;
        }
	}
}
