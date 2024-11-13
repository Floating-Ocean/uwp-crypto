using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
public sealed partial class ClassicPage : Page
{
    public ClassicPage()
    {
        InitializeComponent();
    }

    // 处理类型选择交互
    private void GeneralType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var selected = e.AddedItems[0].ToString();

        switch (selected)
        {
            case "内置":
                InnerType.Visibility = Visibility.Visible;
                IdeType.Visibility = Visibility.Collapsed;
                ShiftPanel.Visibility = Visibility.Collapsed;
                AffPanel.Visibility = Visibility.Collapsed;
                if (InnerType.SelectedIndex == 1)
                {
                    VigenerePanel.Visibility = Visibility.Visible;
                    RailPanel.Visibility = Visibility.Collapsed;
                }
                else if (InnerType.SelectedIndex == 2)
                {
                    VigenerePanel.Visibility = Visibility.Collapsed;
                    RailPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    VigenerePanel.Visibility = Visibility.Collapsed;
                    RailPanel.Visibility = Visibility.Collapsed;
                }
                break;

            default:
            case "自定义":
                InnerType.Visibility = Visibility.Collapsed;
                IdeType.Visibility = Visibility.Visible;
                VigenerePanel.Visibility = Visibility.Collapsed;
                RailPanel.Visibility = Visibility.Collapsed;
                if (IdeType.SelectedIndex == 0)
                {
                    ShiftPanel.Visibility = Visibility.Visible;
                    AffPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShiftPanel.Visibility = Visibility.Collapsed;
                    AffPanel.Visibility = Visibility.Visible;
                }
                break;
        }
    }

    private void IdeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var selected = e.AddedItems[0].ToString();

        switch (selected)
        {
            case "移位密码":
                ShiftPanel.Visibility = Visibility.Visible;
                AffPanel.Visibility = Visibility.Collapsed;
                break;

            default:
            case "自定义密码":
                ShiftPanel.Visibility = Visibility.Collapsed;
                AffPanel.Visibility = Visibility.Visible;
                break;
        }
    }

    private void InnerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var selected = e.AddedItems[0].ToString();

        switch (selected)
        {
            case "维吉尼亚密码":
                VigenerePanel.Visibility = Visibility.Visible;
                RailPanel.Visibility = Visibility.Collapsed;
                break;

            case "栅栏密码":
                VigenerePanel.Visibility = Visibility.Collapsed;
                RailPanel.Visibility = Visibility.Visible;
                break;

            default:
                VigenerePanel.Visibility = Visibility.Collapsed;
                RailPanel.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private async Task<bool> CheckProperties()
    {
        if (GeneralType.SelectedIndex == 0)
        {
            switch (InnerType.SelectedIndex)
            {
                case 1:
                    if (VigenereKey.Text.Length == 0)
                    {
                        await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入密钥。");
                        return false;
                    }
                    return true;
                case 2:
                    if (RailCnt.Text.Length == 0)
                    {
                        await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入行数。");
                        return false;
                    }
                    return true;
                default:
                    return true;
            }
        }
        else
        {
            switch (IdeType.SelectedIndex)
            {
                case 0:
                    if (ShiftCnt.Text.Length == 0)
                    {
                        await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入左移位数量。");
                        return false;
                    }
                    return true;
                case 1:
                    if (AffA.Text.Length == 0)
                    {
                        await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入A。");
                        return false;
                    }
                    if (AffB.Text.Length == 0)
                    {
                        await QuickTools.OpenSimpleDialog(XamlRoot, "参数缺失", "请输入B。");
                        return false;
                    }
                    return true;
                default:
                    return true;
            }
        }
    }

    private void ChangeEnability(bool enability)
    {
        var views = new List<Control>()
        {
            GeneralType, InnerType, IdeType, OutType, InputBox, OutputBox, Paste, Copy, RunType, VigenereKey, RailCnt, ShiftCnt, AffA, AffB, TranformButton, ClearButton
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

        if (!await CheckProperties())
        {
            return;
        }

        ChangeEnability(false);
        ProgressAnim.IsActive = true;

        var generalType = GeneralType.SelectedIndex;
        var innerType = InnerType.SelectedIndex;
        var ideType = IdeType.SelectedIndex;
        var outType = OutType.SelectedIndex;
        var vigenerKey = VigenereKey.Text;
        var railCnt = (long)RailCnt.Value;
        var shiftCnt = (long)ShiftCnt.Value;
        var affA = (long)AffA.Value;
        var affB = (long)AffB.Value;
        var runType = RunType.SelectedIndex;

        await Task.Run(() => RunCrypto(generalType, innerType, ideType, outType, vigenerKey, railCnt, shiftCnt, affA, affB, runType, input));

        ProgressAnim.IsActive = false;
        ChangeEnability(true);
    }

    private void RunCrypto(int generalType, int innerType, int ideType, int outType, string vigenerKey, long railCnt, long shiftCnt, long affA, long affB, int runType, string input)
    {
        var result = string.Empty;
        if (generalType == 0)
        {
            switch (innerType)
            {
                case 0:
                    if (runType == 0)
                    {
                        result = ClassicHelper.EncryptShift(input, 3);
                    }
                    else
                    {
                        result = ClassicHelper.DecryptShift(input, 3);
                    }
                    break;
                case 1:
                    if (runType == 0)
                    {
                        result = ClassicHelper.EncryptVigenere(input, vigenerKey);
                    }
                    else
                    {
                        result = ClassicHelper.DecryptVigenere(input, vigenerKey);
                    }
                    break;
                case 2:
                    if (runType == 0)
                    {
                        result = ClassicHelper.EncryptRail(input, railCnt);
                    }
                    else
                    {
                        result = ClassicHelper.DecryptRail(input, railCnt);
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (ideType)
            {
                case 0:
                    if (runType == 0)
                    {
                        result = ClassicHelper.EncryptShift(input, shiftCnt);
                    }
                    else
                    {
                        result = ClassicHelper.DecryptShift(input, shiftCnt);
                    }
                    break;
                case 1:
                    if (runType == 0)
                    {
                        result = ClassicHelper.EncryptAffine(input, affA, affB);
                    }
                    else
                    {
                        result = ClassicHelper.DecryptAffine(input, affA, affB);
                    }
                    break;
                default:
                    break;
            }
        }

        if (outType == 1)
        {
            result = result.ToUpper();
        }
        else if (outType == 2)
        {
            result = result.ToLower();
        }

        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
        {
            OutputBox.Document.SetText(TextSetOptions.None, result);
        });
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
