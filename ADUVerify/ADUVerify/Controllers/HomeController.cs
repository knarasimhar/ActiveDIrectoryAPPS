using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

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

        public ActionResult ListADGroups()
        {
            string Uname = "narasimha";
            if (Uname == null)
            {
               // ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/getADGroupsByUser?uname=xxxx";
                return View();
            }

            try
            {
                ViewBag.Message = GetAdGroupsForUser22(Uname, "mylab.local");

                if (ViewBag.Message.Count == 0)
                {
                    ViewBag.Message = "No group mapped";
                }
            }
            catch { ViewBag.Message = "user not found"; }

           
            /*
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
            }*/
            return View();
        }

        public ActionResult CreateADGroups(string _Groupname)
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


        public ActionResult SetADGroupsUser(string OU, string CN, string Gname,string Gtype)
        {

            if (Gname == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/SetADGroupsUser?ou=&cn=&type=&gname=xxxx";
                return View();
            }

            ViewBag.message = Gname + " created successfully";


            DirectoryEntry ouEntry = new DirectoryEntry("LDAP://OU=hyderabad,DC=mylab,DC=local");


            try
            {
                DirectoryEntry childEntry = ouEntry.Children.Add("CN=" + Gname, "group");
                childEntry.CommitChanges();
                ouEntry.CommitChanges();
                childEntry.Properties["samAccountName"].Value = Gname;

                childEntry.CommitChanges();
                // Set the group type to a secured domain local group.
                /*ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_GLOBAL_GROUP | ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_SECURITY_ENABLED;

                if (Gtype == "global")
                    childEntry.Properties["groupType"].Value = ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_GLOBAL_GROUP;
                else
                    childEntry.Properties["groupType"].Value = "ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP|ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_SECURITY_ENABLED";


                childEntry.CommitChanges();
                int val = (int)childEntry.Properties["userAccountControl"].Value;
                childEntry.Properties["userAccountControl"].Value = val & ~0x2;
                childEntry.CommitChanges();*/
                ViewBag.Message = "your new Active Directory group added successfully.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Exception ." + ex.Message;
            }


            return View();


           
          
        }

        public ActionResult getADGroupsByUser(string Uname)
        {

            if (Uname == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/getADGroupsByUser?uname=xxxx";
                return View();
            }

            try
            {
                ViewBag.Message = GetAdGroupsForUser2(Uname, "mylab.local");
            }
            catch { ViewBag.Message = "user not found";  }

            if (ViewBag.Message.Count == 0)
            {
                ViewBag.Message = "No group mapped";
            }
            /*
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
            }*/
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

       
            
             // Usage: GetAdGroupsForUser2("domain\user") or GetAdGroupsForUser2("user","domain")
        public static List<string> GetAdGroupsForUser22(string userName, string domainName = null)
        {
            var result = new List<string>();

            if (userName.Contains('\\') || userName.Contains('/'))
            {
                domainName = userName.Split(new char[] { '\\', '/' })[0];
                userName = userName.Split(new char[] { '\\', '/' })[1];
            }

            using (PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName))
            using (UserPrincipal user = UserPrincipal.FindByIdentity(domainContext, userName))
            using (var searcher = new DirectorySearcher(new DirectoryEntry("LDAP://OU=hyderabad,DC=mylab,DC=local")))
            {
                searcher.Filter = String.Format("(&(objectCategory=group))", user.DistinguishedName);
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add("cn");

                foreach (SearchResult entry in searcher.FindAll())
                    if (entry.Properties.Contains("cn"))
                        result.Add(entry.Properties["cn"][0].ToString());
            }

            return result;
        }
        
        // Usage: GetAdGroupsForUser2("domain\user") or GetAdGroupsForUser2("user","domain")
        public static List<string> GetAdGroupsForUser2(string userName, string domainName = null)
        {
            var result = new List<string>();

            if (userName.Contains('\\') || userName.Contains('/'))
            {
                domainName = userName.Split(new char[] { '\\', '/' })[0];
                userName = userName.Split(new char[] { '\\', '/' })[1];
            }

            using (PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName))
            using (UserPrincipal user = UserPrincipal.FindByIdentity(domainContext, userName))
            using (var searcher = new DirectorySearcher(new DirectoryEntry("LDAP://" + domainContext.Name)))
            {
                searcher.Filter = String.Format("(&(objectCategory=group)(member={0}))", user.DistinguishedName);
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add("cn");

                foreach (SearchResult entry in searcher.FindAll())
                    if (entry.Properties.Contains("cn"))
                        result.Add(entry.Properties["cn"][0].ToString());
            }

            return result;
        }

        public ActionResult getADPermissionsByUser(string Uname)
        {

            if (Uname == null)
            {
                //ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/getADPermissionsByUser?uname=xxxx";
                return View();
            }

            try
            {
                ADUVerify.Models.AdRoleModel AdPobj = new ADUVerify.Models.AdRoleModel();

                ViewBag.Message = AdPobj.GetRolesForUser(Uname);
            }
            catch { ViewBag.Message = "user not found"; }

          
       
            return View();
        }

        public ActionResult GetAllRoles()
        {

            

            try
            {
                ADUVerify.Models.AdRoleModel AdPobj = new ADUVerify.Models.AdRoleModel();

                ViewBag.Message = AdPobj.GetAllRoles();
            }
            catch { ViewBag.Message = "not found"; }



            return View();
        }

              public ActionResult getADAccessRules()
        {
            DirectoryEntry entry = new DirectoryEntry(
            "LDAP://OU=hyderabad,DC=mylab,DC=local",
            null,
            null,
            AuthenticationTypes.Secure
            );
           
           
                       //var sid = (SecurityIdentifier)user.ProviderUserKey;
            ActiveDirectorySecurity sec = entry.ObjectSecurity;

                //  PrintSD(sec);

                try
                {
                // respective user 
                PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, "mylab.local");
                GroupPrincipal user = GroupPrincipal.FindByIdentity(domainContext, "developer");
                var sid = user.Sid;
               
                ViewBag.Message += "</br></br> SID: " + user.Sid.ToString();
                NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
                foreach (ActiveDirectoryAccessRule rule in sec.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    if (rule.ObjectType.ToString() == sid.ToString())
                    {
                        
                        ViewBag.Message += "</br> Identity: " + rule.IdentityReference.ToString()
                                       + "</br> AccessControlType: " + rule.AccessControlType.ToString()
                                       + "</br> ActiveDirectoryRights: " + rule.ActiveDirectoryRights.ToString()
                                       + "</br> InheritanceType: " + rule.InheritanceType.ToString()
                                       + "</br> ObjectFlags: " + rule.ObjectFlags.ToString() + "</br>";
                    }
                }
               // ViewBag.Message += sec.GetOwner(typeof(ntAccount));
                //  ViewBag.Message += " Owner: " + sec.GetOwner(typeof(NTAccount("mylab.user", "user1")));
                //  ViewBag.Message += " Group: " + sec.GetOwner(typeof(Account));
            }
                catch 
                {

                    //throw;
                }

                ViewBag.Message += "</br></br>=====Security Descriptor=====";
                ViewBag.Message += " Owner: " + sec.GetOwner(typeof(NTAccount));
                ViewBag.Message += " Group: " + sec.GetOwner(typeof(NTAccount));


                AuthorizationRuleCollection rules = null;
                rules = sec.GetAccessRules(true, true, typeof(NTAccount));

                foreach (ActiveDirectoryAccessRule rule in rules)
                {
                    PrintAce(rule);
                    ViewBag.Message += "</br> Identity: " + rule.IdentityReference.ToString()
                                       + "</br> AccessControlType: " + rule.AccessControlType.ToString()
                                       + "</br> ActiveDirectoryRights: " + rule.ActiveDirectoryRights.ToString()
                                       + "</br> InheritanceType: " + rule.InheritanceType.ToString()
                                       + "</br> ObjectFlags: " + rule.ObjectFlags.ToString() + "</br>"
                                       +  "</br> ObjectType: " + rule.ObjectType.ToString() + "</br>";
            }
            
            return View();
        }

        public static void PrintAce(ActiveDirectoryAccessRule rule)
        {
            Console.WriteLine("=====ACE=====");
            Console.Write(" Identity: ");
            Console.WriteLine(rule.IdentityReference.ToString());
            Console.Write(" AccessControlType: ");
            Console.WriteLine(rule.AccessControlType.ToString());
            Console.Write(" ActiveDirectoryRights: ");
            Console.WriteLine(
            rule.ActiveDirectoryRights.ToString());
            Console.Write(" InheritanceType: ");
            Console.WriteLine(rule.InheritanceType.ToString());
            Console.Write(" ObjectType: ");
            if (rule.ObjectType == Guid.Empty)
                Console.WriteLine("");
            else
                Console.WriteLine(rule.ObjectType.ToString());

            Console.Write(" InheritedObjectType: ");
            if (rule.InheritedObjectType == Guid.Empty)
                Console.WriteLine("");
            else
                Console.WriteLine(
                rule.InheritedObjectType.ToString());
            Console.Write(" ObjectFlags: ");
            Console.WriteLine(rule.ObjectFlags.ToString());
        }

        public static void PrintSD(ActiveDirectorySecurity sd)
        {
            Console.WriteLine("=====Security Descriptor=====");
            Console.Write(" Owner: ");
            Console.WriteLine(sd.GetOwner(typeof(NTAccount)));
            Console.Write(" Group: ");
            Console.WriteLine(sd.GetGroup(typeof(NTAccount)));
        }





    }
}