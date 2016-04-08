using HtmlAgilityPack;
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
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SubjectPage : Page
    {
        public SubjectPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

        }

        private async System.Threading.Tasks.Task<List<Url>> LoadClass()
        {
            string html = await Helper.GetHtmlLoop("http://www.mzitu.com/zhuanti");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var tags = doc.DocumentNode.Descendants("dl").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value == "tags").FirstOrDefault();

            List<Url> listUrl = new List<Url>();

            for (var i = 0; i < tags.ChildNodes.Count; i++)
            {
                var nodeT = tags.ChildNodes[i];
                if (nodeT.NodeType == HtmlNodeType.Element && nodeT.Name == "dt")
                {

                    string title = nodeT.InnerText;

                    for (var j = i + 1; j < tags.ChildNodes.Count; j++)
                    {
                        var nodeC = tags.ChildNodes[j];
                        if (nodeC.NodeType == HtmlNodeType.Element && nodeC.Name == "dd")
                        {
                            var a = nodeC.FirstChild;

                            listUrl.Add(new Url
                            {
                                LinkUrl = a.GetAttributeValue("href", ""),
                                ImageUrl = a.FirstChild.GetAttributeValue("src", ""),
                                Remark = a.FirstChild.GetAttributeValue("alt", ""),
                                Remark1 = title
                            });
                        }
                        else
                        {
                            if (nodeC.NodeType == HtmlNodeType.Element && nodeC.Name == "dt")
                            {
                                break;
                            }
                        }
                    }

                }
            }
            return listUrl;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                return;
            }


            List<Url> listUrl = await LoadClass();

            var varUrl = from t in listUrl group t by t.Remark1;

            CollectionViewSource collectionVS = new CollectionViewSource();
            collectionVS.IsSourceGrouped = true;
            collectionVS.Source = varUrl;
            gridTitle.ItemsSource = collectionVS.View.CollectionGroups;
            listContent.ItemsSource = collectionVS.View;
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tag = (sender as StackPanel).Tag;
            if (tag == null)
            {
                return;
            }
            this.Frame.Navigate(typeof(GridPage), tag.ToString());
        }
    }
}
