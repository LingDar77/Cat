using System.Security.Cryptography;
using System.Text;

namespace TUI.Utillities
{
    public static class Encrpto
    {
        public static string Sha256Encrpto(string content)
        {
            using SHA256 sha256 = SHA256.Create();
            // Convert the input string to a byte array
            byte[] inputBytes = Encoding.UTF8.GetBytes(content);

            // Compute the hash value of the input byte array
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert the hash byte array to a hexadecimal string
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}