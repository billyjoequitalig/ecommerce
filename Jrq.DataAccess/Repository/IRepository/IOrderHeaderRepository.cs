using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jrq.Models;

namespace Jrq.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id, string orderStatus, String? paymmentStatus = null);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        
    }
}
