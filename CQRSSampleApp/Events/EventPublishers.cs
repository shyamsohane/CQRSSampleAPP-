using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata;
using CQRSSampleApp.Models.Mongo;
using CQRSSampleApp.Models;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace CQRSSampleApp.Events
{
    public class AMQPEventPublisher
    {
        private readonly ConnectionFactory connectionFactory;
        public AMQPEventPublisher(IHostingEnvironment env)
        {
            connectionFactory = new ConnectionFactory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables();

            builder.Build().GetSection("amqp").Bind(connectionFactory);
        }
        public void PublishEvent<T>(T @event) where T : IEvent
        {
            using (IConnection conn = connectionFactory.CreateConnection())
            {
                using (RabbitMQ.Client.IModel channel = conn.CreateModel())
                {
                    var queue = @event is CustomerCreatedEvent ?
                        Constants.QUEUE_CUSTOMER_CREATED : @event is CustomerUpdatedEvent ?
                            Constants.QUEUE_CUSTOMER_UPDATED : Constants.QUEUE_CUSTOMER_DELETED;
                    channel.QueueDeclare(
                        queue: queue,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                    var body = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(@event));
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: queue,
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }
    }

    public class Constants
    {
        public const string QUEUE_CUSTOMER_CREATED = "customer_created";
        public const string QUEUE_CUSTOMER_UPDATED = "customer_updated";
        public const string QUEUE_CUSTOMER_DELETED = "customer_deleted";
    }

    public interface IEvent
    {
    }
    public class CustomerCreatedEvent : IEvent
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<PhoneCreatedEvent> Phones { get; set; }
        public CustomerEntity ToCustomerEntity()
        {
            return new CustomerEntity
            {
                Id = this.Id,
                Email = this.Email,
                Name = this.Name,
                Age = this.Age,
                Phones = this.Phones.Select(phone => new PhoneEntity
                {
                    Type = phone.Type,
                    AreaCode = phone.AreaCode,
                    Number = phone.Number
                }).ToList()
            };
        }
    }
    public class PhoneCreatedEvent : IEvent
    {
        public PhoneType Type { get; set; }
        public int AreaCode { get; set; }
        public int Number { get; set; }
    }

    public class CustomerUpdatedEvent : IEvent
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<PhoneCreatedEvent> Phones { get; set; }
        public CustomerEntity ToCustomerEntity(CustomerEntity entity)
        {
            return new CustomerEntity
            {
                Id = this.Id,
                Email = entity.Email,
                Name = entity.Name.Equals(this.Name) ? entity.Name : this.Name,
                Age = entity.Age.Equals(this.Age) ? entity.Age : this.Age,
                Phones = GetNewOnes(entity.Phones).Select(phone => new PhoneEntity { AreaCode = phone.AreaCode, Number = phone.Number }).ToList()
            };
        }
        private List<PhoneEntity> GetNewOnes(List<PhoneEntity> Phones)
        {
            return Phones.Where(a => !this.Phones.Any(x => x.Type == a.Type
                && x.AreaCode == a.AreaCode
                && x.Number == a.Number)).ToList<PhoneEntity>();
        }
    }

    public class CustomerDeletedEvent : IEvent
    {
        public long Id { get; set; }
    }
}
