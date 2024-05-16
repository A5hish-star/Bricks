using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bricks.Data;
using Bricks.Helper;
using Bricks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Bricks.Controllers
{
    [Authorize]
    public class ProductionController : Controller
    {
        private readonly AppDBContext _context;

        public ProductionController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult ProductList()
        {
            var data = _context.production.ToList();
            // int TotalQuantity = data.Sum(x => x.Quantity);
            // ViewData["TotalQuantity"] = TotalQuantity;
            return View(data);
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult ProductDetails()
        {
            var model = _context.Catagory.Select(item => item.Name).Distinct().ToList();
            @ViewBag.Catagory = new SelectList(model);
            var unit = _context.Unit.Select(x => x.UnitName).Distinct().ToList();
            @ViewBag.Unit = new SelectList(unit);
            var product = _context.product.Select(x =>x.ProductName).Distinct().ToList();
            @ViewBag.Product = new SelectList(product);
            return View();
        }

        [HttpPost]
        public IActionResult ProductDetails(Production model)
        {
            if(ModelState.IsValid)
            {
                var existingProduct = _context.production.FirstOrDefault(x => x.ProductName == model.ProductName && x.Unit == model.Unit);
                if(existingProduct != null)
                {
                    existingProduct.Date = model.Date;
                    existingProduct.Quantity += model.Quantity;
                }
                else
                {
                    var newProduct = new Production
                    {
                        Date = model.Date,
                        ProductName = model.ProductName,
                        Catagory = model.Catagory,
                        Quantity = model.Quantity,
                        Unit = model.Unit
                    };
                    _context.production.Add(newProduct);
                } 
                _context.SaveChanges();
                return RedirectToAction("ProductList","Production");
            }
            return View();
        }
        public IActionResult EditProduction(int id)
        {
            var model = _context.Catagory.Select(item => item.Name).Distinct().ToList();
            @ViewBag.Catagory = new SelectList(model);
            var unit = _context.Unit.Select(x => x.UnitName).Distinct().ToList();
            @ViewBag.Unit = new SelectList(unit);
            var product = _context.product.Select(x =>x.ProductName).Distinct().ToList();
            @ViewBag.Product = new SelectList(product);
            var data = _context.production.Where(x => x.Id == id).FirstOrDefault();
            return View(data);
        }

        [HttpPost]
        public IActionResult EditProduction(Production model)
        {
            var data = _context.production.Where(x => x.Id == model.Id).FirstOrDefault();
            if(data != null)
            {
                data.Date = model.Date;
                data.ProductName = model.ProductName;
                data.Catagory = model.Catagory;
                data.Quantity = model.Quantity;
                data.Unit = model.Unit;

                _context.SaveChanges();
                return RedirectToAction("ProductList","Production");
            }
            return View();
        }
        public IActionResult DeleteProduction(int id)
        {
            var data = _context.production.Where(x => x.Id == id).SingleOrDefault();
            if(data == null)
            {
                return NotFound();
            }
            _context.production.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("ProductList","Production");
        }
        public IActionResult Sales()
        {
            var model = _context.Catagory.Select(item => item.Name).Distinct().ToList();
            @ViewBag.SelectList = new SelectList(model);
            var product = _context.product.Select(x => x.ProductName).Distinct().ToList();
            @ViewBag.Product = new SelectList(product);
            var catagory = _context.Catagory.Select(x => x.Name).Distinct().ToList();
            @ViewBag.Catagory = new SelectList(catagory);
            var customer = _context.Customer.Select(x => x.CustomerName).Distinct().ToList();
            @ViewBag.Customer = new SelectList(customer);
            var unit = _context.Unit.Select(x => x.UnitName).Distinct().ToList();
            @ViewBag.Unit = new SelectList(unit);
            return View();
        }

        [HttpPost]
        public IActionResult Sales(Sales model)
        {
            if(model != null)
            {
                model.Price = model.AmountOfBricks * model.PriceperBrick;
                if(model.Price !=null)
                {
                    model.AmountRemaining = model.Price - model.AmountPaid;
                }
                bool Paid = model.Price == model.AmountPaid;
                if(Paid)
                {
                    model.Status = "Paid";
                }
                else
                {
                    model.Status = "Remaining";
                }
                var productModel = _context.product.FirstOrDefault(x =>x.ProductName == model.ProductName);
                if(productModel != null)
                {
                    model.PriceperBrick = productModel.Price;
                }
                var existingStocks = _context.production.FirstOrDefault(x =>x.ProductName == model.ProductName && x.Unit == model.Unit);
                if(existingStocks != null)
                {
                    var data = model.AmountOfBricks <= existingStocks.Quantity && model.AmountOfBricks !=0;
                    if(data)
                    {
                        existingStocks.Quantity -= model.AmountOfBricks;
                    }
                    else
                    {
                        TempData["error"]="Product not found";
                        return View();
                    }
                }
                _context.buyers.Add(model);
                _context.SaveChanges();
                return RedirectToAction("BuyerDetails","Production");
            }
            return View (model);
        }

        [HttpGet]
        public async Task<IActionResult> BuyerDetails (string searchString,string currentFilter,int pageNumber)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            
            var data = from s in _context.buyers
                   select s;
            if(!string.IsNullOrEmpty(searchString))
            {
                data = data.Where(x => x.Status.Contains(searchString));
            }
            int pageSize = 5;
            return View(await PaginatedList<Sales>.CreateAsync(data.AsNoTracking(), pageNumber, pageSize));
        }
        [HttpPost]
        public IActionResult BuyerDetails(string status)
        {
            var data = _context.buyers.ToList();
            var model = from x in _context.buyers select x;
            if(!String.IsNullOrEmpty(status))
            {
                model = model.Where(x =>x.Status.Contains(status)).AsNoTracking();
                return View (model);
            }
            return View (data);
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult DashBoard(Production model)
        {
            DateTime SevenDaysAgo = DateTime.Now.Date.AddDays(-7);
            var TotalProduce = _context.production.ToList();
            int total = TotalProduce.Where(x => DateTime.Parse(x.Date).Date >= SevenDaysAgo && DateTime.Parse(x.Date).Date <= DateTime.Now.Date).Sum(x =>x.Quantity);
            ViewData["total"] = total;
            string Date = DateTime.Now.ToString("dd/MM/yyyy");
            ViewData["Date"] = Date;
            var TotalStocks = _context.production.ToList();
            int totalstocks = TotalStocks.Sum(x =>x.Quantity);
            ViewData["totalstocks"] = totalstocks;
            var TotalSales = _context.buyers.ToList();
            int totalsales = TotalSales.Where(x => DateTime.Parse(x.Date).Date >= SevenDaysAgo && DateTime.Parse(x.Date).Date <= DateTime.Now.Date).Sum(x =>x.AmountOfBricks);
            ViewData["totalsales"] = totalsales;
            return View();
        }
        public IActionResult ChangePassword()
        {
            var model = _context.login.SingleOrDefault();
            ViewBag.Name = model.Name;
            ViewBag.Password = model.Password;
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(Login model)
        {
            var data = _context.login.Where(x => x.Name == model.Name).SingleOrDefault();
            if(data !=null)
            {
                data.Name = model.Name;
                data.Password = model.Password;

                _context.SaveChanges();
                return RedirectToAction("DashBoard","Production");
            }
            return View (data);
        }

        public IActionResult Delete(int id)
        {
            var data = _context.buyers.Where(x => x.Id == id).SingleOrDefault();
            if(data == null)
            {
                return NotFound();
            }
            _context.buyers.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("BuyerDetails","Production");
        }
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Edit (int id)
        {
            var model = _context.Catagory.Select(item => item.Name).Distinct().ToList();
            @ViewBag.SelectList = new SelectList(model);
            var product = _context.product.Select(x => x.ProductName).Distinct().ToList();
            @ViewBag.Product = new SelectList(product);
            var catagory = _context.Catagory.Select(x => x.Name).Distinct().ToList();
            @ViewBag.Catagory = new SelectList(catagory);
            var customer = _context.Customer.Select(x => x.CustomerName).Distinct().ToList();
            @ViewBag.Customer = new SelectList(customer);
            var unit = _context.Unit.Select(x => x.UnitName).Distinct().ToList();
            @ViewBag.Unit = new SelectList(unit);
            var data = _context.buyers.Where(x =>x.Id == id).FirstOrDefault();
            return View(data);
        }
        [HttpPost]
        public IActionResult Edit (Sales model)
        {
            var data = _context.buyers.Where(x => x.Id == model.Id).SingleOrDefault();
            if(data != null)
            {
                data.Date = model.Date;
                data.BuyerName = model.BuyerName;
                data.ProductName = model.ProductName;
                data.Catagory = model.Catagory;
                data.AmountOfBricks = model.AmountOfBricks;
                data.Unit = model.Unit;
                data.PriceperBrick = model.PriceperBrick;
                data.Price = model.Price;
                data.AmountPaid = model.AmountPaid;
                data.AmountRemaining = model.AmountRemaining;
                data.ModeOfPayment = model.ModeOfPayment;
                bool Paid = model.Price == model.AmountPaid;
                if(Paid)
                {
                    model.Status = "Paid";
                }
                else
                {
                    model.Status = "Remaining";
                }
                data.Status = model.Status;
                _context.SaveChanges();
                return RedirectToAction("BuyerDetails","Production");
            }
            return View();
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Unit()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Unit(Unit model)
        {
            if(ModelState.IsValid)
            {
                var existingUnit = _context.Unit.FirstOrDefault(x => x.Id == model.Id);
                if(existingUnit != null)
                {
                    existingUnit.UnitName = model.UnitName;
                }
                else{
                    var newUnit = new Unit
                    {
                        UnitName = model.UnitName
                    };
                    _context.Unit.Add(newUnit);
                }
                _context.SaveChanges();
                return RedirectToAction("unitView","Production");
            }
            return View();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult unitView()
        {
            var data = _context.Unit.ToList();
            return View(data);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Catagory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Catagory(Catagory model)
        {
            if(ModelState.IsValid)
            {
                var newCatagory = new Catagory{
                    Name = model.Name
                };
                _context.Catagory.Add(newCatagory);
                _context.SaveChanges();
                return RedirectToAction("CatagoryTable","production");
            }
            return View();
        }
        
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult CatagoryTable()
        {
            var data = _context.Catagory.ToList();
            return View(data);
        }

        public IActionResult Product()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Product(Product model)
        {
            if(ModelState.IsValid)
            {
                var existingProduct = _context.product.FirstOrDefault(x => x.ProductName == model.ProductName);
                if(existingProduct != null)
                {
                    existingProduct.ProductName = model.ProductName;
                    existingProduct.Price = model.Price;
                    existingProduct.Description = model.Description;
                }
                else
                {
                    var newProduct = new Product{
                        ProductName = model.ProductName,
                        Price = model.Price,
                        Description = model.Description
                    };
                    _context.product.Add(newProduct);
                }
                _context.SaveChanges();
                return RedirectToAction("Producttable","Production");
            }
            return View();
        }
        public IActionResult Producttable()
        {
            var data = _context.product.ToList();
            return View(data);
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var data = _context.product.Where(x => x.Id == id).FirstOrDefault();
            return View(data);
        }

        [HttpPost]

        public IActionResult EditProduct(Product model)
        {
            var data = _context.product.Where(x => x.Id == model.Id).SingleOrDefault();
            if(data != null)
            {
                data.ProductName = model.ProductName;
                data.Price = model.Price;
                data.Description = model.Description;

                _context.SaveChanges();
                return RedirectToAction("Producttable","Production");
            }
            return View();
        }
        public IActionResult DeleteProduct(int id)
        {
            var data = _context.product.Where(x => x.Id == id).SingleOrDefault();
            if(data == null)
            {
                return NotFound();
            }
            _context.product.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("Producttable","Production");
        }

        [HttpGet]
        public IActionResult GetPrice(string ProductName)
        {
            var price = _context.product.FirstOrDefault(p => p.ProductName == ProductName);
            return Json(price?.Price?? 0);
        }
        [HttpGet]
        public IActionResult EditCatagory(int id)
        {
            var data = _context.Catagory.Where(x => x.Id == id).FirstOrDefault();
            return View(data);
        }
        [HttpPost]
        public IActionResult EditCatagory(Catagory model)
        {
            var data = _context.Catagory.Where(x => x.Id == model.Id).SingleOrDefault();
            if(data != null)
            {
                data.Name = model.Name;
                
                _context.SaveChanges();
                return RedirectToAction("CatagoryTable","Production");
            }
            return View();
        }
        public IActionResult DeleteCatagory(int id)
        {
            var data = _context.Catagory.Where(x => x.Id == id).SingleOrDefault();
            if(data == null){
                return NotFound();
            }
            _context.Catagory.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("CatagoryTable","Production");
        }

        [HttpGet]
        public IActionResult EditUnit(int id)
        {
            var data = _context.Unit.Where(x => x.Id == id).FirstOrDefault();
            return View(data);
        }
        [HttpPost]
        public IActionResult EditUnit(Unit model)
        {
            var data = _context.Unit.Where(x => x.Id == model.Id).SingleOrDefault();
            if(data != null)
            {
                data.UnitName = model.UnitName;
                
                _context.SaveChanges();
                return RedirectToAction("unitView","Production");
            }
            return View();
        }
        public IActionResult DeleteUnit(int id)
        {
            var data = _context.Unit.Where(x => x.Id == id).SingleOrDefault();
            if(data == null){
                return NotFound();
            }
            _context.Unit.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("unitView","Production");
        }
    }
}