using DBHelper.DateBase;
using DBHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper.Dal
{
    public class CollectionService
    {
        DbContext db = new DbContext();

        public void CreateTable(Type tableName)
        {
            using (var connection = db.GetDbConnection())
            {
                var table = connection.GetTableInfo(tableName.ToString());
                if (table.Count < 1)
                {
                    connection.CreateTable(tableName);
                }
            }
        }

        public int Add(Collection model)
        {
            var m = GetModel(r => r.LinkUrl == model.LinkUrl);
            if (m != null)
            {
                return 0;
            }
            return db.Insert<Collection>(model);
        }
        public int Delete(Func<Collection, bool> where)
        {
            var model = GetModel(where);
            if (model == null)
            {
                return 0;
            }
            return db.Delete<Collection>(model);
        }

        public Collection GetModel(Func<Collection, bool> where)
        {
            return db.GetModel<Collection>(where);
        }

        public List<Collection> GetList(Func<Collection, bool> where)
        {
            return db.GetList<Collection>(where).OrderByDescending(r=>r.ID).ToList();
        }

        public IEnumerable<Collection> GetListRandom(int num)
        {
            using (var connection = db.GetDbConnection())
            {
                string sql = " SELECT * FROM Collection ORDER BY RANDOM() limit " + num.ToString() + " ;";
                var list = connection.DeferredQuery<Collection>(sql).ToList();
                return list;
            }

        }
    }
}
