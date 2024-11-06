using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UWP_Crypto.Utils;

// DES, 3DES, AES
public class SymmetricHelper(SymmetricHelper.Algorithm type, string key, string iv, SymmetricHelper.OutEncoding encode)
{
    public enum Algorithm
    {
        DES = 1, TripleDES = 2, AES = 3,
    }

    public enum OutEncoding
    {
        Base64 = 1, Hex = 2,
    }

    private readonly Algorithm Type = type;
    private readonly string Key = key, IV = iv;
    private readonly OutEncoding Encode = encode;

    private SymmetricAlgorithm CreateAlgorithm()
    {
        return Type switch
        {
            Algorithm.AES => Aes.Create(),
            Algorithm.TripleDES => TripleDES.Create(),
            Algorithm.DES => DES.Create(),
            _ => null,
        };
    }

    public string Encrypt(string plainText)
    {
        var sym = CreateAlgorithm();
        sym.Key = Encoding.UTF8.GetBytes(Key);
        if (IV != null)
        {
            sym.IV = Encoding.UTF8.GetBytes(IV);
        }
        else
        {
            sym.IV = new byte[Type == Algorithm.AES ? 16 : 8];  // 零向量（不赋值的话默认为时间）
        }

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var memoryStream = new MemoryStream();
        var cryptoStream = new CryptoStream(memoryStream, sym.CreateEncryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        cryptoStream.FlushFinalBlock();

        if (Encode == OutEncoding.Base64)
        {
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        else
        {
            return Convert.ToHexString(memoryStream.ToArray());
        }
    }

    public string Decrypt(string cipherText)
    {
        var sym = CreateAlgorithm();
        sym.Key = Encoding.UTF8.GetBytes(Key);
        if (IV != null)
        {
            sym.IV = Encoding.UTF8.GetBytes(IV);
        }
        else
        {
            sym.IV = new byte[Type == Algorithm.AES ? 16 : 8];
        }

        byte[] cipherBytes;
        if (Encode == OutEncoding.Base64)
        {
            cipherBytes = Convert.FromBase64String(cipherText);
        }
        else
        {
            cipherBytes = Convert.FromHexString(cipherText);
        }
        
        var memoryStream = new MemoryStream();
        var cryptoStream = new CryptoStream(memoryStream, sym.CreateDecryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
        cryptoStream.FlushFinalBlock();
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
}
