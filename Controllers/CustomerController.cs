using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bricks.Data;
using Bricks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bricks.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly AppDBContext _context;

        public CustomerController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult CustomerDetails()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CustomerDetails(Customer model)
        {
            if(ModelState.IsValid)
            {
                var newCustomer = new Customer
                {
                    CustomerName = model.CustomerName,
                    MobileNo = model.MobileNo,
                    Email = model.Email,
                    ShippingAddress = model.ShippingAddress,
                    PAN = model.PAN
                };
                _context.Customer.Add(newCustomer);
                _context.SaveChanges();
                return RedirectToAction("CustomerList","Customer");
            }
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult CustomerList()
        {
            var data = _context.Customer.ToList();
            return View(data);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Edit(int id)
        {
            var data = _context.Customer.Where(x =>x.Id == id).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(Customer model)
        {
            var data = _context.Customer.Where(x => x.Id == model.Id).SingleOrDefault();
            if(data != null)
            {
                data.CustomerName = model.CustomerName;
                data.MobileNo = model.MobileNo;
                data.Email = model.Email;
                data.ShippingAddress = model.ShippingAddress;
                data.PAN = model.PAN;

                _context.SaveChanges();
                return RedirectToAction("CustomerList","Customer");
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            var data = _context.Customer.Where(x => x.Id == id).SingleOrDefault();
            if(data == null)
            {
                return NotFound();
            }
            _context.Customer.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("CustomerList","Customer");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}