namespace Cat.Utilities
{
    using System.Security.Cryptography;
    using System.Text;

    public static class Encrpto
    {
        public static string Sha256Encrpto(string content)
        {
            using var sha256 = SHA256.Create();
            using var block = zstring.Block();
            // Convert the input string to a byte array
            byte[] inputBytes = Encoding.UTF8.GetBytes(content);

            // Compute the hash value of the input byte array
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert the hash byte array to a hexadecimal string
            zstring sb = "";
            foreach (byte b in hashBytes)
            {
                sb = zstring.Concat(sb, b.ToString("x2"));
            }
            return sb;
        }
    }
}