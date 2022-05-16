using EventSourcing.API.Dtos;
using MediatR;

namespace EventSourcing.API.Commands
{
    //MediatR --> IRequest
    public class CreateProductCommand:IRequest
    {
        public CreateProductDto CreateProductDto { get; set; }
    }
}
