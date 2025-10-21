using System;
using System.Linq;
using System.Text;
using Waher.Security;

namespace TAG.Networking.DockerRegistry.Model
{
	[Serializable]
	public class HashDigest : IComparable
	{
		public HashFunction HashFunction;
		public byte[] Hash;

		public HashDigest()
		{
		}

		public HashDigest(string s)
			: this(HashFunction.SHA256, Encoding.UTF8.GetBytes(s))
		{

		}

		public HashDigest(HashFunction Function, byte[] data)
		{
			this.HashFunction = Function;
			this.Hash = Hashes.ComputeHash(Function, data);
		}

		public static bool TryParseDigest(string s, out HashDigest Digest)
		{
			HashFunction Function = default;
			Digest = null;

			int i = s.IndexOf(':');
			if (i < 0)
				return false;

			if (!Enum.TryParse(s[..i], true, out Function))
				return false;

			try
			{
				Digest = new HashDigest()
				{
					Hash = Hashes.StringToBinary(s[(i + 1)..]),
					HashFunction = Function
				};
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is HashDigest D)
			{
				if (this.HashFunction != D.HashFunction)
					return false;
				return this.Hash.SequenceEqual(D.Hash);
			}
			else
				return false;
		}

		public override int GetHashCode()
		{
			return this.HashFunction.GetHashCode() ^ ComputeHash(this.Hash);
		}

		public override string ToString()
		{
			if (Hash == null)
				return "";
			return HashFunction.ToString().ToLower() + ":" + Hashes.BinaryToString(Hash);
		}

		public int CompareTo(HashDigest other)
		{
			int HashCodeDiference = other.HashFunction.GetHashCode() - this.HashFunction.GetHashCode();

			if (HashCodeDiference != 0)
				return HashCodeDiference;

			return BinaryCompare(Hash, other.Hash);
		}

		public int CompareTo(object other)
		{
			if (other is HashDigest D)
				return this.CompareTo(D);
			else
				throw new ArgumentException("Object is not a HashDigest.", nameof(other));
		}

		internal static int ComputeHash(params byte[] data)
		{
			unchecked
			{
				const int p = 16777619;
				int hash = (int)2166136261;

				for (int i = 0; i < data.Length; i++)
					hash = (hash ^ data[i]) * p;

				return hash;
			}
		}
		internal static int BinaryCompare(byte[] b1, byte[] b2)
		{
			int c1 = b1.Length;
			int c2 = b2.Length;
			int c = Math.Min(c1, c2);
			int i, j;

			for (i = 0; i < c; i++)
			{
				j = b1[i] - b2[i];
				if (j != 0)
					return j;
			}

			return c1 - c2;
		}
	}
}
