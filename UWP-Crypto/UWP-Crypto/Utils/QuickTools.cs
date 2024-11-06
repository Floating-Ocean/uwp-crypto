using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

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
}
