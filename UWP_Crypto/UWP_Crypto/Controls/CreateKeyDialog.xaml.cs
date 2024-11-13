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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_Crypto.Controls;
public sealed partial class CreateKeyDialog : UserControl
{
    public CreateKeyDialog()
    {
        InitializeComponent();
    }

    private void Encrypt_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        EncryptPasswd.Visibility = Visibility.Visible;
        EncryptPasswdCheck.Visibility = Visibility.Visible;
    }

    private void Encrypt_Unchecked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        EncryptPasswd.Visibility = Visibility.Collapsed;
        EncryptPasswdCheck.Visibility = Visibility.Collapsed;
    }

    public void RecoverState(Tuple<string, bool, string> cache)
    {
        if (cache == null)
        {
            return;
        }

        KeyName.Text = cache.Item1;
        EncryptChecker.IsChecked = cache.Item2;
        if (cache.Item2)
        {
            EncryptPasswd.Visibility = Visibility.Visible;
            EncryptPasswdCheck.Visibility = Visibility.Visible;
        }
        else
        {
            EncryptPasswd.Visibility = Visibility.Collapsed;
            EncryptPasswdCheck.Visibility = Visibility.Collapsed;
        }

        EncryptPasswd.Password = cache.Item3;
    }

    public Tuple<string, bool, string> GetState()
    {
        return new Tuple<string, bool, string>(KeyName.Text, EncryptChecker.IsChecked == true, EncryptPasswd.Password);
    }

    public async Task<bool> ConfirmCreatingKey()
    {
        if (KeyName.Text.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入密钥别称。密钥别称用于在本系统中唯一标识密钥。");
            return false;
        }

        if (!QuickTools.IsValidFileName(KeyName.Text))
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "密钥别称不能包含 \\, /, :, *, ?, \", <, >, |。");
            return false;
        }

        if (EncryptChecker.IsChecked == true)
        {
            if (EncryptPasswd.Password.Length == 0)
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入加密私钥的密码。在后续使用该私钥时都需要提供该密码。");
                return false;
            }
            if (EncryptPasswdCheck.Password.Length == 0)
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入再次输入加密私钥的密码。");
                return false;
            }
            if (EncryptPasswd.Password != EncryptPasswdCheck.Password)
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "两次输入的密码不一致。");
                return false;
            }
        }

        var rsa = RSAHelper.GenerateNewKey((1 << KeySize.SelectedIndex) * 1024); //1024, 2048, 4096
        var added = await RSAHelper.SaveKey(
            rsa,
            KeyName.Text,
            EncryptChecker.IsChecked == true ? EncryptPasswd.Password : null
        );

        if (!added)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "该别名已存在，请重新输入。");
            return false;
        }

        return true;
    }
}
