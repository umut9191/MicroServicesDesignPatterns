namespace EventSourcing.API.Dtos
{
    public class ChangedProductPriceDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
    }
}
