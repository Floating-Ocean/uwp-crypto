using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UWP_Crypto.Lang;
using Windows.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UWP_Crypto.Utils;
public static class RSAHelper
{
    public enum OutEncoding
    {
        Base64 = 1, Hex = 2,
    }

    public static async Task<string> Encrypt(string keyName, string plainText, OutEncoding encoding)
    {
        // 公钥加密
        var key = await GetKey(keyName, false);
        if (key == null)
        {
            return null;
        }

        var rsa = RSA.Create();
        rsa.ImportFromPem(key);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        // rsa存在取模，因而有最大长度：密钥长度 - Padding对应大小
        var splitLength = rsa.KeySize / 8 - 66;
        var splitsCount = (plainBytes.Length + splitLength - 1) / splitLength;
        var result = new StringBuilder();

        for (int i = 0, cursor = 0; i < splitsCount; i++, cursor += splitLength)
        {
            var currentSplit = cursor + splitLength < plainBytes.Length ? plainBytes[cursor..(cursor + splitLength)] : plainBytes[cursor..];

            if (i > 0)
            {
                result.Append('#');  // 用 # 作为分隔符 (#不在Base64和Hex字符集中)
            }

            var encryptedBytes = rsa.Encrypt(currentSplit, RSAEncryptionPadding.OaepSHA256);

            if (encoding == OutEncoding.Base64)
            {
                result.Append(Convert.ToBase64String(encryptedBytes));
            }
            else
            {
                result.Append(Convert.ToHexString(encryptedBytes));
            }
        }

        return result.ToString();
    }

