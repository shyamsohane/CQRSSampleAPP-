using CQRSSampleApp.Events;
using CQRSSampleApp.Models;
using CQRSSampleApp.Models.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSSampleApp.Commands
{
    public abstract class Command
    {
        public long Id { get; set; }
    }

    public class CreateCustomerCommand : Command
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public List<CreatePhoneCommand> Phones { get; set; }
        public CustomerCreatedEvent ToCustomerEvent(long id)
        {
            return new CustomerCreatedEvent
            {
                Id = id,
                Name = this.Name,
                Email = this.Email,
                Age = this.Age,
                Phones = this.Phones.Select(phone => new PhoneCreatedEvent { AreaCode = phone.AreaCode, Number = phone.Number }).ToList()
            };
        }
        public CustomerRecord ToCustomerRecord()
        {
            return new CustomerRecord
            {
                Name = this.Name,
                Email = this.Email,
                Age = this.Age,
                Phones = this.Phones.Select(phone => new PhoneRecord { AreaCode = phone.AreaCode, Number = phone.Number }).ToList()
            };
        }
    }
    public class CreatePhoneCommand : Command
    {
        public PhoneType Type { get; set; }
        public int AreaCode { get; set; }
        public int Number { get; set; }
    }
    public class UpdateCustomerCommand : Command
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<CreatePhoneCommand> Phones { get; set; }
        public CustomerUpdatedEvent ToCustomerEvent()
        {
            return new CustomerUpdatedEvent
            {
                Id = this.Id,
                Name = this.Name,
                Age = this.Age,
                Phones = this.Phones.Select(phone => new PhoneCreatedEvent
                {
                    Type = phone.Type,
                    AreaCode = phone.AreaCode,
                    Number = phone.Number
                }).ToList()
            };
        }
        public CustomerRecord ToCustomerRecord(CustomerRecord record)
        {
            record.Name = this.Name;
            record.Age = this.Age;
            record.Phones = this.Phones.Select(phone => new PhoneRecord
            {
                Type = phone.Type,
                AreaCode = phone.AreaCode,
                Number = phone.Number
            }).ToList()
                ;
            return record;
        }
    }

    public class DeleteCustomerCommand : Command
    {
        internal CustomerDeletedEvent ToCustomerEvent()
        {
            return new CustomerDeletedEvent
            {
                Id = this.Id
            };
        }
    }

    public interface ICommandHandler<T> where T : Command
    {
        void Execute(T command);
    }

    public class CustomerCommandHandler : ICommandHandler<Command>
    {
        private CustomerSQLiteRepository _repository;
        private AMQPEventPublisher _eventPublisher;
        public CustomerCommandHandler(AMQPEventPublisher eventPublisher, CustomerSQLiteRepository repository)
        {
            _eventPublisher = eventPublisher;
            _repository = repository;
        }
        public void Execute(Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command is null");
            }
            if (command is CreateCustomerCommand createCommand)
            {
                CustomerRecord created = _repository.Create(createCommand.ToCustomerRecord());
                _eventPublisher.PublishEvent(createCommand.ToCustomerEvent(created.Id));
            }
            else if (command is UpdateCustomerCommand updateCommand)
            {
                CustomerRecord record = _repository.GetById(updateCommand.Id);
                _repository.Update(updateCommand.ToCustomerRecord(record));
                _eventPublisher.PublishEvent(updateCommand.ToCustomerEvent());
            }
            else if (command is DeleteCustomerCommand deleteCommand)
            {
                _repository.Remove(deleteCommand.Id);
                _eventPublisher.PublishEvent(deleteCommand.ToCustomerEvent());
            }
        }
    }
}
