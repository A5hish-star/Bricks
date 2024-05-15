using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bricks.Data;
using Bricks.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Bricks.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDBContext _context;

        public LoginController(AppDBContext context)
        {
            _context = context;
        }
        public IActionResult LoginForm()
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DashBoard","Production");
            }
            return View();
        }
        [HttpPost]
        public IActionResult LoginForm(Login model)
        {
            if(ModelState.IsValid)
            {
                var data = _context.login.Where(x => x.Name == model.Name).SingleOrDefault();
                if(data !=null)
                {
                    bool IsValid = data.Name == model.Name && data.Password == model.Password;
                    if(IsValid)
                    {
                        var identity = new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name,model.Name)},
                        CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);
                        HttpContext.Session.SetString("Username",model.Name);
                        return RedirectToAction("DashBoard","Production");
                    }
                    else
                    {
                        TempData["passworderror"]="Invalid Password";
                        return View(model);
                    }
                }
                else
                {
                    TempData["error"]="Not Found";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }
        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var storedCookies = Request.Cookies.Keys;
            foreach(var cookies in storedCookies)
            {
                Response.Cookies.Delete(cookies);
            }
            return RedirectToAction("LoginForm","Login");
        }
    }
}