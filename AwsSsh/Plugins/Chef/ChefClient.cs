using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace AwsSsh.Plugins.Chef
{
	public class ChefClient
	{
		public static string ChefRequest(string path, string query = null)
		{
			WebClient client = new WebClient();
			var date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

			client.Headers.Add("Accept", "application/json");
			client.Headers.Add("X-Ops-Timestamp", date);
			client.Headers.Add("X-Ops-UserId", App.Settings.ChefUser);
			client.Headers.Add("X-Ops-Content-Hash", Hash(""));
			client.Headers.Add("X-Ops-Sign", "algorithm=sha1;version=1.0");

			//"Method:HTTP_METHOD\nHashed Path:HASHED_PATH\nX-Ops-Content-Hash:HASHED_BODY\nX-Ops-Timestamp:TIME\nX-Ops-UserId:USERID"
			var headers = String.Format("Method:GET\nHashed Path:{3}\nX-Ops-Content-Hash:{0}\nX-Ops-Timestamp:{1}\nX-Ops-UserId:{2}", Hash(""), date, App.Settings.ChefUser, Hash(path));

			var sign = Sign(headers);
			int n = 1;
			while (sign.Length > 0)
			{
				int chars = Math.Min(60, sign.Length);
				client.Headers.Add("X-Ops-Authorization-" + n, sign.Substring(0, chars));
				sign = sign.Remove(0, chars);
				n++;
			}

			return client.DownloadString("http://chef.redhelper.ru:4000" + path + (string.IsNullOrEmpty(query) ? "" : "?" + query));
		}

		public static string Hash(string s)
		{
			return Convert.ToBase64String(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(s)));
		}

		public static string Sign(string s)
		{
			var bytes = Encoding.UTF8.GetBytes(s);

			AsymmetricCipherKeyPair key;

			using (var reader = File.OpenText(App.Settings.ChefKey))
				key = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

			ISigner sig = SignerUtilities.GetSigner("RSA");
			sig.Init(true, key.Private);
			sig.BlockUpdate(bytes, 0, bytes.Length);
			byte[] signature = sig.GenerateSignature();
			var signedString = Convert.ToBase64String(signature);
			return signedString;

		}
	}
}


//X-Ops-Timestamp: A timestamp in ISO-8601 format. The time must be in UTC, indicated by a trailing Z, and separated by the character "T". The following is an example of a time in this format:
//2011-10-14T18:17:48Z
//X-Ops-UserId: The name of the API client whose private key will be used to create the authorization header.
//X-Ops-Content-Hash: The body of the request, hashed using SHA1 and encoded using Base64. See Hashing below.
//X-Ops-Sign: Set to "version=1.0"
//X-Ops-Authorization-N: One or more headers whose value are the signature of the "canonical headers", encoded in Base64. See Canonical Headers and X-Ops-Authorization Headers below.