using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UWP_Crypto.Utils;

// DES, 3DES, AES
public class ClassicHelper()
{
    // 移位变换
    public string EncryptShift(string plainText, long shiftCnt)
    {
        var encryptedText = new StringBuilder();

        foreach (var c in plainText)
        {
            int offset = char.IsUpper(c) ? 'A' : 'a';
            if (!char.IsLetter(c))
            {
                encryptedText.Append(c);
                continue;
            }
            encryptedText.Append((char)(QuickTools.Mod26(c - offset - shiftCnt) + offset));
        }

        return encryptedText.ToString();
    }

    // 逆操作
    public string DecryptShift(string encryptText, long shiftCnt)
    {
        return EncryptShift(encryptText, -shiftCnt);
    }

    // 仿射变换
    public string EncryptAffine(string plainText, long A, long B)
    {
        var encryptedText = new StringBuilder();

        foreach (var c in plainText)
        {
            int offset = char.IsUpper(c) ? 'A' : 'a';

            if (!char.IsLetter(c))
            {
                encryptedText.Append(c);
                continue;
            }

            encryptedText.Append((char)(QuickTools.Mod26((c - offset) * A + B) + offset));
        }

        return encryptedText.ToString();
    }

    // 逆操作 Ax+B=y => (y-B)/A=x
    public string DecryptAffine(string encryptText, long A, long B)
    {
        var plainText = new StringBuilder();

        foreach (var c in encryptText)
        {
            int offset = char.IsUpper(c) ? 'A' : 'a';

            if (!char.IsLetter(c))
            {
                plainText.Append(c);
                continue;
            }

            plainText.Append((char)(QuickTools.Mod26(c - offset - B) * QuickTools.InverseMod26(A) % 26 + offset));
        }

        return plainText.ToString();
    }

    // 维吉尼亚 Ci+Ki
    private string Vigenere(string plainText, string key, bool encrypt)
    {
        var encryptedText = new StringBuilder();
        var keyIndex = 0;

        foreach (var c in plainText)
        {
            int offset = char.IsUpper(c) ? 'A' : 'a';

            if (!char.IsLetter(c))
            {
                encryptedText.Append(c);
                continue;
            }

            var keyShift = key[keyIndex] - 'A';
            keyShift = encrypt ? keyShift : 26 - keyShift;

            encryptedText.Append((char)(QuickTools.Mod26((c - offset + keyShift) % 26) + offset));

            keyIndex = (keyIndex + 1) % key.Length;  // 循环使用密钥
        }

        return encryptedText.ToString();
    }

    public string EncryptVigenere(string plainText, string key)
    {
        return Vigenere(plainText, key, true);
    }

    public string DecryptVigenere(string encryptText, string key)
    {
        return Vigenere(encryptText, key, false);
    }
}
