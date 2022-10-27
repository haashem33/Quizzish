using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using quizzish.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Collections;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;

namespace quizzish.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("quiz", "Home");
            }
            return View();

        }
        //post metoden för att logga in användaren
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginKontroll user, string returnUrl = "")
        {
            //Kontrollera användarnamn
            bool userOk = CheckUserFromDB(user);
            if (userOk == true)
            {
                // Allt stämmer, logga in användaren
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.anvnamn));
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
                // Skicka användaren vidare till returnUrl om den finns annars till startsidan
                if (returnUrl != "")
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("quiz", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Inloggningen inte godkänd";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

        }
        //Kontroll metoden för att Kontrollera om användaren har ett konto
        private bool CheckUserFromDB(LoginKontroll userInfo)
        {
            int count = 0;
            using (dbskapare database = new dbskapare())
            {
                var validUsers = database.Kontons.Where(u => u.Anvandnamn == userInfo.anvnamn).
                Where(u => u.losenord == userInfo.losen);
                count = validUsers.Count();
            }
            if (count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // registrera vyn
        public IActionResult Registrera()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("quiz", "Home");
            }
            return View();
        }
        //LÄGG TILL ETT KONTO i databasen
        [HttpPost]
        public IActionResult Laggtill(Konto NyttKonto)
        {
            using (dbskapare database = new dbskapare())
            {
                database.Kontons.Add(NyttKonto);
                database.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [Authorize]
        [HttpGet]
        public IActionResult Quiz()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult Quiz2(List<fragor> v, ValdKat katten)
        {
            
            if (v.Count == 0)//kolla om ajax har skickat med en id
            {
                List<fragor> flist = new List<fragor>();
                List<svar> fsvar = new List<svar>();
                //hämtar frågor fån databasen genom valda katogorin
                var fgfromdb = fragorFromDb(katten);
                //Väljer 5 slumpmässiga frågor
                var randomNums = PickRandom(fgfromdb.sfragor, 5);
                // omvandlar från IEnumerable till frågor lista
                foreach (var item in randomNums)
                {
                    flist.Add(item);
                }
               //hämtar in alla svar från databasen med samma katogori som frågan
               var rsvarfdb = fragorFromDb(fgfromdb, flist[0], katten);

                //Väljer 4 slumpmässiga svar
                var sfsvar = PickRandom(rsvarfdb.svarerna, 4);


                //omvandlar från IEnumerable till svar lista
                foreach (var svar in sfsvar)
                {
                    fsvar.Add(svar);
                }

                //kollar om det finns en random svar som är samma som fråganssvar och den tar bort den eftersom d finns kan bar finnsas max en dubbel svar har jag med mig 1 extra slumpmässig svar som ersätter den dubblerande svaret
                foreach (var item in fsvar.ToList())
                {
                    if(item.svarid == flist[0].svarid)
                    {
                        fsvar.Remove(item);
                    }
                }

                
                //skapa en random postioner array för alla svar
                Random random = new Random();
                var array = new int[] { 1, 2, 3, 4 };
                var sarray = Shuffle(random,array);
                //skapar en array av alla svar
                var korrektsvar = fgfromdb.svarerna.Find(x => x.svarid == flist[0].svarid).svaret;
                var fsarray = new string[] { fsvar[0].svaret, fsvar[1].svaret, fsvar[2].svaret, korrektsvar };

                ViewBag.Country = 0;
                ViewBag.fragan = flist[0].fraga;
                ViewBag.svarray = fsarray;
                ViewBag.arra = sarray;
                ViewBag.sports = katten.sport.ToString();
                ViewBag.geo = katten.geog.ToString();
                ViewBag.ratta = 0;
                ViewBag.fela = 0;
                return PartialView(flist);

            }
            else if (Int32.Parse(Request.Form["Country"]) < 4)
            {
                //try och catch för att kunna hämta in värdet på antal felaktiga svar;
                try
                {  
                    var felaks = int.Parse(Request.Form["fel"]);
                    ViewBag.fela = felaks;

                }
                catch (Exception)
                {

                    var felaks = 0;
                    ViewBag.fela = felaks;


                }
                //try och catch för att kunna hämta in värdet på antal rätta svar;
                try
                {
                    var rattas = int.Parse(Request.Form["ratt"]);
                    ViewBag.ratta = rattas;
                }
                catch (Exception)
                {
                    var rattas = 0;
                    ViewBag.ratta = rattas;
                }
                
                //hämtar in gamla listan av frågor detta för att undvika upprepande av frågor och miska på antal gånger man behöver prata med databasen
                List<fragor> flist = new List<fragor>();
                flist = v;
                //antal frågor som är gjorda
                var country = Int32.Parse(Request.Form["Country"]);
                country++;
                //katogorin som användaren valde skickat från sidan
                ValdKat kattens = new ValdKat();
                kattens.sport= bool.Parse(Request.Form["sports"].ToString());
                kattens.geog = bool.Parse(Request.Form["geo"].ToString()); 

                List<svar> fsvar = new List<svar>();
                //hämtar in alla svar från databasen med samma katogori som frågan
                var rsvarfdb = svarFromDb(flist[country], kattens);

                //Väljer 4 slumpmässiga svar
                var sfsvar = PickRandom(rsvarfdb, 4);


                //omvandlar från IEnumerable till svar lista
                foreach (var svar in sfsvar)
                {
                    fsvar.Add(svar);
                }

                //kollar om det finns en random svar som är samma som fråganssvar och den tar bort den eftersom d finns kan bar finnsas max en dubbel svar har jag med mig 1 extra slumpmässig svar som ersätter den dubblerande svaret
                foreach (var item in fsvar.ToList())
                {
                    if (item.svarid == flist[country].svarid)
                    {
                        fsvar.Remove(item);
                    }
                }

                
                //skapa en random postioner array för alla svar
                Random random = new Random();
                var array = new int[] { 1, 2, 3, 4 };
                var sarray = Shuffle(random, array);
                //skapar en array av alla svar
                var korrektsvar = rsvarfdb.Find(x => x.svarid == flist[country].svarid).svaret;

                var fsarray = new string[] { fsvar[0].svaret, fsvar[1].svaret, fsvar[2].svaret, korrektsvar };
                ViewBag.Country = country;
                ViewBag.fragan = flist[country].fraga;
                ViewBag.svarray = fsarray;
                ViewBag.arra = sarray;
                ViewBag.sports = kattens.sport.ToString();
                ViewBag.geo = kattens.geog.ToString();

                return PartialView(flist);
            }
            else
            {
                var rattas = int.Parse(Request.Form["ratt"]);
                ViewBag.ratta = rattas;
                return PartialView();
            }

        }

        [HttpPost]
        public IActionResult checka()
        {
            var svaret = int.Parse(Request.Form["select"]);
            try
            {
                if (svaret == 4)
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        //code av stackoverflow för att slumpassig positionera array
        public static int[] Shuffle(Random rng, int[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }




        // väljer 5 olika frågor av valda kategorin
        static Random rnd = new Random();
        public static IEnumerable<T> PickRandom<T>( IList<T> source, int count)
        {
            bool isEmpty = !source.Any();
            if (isEmpty)
            {
                return Enumerable.Empty<T>();
            }
                return source.OrderBy(x => rnd.Next()).Take(count); 
            
        }

        //retunerar svar från databasen till min viewmodel, alla svar är relaterade till frågan genom att använda alla svar som har samma katogori som frågan 
        public static viewmodel fragorFromDb(viewmodel vm, fragor fg, ValdKat katten)
        {
                using (dbskapare database = new dbskapare())
                {
                    string search = fg.svarkat;
                    vm.svarerna = database.svars.Where(x => x.svarkat == search).ToList();
                }


            return vm;
            
        }
        //retunerar svar från databasen till min svarlista, alla svar är relaterade till frågan genom att använda alla svar som har samma katogori som frågan
        public static List<svar> svarFromDb(fragor fg, ValdKat katten)
        {
            List<svar> svarList = new List<svar>();

                using (dbskapare database = new dbskapare())
                {
                    string search = fg.svarkat;
                    svarList = database.svars.Where(x => x.svarkat == search).ToList();
                }

            return svarList;

        }


        //hämtar alla frågor baserad på katogorin som är valda
        public static viewmodel fragorFromDb(ValdKat katten)
        {
            viewmodel vm = new viewmodel();

            if (katten.sport && katten.geog)
            {
                using (dbskapare database = new dbskapare())
                {
                    vm.sfragor = database.fragors.ToList();

                }
            }
            else if (katten.sport && !katten.geog)
            {
                using (dbskapare database = new dbskapare())
                {
                    vm.sfragor = database.fragors.Where(p => p.kat == "sport").ToList();
                }
            }
            else if (!katten.sport && katten.geog)
            {
                using (dbskapare database = new dbskapare())
                {
                    vm.sfragor = database.fragors.Where(p => p.kat == "geografi").ToList();

                }
            }
            return vm;
        }
        
        //post metoden för att logga ut användaren
        public async Task<IActionResult> SignOutUser()
        {
            await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
