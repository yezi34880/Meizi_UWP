using DBHelper.Dal;
using DBHelper.Model;
using HtmlAgilityPack;
using System;
using System.Linq;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Meizi
{
    /// <summary>
    /// 单张图片展示页面
    /// </summary>
    public sealed partial class ShowPage : Page
    {
        Url url;
        DispatcherTimer timer = new DispatcherTimer();
        public ShowPage()
        {
            this.InitializeComponent();

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //这个e.Parameter是获取传递过来的参数
            this.url = (Url)e.Parameter;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string html = await Helper.GetHttpWebRequest(url.LinkUrl);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                string title = doc.DocumentNode.Descendants("h2").Where(d =>
                   d.Attributes.Contains("class") && d.Attributes["class"].Value == "main-title"
                    ).FirstOrDefault().InnerText;

                var liPageNavi = doc.DocumentNode.Descendants("div").Where(d =>
                    d.Attributes.Contains("class") && d.Attributes["class"].Value == "pagenavi"
                ).FirstOrDefault().ChildNodes;

                int count;
                count = int.Parse(liPageNavi[liPageNavi.Count - 3].FirstChild.InnerText);

                textIndex.Text = String.Format("(1/{0})", count.ToString());
                textTitle.Text = String.Format("{0}              {0}",title);

                for (int i = 0; i < count; i++)
                {
                    LoadImage(i);
                }

                CollectionService dal = new CollectionService();
                var model = dal.GetModel(r => r.ImageUrl == url.ImageUrl);
                if (model != null)
                {
                    tooglebtnCollect.IsChecked = true;
                }

                timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            catch (Exception ex)
            {
                Helper.WriteExceptionLog(ex.Message);
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            var left = textTitle.Margin.Left - 1;
            if (textTitle.Margin.Left + textTitle.ActualWidth/2 <= 0)
            {
                left = 0;
            }
            textTitle.Margin = new Thickness(left, textTitle.Margin.Top, textTitle.Margin.Right, textTitle.Margin.Bottom);
        }

        private async void LoadImage(int index)
        {
            try
            {
                string linkUrl = index > 1 ? String.Format("{0}/{1}", this.url.LinkUrl, index.ToString()) : this.url.LinkUrl;
                string html = await Helper.GetHttpWebRequest(linkUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                string imageUrl = doc.DocumentNode.Descendants("div").Where(d =>
                     d.Attributes.Contains("class") && d.Attributes["class"].Value == "main-image"
                ).FirstOrDefault().FirstChild.FirstChild.FirstChild.GetAttributeValue("src", "");

                FlipViewItem flipitem = new FlipViewItem();
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(imageUrl));
                image.Height = flipMain.ActualHeight;
                ScrollViewer scrollview = new ScrollViewer();
                scrollview.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                scrollview.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                scrollview.ZoomMode = ZoomMode.Enabled;
                scrollview.Content = image;
                flipitem.Tag = imageUrl;
                flipitem.Content = scrollview;

                flipitem.RightTapped += Flipitem_RightTapped;
                flipMain.Items.Add(flipitem);

            }
            catch (Exception ex)
            {
                Helper.WriteExceptionLog(ex.Message);
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

        private void flipMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ((FlipView)sender).SelectedIndex + 1;

            textIndex.Text = String.Format("({0}/{1}", index.ToString(), textIndex.Text.Substring(textIndex.Text.IndexOf('/') + 1));
        }

        private void tooglebtnCollect_Click(object sender, RoutedEventArgs e)
        {
            var isCheck = tooglebtnCollect.IsChecked;
            if (isCheck == true)
            {
                CollectionService dal = new CollectionService();
                dal.Add(new Collection
                {
                    LinkUrl = this.url.LinkUrl,
                    ImageUrl = this.url.ImageUrl,
                    Title = ""
                });
            }
            if (isCheck == false)
            {
                CollectionService dal = new CollectionService();
                dal.Delete(r => r.ImageUrl == this.url.ImageUrl);
            }

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }
    }

}
