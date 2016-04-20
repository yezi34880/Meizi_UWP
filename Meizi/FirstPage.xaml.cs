using DBHelper.Dal;
using DBHelper.Model;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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

        public FirstPage()
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
            Load("http://www.mzitu.com/");
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
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int countInRow = (int)mainContent.ActualWidth / 200;
            if (countInRow < 2)
            {
                countInRow = 2;
            }
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

        private async void Scrollbar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //滚动到底部 加载更多
            if (!Loading.IsActive && e.NewValue == (sender as ScrollBar).Maximum)
            {
                var page = mainContent.Tag as PageNavi;
                if (page.pageNow >= page.pageCount)
                {
                    return;
                }
                Loading.IsActive = true;

                try
                {
                    var index = page.pageNow;
                    var item = mainNavigationList.SelectedItem as ListBoxItem;

                    var LinkUrl = item == null ? "http://www.mzitu.com/" : item.Tag.ToString();
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
            Url urlDetail = e.AddedItems[0] as Url;
            this.Frame.Navigate(typeof(ShowPage), urlDetail);
        }


        #region 汉堡菜单
        private void menu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mainSplitView.IsPaneOpen = !mainSplitView.IsPaneOpen;
        }

        private async void ListBoxItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var item = sender as ListBoxItem;
                if (item.Name == "AboutItem") //关于
                {
                    AboutDialog ad = new AboutDialog();
                    await ad.ShowAsync();
                    return;
                }
                if (item.Name == "SubjectItem") //专题分类
                {
                    this.Frame.Navigate(typeof(SubjectPage));
                    return;
                }
                if (item.Name == "CollectItem")  //收藏
                {
                    this.Frame.Navigate(typeof(CollectionPage));
                    return;
                }

                //其他页面
                var LinkUrl = (item.Tag ?? "").ToString();
                if (String.IsNullOrEmpty(LinkUrl))
                {
                    return;
                }
                Loading.IsActive = true;
                txtTitle.Text = (item.Content as StackPanel).Tag.ToString();
                string html = await Helper.GetHttpWebRequest(LinkUrl);
                Helper.ShowImageList(html, mainContent);
                Loading.IsActive = false;
            }
            catch (Exception ex)
            {
                Helper.WriteExceptionLog(ex.Message);
            }
        }

        #endregion

        private async void MenuFlyoutItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var item = sender as MenuFlyoutItem;
            if (item != null)
            {
                Loading.IsActive = true;
                string html = await Helper.GetHttpWebRequest(item.Tag.ToString());
                Helper.ShowImageList(html, mainContent);
                Loading.IsActive = false;
            }
        }
    }
}
