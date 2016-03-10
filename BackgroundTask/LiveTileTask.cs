using DBHelper.Dal;
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
    /// 后台任务 更新 动态磁贴 (动态磁贴展示收藏内容)
    /// </summary>
    public sealed class LiveTileTask : IBackgroundTask
    {
        public  void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

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
                <binding template = 'TileLarge' branding = 'name' >
                  <image src='{0}' placement='background'/>
                </binding>
              </visual>
            </tile>";

        public void UpdateTile()
        {
            try
            {
                CollectionService dal = new CollectionService();
                var listUrls = dal.GetListRandom(5);

                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueueForWide310x150(true);
                updater.EnableNotificationQueueForSquare150x150(true);
                updater.EnableNotificationQueueForSquare310x310(true);
                updater.EnableNotificationQueue(true);
                updater.Clear();

                foreach (var url in listUrls)
                {
                    var xmlDoc = new XmlDocument();
                    var xml = string.Format(TileTemplateXml, url.ImageUrl);
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
