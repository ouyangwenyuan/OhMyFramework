using System;
using System.IO;
using System.Security.Cryptography;

namespace DragonU3DSDK.Util
{
    public class RijndaelManager : Manager<RijndaelManager>
    {
        byte[] rijndaelKey = new byte[] { 0x0F, 0x02, 0x01, 0x02, 0x01, 0x02, 0x01, 0x02,
                                      0x02, 0x02, 0x01, 0x02, 0x01, 0x02, 0x01, 0x02,
                                      0x04, 0x02, 0x01, 0x02, 0x01, 0x02, 0x01, 0x02,
                                      0x04, 0x02, 0x01, 0x02, 0x01, 0x02, 0x01, 0x02 };
        public byte[] RijndaelKey
        {
            get
            {
                return rijndaelKey;
            }
            set
            {
                rijndaelKey = value;
            }
        }

        byte[] rijndaelIV = new byte[] {  0x01, 0x02, 0x0E, 0x02, 0x01, 0x90, 0x01, 0x02,
                                      0x01, 0x02, 0xFF, 0x02, 0x01, 0xFE, 0x01, 0x02 };
        public byte[] RijndaelIV
        {
            get
            {
                return rijndaelIV;
            }
            set
            {
                rijndaelIV = value;
            }
        }

        public byte[] EncryptBytesToBytes(byte[] plainBytes)
        {
            // Check arguments.
            if (plainBytes == null || plainBytes.Length <= 0)
                throw new ArgumentNullException("plainBytes");
            if (RijndaelKey == null || RijndaelKey.Length <= 0)
                throw new ArgumentNullException("Key");
            if (RijndaelIV == null || RijndaelIV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = RijndaelKey;
                rijAlg.IV = RijndaelIV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter swEncrypt = new BinaryWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainBytes);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public byte[] EncryptStringToBytes(string plainText)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (RijndaelKey == null || RijndaelKey.Length <= 0)
                throw new ArgumentNullException("Key");
            if (RijndaelIV == null || RijndaelIV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = RijndaelKey;
                rijAlg.IV = RijndaelIV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        public byte[] DecryptBytesFromBytes(byte[] cipherBytes)
        {
            // Check arguments.
            if (cipherBytes == null || cipherBytes.Length <= 0)
                throw new ArgumentNullException("cipherBytes");
            if (RijndaelKey == null || RijndaelKey.Length <= 0)
                throw new ArgumentNullException("Key");
            if (RijndaelIV == null || RijndaelIV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            byte[] plainBytes = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = RijndaelKey;
                rijAlg.IV = RijndaelIV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader srDecrypt = new BinaryReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plainBytes = srDecrypt.ReadBytes(int.MaxValue);
                        }
                    }
                }

            }

            return plainBytes;

        }

        public string DecryptStringFromBytes(byte[] cipherText)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (RijndaelKey == null || RijndaelKey.Length <= 0)
                throw new ArgumentNullException("Key");
            if (RijndaelIV == null || RijndaelIV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = RijndaelKey;
                rijAlg.IV = RijndaelIV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }
}
