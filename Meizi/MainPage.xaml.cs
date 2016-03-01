using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Meizi
{
    /// <summary>
    /// 主页面 （框架）
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            SystemNavigationManager.GetForCurrentView().BackRequested += (sender0, e0) =>
            {
                if (frameMain.CanGoBack)
                {
                    e0.Handled = true;
                    frameMain.GoBack();
                }
            };
        }

        private void ListBoxItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListBoxItem tapped_item = sender as ListBoxItem;
            if (tapped_item != null && tapped_item.Tag != null && tapped_item.Tag.ToString().Equals("0")) //汉堡按钮
            {
                mainSplitView.IsPaneOpen = !mainSplitView.IsPaneOpen;
            }
        }



        private void mainNavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                return;
            }
            var index = (e.AddedItems[0] as ListBoxItem).Tag.ToString();
            if (index == "0")
            {
                return;
            }
            Url url = new Url();
            switch (index)
            {
                case "1": url.LinkUrl = "http://www.mzitu.com/"; break;
                case "2": url.LinkUrl = "http://www.mzitu.com/xinggan"; break;
                case "3": url.LinkUrl = "http://www.mzitu.com/japan"; break;
                case "4": url.LinkUrl = "http://www.mzitu.com/taiwan"; break;
                case "5": frameMain.Navigate(typeof(CollectionPage)); return;

            }
            if (String.IsNullOrEmpty(url.LinkUrl))
            {
                return;
            }
            frameMain.Navigate(typeof(FirstPage), url);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            frameMain.Navigate(typeof(FirstPage), new Url
            {
                ImageUrl = "",
                LinkUrl = "http://www.mzitu.com/"
            });
        }

        private void frameMain_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Parameter == null)
            {
                return;
            }
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                frameMain.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            var p = ((Url)e.Parameter).LinkUrl;
            if (p == "http://www.mzitu.com/" || p == "http://www.mzitu.com/xinggan" || p == "http://www.mzitu.com/japan" || p == "http://www.mzitu.com/taiwan")
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

        }

    }
}
