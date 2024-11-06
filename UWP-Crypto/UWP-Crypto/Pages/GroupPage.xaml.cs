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

        if (selected == "������")
        {
            IVPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            IVPanel.Visibility = Visibility.Visible;
        }
    }

    // �����Կ����
    private async Task<bool> CheckKeyLength()
    {
        if (KeyVal.Text.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "����ȱʧ", "��������Կ��");
            return false;
        }

        var Length = Encoding.UTF8.GetBytes(KeyVal.Text).Length * 8;
        switch (GeneralType.SelectedIndex)
        {
            case 0:
                if (Length != 64)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "��������", "DES��Կ���ȱ���Ϊ64λ��\n��ǰ����: " + Length + "λ��");
                    return false;
                }
                break;
            case 1:
                if (Length != 128 && Length != 192)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "��������", "3DES��Կ���ȱ���Ϊ128λ��192λ��\n��ǰ����: " + Length + "λ��");
                    return false;
                }
                break;
            case 2:
                if (Length != 128 && Length != 192 && Length != 256)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "��������", "AES��Կ���ȱ���Ϊ128λ��196λ��256λ��\n��ǰ����: " + Length + "λ��");
                    return false;
                }
                break;
            default:
                return false;
        }
        return true;
    }

    // ���IV����
    private async Task<bool> CheckIVLength()
    {
        if (IVType.SelectedIndex == 0)
        {
            return true;
        }

        if (IVVal.Text.Length == 0)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "����ȱʧ", "������IV��");
            return false;
        }

        var Length = Encoding.UTF8.GetBytes(IVVal.Text).Length * 8;
        switch (GeneralType.SelectedIndex)
        {
            case 0:
            case 1:
                if (Length != 64)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "��������", "DES/3DES��IV���ȱ���Ϊ64λ��\n��ǰ����: " + Length + "λ��");
                    return false;
                }
                break;
            case 2:
                if (Length != 128)
                {
                    await QuickTools.OpenSimpleDialog(XamlRoot, "��������", "AES��IV���ȱ���Ϊ128λ��\n��ǰ����: " + Length + "λ��");
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
            await QuickTools.OpenSimpleDialog(XamlRoot, "����ȱʧ", "��������Ҫ��������ݡ�");
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
            await QuickTools.OpenSimpleDialog(XamlRoot, "��ʽ����", "����Ƿ��������ʽ��ѡ����ȷ�ı�������ԡ�");
            return;
        }
        catch (Exception)
        {
            await QuickTools.OpenSimpleDialog(XamlRoot, "��Կ��IV����", "����ʧ�ܣ�������Կ��IV����ȷ�Ժ����ԡ�");
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
            await QuickTools.OpenSimpleDialog(XamlRoot, "�޷�ճ��", "�������ϲ��Ǵ��ı���Ϣ��");
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
