using DBHelper.Dal;
using DBHelper.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Meizi
{
    /// <summary>
    /// 收藏页面
    /// </summary>
    public sealed partial class CollectionPage : Page
    {
        public CollectionPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

        }

        private void mainContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                return;
            }

            var image = (e.AddedItems[0] as GridViewItem).Content as Image;
            Url urlDetail = new Url
            {
                LinkUrl = image.Tag.ToString(),
                ImageUrl = (image.Source as BitmapImage).UriSource.AbsoluteUri
            };
            this.Frame.Navigate(typeof(ShowPage), urlDetail);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int countInRow = (int)mainContent.ActualWidth / 200;
            var imageWidth = mainContent.ActualWidth / countInRow - 5;
            foreach (var item in mainContent.Items)
            {

                var image = ((GridViewItem)item).Content as Image;
                image.Width = imageWidth;
                image.Height = imageWidth / 2 * 3;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                return;
            }

            int countInRow = (int)mainContent.ActualWidth / 200;
            var imageWidth = mainContent.ActualWidth / countInRow - 5;

            CollectionService dal = new CollectionService();
            var listUrls = dal.GetList(r => true);
            //mainContent.ItemsSource = listUrls;
            foreach (var item in listUrls)
            {
                GridViewItem gvi = new GridViewItem();
                Image img = new Image();
                img.Source = new BitmapImage(new Uri(item.ImageUrl));
                img.Tag = item.LinkUrl;
                img.Width = imageWidth;
                img.Height = imageWidth / 2 * 3;
                gvi.Content = img;
                mainContent.Items.Add(gvi);
            }
            Loading.IsActive = false;
        }
    }
}
