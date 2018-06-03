﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SecureRemotePassword
{
	/// <summary>
	/// Hashing algorithms for the SRP-6a protocol.
	/// </summary>
	/// <typeparam name="T">Hash algorithm type.</typeparam>
	public class SrpHash<T> : ISrpHash
		where T : HashAlgorithm
	{
		/// <summary>
		/// Gets the hash function.
		/// </summary>
		public SrpHash HashFunction { get; } = ComputeHash;

		/// <summary>
		/// Gets the size of the hash in bytes.
		/// </summary>
		public int HashSizeBytes { get; } = Hasher.HashSize / 8;

		/// <summary>
		/// Gets the name of the algorithm.
		/// </summary>
		public string AlgorithmName { get; } = typeof(T).Name;

		private static SrpInteger ComputeHash(params object[] values) =>
			ComputeHash(Combine(values.Select(v => GetBytes(v))));

		private static T Hasher { get; } = CreateHasher();

		/// <summary>
		/// Creates the hasher of the given type <typeparamref name="T"/>.
		/// </summary>
		public static T CreateHasher() => (T)SrpHashTools.CreateHasher(typeof(T).Name);

		private static SrpInteger ComputeHash(byte[] data)
		{
			var hash = Hasher.ComputeHash(data);
			return SrpInteger.FromByteArray(hash);

			// should yield the same result:
			// var hex = hash.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2")), sb => sb.ToString());
			// return SrpInteger.FromHex(hex);
		}

		private static byte[] EmptyBuffer { get; } = new byte[0];

		private static byte[] GetBytes(object obj)
		{
			if (obj == null)
			{
				return EmptyBuffer;
			}

			var value = obj as string;
			if (!string.IsNullOrEmpty(value))
			{
				return Encoding.UTF8.GetBytes(value);
			}

			var integer = obj as SrpInteger;
			if (integer != null)
			{
				return integer.ToByteArray();
			}

			return EmptyBuffer;
		}

		private static byte[] Combine(IEnumerable<byte[]> arrays)
		{
			var rv = new byte[arrays.Sum(a => a.Length)];
			var offset = 0;

			foreach (var array in arrays)
			{
				Buffer.BlockCopy(array, 0, rv, offset, array.Length);
				offset += array.Length;
			}

			return rv;
		}
	}
}
