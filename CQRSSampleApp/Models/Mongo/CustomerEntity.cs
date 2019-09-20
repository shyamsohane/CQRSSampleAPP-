using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSSampleApp.Models.Mongo
{
    public class CustomerEntity
    {
        [BsonElement("Id")]
        public long Id { get; set; }
        [BsonElement("Email")]
        public string Email { get; set; }
        [BsonElement("Name")]
        public string Name { get; set; }
        [BsonElement("Age")]
        public int Age { get; set; }
        [BsonElement("Phones")]
        public List<PhoneEntity> Phones { get; set; }
    }

    public partial class PhoneEntity
    {
		[BsonElement("Type")]
		public PhoneType Type { get; set; }
		[BsonElement("AreaCode")]
		public int AreaCode { get; set; }
		[BsonElement("Number")]
		public int Number { get; set; }
	}
}
