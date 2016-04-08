using DBHelper.Dal;
using DBHelper.Model;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
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

        List<Url> listGuess = new List<Url>();


        public ShowPage()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.url = (Url)e.Parameter;
        }

        private async void LoadPage()
        {
            try
            {
                string html = await Helper.GetHtmlLoop(url.LinkUrl);

                tooglebtnSplit.IsChecked = true; //默认展开侧面列表

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
                textTitle.Text = String.Format("{0}              {0}", title);

                CollectionService dal = new CollectionService();
                var model = dal.GetModel(r => r.ImageUrl == url.ImageUrl);
                if (model != null)
                {
                    tooglebtnCollect.IsChecked = true;
                }

                timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                timer.Tick += Timer_Tick;
                timer.Start();

                listGuess.Clear();
                flipMain.Items.Clear();
                listviewMain.Items.Clear();
                for (int i = 0; i < count; i++)
                {
                    await LoadImage(i);
                }

            }
            catch (Exception ex)
            {
                Helper.WriteExceptionLog(ex.Message);
            }

        }


        private void Timer_Tick(object sender, object e)
        {
            var left = textTitle.Margin.Left - 1;
            if (textTitle.Margin.Left + textTitle.ActualWidth / 2 <= 0)
            {
                left = 0;
            }
            textTitle.Margin = new Thickness(left, textTitle.Margin.Top, textTitle.Margin.Right, textTitle.Margin.Bottom);
        }

        private async Task<string> LoadImage(int index)
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
                ScrollViewer scrollview = new ScrollViewer();
                scrollview.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollview.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollview.ZoomMode = ZoomMode.Enabled;

                Image image = new Image();
                image.Source = new BitmapImage(new Uri(imageUrl));
                image.Height = flipMain.ActualHeight;

                scrollview.Content = image;
                flipitem.Tag = imageUrl;
                flipitem.Content = scrollview;

                flipitem.RightTapped += Flipitem_RightTapped;
                flipMain.Items.Add(flipitem);

                ListViewItem lvi = new ListViewItem();
                Image image1 = new Image();
                image1.Source = new BitmapImage(new Uri(imageUrl));

                image1.Width = listviewMain.ActualWidth;
                lvi.Padding = new Thickness(-8, 0, 0, 3);
                lvi.Content = image1;
                listviewMain.Items.Add(lvi);


                //加载 猜你喜欢
                var divGuess = doc.DocumentNode.Descendants("dl").Where(d =>
                       d.Attributes.Contains("class") && d.Attributes["class"].Value == "hotlist"
                ).FirstOrDefault();
                foreach (var node in divGuess.ChildNodes)
                {
                    if (node.NodeType == HtmlNodeType.Element && node.Name == "dd")
                    {
                        var a = node.FirstChild;
                        listGuess.Add(new Url
                        {
                            LinkUrl = a.GetAttributeValue("href", ""),
                            ImageUrl = a.FirstChild.GetAttributeValue("data-original", "")
                        });
                    }
                }

                var divGuess1 = doc.DocumentNode.Descendants("div").Where(d =>
                         d.Attributes.Contains("class") && d.Attributes["class"].Value == "widgets_top"
                ).FirstOrDefault();
                foreach (var node in divGuess.ChildNodes)
                {
                    if (node.NodeType == HtmlNodeType.Element && node.Name == "a")
                    {
                        var url = new Url
                        {
                            LinkUrl = node.GetAttributeValue("href", ""),
                            ImageUrl = node.FirstChild.GetAttributeValue("src", "")
                        };
                        listGuess.Add(url);
                    }
                }
                if (listGuess.Count < 10)
                {
                    ShowGuess(0, 10);
                }

            }
            catch (Exception ex)
            {
                Helper.WriteExceptionLog(ex.Message);
            }
            return "";
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
            if (index < 1)
            {
                return;
            }
            textIndex.Text = String.Format("({0}/{1}", index.ToString(), textIndex.Text.Substring(textIndex.Text.IndexOf('/') + 1));

            stackpanelGuess.Children.Clear();
            ShowGuess(10 * (index - 1), index * 10);
        }

        /// <summary>
        /// 展示【猜你喜欢】
        /// </summary>
        /// <param name="indexStart">开始图片</param>
        /// <param name="indexEnd">结束图片</param>
        private void ShowGuess(int indexStart, int indexEnd)
        {
            for (int i = indexStart; i < indexEnd; i++)
            {
                if (listGuess.Count > i)
                {
                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(listGuess[i].ImageUrl));
                    image.Tag = listGuess[i].LinkUrl;
                    image.Height = 90;
                    image.Tapped += Image_Tapped;
                    stackpanelGuess.Children.Add(image);

                }
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var image = sender as Image;
            if (image == null)
            {
                return;
            }

            Url urlDetail = new Url
            {
                LinkUrl = image.Tag.ToString(),
                ImageUrl = (image.Source as BitmapImage).UriSource.AbsoluteUri
            };

            this.Frame.Navigate(typeof(ShowPage), urlDetail);

        }

        private void listviewMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            flipMain.SelectedIndex = (sender as ListView).SelectedIndex;

        }


        #region 收藏，数据库
        private void tooglebtnCollect_Checked(object sender, RoutedEventArgs e)
        {
            CollectionService dal = new CollectionService();
            dal.Add(new Collection
            {
                LinkUrl = this.url.LinkUrl,
                ImageUrl = this.url.ImageUrl,
                Title = ""
            });

        }

        private void tooglebtnCollect_Unchecked(object sender, RoutedEventArgs e)
        {
            CollectionService dal = new CollectionService();
            dal.Delete(r => r.ImageUrl == this.url.ImageUrl);
        }
        #endregion

        #region 打开、折叠左侧列表
        private void tooglebtnSplit_Unchecked(object sender, RoutedEventArgs e)
        {
            gridMain.ColumnDefinitions[0].Width = new GridLength(0);
        }

        private void tooglebtnSplit_Checked(object sender, RoutedEventArgs e)
        {
            gridMain.ColumnDefinitions[0].Width = new GridLength(160);
        }
        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPage();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (timer != null && timer.IsEnabled)
            {
                timer.Stop();
            }
        }
    }

}
