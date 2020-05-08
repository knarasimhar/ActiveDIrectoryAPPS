using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

namespace ADUVerify.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult CreateADUser(string OU,string DC,string _DC,string Uname, string Pwd)
        {


            if (Uname == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/CreateADUser?ou=&dc=&_dc&uname=xxxx&pwd=xxxx";
                return View();
            }

           

            DirectoryEntry ouEntry = new DirectoryEntry("LDAP://OU=hyderabad,DC=mylab,DC=local");

            
                try
                {
                    DirectoryEntry childEntry = ouEntry.Children.Add("CN=" + Uname, "user");
                    childEntry.CommitChanges();
                    ouEntry.CommitChanges();
                    childEntry.Invoke("SetPassword", new object[] { Pwd });
                    childEntry.Properties["samAccountName"].Value = Uname;
                
                  
                childEntry.CommitChanges();
                int val = (int)childEntry.Properties["userAccountControl"].Value;
                childEntry.Properties["userAccountControl"].Value = val & ~0x2;
                childEntry.CommitChanges();
                ViewBag.Message = "your new Active Directory user added successfully.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Exception ." + ex.Message;
                }

           
            return View();
        }
        

        [HttpGet]

        public ActionResult ValidateADUser(string Uname, string Pwd)
        {

            if (Uname == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/ValidateADUser?uname=xxxx&pwd=xxxx";
                return View();
            }
            bool isValid = false;


            using (DirectoryEntry adsEntry = new DirectoryEntry("LDAP://OU=hyderabad,DC=mylab,DC=local", Uname, Pwd))
            {
                using (DirectorySearcher adsSearcher = new DirectorySearcher(adsEntry))
                {
                    //adsSearcher.Filter = "(&(objectClass=user)(objectCategory=person))";
                    adsSearcher.Filter = "(sAMAccountName=" + Uname + ")";

                    try
                    {
                        SearchResult adsSearchResult = adsSearcher.FindOne();
                        isValid = true;

                        //strAuthenticatedBy = "Active Directory";
                       // strError = "User has been authenticated by Active Directory.";
                    }
                    catch(Exception ex)
                    {
                        // Failed to authenticate. Most likely it is caused by unknown user
                        // id or bad strPassword.
                       // strError = ex.Message;
                    }
                    finally
                    {
                        adsEntry.Close();
                    }
                }
            }
            // create a "principal context" - e.g. your domain (could be machine, too)
          /*  using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "mylab.local"))
            {
                // validate the credentials
                isValid = pc.ValidateCredentials(Uname, Pwd);
            }*/
            ViewBag.Message = "Your User validation is :" + isValid.ToString();
            return View();
        }

        public ActionResult ListADUsers()
        {
            using (var context = new PrincipalContext(ContextType.Domain, "mylab.local"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    ViewBag.Message = "<table><tr><th>User Name</th></tr>";
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        // ViewBag.Message += "</br>First Name: " + de.Properties["givenName"].Value;
                        //   ViewBag.Message += "Last Name : " + de.Properties["sn"].Value;
                        ViewBag.Message += "<tr><td>" + de.Properties["samAccountName"].Value + "</tr></td>";
                        //  ViewBag.Message += "User principal name: " + de.Properties["userPrincipalName"].Value;

                    }
                    ViewBag.Message += "</table>";
                }
            }
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}