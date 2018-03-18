﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace PassStorage2.Base.DataCryptoLayer
{
    public static class SHA
    {
        public static string GenerateSHA256String(string inputString)
        {
            try
            {
                SHA256 sha256 = SHA256Managed.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(inputString);
                byte[] hash = sha256.ComputeHash(bytes);
                return GetStringFromHash(hash);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
                return null;
            }
        }

        public static string GenerateSHA512String(string inputString)
        {
            try
            {
                SHA512 sha512 = SHA512Managed.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(inputString);
                byte[] hash = sha512.ComputeHash(bytes);
                return GetStringFromHash(hash);

            }
            catch (Exception e)
            {
                Logger.Instance.Error(e);
                return null;
            }
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public static bool Equals(string shaHash, string value, bool isSha512 = false)
        {
            return (isSha512 ? GenerateSHA512String(value) : GenerateSHA256String(value)).Equals(shaHash);
        }
    }
}