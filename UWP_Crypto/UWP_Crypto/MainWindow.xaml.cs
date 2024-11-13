using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Composition.SystemBackdrops;
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

namespace UWP_Crypto;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
        ContentFrame.Navigate(
                   typeof(Pages.ClassicPage),
                   null,
                   new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                   );

        SystemBackdrop = new MicaBackdrop()
        {
            Kind = MicaKind.Base
        };

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }

    public string GetAppTitleFromSystem()
    {
        return Windows.ApplicationModel.Package.Current.DisplayName;
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender,
                  NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            ContentFrame.Navigate(typeof(Pages.SettingsPage), null, args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            var newPage = Type.GetType(args.InvokedItemContainer.Tag.ToString());
            ContentFrame.Navigate(
                   newPage,
                   null,
                   args.RecommendedNavigationTransitionInfo
                   );
        }
    }

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;

        if (ContentFrame.SourcePageType == typeof(Pages.SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
        }
        else if (ContentFrame.SourcePageType != null)
        {
            // 处理一下嵌套问题
            foreach (var item in NavigationViewControl.MenuItems.OfType<NavigationViewItem>())
            {
                if (item.Tag != null && item.Tag.Equals(ContentFrame.SourcePageType.FullName.ToString()))
                {
                    NavigationViewControl.SelectedItem = item;
                }
                var subItem = item.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(n => n.Tag != null && n.Tag.Equals(ContentFrame.SourcePageType.FullName.ToString()));
                if (subItem != null)
                {
                    NavigationViewControl.SelectedItem = subItem;
                }
            }
        }

        NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
    }
}
