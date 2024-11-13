using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using UWP_Crypto.Utils;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_Crypto.Controls;
public sealed partial class ImportKeyDialog : UserControl
{
    private StorageFile PubFile, PrvFile;

    public ImportKeyDialog()
    {
        InitializeComponent();
    }

    private string ParseFileName(string fileName)
    {
        if(fileName.EndsWith(".pub.pem") || fileName.EndsWith(".prv.pem"))
        {
            return fileName[..^8];
        }
        else
        {
            return fileName[..^4];
        }
    }

    private async void PickPubKeyButton_Click(object sender, RoutedEventArgs e)
    {
        var pubFile = await QuickTools.OpenFilePicker([".pem"]);
        if (pubFile == null)
        {
            return;
        }

        if (!await RSAHelper.CheckPubKey(pubFile))
        {
            PickPubKeyInfo.Text = "不是有效的公钥pem文件";
            return;
        }

        PubFile = pubFile;

        PickPubKeyInfo.Text = "已选择: " + PubFile.Name;

        if (KeyName.Text.Length == 0)
        {
            KeyName.Text = ParseFileName(PubFile.Name);
        }
    }

    private async void PickPrvKeyButton_Click(object sender, RoutedEventArgs e)
    {
        var prvFile = await QuickTools.OpenFilePicker([".pem"]);
        if (prvFile == null)
        {
            return;
        }

        if (!await RSAHelper.CheckPrvKey(prvFile))
        {
            PickPrvKeyInfo.Text = "不是有效的私钥pem文件";
            return;
        }

        PrvFile = prvFile;

        PickPrvKeyInfo.Text = "已选择: " + PrvFile.Name;

        if (KeyName.Text.Length == 0)
        {
            KeyName.Text = ParseFileName(PrvFile.Name);
        }
    }

    public void RecoverState(Tuple<StorageFile, StorageFile, string> cache)
    {
        if (cache == null)
        {
            return;
        }

        PubFile = cache.Item1;
        PrvFile = cache.Item2;
        KeyName.Text = cache.Item3;

        if (PubFile != null)
        {

            PickPubKeyInfo.Text = "已选择: " + PubFile.Name;
        }
        if (PrvFile != null)
        {

            PickPrvKeyInfo.Text = "已选择: " + PrvFile.Name;
        }
    }

    public Tuple<StorageFile, StorageFile, string> GetState()
    {
        return new Tuple<StorageFile, StorageFile, string>(PubFile, PrvFile, KeyName.Text);
    }

    public async Task<bool> ConfirmImportingKey()
    {
        if (KeyName.Text.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入密钥别称。密钥别称用于在本系统中唯一标识密钥。");
            return false;
        }

        if (PubFile == null)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "文件缺失", "请选择公钥。");
            return false;
        }

        var isImported = await RSAHelper.ImportKeys(PubFile, PrvFile, KeyName.Text);

        if (!isImported)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "该别名已存在，请重新输入。");
            return false;
        }

        return true;
    }
}
