using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Model;

namespace CommonLibrary
{
    public class CustomerDatabase
    {
        public Dictionary<long, Customer> Customers { get; private set; } = new Dictionary<long, Customer>
        {
            { 1, new Customer(1, "Marko Marković", 100.0) },
            { 2, new Customer(2, "Jovan Jovanović", 150.0) }
        };
    }
}
