/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：EncryptDecrypt
// 创建日期：2019-1-10
// 创建者：waicheng.wang
// 模块描述：AssetBundle包的加密，解密
//-------------------------------------------------------------------------------------------*/

using System;
using System.Security.Cryptography;
using System.Text;

namespace DragonU3DSDK.Asset
{
    public static class EncryptDecrypt
    {
        // 128位加密：16字节
        private const string Key = "wodemimahenchang";
        private static ICryptoTransform encryptor;
        private static ICryptoTransform decryptor;

        static void Init()
        {
            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(Key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            encryptor = rm.CreateEncryptor();
            decryptor = rm.CreateDecryptor();
        }

        // 加密
        public static string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(EncryptBytes(toEncryptArray));
        }

        // 解密
        public static string Decrypt(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            byte[] bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(DecryptBytes(bytes));
        }

        // 加密
        public static byte[] EncryptBytes(byte[] input)
        {
            if (input == null) return null;

            byte[] toEncryptArray = input;

            if (encryptor == null)
            {
                Init();
            }

            byte[] resultArray = encryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return resultArray;

        }

        // 解密
        public static byte[] DecryptBytes(byte[] input)
        {
            if (input == null) return null;

            byte[] toEncryptArray = input;

            if (decryptor == null)
            {
                Init();
            }

            byte[] resultArray = decryptor.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return resultArray;
        }
    }
}
