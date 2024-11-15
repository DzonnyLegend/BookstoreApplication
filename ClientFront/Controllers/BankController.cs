using Microsoft.AspNetCore.Mvc;
using CommonLibrary.Model;
using CommonLibrary;

namespace ClientFront.Controllers
{
    public class BankController : Controller
    {
        private readonly CustomerDatabase _customerDatabase;

        public BankController(CustomerDatabase customerDatabase)
        {
            _customerDatabase = customerDatabase;
        }

        // Prikaz svih računa
        public IActionResult Index()
        {
            var model = _customerDatabase.Customers.Values.ToList();
            return View(model);
        }

        // Prikaz forme za kreiranje novog računa
        [HttpGet]
        public IActionResult CreateAccount()
        {
            return View();
        }

        // Kreiranje novog računa
        [HttpPost]
        public IActionResult CreateAccount(string fullName, double initialBalance)
        {
            if (ModelState.IsValid)
            {
                long newClientId = _customerDatabase.Customers.Keys.Max() + 1; // Generiše novi ID za korisnika
                var newCustomer = new Customer
                {
                    UserId = newClientId,
                    FullName = fullName,
                    AccountBalance = initialBalance
                };

                _customerDatabase.Customers.Add(newClientId, newCustomer);
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Details(long userId)
        {
            var customer = _customerDatabase.Customers.GetValueOrDefault(userId);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

    }
}
