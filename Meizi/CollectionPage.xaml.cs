using DBHelper.Dal;
using DBHelper.Model;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
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

        private async void btnDownloadAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                CollectionService dal = new CollectionService();
                var listUrls = dal.GetList(r => true);
                if (listUrls.Count > 0)
                {
                    FolderPicker picker = new FolderPicker();
                    picker.FileTypeFilter.Add(".jpg");
                    var sfolder = await picker.PickSingleFolderAsync();
                    if (sfolder != null)
                    {
                        BackgroundDownloader downloader = new BackgroundDownloader();
                        ToastHelper.ToastShow("开始下载");
                        foreach (var item in listUrls)
                        {
                            string html = await Helper.GetHtmlLoop(item.LinkUrl);

                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(html);
                            string title = doc.DocumentNode.Descendants("h2").Where(d =>
                               d.Attributes.Contains("class") && d.Attributes["class"].Value == "main-title"
                                ).FirstOrDefault().InnerText;
                            var liPageNavi = doc.DocumentNode.Descendants("div").Where(d =>
                                d.Attributes.Contains("class") && d.Attributes["class"].Value == "pagenavi"
                                ).FirstOrDefault().ChildNodes;

                            var folder = await sfolder.CreateFolderAsync(title,CreationCollisionOption.OpenIfExists);

                            int count;
                            count = int.Parse(liPageNavi[liPageNavi.Count - 3].FirstChild.InnerText);
                            for (int i = 0; i < count; i++)
                            {
                                string linkUrl = i > 1 ? String.Format("{0}/{1}", item.LinkUrl, i.ToString()) : item.LinkUrl;
                                string html1 = await Helper.GetHttpWebRequest(linkUrl);
                                HtmlDocument doc1 = new HtmlDocument();
                                doc1.LoadHtml(html1);

                                string imageUrl = doc1.DocumentNode.Descendants("div").Where(d =>
                                     d.Attributes.Contains("class") && d.Attributes["class"].Value == "main-image"
                                ).FirstOrDefault().FirstChild.FirstChild.FirstChild.GetAttributeValue("src", "");
                                StorageFile sFile = await folder.CreateFileAsync(DateTime.Now.ToString("yyyyMMddHHmmssfff") + imageUrl.Substring(imageUrl.LastIndexOf('.')));
                                var downloadOp = downloader.CreateDownload(new Uri(imageUrl), sFile);
                                await downloadOp.StartAsync();
                            }
                        }
                        ToastHelper.ToastShow("下载完成");

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
