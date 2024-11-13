using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Model
{
    [DataContract]
    public class Customer
    {
        [DataMember]
        public long UserId { get; set; }

        [DataMember]
        public string FullName { get; set; }  // Kombinacija imena i prezimena

        [DataMember]
        public double AccountBalance { get; set; }  // Balans na računu korisnika

        public Customer(long userId, string fullName, double accountBalance)
        {
            UserId = userId;
            FullName = fullName;
            AccountBalance = accountBalance;
        }
    }
}
