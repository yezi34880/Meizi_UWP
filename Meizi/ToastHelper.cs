using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Meizi
{
    public class ToastHelper
    {
        public static void ToastShow(string text)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(text));
            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("src", $"ms-appx:///Assets/img/M3.png");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "logo");
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");
            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", $"ms-winsoundevent:Notification.Default");
            toastNode.AppendChild(audio);
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }



    }
}
