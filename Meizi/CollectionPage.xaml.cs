using DBHelper.Dal;
using DBHelper.Model;
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

        private List<Collection> listUrls = new List<Collection>();

        public CollectionPage()
        {
            this.InitializeComponent();
            CollectionService dal = new CollectionService();
            listUrls = dal.GetList(r => true);
            Loading.IsActive = false;
        }

        private void mainContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                return;
            }
            var image = (Collection)e.AddedItems[0];
            Url urlDetail = new Url
            {
                LinkUrl = image.LinkUrl,
                ImageUrl = image.ImageUrl
            };
            this.Frame.Navigate(typeof(ShowPage), urlDetail);
        }

    }
}
