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
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GridPage : Page
    {
        string loadUrl;

        public GridPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                return;
            }
            //这个e.Parameter是获取传递过来的参数
            loadUrl = e.Parameter.ToString();
            Load(loadUrl);
        }
        private async void Load(string strUrl)
        {
            try
            {
                string html = await Helper.GetHtmlLoop(strUrl);


                Helper.ShowImageList(html, mainContent);
                Loading.IsActive = false;

            }
            catch (Exception e)
            {
                Helper.WriteExceptionLog(e.Message);
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

                    var LinkUrl = loadUrl;
                    if (String.IsNullOrEmpty(LinkUrl))
                    {
                        return;
                    }
                    string url = LinkUrl + "/page/" + (index + 1).ToString();

                    string html;
                    do
                    {
                        html = await Helper.GetHttpWebRequest(url);
                        if (String.IsNullOrEmpty(html) == false)
                        {
                            break;
                        }
                    }
                    while (true);
                    Helper.AppendImageList(html, mainContent);
                    page.pageNow++;
                }
                catch (Exception ex)
                {
                    Helper.WriteExceptionLog(ex.Message);
                }
                Loading.IsActive = false;
            }
        }
        private void mainContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                return;
            }
            var gvi = e.AddedItems[0] as GridViewItem;
            var image = gvi.Content as Image;

            Url urlDetail = new Url()
            {
                ImageUrl = (image.Source as BitmapImage).UriSource.OriginalString,
                LinkUrl = image.Tag.ToString()
            };
            this.Frame.Navigate(typeof(ShowPage), urlDetail);

        }

    }
}
