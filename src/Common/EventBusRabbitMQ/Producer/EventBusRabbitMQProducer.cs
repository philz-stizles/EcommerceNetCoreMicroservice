﻿using EventBusRabbitMQ.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace EventBusRabbitMQ.Producer
{
    public class EventBusRabbitMQProducer
    {
        private readonly IRabbitMQConnection _rabbitMQConnection;

        public EventBusRabbitMQProducer(IRabbitMQConnection rabbitMQConnection)
        {
            _rabbitMQConnection = rabbitMQConnection ?? throw new ArgumentNullException(nameof(rabbitMQConnection));
        }

        public void PublishBasketCheckout(string queueName, BasketCheckoutEvent publishModel)
        {
            using (var channel = _rabbitMQConnection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var message = JsonConvert.SerializeObject(publishModel);
                var body = Encoding.UTF8.GetBytes(message);

                IBasicProperties properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = 2;

                channel.ConfirmSelect();
                channel.BasicPublish(exchange: "", routingKey: queueName, mandatory: true, basicProperties: properties, body: body);
                channel.WaitForConfirmsOrDie();

                channel.BasicAcks += (sender, EventArgs) =>
                {
                    Console.WriteLine("Sent RabbitMQ");
                };

                channel.ConfirmSelect();
            }
        }
    }
}
