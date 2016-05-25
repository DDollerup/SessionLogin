using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SessionLogin.Factories;
using SessionLogin.Models;
using System.Security.Cryptography; // Skal bruges for at kunne få fat i klassen SHA512
using System.Text; // Skal bruges for at kunne konvertere bytes til tekst

namespace SessionLogin.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        UserFactory userFac = new UserFactory();

        /* Vi skal have denne metode i Controller'en, da metoden altid bliver kørt, FØR, alle views.
         * På denne måde kan vi tjekke om brugeren er logget ind som administrator på alle views.
         * Hvis ikke man er logget ind, bliver man sendt tilbage til Login */
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["UserLoggedIn"] == null && !Request.RawUrl.ToLower().Contains("login"))
            {
                Response.Redirect("/Admin/Admin/Login");
            }

            base.OnActionExecuting(filterContext);
        }

        // GET: Admin/Admin
        public ActionResult Index()
        {
            return View();
        }

        /* Her har vi Login action til Login view'et - Altså her hvor bruger skal logge ind.
         * Se Login.cshtml i Views mappen */
        public ActionResult Login()
        {
            return View();
        }

        /* Denne metode bliver kørt når man trykker Login på Login view'et.
         * Vi får et username og et password fra brugeren, som vi kan bruge til at tjekke
         * om brugeren eksistere i databasen. Hvis vi får en bruger, bliver man sendt til
         * Index metoden i admin area'et, hvis ikke får man en besked om at brugernavnet eller password'et
         * ikke kan findes. */
        [HttpPost]
        public ActionResult LoginSubmit(string username, string password)
        {
            SHA512 typedPassword = new SHA512Managed();
            typedPassword.ComputeHash(Encoding.ASCII.GetBytes(password));
            string hashedPassword = BitConverter.ToString(typedPassword.Hash).Replace("-", "");

            User userToLogin = userFac.UserLogin(username, hashedPassword);

            if (userToLogin != null)
            {
                Session["UserLoggedIn"] = userToLogin;
                return RedirectToAction("Index");
            }
            else
            {
                TempData["MSG"] = "Wrong username or password.";
                return RedirectToAction("Login");
            }
        }

        /* Denne action skal køres igennem et normalt link (a tag) på admin siden */
        public ActionResult Logout()
        {
            Session["UserLoggedIn"] = null;
            return Redirect("/Home/Index");
        }

        public ActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddUserSubmit(User user)
        {

            SHA512 newPassword = new SHA512Managed();
            newPassword.ComputeHash(Encoding.ASCII.GetBytes(user.Password));
            string hashedPassword = BitConverter.ToString(newPassword.Hash).Replace("-", "");

            user.Password = hashedPassword;
            userFac.Add(user);


            return RedirectToAction("Index");
        }
    }
}