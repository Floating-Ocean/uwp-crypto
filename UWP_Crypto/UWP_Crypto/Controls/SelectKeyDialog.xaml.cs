using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using UWP_Crypto.Utils;
using System.Collections.ObjectModel;
using UWP_Crypto.Pages;
using UWP_Crypto.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_Crypto.Controls;
public sealed partial class SelectKeyDialog : UserControl
{
    public ObservableCollection<PublicKeyItem> PublicKeyItems { get; set; } = [];

    public SelectKeyDialog()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshKeys();
    }

    private async Task RefreshKeys()
    {
        await Task.Run(async () =>
        {
            var keys = await RSAHelper.ListKeys();
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                ListProgress.IsActive = false;
                PublicKeyItems.Clear();
                foreach (var key in keys)
                {
                    PublicKeyItems.Add(new PublicKeyItem(key));
                }
            });
        });
    }

    public async Task<PublicKeyItem> ConfirmSelection()
    {
        if (KeyList.SelectedItem == null)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "未选择密钥", "请点击选择一个密钥并继续。");
            return null;
        }

        return KeyList.SelectedItem as PublicKeyItem;
    }
}

