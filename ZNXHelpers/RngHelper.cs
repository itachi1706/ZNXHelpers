using System;
using System.Security.Cryptography;

namespace ZNXHelpers
{
    public class RngHelper
    {
        public string GeneratePassword(int size)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] buffer = new byte[size];
                rng.GetBytes(buffer);
                string result = Convert.ToBase64String(buffer);
                return result;
            }
        }
    }
}
