
using MassTransit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateInstance : SagaStateMachineInstance
    {
        //eventleri(instanceleri) ayırmak için kullanılıyor.CorrelationId default geldi
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }//(initial,OrderCreated,StockReserved,StockNotReserved,PaymentCompleted,PaymentFailed,Final)
        public int BuyerId { get; set; }
        public int OrderId { get; set; }
        //payment;
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        [Column(TypeName ="decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedDate { get; set; }

        public override string ToString()
        {
            //return base.ToString();
            var properties = GetType().GetProperties();
            var sb = new StringBuilder();
            properties.ToList().ForEach(p => {
                var value = p.GetValue(this,null);
                sb.AppendLine($"{p.Name}:{value?.ToString()}");
            });
            sb.AppendLine("------------------------------");
            return sb.ToString();
        }
    }
}
