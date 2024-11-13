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
using Windows.Foundation;
using Windows.Foundation.Collections;

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
        InputBox.Document.GetText(TextGetOptions.AdjustCrlf, out input);

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

        var algorithm = GeneralType.SelectedIndex switch
        {
            2 => SymmetricHelper.Algorithm.AES,
            1 => SymmetricHelper.Algorithm.TripleDES,
            _ => SymmetricHelper.Algorithm.DES,
        };
        var key = KeyVal.Text;
        var iv = IVType.SelectedIndex == 0 ? null : IVVal.Text;
        var encoding = OutType.SelectedIndex == 0 ? SymmetricHelper.OutEncoding.Base64 : SymmetricHelper.OutEncoding.Hex;
        var runType = RunType.SelectedIndex;

        await Task.Run(() => RunCrypto(algorithm, key, iv, encoding, runType, input));

        ProgressAnim.IsActive = false;
        ChangeEnability(true);
    }

    private void RunCrypto(SymmetricHelper.Algorithm alogrithm, string key, string iv, SymmetricHelper.OutEncoding encoding, int runType, string input)
    {
        var helper = new SymmetricHelper(alogrithm, key, iv, encoding);

        try
        {
            var result = string.Empty;
            if (runType == 0)
            {
                result = helper.Encrypt(input);
            }
            else
            {
                result = helper.Decrypt(input);
            }

            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                OutputBox.Document.SetText(TextSetOptions.None, result);
            });
        }
        catch (FormatException)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "格式错误", "输入非法，请检查格式并选择正确的编码后重试。");
            });
            return;
        }
        catch (Exception)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "密钥或IV错误", "操作失败，请检查密钥或IV的正确性后重试。");
            });
            return;
        }
    }

    private async void Paste_Click(object sender, RoutedEventArgs e)
    {
        var clipboard = await QuickTools.GetClipboard(XamlRoot);
        if (clipboard != null)
        {
            InputBox.Document.SetText(TextSetOptions.None, clipboard);
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        string output;
        OutputBox.Document.GetText(TextGetOptions.AdjustCrlf, out output);
        QuickTools.CopyToClipboard(output);
    }

    private void RunType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        if (RunType.SelectedIndex == 0)
        {
            InputBox.Header = "输入明文";
            OutputBox.Header = "加密后的结果";
        }
        else
        {
            InputBox.Header = "输入密文";
            OutputBox.Header = "解密后的结果";
        }
    }

    private void DeleteContent_Click(object sender, RoutedEventArgs e)
    {
        if (ClearButton.Flyout is Flyout f)
        {
            f.Hide();
        }
        InputBox.Document.SetText(TextSetOptions.None, "");
        OutputBox.Document.SetText(TextSetOptions.None, "");
    }
}
