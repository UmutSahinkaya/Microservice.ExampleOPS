using MassTransit;
using Shared.Events;

namespace PaymentAPI.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        readonly IPublishEndpoint _endpoint;

        public StockReservedEventConsumer(IPublishEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            //Ödeme işlemleri
            if (true)
            {
                // Ödeme başarıyla tamamlandı.
                PaymentCompletedEvent paymentCompletedEvent = new()
                {
                    OrderId = context.Message.OrderId
                };
                _endpoint.Publish(paymentCompletedEvent);
                Console.WriteLine("Ödeme Başarılı...");
            }
            else
            {
                //Ödemede sıkıntı
                PaymentFailedEvent paymentFailedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Bakiye Yetersiz"
                };
                _endpoint.Publish(paymentFailedEvent);
                Console.WriteLine("Ödeme Başarısız...");

            }
            return Task.CompletedTask;
        }
    }
}
