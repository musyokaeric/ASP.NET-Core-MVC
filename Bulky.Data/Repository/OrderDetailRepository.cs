using Bulky.Data.Data;
using Bulky.Data.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Data.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrderDetailRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Update(OrderDetail orderDetail) => dbContext.Update(orderDetail);
    }
}
