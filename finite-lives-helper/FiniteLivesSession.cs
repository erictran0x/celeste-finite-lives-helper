using System.Text;
using System.Security.Cryptography;

namespace Celeste.Mod.FiniteLives
{
    public class FiniteLivesSession : EverestModuleSession
    {
        public int LifeCount { get; set; } = 1;
        public bool InfiniteLives { get; set; } = true;
        public string Checksum { get; set; } = "";

        public string HashCode()
        {
			// Salt the data
			string salt = "exWOhxnV2KLuSvOdE70k";
			string str = salt.Substring(10) + LifeCount.ToString() + salt.Substring(0, 6) + InfiniteLives.GetHashCode().ToString() + salt.Substring(3, 8);

			// Create a SHA256 hash from salted data
			using (SHA256 sha256Hash = SHA256.Create())
			{
				// ComputeHash - returns byte array  
				byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

				// Convert byte array to a string   
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < bytes.Length; i++)
					builder.Append(bytes[i].ToString("x2"));

				return builder.ToString();
			}
		}
    }
}
