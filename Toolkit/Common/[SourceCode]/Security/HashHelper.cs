using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Twilight_Dream.Security
{
	public class HashHelper
	{
		/// <summary>
		/// for custom class need [Serializable]
		/// to ignore https://stackoverflow.com/questions/33489930/
		/// ignore-non-serialized-property-in-binaryformatter-serialization
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public byte[] Byte(object value)
		{
			/*https://stackoverflow.com/questions/1446547/
              how-to-convert-an-object-to-a-byte-array-in-c-sharp*/
			using (var ms_object = new MemoryStream())
			{
				BinaryFormatter bf_object = new BinaryFormatter();
				bf_object.Serialize(ms_object, value == null ? "null" : value);
				return ms_object.ToArray();
			}
		}

		public byte[] Hash(byte[] value)
		{
			/*https://support.microsoft.com/en-za/help/307020/
              how-to-compute-and-compare-hash-values-by-using-visual-cs*/
			/*https://andrewlock.net/why-is-string-gethashcode-
              different-each-time-i-run-my-program-in-net-core*/
			byte[] result = SHA256Managed.Create().ComputeHash(value);
			return result;
		}

		public byte[] Combine(params byte[][] values)
		{
			/*https://stackoverflow.com/questions/415291/
              best-way-to-combine-two-or-more-byte-arrays-in-c-sharp*/
			byte[] target_array = new byte[values.Sum(a => a.Length)];
			int offset = 0;
			foreach (byte[] source_array in values)
			{
				System.Buffer.BlockCopy(source_array, 0, target_array, offset, source_array.Length);
				offset += source_array.Length;
			}
			return target_array;
		}

		public string String(byte[] hash)
		{
			/*https://stackoverflow.com/questions/1300890/
              md5-hash-with-salt-for-keeping-password-in-db-in-c-sharp*/
			StringBuilder sb_object = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb_object.Append(hash[i].ToString("x2"));     /*do not make it X2*/
			}
			var result = sb_object.ToString();
			return result;
		}

		public byte[] Hash(params object[] values)
		{
			byte[][] bytes = new byte[values.Length][];
			for (int i = 0; i < values.Length; i++)
			{
				bytes[i] = Byte(values[i]);
			}
			byte[] combined = Combine(bytes);
			byte[] combinedHash = Hash(combined);
			return combinedHash;
		}

		/*https://stackoverflow.com/questions/5868438/c-sharp-generate-a-random-md5-hash*/
		public string HashString(string value, Encoding text_encoding = null)
		{
			if (text_encoding == null)
			{
				text_encoding = Encoding.UTF8;
			}
			byte[] bytes = text_encoding.GetBytes(value);
			byte[] hash = Hash(bytes);
			string result = String(hash);
			return result;
		}

		public string HashString(params object[] values)
		{
			/*Add more not constant properties as needed*/
			var hash = Hash(values);
			var value = String(hash);
			return value;
		}
	}
}
