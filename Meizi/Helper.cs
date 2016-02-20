using System;
using Windows.Web.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.Storage.Streams;
using System.IO.IsolatedStorage;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System.Linq;

namespace Meizi
{
    public class Helper
    {

        //获取HTML源代码
        public static async Task<string> GetHttpWebRequest(string url)
        {
            try
            {
                using (HttpClient http = new HttpClient())
                using (HttpRequestMessage requestmsg = new HttpRequestMessage())
                {
                    requestmsg.Method = HttpMethod.Get;
                    requestmsg.RequestUri = new Uri(url);
                    requestmsg.Headers.Append("Accept-Language", "zh-CN,zh;q=0.8");
                    requestmsg.Headers.Append("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.80 Safari/537.36 QQBrowser/9.3.6581.400");

                    using (HttpResponseMessage response = await http.SendRequestAsync(requestmsg))
                    {
                        return response.Content.ToString();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static void ShowImageList(string html, GridView mainContent)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            double imageHeight = 354;
            double imageWidth = 236;
            var lis = doc.GetElementbyId("pins").SelectNodes("li");
            mainContent.Items.Clear();
            foreach (var li in lis)
            {
                var a = li.FirstChild;
                GridViewItem gvi = new GridViewItem();
                gvi.Tag = li.FirstChild.GetAttributeValue("href", "");

                var img = a.FirstChild;
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(img.GetAttributeValue("data-original", "")));
                image.Height = imageHeight;
                image.Width = imageWidth;

                gvi.Content = image;
                mainContent.Items.Add(gvi);
            }
            var pagenavi = doc.DocumentNode.Descendants("a").Where(d =>
                d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("page-numbers")
                ).ToList();
            var pageCount = int.Parse(pagenavi[pagenavi.Count - 2].FirstChild.InnerText);

            mainContent.Tag = new PageNavi()
            {
                pageNow = 1,
                pageCount = pageCount
            };
        }

        /// <summary>
        /// 加载更多
        /// </summary>
        /// <param name="html"></param>
        /// <param name="mainContent"></param>
        /// <param name="index">要加载的页数</param>
        public static void AppendImageList(string html, GridView mainContent)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            double imageHeight = 354;
            double imageWidth = 236;
            var lis = doc.GetElementbyId("pins").SelectNodes("li");
            //mainContent.Items.Clear();
            foreach (var li in lis)
            {
                var a = li.FirstChild;
                GridViewItem gvi = new GridViewItem();
                gvi.Tag = li.FirstChild.GetAttributeValue("href", "");

                var img = a.FirstChild;
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(img.GetAttributeValue("data-original", "")));
                image.Height = imageHeight;
                image.Width = imageWidth;
                gvi.Content = image;
                mainContent.Items.Add(gvi);
            }
        }

        /// <summary>
        /// 获取 生成树 中的子控件
        /// </summary>
        /// <typeparam name="T">子控件类型</typeparam>
        /// <param name="parent">父</param>
        /// <param name="name">子控件名称</param>
        /// <returns></returns>
        public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T>(child, name);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }

    public class PageNavi
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int pageNow { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int pageCount { get; set; }
    }
}
