using QRCoder;
using System.Security.Cryptography;
using System.Text;

namespace GiaNguyenCheck.Utilities
{
    public static class QrHelper
    {
        public static byte[] GenerateQrCode(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(20);
        }

        public static string Encrypt(string plainText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.GenerateIV();
            var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(aes.IV.Concat(cipherBytes).ToArray());
        }

        public static string Decrypt(string cipherText, string key)
        {
            var fullBytes = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            aes.IV = fullBytes.Take(16).ToArray();
            var decryptor = aes.CreateDecryptor();
            var cipherBytes = fullBytes.Skip(16).ToArray();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
} 