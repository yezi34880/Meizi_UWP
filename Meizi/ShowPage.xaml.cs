using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Meizi
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowPage : Page
    {
        string url;
        public ShowPage()
        {
            this.InitializeComponent();

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //这个e.Parameter是获取传递过来的参数
            this.url = e.Parameter.ToString();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string html = await Helper.GetHttpWebRequest(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                string title = doc.DocumentNode.Descendants("h2").Where(d =>
                   d.Attributes.Contains("class") && d.Attributes["class"].Value == "main-title"
                    ).FirstOrDefault().InnerText;
                textTitle.Text = title;

                var liPageNavi = doc.DocumentNode.Descendants("div").Where(d =>
                    d.Attributes.Contains("class") && d.Attributes["class"].Value == "pagenavi"
                ).FirstOrDefault().ChildNodes;
                int count;
                //if (liPageNavi.Count() > 3)
                //{
                count = int.Parse(liPageNavi[liPageNavi.Count - 3].FirstChild.InnerText);
                //}
                //else
                //{
                //    count = 3;
                //}

                for (int i = 0; i < count; i++)
                {
                    LoadImage(i);
                }
                //var lis = doc.GetElementbyId("pins").SelectNodes("li");

            }
            catch (Exception)
            {

            }

        }

        private async void LoadImage(int index)
        {
            try
            {
                string imageHtmlUrl = index > 1 ? String.Format("{0}/{1}", this.url, index.ToString()) : this.url;
                string html = await Helper.GetHttpWebRequest(imageHtmlUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                string imageUrl = doc.DocumentNode.Descendants("div").Where(d =>
                     d.Attributes.Contains("class") && d.Attributes["class"].Value == "main-image"
                ).FirstOrDefault().FirstChild.FirstChild.FirstChild.GetAttributeValue("src", "");

                FlipViewItem flipitem = new FlipViewItem();

                Image image = new Image();
                image.Source = new BitmapImage(new Uri(imageUrl));
                flipitem.Tag = imageUrl;
                flipitem.Content = image;

                flipitem.RightTapped += Flipitem_RightTapped;
                flipMain.Items.Add(flipitem);

            }
            catch (Exception)
            {

            }
        }

        private void Flipitem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var MyFlyout = this.Resources["ImageMenuFlyout"] as MenuFlyout;
            var obj = (FlipViewItem)sender;
            MyFlyout.ShowAt(obj, e.GetPosition(obj));
        }

        private async void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            var nowItem = flipMain.SelectedItem as FlipViewItem;
            if (nowItem == null)
            {
                return;
            }
            var imagePath = nowItem.Tag.ToString();

            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPG文件", new[] { ".jpg" });
            //picker.FileTypeChoices.Add("所有文件", new[] { ".*" });
            picker.SuggestedFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var sFile = await picker.PickSaveFileAsync();
            if (sFile != null)
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                var downloadOp = downloader.CreateDownload(new Uri(imagePath), sFile);
                await downloadOp.StartAsync();
            }
        }

        private async void SaveImageAll_Click(object sender, RoutedEventArgs e)
        {
            var nowItem = flipMain.SelectedItem as FlipViewItem;
            if (nowItem == null)
            {
                return;
            }
            var imagePath = nowItem.Tag.ToString();

            FolderPicker picker = new FolderPicker();

            picker.FileTypeFilter.Add(".jpg");
            //picker.FileTypeFilter.Add("*");

            var sfolder = await picker.PickSingleFolderAsync();
            if (sfolder != null)
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                foreach (FlipViewItem item in flipMain.Items)
                {
                    var imageUrl = item.Tag.ToString();
                    StorageFile sFile = await sfolder.CreateFileAsync(DateTime.Now.ToString("yyyyMMddHHmmssfff") + imageUrl.Substring(imageUrl.LastIndexOf('.')));
                    var downloadOp = downloader.CreateDownload(new Uri(imageUrl), sFile);
                    await downloadOp.StartAsync();
                }
            }

        }


    }

}
