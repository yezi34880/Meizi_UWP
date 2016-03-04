using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    /// 列表展示图片页面，解析一级目录网址
    /// </summary>
    public sealed partial class FirstPage : Page
    {
        Url pageUrl;

        public FirstPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }


        private async void Load()
        {
            try
            {
                if (String.IsNullOrEmpty(this.pageUrl.LinkUrl))
                {
                    return;
                }
                string html = await Helper.GetHttpWebRequest(this.pageUrl.LinkUrl);
                Helper.ShowImageList(html, mainContent);
                Loading.IsActive = false;



            }
            catch (Exception)
            {

            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                return;
            }
            //这个e.Parameter是获取传递过来的参数
            this.pageUrl = (Url)e.Parameter;
            Load();
        }

        private void mainContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                return;
            }
            var item = (GridViewItem)e.AddedItems[0];
            Url urlDetail = new Url
            {
                LinkUrl = item.Tag.ToString(),
                ImageUrl = (((Image)item.Content).Source as BitmapImage).UriSource.AbsoluteUri
            };
            if (String.IsNullOrEmpty(urlDetail.LinkUrl))
            {
                return;
            }
            this.Frame.Navigate(typeof(ShowPage), urlDetail);
        }

        private async void Scrollbar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //滚动到底部 加载更多
            if (!Loading.IsActive && e.NewValue == (sender as ScrollBar).Maximum)
            {
                var page = mainContent.Tag as PageNavi;
                if (page.pageNow > page.pageCount)
                {
                    return;
                }
                Loading.IsActive = true;

                try
                {
                    var index = page.pageNow;
                    string url = this.pageUrl.LinkUrl + "/page/" + (index + 1).ToString();
                    string html = await Helper.GetHttpWebRequest(url);
                    Helper.AppendImageList(html, mainContent);
                    page.pageNow++;
                }
                catch (Exception)
                {


                }
                Loading.IsActive = false;
            }
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

        private void mainContent_Loaded(object sender, RoutedEventArgs e)
        {
            //获取 滚动条 
            var scrollviewer = Helper.FindVisualChildByName<ScrollViewer>(mainContent, "ScrollViewer");
            var scrollbar = Helper.FindVisualChildByName<ScrollBar>(scrollviewer, "VerticalScrollBar");
            if (scrollbar != null)
            {
                scrollbar.ValueChanged += Scrollbar_ValueChanged;//绑定滚动事件
            }
        }
    }
}
