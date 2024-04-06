using System;
using System.Security.Cryptography;
using System.Text;

namespace Kafe.Common
{
    public static class Hash
    {
        public static string Sha512(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(SHA512.HashData(bytes));
        }
    }
}
