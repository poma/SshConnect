using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace AwsSsh.Plugins.Chef
{
	public class ChefClient
	{
		public static void ChefRequest()
		{
			WebClient client = new WebClient();
			var date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			string user = "admin";

			client.Headers.Add("X-Ops-Timestamp", date);
            client.Headers.Add("X-Ops-UserId", user);
			client.Headers.Add("X-Ops-Content-Hash", Hash(""));
			client.Headers.Add("X-Ops-Sign", "version=1.0");
			client.Headers.Add("X-Ops-Authorization-N", );

			var headers = String.Format("Method:GET\nHashed Path:/nodes\nX-Ops-Content-Hash:{0}\nX-Ops-Timestamp:{1}\nX-Ops-UserId:{2}", Hash(""), date, user);
			var sign = Sign(headers);

            sign.Split()


			client.DownloadString("http://chef.redhelper.ru:4000/nodes");
		}

		public static string Hash(string s)
		{
			return Convert.ToBase64String(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(s)));
		}

		public static string Sign(string s)
		{
			RSAParameters keyPair;

			using (var reader = File.OpenText(App.Settings.ChefPrivateKey))
				keyPair = new ThirdParty.BouncyCastle.OpenSsl.PemReader(reader).ReadPrivatekey();

			var rsa = new RSACryptoServiceProvider();
			rsa.ImportParameters(keyPair);

			var signed = rsa.SignData(Encoding.UTF8.GetBytes(s), new SHA1CryptoServiceProvider());

			return Convert.ToBase64String(signed);
		}
	}
}


//X-Ops-Timestamp: A timestamp in ISO-8601 format. The time must be in UTC, indicated by a trailing Z, and separated by the character "T". The following is an example of a time in this format:
//2011-10-14T18:17:48Z
//X-Ops-UserId: The name of the API client whose private key will be used to create the authorization header.
//X-Ops-Content-Hash: The body of the request, hashed using SHA1 and encoded using Base64. See Hashing below.
//X-Ops-Sign: Set to "version=1.0"
//X-Ops-Authorization-N: One or more headers whose value are the signature of the "canonical headers", encoded in Base64. See Canonical Headers and X-Ops-Authorization Headers below.