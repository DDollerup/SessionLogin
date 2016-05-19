using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SessionLogin.Factories;
using SessionLogin.Models;

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
            User userToLogin = userFac.UserLogin(username, password);

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
    }
}