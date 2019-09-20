using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSSampleApp.Models
{
    public class CustomerRecord
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public virtual List<PhoneRecord> Phones { get; set; }
    }

    public class PhoneRecord
    {
        public long Id { get; set; }
        public PhoneType Type { get; set; }
        public int AreaCode { get; set; }
        public int Number { get; set; }
    }

    public enum PhoneType
    {
        HOMEPHONE, CELLPHONE, WORKPHONE
    }
}
