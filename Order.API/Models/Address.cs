using Microsoft.EntityFrameworkCore;

namespace Order.API.Models
{
    [Owned]//Address is not different table. It is Order's table.
    public class Address
    {
        public string Line { get; set; }
        public string Province { get; set; }
        public string District  { get; set; }
    }
}
