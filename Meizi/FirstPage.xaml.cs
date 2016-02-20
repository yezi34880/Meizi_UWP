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
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FirstPage : Page
    {
        string pageUrl;

        bool isLoading = false;
        public FirstPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //这个e.Parameter是获取传递过来的参数
            this.pageUrl = e.Parameter.ToString();
        }

        private void mainContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                return;
            }
            var urlDetail = ((GridViewItem)e.AddedItems[0]).Tag.ToString();

            this.Frame.Navigate(typeof(ShowPage), urlDetail);

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string html = await Helper.GetHttpWebRequest(pageUrl);
                Helper.ShowImageList(html, mainContent);

            }
            catch (Exception)
            {

            }

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
            if (!isLoading && e.NewValue == (sender as ScrollBar).Maximum)
            {
                var page = mainContent.Tag as PageNavi;
                if (page.pageNow > page.pageCount)
                {
                    return;
                }
                isLoading = true;
                try
                {
                    var index = page.pageNow;
                    string url = this.pageUrl + "/page/" + (index + 1).ToString();
                    string html = await Helper.GetHttpWebRequest(url);
                    Helper.AppendImageList(html, mainContent);
                    page.pageNow++;
                }
                catch (Exception)
                {


                }

                isLoading = false;
            }
        }

    }


}
