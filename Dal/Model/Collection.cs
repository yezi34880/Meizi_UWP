using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper.Model
{
    public class Collection
    {
        [AutoIncrement, PrimaryKey]
        public int ID { get; set; }

        public string Title { get; set; }


        /// <summary>
        /// 链接 URL，用于页面跳转
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 图片URL ，用于图片展示
        /// </summary>
        public string LinkUrl { get; set; }
    }
}
