using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
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
using UWP_Crypto.Controls;
using UWP_Crypto.Data;
using UWP_Crypto.Lang;
using UWP_Crypto.Utils;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_Crypto.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PublicSignPage : Page
{
    private PublicKeyItem SelectedKey;

    public PublicSignPage()
    {
        InitializeComponent();
    }

    private async Task OpenSelectDialog()
    {
        if (!await RSAHelper.CheckKeyExistence())
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "��Կ������", "���� \"��Կ����\" ҳ�½�������Կ�����ԡ�");
            return;
        }

        var process = new SelectKeyDialog();
        var dialog = new ContentDialog
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = SelectedKey == null ? "ѡ����Կ" : "����ѡ����Կ",
            PrimaryButtonText = "ȷ��",
            CloseButtonText = "ȡ��",
            DefaultButton = ContentDialogButton.Primary,
            Content = process,
        };

        var result = await dialog.ShowAsync();

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var selectedKey = await process.ConfirmSelection();

        if (selectedKey != null)
        {
            SelectedKey = selectedKey;
            RefreshKey();
            return;
        }

        // ���²���
        await OpenSelectDialog();
    }

    private void RefreshKey()
    {
        if (SelectedKey == null)
        {
            return;
        }

        KeyIcon.Glyph = "\uEB95";
        KeyName.Text = SelectedKey.Name;
        KeyInfo.Text = SelectedKey.Info;

        CheckPrvKeyExistence();
    }

    private void CheckPrvKeyExistence()
    {
        if (RunType.SelectedIndex == 0 && SelectedKey != null && !SelectedKey.Holder.HasPrivate)
        {
            NoPrvKeyInfo.Visibility = Visibility.Visible;
            TranformButton.IsEnabled = false;
        }
        else
        {
            NoPrvKeyInfo.Visibility = Visibility.Collapsed;
            TranformButton.IsEnabled = true;
        }
    }

    private void ChangeEnability(bool enability)
    {
        var views = new List<Control>()
        {
            ChangeKeyButton, InputBox, OutputBox, Paste, Copy, RunType, OutType, TranformButton, ClearButton
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

        string input, output;
        InputBox.Document.GetText(TextGetOptions.AdjustCrlf, out input);
        OutputBox.Document.GetText(TextGetOptions.AdjustCrlf, out output);

        if (input.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "����ȱʧ", "��������Ҫ��������ݡ�");
            return;
        }

        if (RunType.SelectedIndex == 1 && output.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "����ȱʧ", "��������ҪУ������ݵĹ�ϣֵ��");
            return;
        }

        if (SelectedKey == null)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "��Կȱʧ", "��ѡ��һ����Կ��");
            return;
        }

        string password = null;
        if (RunType.SelectedIndex == 0 && SelectedKey.Holder.IsEncrypted)
        {
            var entered = await QuickTools.OpenPasswordDialog(XamlRoot, "���뱣��˽Կ�Ŀ���");
            if (entered == null)
            {
                return;
            }
            password = entered;
        }

        ChangeEnability(false);
        ProgressAnim.IsActive = true;

        var runType = RunType.SelectedIndex;
        var outType = OutType.SelectedIndex;

        await Task.Run(() => RunCrypto(runType, outType, input, output, password));

        ProgressAnim.IsActive = false;
        ChangeEnability(true);
    }

    private async void RunCrypto(int runType, int outType, string input, string output, string password)
    {
        try
        {
            if (runType == 0)
            {
                var result = string.Empty;
                if (outType == 0)
                {
                    result = await RSAHelper.Sign(SelectedKey.Name, input, RSAHelper.OutEncoding.Base64, password);
                }
                else
                {
                    result = await RSAHelper.Sign(SelectedKey.Name, input, RSAHelper.OutEncoding.Hex, password);
                }
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    OutputBox.Document.SetText(TextSetOptions.None, result);
                });
            }
            else
            {
                bool? result = null;
                if (outType == 0)
                {
                    result = await RSAHelper.Verify(SelectedKey.Name, input, output, RSAHelper.OutEncoding.Base64);
                }
                else
                {
                    result = await RSAHelper.Verify(SelectedKey.Name, input, output, RSAHelper.OutEncoding.Hex);
                }
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
                {
                    if (result == true)
                    {
                        await QuickTools.OpenSimpleDialog(XamlRoot, "У��ɹ�", "��ϣֵУ��ͨ����");
                    }
                    else
                    {
                        await QuickTools.OpenSimpleDialog(XamlRoot, "У��ʧ��", "��ϣֵ���ı�����Կ��ƥ�䡣");
                    }
                });
            }
        }
        catch (FormatException)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "��ʽ����", "����Ƿ��������ʽ��ѡ����ȷ�ı�������ԡ�");
            });
            return;
        }
        catch (CryptographicException)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "��Կ����",
                    "��������Կ��ƥ��" + ((password != null && runType == 1) ? "��˽Կ�������" : "��"));
            });
            return;
        }
        catch (KeyDeletedException)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
            {
                await QuickTools.OpenSimpleDialog(XamlRoot, "��Կȱʧ", "��ǰѡ�����Կ�����ѱ�ɾ����");
            });
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

    private async void ChangeKey_Click(object sender, RoutedEventArgs e)
    {
        // ��ֹ�������� (���������)
        ChangeKeyButton.IsEnabled = false;
        await OpenSelectDialog();
        ChangeKeyButton.IsEnabled = true;
    }

    private void RunType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        CheckPrvKeyExistence();

        if (RunType.SelectedIndex == 0)
        {
            InputBox.Header = "��������";
            OutputBox.Header = "ǩ����Ĺ�ϣֵ";
            Copy.Content = "���Ƶ�������";
        }
        else
        {
            InputBox.Header = "��������";
            OutputBox.Header = "�����Ӧ�Ĺ�ϣֵ";
            Copy.Content = "�Ӽ������ȡ";
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
