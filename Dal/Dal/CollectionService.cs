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
        public int Add(Collection model)
        {
            return db.Insert<Collection>(model);
        }
        public int Delete(Func<Collection, bool> where)
        {
            var model = GetModel(where);
            return db.Delete<Collection>(model);
        }

        public Collection GetModel(Func<Collection, bool> where)
        {
            return db.GetModel<Collection>(where);
        }

        public List<Collection> GetList(Func<Collection, bool> where)
        {
            return db.GetList<Collection>(where);
        }

    }
}
