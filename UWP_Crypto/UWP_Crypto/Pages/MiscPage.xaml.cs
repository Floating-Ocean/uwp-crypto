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
public sealed partial class MiscPage : Page
{
    public MiscPage()
    {
        InitializeComponent();
    }

    private void ChangeEnability(bool enability)
    {
        var views = new List<Control>()
        {
            GeneralType, InputBox, OutputBox, Paste, Copy, RunType, TranformButton, ClearButton
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

        ChangeEnability(false);
        ProgressAnim.IsActive = true;

        var generalType = GeneralType.SelectedIndex;
        var runType = RunType.SelectedIndex;

        await Task.Run(() => RunCrypto(generalType, runType, input));

        ProgressAnim.IsActive = false;
        ChangeEnability(true);
    }

    private void RunCrypto(int generalType, int runType, string input)
    {
        try
        {
            var result = string.Empty;
            if (generalType == 0)
            {
                if (runType == 0)
                {
                    result = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
                }
                else
                {
                    result = Encoding.UTF8.GetString(Convert.FromBase64String(input));
                }
            }
            else
            {
                if (runType == 0)
                {
                    result = Convert.ToHexString(Encoding.UTF8.GetBytes(input));
                }
                else
                {
                    result = Encoding.UTF8.GetString(Convert.FromHexString(input));
                }
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