    public static async Task<string> Decrypt(string keyName, string cipherText, OutEncoding encoding, string password)
    {
        // 私钥解密
        var key = await GetKey(keyName, true);
        if (key == null)
        {
            return null;
        }

        var rsa = RSA.Create();
        if (password == null)
        {
            rsa.ImportFromPem(key);
        }
        else
        {
            rsa.ImportFromEncryptedPem(key, password);
        }

        var memoryStream = new MemoryStream();
        foreach (var cipherSplit in cipherText.Split("#"))
        {
            byte[] cipherBytes;
            if (encoding == OutEncoding.Base64)
            {
                cipherBytes = Convert.FromBase64String(cipherSplit);
            }
            else
            {
                cipherBytes = Convert.FromHexString(cipherSplit);
            }

            memoryStream.Write(rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256));
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public static async Task<string> Sign(string keyName, string plainText, OutEncoding encoding, string password)
    {
        // 私钥签名
        var key = await GetKey(keyName, true) ?? throw new KeyDeletedException("key " + keyName + " deleted.");
        var rsa = RSA.Create();
        if (password == null)
        {
            rsa.ImportFromPem(key);
        }
        else
        {
            rsa.ImportFromEncryptedPem(key, password);
        }

        var signHash = rsa.SignData(Encoding.UTF8.GetBytes(plainText), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        if (encoding == OutEncoding.Base64)
        {
            return Convert.ToBase64String(signHash);
        }
        else
        {
            return Convert.ToHexString(signHash);
        }
    }

    public static async Task<bool?> Verify(string keyName, string plainText, string hash, OutEncoding encoding)
    {
        // 公钥校验
        var key = await GetKey(keyName, false) ?? throw new KeyDeletedException("key " + keyName + " deleted.");
        var rsa = RSA.Create();
        rsa.ImportFromPem(key);

        byte[] hashBytes;
        if (encoding == OutEncoding.Base64)
        {
            hashBytes = Convert.FromBase64String(hash);
        }
        else
        {
            hashBytes = Convert.FromHexString(hash);
        }

        return rsa.VerifyData(Encoding.UTF8.GetBytes(plainText), hashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public static RSA GenerateNewKey(int keySize)
    {
        return RSA.Create(keySize);
    }

    private static async Task<StorageFolder> GetKeyFolder()
    {
        var storageFolder = ApplicationData.Current.LocalFolder;
        return await QuickTools.GetOrCreateSubFolder(storageFolder, "keys");
    }

    public static async Task<bool> CheckKeyExistence()
    {
        var keyFolder = await GetKeyFolder();
        var keys = await keyFolder.GetFilesAsync();

        return keys.Count > 0;
    }

    public static async Task<string> GetKey(string name, bool isPrivate)
    {
        var keyFolder = await GetKeyFolder();
        var fileName = name + (isPrivate ? ".prv.pem" : ".pub.pem");
        if (!await QuickTools.CheckFileExistence(keyFolder, fileName))
        {
            return null;
        }

        var file = await keyFolder.GetFileAsync(fileName);

        var reader = new StreamReader(await file.OpenStreamForReadAsync());
        var content = await reader.ReadToEndAsync();
        reader.Close();

        return content;
    }

    public static async Task<bool> CheckPubKey(StorageFile pubFile)
    {
        var reader = new StreamReader(await pubFile.OpenStreamForReadAsync());
        var begin = await reader.ReadLineAsync();
        reader.Close();

        return begin.Equals("-----BEGIN RSA PUBLIC KEY-----") || begin.Equals("-----BEGIN PUBLIC KEY-----");
    }

    public static async Task<bool> CheckPrvKey(StorageFile prvFile)
    {
        var reader = new StreamReader(await prvFile.OpenStreamForReadAsync());
        var begin = await reader.ReadLineAsync();
        reader.Close();

        return begin.Equals("-----BEGIN RSA PRIVATE KEY-----") || begin.Equals("-----BEGIN PRIVATE KEY-----") || begin.Equals("-----BEGIN ENCRYPTED PRIVATE KEY-----");
    }

    public static async Task<bool> ImportKeys(StorageFile pubFile, StorageFile prvFile, string name)
    {
        var keyFolder = await GetKeyFolder();
        if (await CheckNameExistence(name))
        {
            return false;
        }

        if (pubFile != null)
        {
            await pubFile.CopyAsync(keyFolder, name + ".pub.pem");
        }

        if (prvFile != null)
        {
            await prvFile.CopyAsync(keyFolder, name + ".prv.pem");
        }

        return true;
    }

    public static async Task<bool> CheckNameExistence(string name)
    {
        var keyFolder = await GetKeyFolder();
        if (await QuickTools.CheckFileExistence(keyFolder, name + ".pub.pem") || await QuickTools.CheckFileExistence(keyFolder, name + ".prv.pem"))
        {
            return true;
        }

        return false;
    }

    public static async Task<bool> SaveKey(RSA rsa, string name, string passwd)
    {
        var keyFolder = await GetKeyFolder();

        // 文件名冲突
        if (await CheckNameExistence(name))
        {
            return false;
        }

        var targetPubFile = await keyFolder.CreateFileAsync(name + ".pub.pem");
        var targetPrvFile = await keyFolder.CreateFileAsync(name + ".prv.pem");
        var pubWriter = new StreamWriter(await targetPubFile.OpenStreamForWriteAsync());
        var prvWriter = new StreamWriter(await targetPrvFile.OpenStreamForWriteAsync());

        await pubWriter.WriteAsync(rsa.ExportRSAPublicKeyPem());
        pubWriter.Close();

        if (passwd == null)
        {
            await prvWriter.WriteAsync(rsa.ExportPkcs8PrivateKeyPem());
        }
        else
        {
            await prvWriter.WriteAsync(rsa.ExportEncryptedPkcs8PrivateKeyPem(Encoding.UTF8.GetBytes(passwd), new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 100000)));
        }
        prvWriter.Close();

        return true;
    }

    public static async Task DeleteKey(string name)
    {
        var keyFolder = await GetKeyFolder();

        if (await QuickTools.CheckFileExistence(keyFolder, name + ".pub.pem"))
        {
            var pubFile = await keyFolder.GetFileAsync(name + ".pub.pem");
            await pubFile.DeleteAsync();
        }

        if (await QuickTools.CheckFileExistence(keyFolder, name + ".prv.pem"))
        {
            var prvFile = await keyFolder.GetFileAsync(name + ".prv.pem");
            await prvFile.DeleteAsync();
        }
    }

    public static async Task<List<KeyHolder>> ListKeys()
    {
        var keyFolder = await GetKeyFolder();
        var keys = await keyFolder.GetFilesAsync();
        var validKeyMap = new Dictionary<string, KeyHolder>();

        foreach (var key in keys)
        {
            if (key.Name.EndsWith(".prv.pem"))
            {
                var name = key.Name[..^8];
                var reader = new StreamReader(await key.OpenStreamForReadAsync());
                var begin = await reader.ReadLineAsync();

                if (begin != null && (begin.Equals("-----BEGIN RSA PRIVATE KEY-----") || begin.Equals("-----BEGIN PRIVATE KEY-----") || begin.Equals("-----BEGIN ENCRYPTED PRIVATE KEY-----")))
                {
                    var isEncrypted = begin.Equals("-----BEGIN ENCRYPTED PRIVATE KEY-----");
                    if (validKeyMap.TryGetValue(name, out var value))
                    {
                        value.HasPrivate = true;
                        value.IsEncrypted = isEncrypted;
                    }
                    else
                    {
                        validKeyMap.Add(name, new KeyHolder(name, key.DateCreated, true, isEncrypted));
                    }
                }
            }

            if (key.Name.EndsWith(".pub.pem"))
            {
                var name = key.Name[..^8];
                var reader = new StreamReader(await key.OpenStreamForReadAsync());
                var begin = await reader.ReadLineAsync();
                if (begin != null && (begin.Equals("-----BEGIN RSA PUBLIC KEY-----") || begin.Equals("-----BEGIN PUBLIC KEY-----")))
                {
                    if (!validKeyMap.ContainsKey(name))
                    {
                        validKeyMap.Add(name, new KeyHolder(name, key.DateCreated, false, false));
                    }
                }
            }
        }

        var validKeyList = new List<KeyHolder>();
        foreach (var key in validKeyMap.Values)
        {
            validKeyList.Add(key);
        }

        validKeyList.Sort((p1, p2) => -p1.CreateTime.CompareTo(p2.CreateTime));

        return validKeyList;
    }
}

public class KeyHolder(string name, DateTimeOffset createTime, bool hasPrivate, bool isEncrypted)
{
    public readonly string Name = name;
    public readonly DateTimeOffset CreateTime = createTime;
    public bool HasPrivate = hasPrivate;
    public bool IsEncrypted = isEncrypted;
}
