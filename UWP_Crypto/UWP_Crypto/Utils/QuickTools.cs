using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using System.Xml.Linq;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using UWP_Crypto.Controls;

namespace UWP_Crypto.Utils;

public static class QuickTools
{
    public static async Task OpenSimpleDialog(XamlRoot xamlRoot, string title, string message)
    {
        await OpenSimpleDialog(xamlRoot, title, message, false);
    }

    public static async Task<ContentDialogResult> OpenSimpleDialog(XamlRoot xamlRoot, string title, string message, bool cancelEnabled)
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

        if (cancelEnabled)
        {
            dialog.CloseButtonText = "取消";
        }

        return await dialog.ShowAsync();
    }

    public static async Task<string> OpenPasswordDialog(XamlRoot xamlRoot, string title)
    {
        var process = new PasswordDialog();
        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = xamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = title,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            Content = process,
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        return process.ConfirmEntering();
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
            await OpenSimpleDialog(xamlRoot, "无法粘贴", "剪贴板上不是纯文本信息。");
            return null;
        }
    }

    public static void CopyToClipboard(string content)
    {
        var package = new DataPackage();
        package.SetText(content);
        Clipboard.SetContent(package);
    }

    public static async Task<StorageFolder> GetOrCreateSubFolder(StorageFolder folder, string name)
    {
        try
        {
            return await folder.GetFolderAsync(name);
        }
        catch (FileNotFoundException)
        {
            await folder.CreateFolderAsync("keys");
            return await folder.GetFolderAsync(name);
        }
    }

    public static async Task<bool> CheckFileExistence(StorageFolder folder, string name)
    {
        try
        {
            await folder.GetFileAsync(name);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    public static async Task<StorageFile> OpenFilePicker(List<string> acceptSuffix)
    {
        var openPicker = new FileOpenPicker();
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        openPicker.ViewMode = PickerViewMode.List;
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        foreach (var suffix in acceptSuffix)
        {
            openPicker.FileTypeFilter.Add(suffix);
        }

        return await openPicker.PickSingleFileAsync();
    }

    public static async Task<bool?> SaveFilePicker(string suggestFileName, string content, List<Tuple<string, List<string>>> acceptSuffix)
    {
        var savePicker = new FileSavePicker();
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        foreach (var suffix in acceptSuffix)
        {
            savePicker.FileTypeChoices.Add(suffix.Item1, suffix.Item2);
        }
        savePicker.SuggestedFileName = suggestFileName;

        var file = await savePicker.PickSaveFileAsync();
        if (file == null)
        {
            return null;
        }

        // 预先缓存，防止在导出时源文件被改写
        CachedFileManager.DeferUpdates(file);

        var writer = new StreamWriter(await file.OpenStreamForWriteAsync());
        await writer.WriteAsync(content);
        writer.Close();

        var status = await CachedFileManager.CompleteUpdatesAsync(file);
        return status == FileUpdateStatus.Complete || status == FileUpdateStatus.CompleteAndRenamed;
    }

    public static bool IsValidFileName(string fileName)
    {
        char[] invalidChars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];
        foreach (var c in invalidChars)
        {
            if (fileName.Contains(c))
            {
                return false;
            }
        }
        return true;
    }

}