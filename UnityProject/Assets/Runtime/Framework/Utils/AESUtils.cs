using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace DragonU3DSDK
{
    public class AESUtils
    {
        public static byte[] AESEncrypt(string plainText)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(plainText);

            return AESEncrypt(byteArray);
        }

        public static byte[] AESEncrypt(byte[] byteArray)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(ConfigurationController.Instance._AesKey);

            byte[] encrypt = null;
            Rijndael aes = Rijndael.Create();
            aes.GenerateIV();
            aes.Mode = CipherMode.CFB;
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, aes.IV), CryptoStreamMode.Write))
                    {
                        cStream.Write(aes.IV, 0, aes.IV.Length);
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        encrypt = mStream.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtil.Log(e);
                return null;
            }
            finally
            {
                aes.Clear();
            }

            return encrypt;
        }

        public static byte[] AESDecryptBytes(byte[] encryptedArray)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(ConfigurationController.Instance._AesKey);
            var bIV = new byte[16];
            Array.Copy(encryptedArray, 0, bIV, 0, bIV.Length);

            byte[] decrypt = null;
            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.None;
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(encryptedArray, bIV.Length, encryptedArray.Length - bIV.Length);
                        cStream.FlushFinalBlock();
                        decrypt = mStream.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtil.Log(e);
                return null;
            }
            finally
            {
                aes.Clear();
            }
            return decrypt;
        }

        public static string AESDecrypt(byte[] encryptedArray)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(ConfigurationController.Instance._AesKey);
            var bIV = new byte[16];
            Array.Copy(encryptedArray, 0, bIV, 0, bIV.Length);

            string decrypt = null;
            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.CFB;
            aes.Padding = PaddingMode.None;
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(encryptedArray, bIV.Length, encryptedArray.Length - bIV.Length);
                        cStream.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtil.Log(e);
                return null;
            }
            finally
            {
                aes.Clear();
            }
            return decrypt.Substring(0, decrypt.Length - (int)decrypt[decrypt.Length - 1]);
        }

    }
}

