using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Web.Http;

namespace BackgroundTask
{
    /// <summary>
    /// 后台任务 更新 动态磁贴
    /// </summary>
    public sealed class LiveTileTask : IBackgroundTask
    {
        public  void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            //System.Diagnostics.Debug.Write("run");
            UpdateTile();

            deferral.Complete();
        }

        string TileTemplateXml =
            @"<?xml version='1.0' encoding='utf-8' ?>
              <tile>
              <visual>
                <binding template = 'TileMedium' >
                  <image src='{0}' placement='background'/>
                </binding>
                <binding template = 'TileWide'>
                  <image src='{0}' placement='background'/>
                </binding>
                <binding template = 'TileLarge' branding = 'logo' >
                  <image src='{0}' placement='background'/>
                </binding>
              </visual>
            </tile>";

        public async void UpdateTile()
        {
            try
            {
                var rootUrl = "http://www.mzitu.com/";
                string html = "";
                using (HttpClient http = new HttpClient())
                using (HttpRequestMessage requestmsg = new HttpRequestMessage())
                {
                    requestmsg.Method = HttpMethod.Get;
                    requestmsg.RequestUri = new Uri(rootUrl);
                    requestmsg.Headers.Append("Accept-Language", "zh-CN,zh;q=0.8");
                    requestmsg.Headers.Append("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.80 Safari/537.36 QQBrowser/9.3.6581.400");

                    using (HttpResponseMessage response = await http.SendRequestAsync(requestmsg))
                    {
                        html = response.Content.ToString();
                    }
                }

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                var lis = htmlDoc.GetElementbyId("pins").SelectNodes("li").Take(5).ToList();
                var listUrls = new List<string>();
                foreach (var li in lis)
                {
                    var url = li.FirstChild.FirstChild.GetAttributeValue("data-original", "");
                    listUrls.Add(url);
                }

                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueueForWide310x150(true);
                updater.EnableNotificationQueueForSquare150x150(true);
                updater.EnableNotificationQueueForSquare310x310(true);
                updater.EnableNotificationQueue(true);
                updater.Clear();

                foreach (var url in listUrls)
                {
                    var xmlDoc = new XmlDocument();
                    var xml = string.Format(TileTemplateXml, url);
                    xmlDoc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
                    {
                        ProhibitDtd = false,
                        ValidateOnParse = false,
                        ElementContentWhiteSpace = false,
                        ResolveExternals = false
                    });

                    var no = new TileNotification(xmlDoc);
                    updater.Update(no);

                }

            }
            catch (Exception)
            {

                throw;
            }

        }


    }
}
