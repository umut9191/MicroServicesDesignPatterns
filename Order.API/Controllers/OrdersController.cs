using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using Shared.Events;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        //on real word we use services or repository pattern to use instead of context but here it is not main topic
        private readonly AppDbContext _context;
      //  private readonly IPublishEndpoint _publishEndpoint;
        //Masstransit te publish ve send kavramları vardır arasındaki fark;
        //publish edince eğer bu mesajı subscribe eden yok ise bu mesaj boşa gider.
        //publish edince rabbitmq de kimlerin dinlediğini bilmezsin(her subscribe olmuşlar bilgiye ulaşabilir). Bu event direk olarak excahange ye gider yani
        //direk olarak kuyruğa gitmez. Eğer exchangeye giderse ve o exchangeye subscribe olmuş bir kuyruk yok ise
        //boşa gider mesajlarımız.
        //send methodu ise direk olarak kuyruğa gönderir.
        //publish ile gönderilmiş event lere herhangi bir servis subscribe olabilir. Ama send ile gönderilen event
        //direk kuyruğa gittiğinden dolayı sadece kuyruğa subscribe olduğumuzda alabiliriz.
        //Özetle bir eventi birden fazla servis dinleyecek ise publish yapılır.
        //sadece bir servis dinleyecek ise ozaman send kullanılır. bir kuyruğa gönderme yapılır ve onu bir servis dinler.

        // for orchestration;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        public OrdersController(AppDbContext appDbContext,/*IPublishEndpoint publishEndpoint,*/ ISendEndpointProvider sendEndpointProvider)
        {
            _context = appDbContext;
            //  _publishEndpoint = publishEndpoint;
            _sendEndpointProvider = sendEndpointProvider;
        }
        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address {
                    Line = orderCreate.Address.Line,
                    Province =orderCreate.Address.Province,
                    District = orderCreate.Address.District,
                },
                CreatedDate = DateTime.Now,
            };
            orderCreate.orderItems.ForEach(item => {
                newOrder.Items.Add(new OrderItem()
                {
                    Price = item.Price,
                    ProductId = item.ProductId,
                    Count = item.Count,
                });
            });
            await _context.AddAsync(newOrder);
            await _context.SaveChangesAsync();
            //we changed  OrderCreatedEvent to OrderCreatedRequestEvent
            var orderCreatedRequestEvent = new OrderCreatedRequestEvent()
            {
                BuyerId= orderCreate.BuyerId,
                OrderId= newOrder.Id,
                Payment = new PaymentMessage(){
                    CardName =orderCreate.payment.CardName,
                    CardNumber = orderCreate.payment.CardNumber,
                    Expiration=orderCreate.payment.Expiration,
                    CVV = orderCreate.payment.CVV,
                    TotalPrice = orderCreate.orderItems.Sum(x=>x.Price*x.Count),
                }
            };
            orderCreate.orderItems.ForEach((item) => {
                orderCreatedRequestEvent.OrderItems.Add(new OrderItemMessage()
                {
                    ProductId = item.ProductId,
                    Count = item.Count,
                });


            });
            //tabi burada uri belirlerken mass transit in belirlemiş olduğu şekilde kullanacağız;
            var sendEntPoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqSettingsConsts.OrderSaga}"));
            //publish etseydik kuyruk ismini vermeye gerek olmayacaktı. Çünkü kim sucribe olacak ise o kuyruk ismini versin.
            //ama eğer publish edilince dinleyen kimse yok ise işte ozaman event boşa gider. Fakat sen edince kuyruk belli olduğu için 
            //işte ozaman dinleyecek o an orada olmasa bile bağlandığı zaman ilgili event te ulaşabilecek. Yani boşa gitmeyecek.

            await sendEntPoint.Send(orderCreatedRequestEvent);
            //await _publishEndpoint.Publish(orderCreatedEvent);
            return Ok();
        }
    }
}
