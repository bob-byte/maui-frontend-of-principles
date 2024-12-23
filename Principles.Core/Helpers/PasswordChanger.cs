using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Helpers;
public static class PasswordChanger
{
    public static string EncryptNewPassword( string plainText, string firstKey, string secondKey )
    {
        byte[] Key = Encoding.UTF8.GetBytes( firstKey );

        byte[] IV = Encoding.UTF8.GetBytes( secondKey );

        using var aesAlg = Aes.Create();
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor( aesAlg.Key, aesAlg.IV );

        using MemoryStream msEncrypt = new();
        using CryptoStream csEncrypt = new( msEncrypt, encryptor, CryptoStreamMode.Write );
        using StreamWriter swEncrypt = new( csEncrypt );
        
        swEncrypt.Write( plainText );

        return Convert.ToBase64String( msEncrypt.ToArray() );
    }
}
