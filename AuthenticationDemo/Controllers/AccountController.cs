using AuthenticationDemo.CustomAuthentication;
using AuthenticationDemo.DataAccess;
using AuthenticationDemo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AuthenticationDemo.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login(string ReturnURL = "")
        {
            if (User.Identity.IsAuthenticated)
                return LogOut();

            ViewBag.ReturnURL = ReturnURL;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginView loginView, string ReturnURL = "")
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(loginView.UserName, loginView.Password))
                {
                    var user = (CustomMemberShipUser)Membership.GetUser(loginView.UserName, false);

                    if (user != null)
                    {
                        CustomSerializeModel userModel = new CustomSerializeModel()
                        {
                            FirstName = user.FirstName,
                            LastName = user.FirstName,
                            UserId = user.UserId,
                            RoleName = user.Roles.Select(r => r.RoleName).ToList()
                        };

                        var userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, loginView.UserName, DateTime.Now, DateTime.Now, false, userData);
                        string authToken = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie cookie = new HttpCookie("Cookie1", authToken);
                        Response.Cookies.Add(cookie);
                    }

                    if (Url.IsLocalUrl(ReturnURL))
                    {
                        return Redirect(ReturnURL);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }


                }
            }

            ModelState.AddModelError("", "Somethingwrong: username or password invalid....");
            return View(loginView);
        }

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        public ActionResult Registration(RegistrationView registerationView)
        {
            string messageRegistration = string.Empty;
            bool statusRegistration = false;
            if (ModelState.IsValid)
            {
                string userName = Membership.GetUserNameByEmail(registerationView.Email);
                if (!string.IsNullOrEmpty(userName))
                {
                    ModelState.AddModelError("Validation", "User with this emailID already exists.");
                    return View(registerationView);
                }


                using (AuthenticationDB dbContext = new AuthenticationDB())
                {
                    var user = new User()
                    {
                        Username = registerationView.Username,
                        FirstName = registerationView.FirstName,
                        LastName = registerationView.LastName,
                        Email = registerationView.Email,
                        Password = registerationView.Password,
                        ActivationCode = Guid.NewGuid()
                    };
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }
                // VerificationMail
                VerificationEmail(registerationView.Email, registerationView.ActivationCode.ToString());
                messageRegistration = "Your account has been created successfully. ^_^";
                statusRegistration = true;
            }
            else
            {
                messageRegistration = "Some thing wrong.";
            }

            ModelState.AddModelError("", "Somethingwrong: username or password invalid....");
            return View(registerationView);
        }

        [NonAction]
        private void VerificationEmail(string email, string activationCode)
        {
            string url = string.Format("/Account/ActivationAccount/{0}", activationCode);
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, url);

            var fromEmail = new MailAddress("payal2534@gmail.com", "Activation Code");
            var toEmail = new MailAddress(email);

            var fromEmailPassword = "payal@8959";
            var subject = "Activation Account";

            string body = "<br/> Please click on the following link in order to activate your account" + "<br/><a href='" + link + "'> Activation Account ! </a>";
            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true

            })
                smtp.Send(message);
        }

        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("Cookie1", "");
            cookie.Expires = DateTime.Now.AddDays(-1);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", null);
        }
    }
}