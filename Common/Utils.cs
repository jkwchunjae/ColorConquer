using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Security.Cryptography;

namespace Common
{
	public static class Utils
	{
		public static string Input(string question = "")
		{
			Console.Write("{0}: ".With(question));
			return Console.ReadLine();
		}

		#region RSA
		public static string RsaPublicKeyXmlString { get; private set; }
		public static string RsaPrivateKeyXmlString { get; private set; }
		static RSACryptoServiceProvider Rsa = null;

		static void RsaSetKey(string rsaKey)
		{
			if (Rsa == null) Rsa = new RSACryptoServiceProvider();
			Rsa.FromXmlString(rsaKey);
		}

		public static void RsaSetPublicKey(string rsaPublicKeyXmlString)
		{
			RsaPublicKeyXmlString = rsaPublicKeyXmlString;
			RsaSetKey(rsaPublicKeyXmlString);
		}

		public static void RsaSetPrivateKey(string rsaPrivateKeyXmlString)
		{
			RsaPrivateKeyXmlString = rsaPrivateKeyXmlString;
			RsaSetKey(rsaPrivateKeyXmlString);
		}

		public static byte[] RsaEncrypt(this byte[] plain)
		{
			if (RsaPublicKeyXmlString == null) return plain;
			if (plain == null) return (new byte[] { });
			RsaSetKey(RsaPublicKeyXmlString);
			var cypher = new List<byte>();
			int size = 100;
			for (var i = 0; i < plain.Count(); i += size)
			{
				cypher.AddRange(Rsa.Encrypt(plain.Skip(i).Take(size).ToArray(), false));
			}
			return cypher.ToArray();
		}

		public static byte[] RsaDecrypt(this byte[] cypher)
		{
			if (RsaPrivateKeyXmlString == null) return cypher;
			if (cypher == null) return (new byte[] { });
			RsaSetKey(RsaPrivateKeyXmlString);
			var plain = new List<byte>();
			int size = 128;
			for (var i = 0; i < cypher.Count(); i += size)
			{
				plain.AddRange(Rsa.Decrypt(cypher.Skip(i).Take(size).ToArray(), false));
			}
			return plain.ToArray();
		}

		public static string RsaGeneratePrivateKey(int keySize = 2048)
		{
			if (Rsa == null) Rsa = new RSACryptoServiceProvider(keySize); // 한번만 실행 되도록
			RsaPrivateKeyXmlString = Rsa.ToXmlString(true);
			return RsaPrivateKeyXmlString;
		}

		public static string RsaGetPublicKey(string rsaPrivateKeyXmlString)
		{
			RsaSetPrivateKey(rsaPrivateKeyXmlString);
			return Rsa.ToXmlString(false);
		}
		#endregion
	}
}
