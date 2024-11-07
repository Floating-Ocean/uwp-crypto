using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;

namespace UWP_Crypto.Utils;
public static class QuickTools
{
    public static async Task OpenSimpleDialog(XamlRoot xamlRoot, string title, string message)
    {
        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = xamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = title,
            PrimaryButtonText = "好的",
            DefaultButton = ContentDialogButton.Primary,
            Content = message
        };

        await dialog.ShowAsync();
    }

    // 太长了，写函数好读一点
    public static long Mod26(long x)
    {
        return (26 + x % 26) % 26;
    }

    public static long InverseMod26(long x)
    {
        var inv = new int[26]
        {
            -1, 1, -1, 9, -1, 21, -1, 15, -1, 3, -1, 19, -1, -1, -1, 7, -1, 23, -1, 11, -1, 5, -1, 17, -1, 25
        };  // 打表加快求逆速度

        x = Mod26(x);

        if (inv[x] == -1)
        {
            throw new ArgumentException("Inversion failed.");
        }

        return inv[x];
    }

    public static async Task<string> GetClipboard(XamlRoot xamlRoot)
    {
        var package = Clipboard.GetContent();
        if (package.Contains(StandardDataFormats.Text))
        {
            return await package.GetTextAsync();
        }
        else
        {
            await QuickTools.OpenSimpleDialog(xamlRoot, "无法粘贴", "剪贴板上不是纯文本信息。");
            return null;
        }
    }

    public static void CopyToClipboard(string content)
    {
        var package = new DataPackage();
        package.SetText(content);
        Clipboard.SetContent(package);
    }
}
