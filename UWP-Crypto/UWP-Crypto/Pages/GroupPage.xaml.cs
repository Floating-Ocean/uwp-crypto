using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using UWP_Crypto.Utils;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_Crypto.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GroupPage : Page
{
    public GroupPage()
    {
        InitializeComponent();
    }

    private void IVType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var selected = e.AddedItems[0].ToString();

        if (selected == "零向量")
        {
            IVPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            IVPanel.Visibility = Visibility.Visible;
        }
    }

    // 检查密钥长度
    private async Task<bool> CheckKeyLength()
    {
        if (KeyVal.Text.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入密钥。");
            return false;
        }

        var Length = Encoding.UTF8.GetBytes(KeyVal.Text).Length * 8;
        switch (GeneralType.SelectedIndex)
        {
            case 0:
                if (Length != 64)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "DES密钥长度必须为64位。\n当前长度: " + Length + "位。");
                    return false;
                }
                break;
            case 1:
                if (Length != 128 && Length != 192)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "3DES密钥长度必须为128位或192位。\n当前长度: " + Length + "位。");
                    return false;
                }
                break;
            case 2:
                if (Length != 128 && Length != 192 && Length != 256)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "AES密钥长度必须为128位、196位或256位。\n当前长度: " + Length + "位。");
                    return false;
                }
                break;
            default:
                return false;
        }
        return true;
    }

    // 检查IV长度
    private async Task<bool> CheckIVLength()
    {
        if (IVType.SelectedIndex == 0)
        {
            return true;
        }

        if (IVVal.Text.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入IV。");
            return false;
        }

        var Length = Encoding.UTF8.GetBytes(IVVal.Text).Length * 8;
        switch (GeneralType.SelectedIndex)
        {
            case 0:
            case 1:
                if (Length != 64)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "DES/3DES的IV长度必须为64位。\n当前长度: " + Length + "位。");
                    return false;
                }
                break;
            case 2:
                if (Length != 128)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "参数错误", "AES的IV长度必须为128位。\n当前长度: " + Length + "位。");
                    return false;
                }
                break;
            default:
                return false;
        }
        return true;
    }

    private void ChangeEnability(bool enability)
    {
        var views = new List<Control>()
        {
            GeneralType, OutType, InputBox, OutputBox, Paste, Copy, RunType, KeyVal, IVType, IVVal, TranformButton, ClearButton
        };
        foreach (var view in views)
        {
            view.IsEnabled = enability;
        }
    }

    private async void Transform_Click(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        string input;
        InputBox.Document.GetText(TextGetOptions.None, out input);

        if (input.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入需要处理的内容。");
            return;
        }

        if (!await CheckKeyLength() || !await CheckIVLength())
        {
            return;
        }

        ChangeEnability(false);
        ProgressAnim.IsActive = true;

        await RunCrypto();

        ProgressAnim.IsActive = false;
        ChangeEnability(true);
    }

    private async Task RunCrypto()
    {
        var helper = new SymmetricHelper(
            GeneralType.SelectedIndex switch
            {
                2 => SymmetricHelper.Algorithm.AES,
                1 => SymmetricHelper.Algorithm.TripleDES,
                _ => SymmetricHelper.Algorithm.DES,
            },
            KeyVal.Text,
            IVType.SelectedIndex == 0 ? null : IVVal.Text,
            OutType.SelectedIndex == 0 ? SymmetricHelper.OutEncoding.Base64 : SymmetricHelper.OutEncoding.Hex
        );

        string input;
        InputBox.Document.GetText(TextGetOptions.None, out input);

        try
        {
            if (RunType.SelectedIndex == 0)
            {
                OutputBox.Document.SetText(TextSetOptions.None, helper.Encrypt(input));
            }
            else
            {
                OutputBox.Document.SetText(TextSetOptions.None, helper.Decrypt(input));
            }
        }
        catch (FormatException)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "格式错误", "输入非法，请检查格式并选择正确的编码后重试。");
            return;
        }
        catch (Exception)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "密钥或IV错误", "操作失败，请检查密钥或IV的正确性后重试。");
            return;
        }
    }

    private async void Paste_Click(object sender, RoutedEventArgs e)
    {
        var package = Clipboard.GetContent();
        if (package.Contains(StandardDataFormats.Text))
        {
            InputBox.Document.SetText(TextSetOptions.None, await package.GetTextAsync());
        }
        else
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "无法粘贴", "剪贴板上不是纯文本信息。");
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        string output;
        OutputBox.Document.GetText(TextGetOptions.None, out output);

        var package = new DataPackage();
        package.SetText(output);
        Clipboard.SetContent(package);
    }
}
