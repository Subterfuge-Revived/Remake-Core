using System.Security.Cryptography;

namespace Subterfuge.Remake.Server.Database
{
    public class JwtManager
    {
        public static String HashString(string password)
        {
            // Create salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            
            // Create the Rfc2898DeriveBytes and get the hash value: 
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            
            // Combine salt and password bytes
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            
            // Return value
            return Convert.ToBase64String(hashBytes);
        }

        public static Boolean VerifyHashedStringMatches(string rawString, string expectedHash)
        {
            /* Fetch the stored value */
            string savedPasswordHash = expectedHash;
            
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            
            /* Compute the hash on the entered password the user entered using the extracted salt */
            var pbkdf2 = new Rfc2898DeriveBytes(rawString, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            
            /* Compare the results */
            for (int i=0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }
    }
}