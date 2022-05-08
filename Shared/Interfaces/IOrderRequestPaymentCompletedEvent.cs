using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IOrderRequestPaymentCompletedEvent
    {
        public int OrderId { get; set; }
    }
}
