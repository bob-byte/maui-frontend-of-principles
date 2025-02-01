using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Helpers;
public static class DecryptTextHelper
{
    public static string DecryptText( string cipherText, string firstKey, string secondKey )
    {
        byte[] Key = Encoding.UTF8.GetBytes( firstKey );

        byte[] IV = Encoding.UTF8.GetBytes( secondKey );

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor( aesAlg.Key, aesAlg.IV );

            using (MemoryStream msDecrypt = new( Convert.FromBase64String( cipherText ) ))
            using (CryptoStream csDecrypt = new( msDecrypt, decryptor, CryptoStreamMode.Read ))
            using (StreamReader srDecrypt = new( csDecrypt ))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }
}
