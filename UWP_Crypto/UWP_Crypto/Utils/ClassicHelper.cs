using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP_Crypto.Utils;

public static class ClassicHelper
{
    // 移位变换
    public static string EncryptShift(string plainText, long shiftCnt)
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
            encryptedText.Append((char)(QuickTools.Mod26(c - offset + shiftCnt) + offset));
        }

        return encryptedText.ToString();
    }

    // 逆操作
    public static string DecryptShift(string encryptText, long shiftCnt)
    {
        return EncryptShift(encryptText, -shiftCnt);
    }

    // 仿射变换
    public static string EncryptAffine(string plainText, long A, long B)
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
    public static string DecryptAffine(string encryptText, long A, long B)
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
    private static string Vigenere(string plainText, string key, bool encrypt)
    {
        var encryptedText = new StringBuilder();
        var keyIndex = 0;

        foreach (var c in plainText)
        {
            int plainOffset = char.IsUpper(c) ? 'A' : 'a';
            int keyOffset = char.IsUpper(key[keyIndex]) ? 'A' : 'a';

            if (!char.IsLetter(c))
            {
                encryptedText.Append(c);
                continue;
            }

            var keyShift = key[keyIndex] - keyOffset;
            keyShift = encrypt ? keyShift : 26 - keyShift;

            encryptedText.Append((char)(QuickTools.Mod26((c - plainOffset + keyShift) % 26) + plainOffset));

            keyIndex = (keyIndex + 1) % key.Length;  // 循环使用密钥
        }

        return encryptedText.ToString();
    }

    public static string EncryptVigenere(string plainText, string key)
    {
        return Vigenere(plainText, key, true);
    }

    public static string DecryptVigenere(string encryptText, string key)
    {
        return Vigenere(encryptText, key, false);
    }

    // 栅栏密码
    public static string EncryptRail(string plainText, long rail)
    {
        if (rail <= 1)
        {
            return string.Empty;
        }

        rail = Math.Min(plainText.Length, rail);

        var list = new StringBuilder[rail];
        for (var i = 0; i < rail; i++)
        {
            list[i] = new StringBuilder();
        }

        long currentRail = 0;
        foreach (var c in plainText)
        {
            list[currentRail].Append(c);
            currentRail = (currentRail + 1) % rail;
        }

        for (var i = 1; i < rail; i++)
        {
            list[0].Append(list[i]);
        }

        return list[0].ToString();
    }

    public static string DecryptRail(string encryptText, long rail)
    {
        if (rail <= 1)
        {
            return string.Empty;
        }

        rail = Math.Min(encryptText.Length, rail);

        var decryptText = new char[encryptText.Length];
        var currentIdx = 0;
        for (var i = 0; i < rail; i++)
        {
            for (var j = i; j < encryptText.Length; j += (int)rail)
            {
                decryptText[j] = encryptText[currentIdx++];
            }
        }

        return new string(decryptText);
    }
}
