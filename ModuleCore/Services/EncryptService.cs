using System;
using System.Security.Cryptography;
using System.Text;

namespace ModuleCore.Services
{
    public class EncryptService
    {
        /// <summary>
        ///静态无参构造
        /// </summary>
        static EncryptService()
        {
            //默认的密钥
            SecretKey = "tfarcraw";
        }

        /// <summary>
        /// 使用SHA256加密字符串
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string EncrypToSHA(string Source)
        {
            SHA256Managed sha256 = new();
            byte[] s = UTF8Encoding.UTF8.GetBytes(Source);
            byte[] t = sha256.ComputeHash(s);
            return Convert.ToBase64String(t);
        }

        /// <summary>
        /// MD5加密(32位)
        /// </summary>
        /// <param name="str">加密字符</param>
        /// <returns></returns>
        public static string Encrypt(string str)
        {

            string pwd = "";

            byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < hash.Length; i++)
            {
                pwd += hash[i].ToString("X");
            }
            return pwd;
        }

        /// <summary>
        /// 缺省的密钥
        /// </summary>
        public static readonly string SecretKey;

        /// <summary>
        /// 使用缺省密钥字符串加密string
        /// </summary>
        /// <param name="original">明文</param>
        /// <returns>密文</returns>
        public static string EncryptWithSecretKey(string original)
        {
            return Encrypt(original, SecretKey);
        }

        /// <summary>
        /// 使用缺省密钥字符串解密string
        /// </summary>
        /// <param name="original">密文</param>
        /// <returns>明文</returns>
        public static string Decrypt(string original)
        {
            return Decrypt(original, SecretKey, System.Text.Encoding.Default);
        }

        /// <summary>
        /// 使用给定密钥字符串加密string
        /// </summary>
        /// <param name="original">原始文字</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static string Encrypt(string original, string key)
        {
            byte[] buff = System.Text.Encoding.Default.GetBytes(original);
            byte[] kb = System.Text.Encoding.Default.GetBytes(key);
            return Convert.ToBase64String(Encrypt(buff, kb));
        }

        /// <summary>
        /// 使用给定密钥字符串解密string
        /// </summary>
        /// <param name="original">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string Decrypt(string original, string key)
        {
            return Decrypt(original, key, System.Text.Encoding.Default);
        }

        /// <summary>
        /// 使用给定密钥字符串解密string,返回指定编码方式明文
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">字符编码方案</param>
        /// <returns>明文</returns>
        public static string Decrypt(string encrypted, string key, Encoding encoding)
        {
            byte[] buff = Convert.FromBase64String(encrypted);
            byte[] kb = System.Text.Encoding.Default.GetBytes(key);
            return encoding.GetString(Decrypt(buff, kb));
        }

        /// <summary>
        /// 生成MD5摘要
        /// </summary>
        /// <param name="original">数据源</param>
        /// <returns>摘要</returns>
        public static byte[] MakeMd5(byte[] original)
        {
            MD5CryptoServiceProvider hashmd5 = new();
            byte[] keyhash = hashmd5.ComputeHash(original);
            return keyhash;
        }

        /// <summary>
        /// 使用给定密钥加密
        /// </summary>
        /// <param name="original">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] original, byte[] key)
        {
            TripleDESCryptoServiceProvider des = new()
            {
                Key = MakeMd5(key),
                Mode = CipherMode.ECB
            };
            return des.CreateEncryptor().TransformFinalBlock(original, 0, original.Length);
        }

        /// <summary>
        /// 使用给定密钥解密数据
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] encrypted, byte[] key)
        {
            TripleDESCryptoServiceProvider des = new()
            {
                Key = MakeMd5(key),
                Mode = CipherMode.ECB
            };
            return des.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
        }
    }
}