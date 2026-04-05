using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Jrq.DataAccess.Data;
using Jrq.DataAccess.Repository.IRepository;
using Jrq.Models;

namespace Jrq.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository 
    {
        private ApplicationDBContext _db;
        public CategoryRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category obj)
        {
            _db.Category.Update(obj);
        }
    }
}
