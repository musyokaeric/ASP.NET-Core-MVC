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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrderHeaderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Update(OrderHeader orderHeader) => dbContext.Update(orderHeader);

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var order = dbContext.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                order.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    order.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var order = dbContext.OrderHeaders.FirstOrDefault(o => o.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                order.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                order.PaymentIntentId = paymentIntentId;
                order.PaymentDate = DateTime.Now;
            }
        }
    }
}
