using DBHelper.Dal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        }

        private void mainContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                return;
            }
            Url urlDetail = new Url
            {
                LinkUrl = ((GridViewItem)e.AddedItems[0]).Tag.ToString(),
                ImageUrl = (((Image)((GridViewItem)e.AddedItems[0]).Content).Source as BitmapImage).UriSource.AbsoluteUri
            };
            this.Frame.Navigate(typeof(ShowPage), urlDetail);

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CollectionService dal = new CollectionService();
                var list = dal.GetList(r => true);
                double imageHeight = 354;
                double imageWidth = 236;

                foreach (var item in list)
                {
                    GridViewItem gvi = new GridViewItem();
                    gvi.Tag = item.LinkUrl;
                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(item.ImageUrl));
                    image.Height = imageHeight;
                    image.Width = imageWidth;
                    gvi.Content = image;
                    mainContent.Items.Add(gvi);
                }
            }
            catch (Exception)
            {

            }
        }

    }
}
