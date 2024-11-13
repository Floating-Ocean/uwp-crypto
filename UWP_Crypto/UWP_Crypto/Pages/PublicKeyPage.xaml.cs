using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using UWP_Crypto.Controls;
using UWP_Crypto.Data;
using UWP_Crypto.Utils;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_Crypto.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PublicKeyPage : Page
{
    public ObservableCollection<PublicKeyItem> PublicKeyItems = [];

    public PublicKeyPage()
    {
        InitializeComponent();
    }

    private async Task OpenKeyDialog(Tuple<string, bool, string> cache)
    {
        var process = new CreateKeyDialog();
        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "创建新密钥",
            PrimaryButtonText = "创建密钥",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = process,
        };
        process.RecoverState(cache);

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        ChangeEnability(false);
        ProgressAnim.IsActive = true;

        var added = await process.ConfirmCreatingKey();

        ProgressAnim.IsActive = false;
        ChangeEnability(true);

        if (added)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "操作成功", "成功创建密钥");
            await RefreshKeys();
            return;
        }

        // 重新操作
        await OpenKeyDialog(process.GetState());
    }

    private async Task OpenImportDialog(Tuple<StorageFile, StorageFile, string> cache)
    {
        var process = new ImportKeyDialog();
        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "导入密钥",
            PrimaryButtonText = "导入密钥",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = process,
        };
        process.RecoverState(cache);

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var imported = await process.ConfirmImportingKey();

        if (imported)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "操作成功", "成功导入密钥");
            await RefreshKeys();
            return;
        }

        // 重新操作
        await OpenImportDialog(process.GetState());
    }

    private async void NewKeyButton_Click(object sender, RoutedEventArgs e)
    {
        await OpenKeyDialog(null);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
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
                if (PublicKeyItems.Count > 0)
                {
                    EmptyPlaceholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    EmptyPlaceholder.Visibility = Visibility.Visible;
                }
            });
        });
    }

    private void KeyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        ExportButton.IsEnabled = KeyList.SelectedItem != null;

        if(KeyList.SelectedItem != null)
        {
            ExportPrvButton.IsEnabled = (KeyList.SelectedItem as PublicKeyItem).Holder.HasPrivate;
        }
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        await OpenImportDialog(null);
    }

    private async void ExportPub_Click(object sender, RoutedEventArgs e)
    {
        var selected = KeyList.SelectedItem;
        if(selected == null)
        {
            return;
        }

        var file = selected as PublicKeyItem;
        await QuickTools.SaveFilePicker(
            file.Name + ".pub.pem",
            await RSAHelper.GetKey(file.Name, false),
            [new Tuple<string, List<string>>("公钥文件", [".pem"])]
        );
    }

    private async void ExportPrv_Click(object sender, RoutedEventArgs e)
    {
        var selected = KeyList.SelectedItem;
        if (selected == null)
        {
            return;
        }

        var file = selected as PublicKeyItem;
        await QuickTools.SaveFilePicker(
            file.Name + ".prv.pem",
            await RSAHelper.GetKey(file.Name, true),
            [new Tuple<string, List<string>>("私钥文件", [".pem"])]
        );
    }

    private async void KeyDelete_Click(object sender, RoutedEventArgs e)
    {
        var item = (sender as FrameworkElement).DataContext;
        var file = item as PublicKeyItem;

        var result = await QuickTools.OpenSimpleDialog(XamlRoot, "删除密钥", "正在准备删除: " + file.Name + "\n操作不可逆。", true);

        if (result == ContentDialogResult.Primary)
        {
            await RSAHelper.DeleteKey(file.Name);
            PublicKeyItems.Remove(file);

            if (PublicKeyItems.Count > 0)
            {
                EmptyPlaceholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyPlaceholder.Visibility = Visibility.Visible;
            }
        }
    }

    private void ChangeEnability(bool enability)
    {
        var views = new List<Control>()
        {
            KeyList, NewKeyButton, ImportButton, ExportPubButton
        };
        foreach (var view in views)
        {
            view.IsEnabled = enability;
        }

        ExportButton.IsEnabled = enability && KeyList.SelectedItem != null;
        ExportPrvButton.IsEnabled = enability && KeyList.SelectedItem != null && (KeyList.SelectedItem as PublicKeyItem).Holder.HasPrivate;
    }


}