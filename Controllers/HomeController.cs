using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LogRegHash.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace LogRegHash.Controllers
{
    public class HomeController : Controller
    {
        private MyContext db;

        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            db = context;
        }
        public int? uid
        {
            get
            {
                return HttpContext.Session.GetInt32("UserId");
            }
        }
        // REGISTRATION -------------------------------------------------------------------------------
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("create")]
        public IActionResult Create(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email is Already in use.");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);

                db.Add(newUser);
                db.SaveChanges();

                HttpContext.Session.SetInt32("UserId", newUser.UserId);

                return RedirectToAction("Privacy");
            }
            return View("Index");
        }
        // Login -------------------------------------------------------------------------------
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login/request")]
        public IActionResult Logged(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                User userInDB = db.Users.FirstOrDefault(u => u.Email == user.Email);
                if (userInDB == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(user, userInDB.Password, user.Password);

                if (result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                HttpContext.Session.SetInt32("UserId", userInDB.UserId);
                HttpContext.Session.SetString("UserFname", userInDB.FirstName);
                return RedirectToAction("Privacy");
            }
            return View("Login");
        }
        [HttpGet("privacy")]
        public IActionResult Privacy()
        {
        if( uid == null)
        {
            return RedirectToAction("Login");
        }
        User loggedUser = db.Users.FirstOrDefault(u => u.UserId == uid);

            // HttpContext.Session.SetInt32("UserId", user.UserId);
            // int? UserIdSesssion = HttpContext.Session.GetInt32("UserId");
            // var User = db.Users.FirstOrDefault(u => u.UserId == UserIdSesssion);

            return View(loggedUser);
        }
    }
}
