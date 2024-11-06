using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UWP_Crypto.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OldPage : Page
{
    public OldPage()
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
                break;

            default:
            case "自定义":
                InnerType.Visibility = Visibility.Collapsed;
                IdeType.Visibility = Visibility.Visible;
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
}
