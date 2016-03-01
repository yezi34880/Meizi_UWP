using DBHelper.Model;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DBHelper.DateBase
{
    public class DbContext
    {
        /// <summary>
        /// 数据库文件所在路径，这里使用 LocalFolder，数据库文件名叫 test.db。
        /// </summary>
        public readonly string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Meizi.db");

        public SQLiteConnection GetDbConnection()
        {
            try
            {
                // 连接数据库，如果数据库文件不存在则创建一个空数据库。
                var conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
                return conn;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int CreatTable<T>() where T : class
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    return conn.CreateTable<T>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public  int Insert<T>(T model) where T : class
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    return conn.Insert(model);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int Delete<T>(T model) where T : class
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    return conn.Delete(model);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int Update<T>(T model) where T : class
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    return conn.Update(model);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T GetModel<T>(Func<T, bool> expWhere) where T :class
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    var table = conn.Table<T>();
                    return table.Where(expWhere).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> GetList<T>(Func<T, bool> expWhere) where T : class
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    var table = conn.Table<T>();
                    return table.Where(expWhere).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
