using WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using WebApp.Helper;

namespace WebApp.Controllers
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // If they are authorized, handle accordingly
            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                using (WebAppEntities db = new WebAppEntities())
                {
                    string webaddress = db.Settings.Where(x => x.vSettingID == "D000234B-EF28-4S37-L868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                    // Otherwise redirect to your specific authorized area
                    filterContext.Result = new RedirectResult(webaddress);
                }
            }
        }
    }

    public class SettingActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            try
            {
                using (WebAppEntities db = new WebAppEntities())
                {
                    // Site Settings
                    filterContext.Controller.ViewBag.PageTitle = db.Settings.Where(x => x.vSettingID == "D0011XX1-2ES1-4B39-1268-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.SiteCode = db.Settings.Where(x => x.vSettingID == "D0012345-3312-4B27-E268-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.MetaDescription = db.Settings.Where(x => x.vSettingID == "D001534B-JG18-4B27-G868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.MetaKeyword = db.Settings.Where(x => x.vSettingID == "D001634B-EW18-4B37-F868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.SiteAuthor = db.Settings.Where(x => x.vSettingID == "D001831C-1C18-4D37-I868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.SiteRobots = db.Settings.Where(x => x.vSettingID == "D001734B-CS18-4B37-E868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.Company = db.Settings.Where(x => x.vSettingID == "F000134R-1D23-1R37-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.CorporateEmail = db.Settings.Where(x => x.vSettingID == "F0006U4R-1D23-2337-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption;

                    // Theme Settings
                    filterContext.Controller.ViewBag.BodySmallText = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D001XX4B-02VV-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.NavbarSmallText = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D002314B-2212-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.SidebarSmallText = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D003W34B-1212-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.FooterSmallText = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D004430B-JJH1-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.SidebarNavFlatStyle = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D005434B-1232-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.SidebarNavLegacyStyle = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D006434B-0HH2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.SidebarNavCompact = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D007434B-0GG2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.SidebarNavChildIndent = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D008834B-T112-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                    filterContext.Controller.ViewBag.BrandLogoSmallText = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "D009934B-02XX-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";

                    filterContext.Controller.ViewBag.NavbarColorVariant = db.Settings.Where(x => x.vSettingID == "D011434B-22R2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.LinkColorVariant = db.Settings.Where(x => x.vSettingID == "D021434B-12H2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.DarkSidebarVariant = db.Settings.Where(x => x.vSettingID == "D031434B-0GG2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.LightSidebarVariant = db.Settings.Where(x => x.vSettingID == "D041434B-FF12-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                    filterContext.Controller.ViewBag.BrandLogoVariant = db.Settings.Where(x => x.vSettingID == "D051434B-0312-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;

                    // Process all notification for email queuing
                    db.spProcessEmailQueueing();

                    // Applying Business Rules for Transactions
                    db.spProcessBusinessRules();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }
    }

    public class AdminController : Controller
    {
        string webaddress ="";
        private string RedirectAction { get; set; }

        public AdminController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.webaddress = db.Settings.Where(x => x.vSettingID == "D000234B-EF28-4S37-L868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
            }
        }

        [CustomAuthorize]
        public bool isAuthorized(string action)
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                var currentUserId = User.Identity.GetUserId();
                if (currentUserId == null)
                    return false;
                else
                {
                    var roleId = dbcon.AspNetUserRoles.Where(x => x.UserId == currentUserId).FirstOrDefault().RoleId;
                    var url = dbcon.AspNetRoles.Where(x => x.Id == roleId).FirstOrDefault().IndexPage;
                    RedirectAction = url.Split('/')[2];
                    action = "/Admin/" + action;
                    bool isUserAuthorized = dbcon.AspNetUsersMenuPermissions.Where(x => x.AspNetUsersMenu.nvPageUrl == action && x.Id == roleId).Any();
                    if (isUserAuthorized)
                    {
                        VisitCount();
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        public string GetModuleName()
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                try
                {
                    var action = string.Concat("/Admin/", ControllerContext.RouteData.Values["action"].ToString());
                    var menu = dbcon.AspNetUsersMenus.Where(x => x.nvPageUrl == action).FirstOrDefault();
                    if (menu != null) return menu.nvMenuName;

                    return "";
                }
                catch (Exception )
                {
                    throw;
                }
            }
        }

        [CustomAuthorize]
        public void VisitCount()
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                using (var dbContextTransaction = dbcon.Database.BeginTransaction())
                {
                    try
                    {
                        var path = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
                        string ip = "";
                        System.Web.HttpContext cont = System.Web.HttpContext.Current;
                        string ipAddress = cont.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (!string.IsNullOrEmpty(ipAddress))
                        {
                            string[] addresses = ipAddress.Split(',');
                            if (addresses.Length != 0)
                            {
                                ip = addresses[0];
                            }
                        }
                        ip = cont.Request.ServerVariables["REMOTE_ADDR"];
                        AspNetUsersPageVisited anupv = new AspNetUsersPageVisited();
                        anupv.vPageVisitedID = Guid.NewGuid().ToString();
                        anupv.Id = User.Identity.GetUserId();
                        anupv.nvPageName = path;
                        anupv.dDateVisited = DateTime.UtcNow;
                        anupv.nvIPAddress = ip;
                        dbcon.AspNetUsersPageVisiteds.Add(anupv);
                        dbcon.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public bool isAllowedBySettings(string Id)
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                return Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == Id).FirstOrDefault().vSettingOption);
            }
        }

        protected string LinkRedirect(string addr, int df)
        {
            if(string.IsNullOrEmpty(addr))
            {
                if(df == 1)
                    addr = "/Auth/Login";
                else
                    addr = "/BadRequest";
            } 
            return addr;
        }

        /**** Start Dashboard ****/
        public ActionResult Index()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Index";

            if (isAuthorized("Index"))
                return View();
            else
                return Redirect(LinkRedirect(webaddress, 1));
        }

        public ActionResult DashboardTOPipeline()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/DashboardTOPipeline";

            if (isAuthorized("DashboardTOPipeline"))
                return View();
            else
                return Redirect(LinkRedirect(webaddress, 1));
        }

        public ActionResult DashboardTOSchedule()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/DashboardTOSchedule";

            if (isAuthorized("DashboardTOSchedule"))
                return View();
            else
                return Redirect(LinkRedirect(webaddress, 1));
        }

        public ActionResult DashboardTOStatus()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/DashboardTOStatus";

            if (isAuthorized("DashboardTOStatus"))
                return View();
            else
                return Redirect(LinkRedirect(webaddress, 1));
        }

        public ActionResult DashboardTitleStatus()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/DashboardTitleStatus";

            if (isAuthorized("DashboardTitleStatus"))
                return View();
            else
                return Redirect(LinkRedirect(webaddress, 1));
        }

        public ActionResult DashboardElectricMeter()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/DashboardElectricMeter";

            if (isAuthorized("DashboardElectricMeter"))
                return View();
            else
                return Redirect(LinkRedirect(webaddress, 1));
        }
        /**** End Landing ****/

        /**** Start Transactions ****/
        public ActionResult UnitAcceptance()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitAcceptance";

            if (isAuthorized("UnitAcceptance"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult OccupancyPermit()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/OccupancyPermit";

            if (isAuthorized("OccupancyPermit"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitQualifyMasterlist()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitQualifyMasterlist";

            if (isAuthorized("UnitQualifyMasterlist"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitQualifyTurnoverMgmnt(string flx10ms)
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitQualifyTurnoverMgmnt";
            ViewBag.Variable = string.IsNullOrWhiteSpace(flx10ms) ? null : StringCipher.Derypt(flx10ms);

            if (isAuthorized("UnitQualifyTurnoverMgmnt"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitQualifyTurnoverMgmntMod1()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitQualifyTurnoverMgmntMod1";

            if (isAuthorized("UnitQualifyTurnoverMgmnt"))
                return View("UnitQualifyTurnoverMgmnt");
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitQualifyTurnoverMgmntMod2()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitQualifyTurnoverMgmntMod2";

            if (isAuthorized("UnitQualifyTurnoverMgmnt"))
                return View("UnitQualifyTurnoverMgmnt");
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitQualifyTurnoverMgmntMod3()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitQualifyTurnoverMgmntMod3";

            if (isAuthorized("UnitQualifyTurnoverMgmnt"))
                return View("UnitQualifyTurnoverMgmnt");
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitInspectionAcceptanceMgmnt(string flx10ms)
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitInspectionAcceptanceMgmnt";
            ViewBag.Variable = string.IsNullOrWhiteSpace(flx10ms) ? null : StringCipher.Derypt(flx10ms);

            if (isAuthorized("UnitInspectionAcceptanceMgmnt"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitQTHistoricalMgmnt(string flx10ms)
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitQTHistoricalMgmnt";
            ViewBag.Variable = string.IsNullOrWhiteSpace(flx10ms) ? null : StringCipher.Derypt(flx10ms);

            if (isAuthorized("UnitQTHistoricalMgmnt"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitIAHistoricalMgmnt(string flx10ms)
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitIAHistoricalMgmnt";
            ViewBag.Variable = string.IsNullOrWhiteSpace(flx10ms) ? null : StringCipher.Derypt(flx10ms);

            if (isAuthorized("UnitIAHistoricalMgmnt"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult TitlingStatus(string flx10ms)
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/TitlingStatus";
            ViewBag.Variable = string.IsNullOrWhiteSpace(flx10ms) ? null : StringCipher.Derypt(flx10ms);

            if (isAuthorized("TitlingStatus"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult ElectricMeter(string flx10ms)
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/ElectricMeter";
            ViewBag.Variable = string.IsNullOrWhiteSpace(flx10ms) ? null : StringCipher.Derypt(flx10ms);

            if (isAuthorized("ElectricMeter"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult InventoryMeterDeposit(string flx10ms)
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/InventoryMeterDeposit";
            ViewBag.Variable = string.IsNullOrWhiteSpace(flx10ms) ? null : StringCipher.Derypt(flx10ms);

            if (isAuthorized("InventoryMeterDeposit"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** End Transactions ****/

        /**** Start User Management ****/
        public ActionResult Menu()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Menu";

            if (isAuthorized("Menu"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Users()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Users";

            if (isAuthorized("Users"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult CreateUser()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/CreateUser";

            if (isAuthorized("CreateUser"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Role()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Role";

            if (isAuthorized("Role"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult AdminCustomMenu()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/AdminCustomMenu";

            if (isAuthorized("AdminCustomMenu"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        [CustomAuthorize]
        public new ActionResult Profile()
        {
            VisitCount();
            ViewBag.PageUrl = "/Admin/Profile";

            return View();
        }
        /**** End User Management ****/

        /**** Start Logs ****/
        public ActionResult ChangeLog()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/ChangeLog";

            if (isAuthorized("ChangeLog"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult LogDashboard()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/LogDashboard";

            if (isAuthorized("LogDashboard"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult PendingEmailVerification()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/PendingEmailVerification";

            if (isAuthorized("PendingEmailVerification"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
       
        public ActionResult LoginHistory()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/LoginHistory";

            if (isAuthorized("LoginHistory"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult PageVisited()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/PageVisited";

            if (isAuthorized("PageVisited"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));

        }
        /**** End Logs ****/

        /**** Start Approval Workflow ****/
        public ActionResult ApprovalStages()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/ApprovalStages";

            if (isAuthorized("ApprovalStages"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        public ActionResult ApprovalTemplate()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/ApprovalTemplate";

            if (isAuthorized("ApprovalTemplate"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** End Approval Workflow ****/

        /**** Start Maintenance ****/
        public ActionResult UserProject()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UserProject";

            if (isAuthorized("UserProject"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Option()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Option";

            if (isAuthorized("Option"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Company()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Company";

            if (isAuthorized("Company"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Project()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Project";

            if (isAuthorized("Project"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitType()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitType";

            if (isAuthorized("UnitType"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Phase()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Phase";

            if (isAuthorized("Phase"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult UnitInventory()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/UnitInventory";

            if (isAuthorized("UnitInventory"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Customer()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Customer";

            if (isAuthorized("Customer"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** End Maintenance ****/

        /**** Start Administration ****/
        public ActionResult DocumentaryRequirement()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/DocumentaryRequirement";

            if (isAuthorized("DocumentaryRequirement"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult TurnoverOption()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/TurnoverOption";

            if (isAuthorized("TurnoverOption"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult TurnoverStatus()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/TurnoverStatus";

            if (isAuthorized("TurnoverStatus"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult PunchlistCategory()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/PunchlistCategory";

            if (isAuthorized("PunchlistCategory"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult TitlingLocation()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/TitlingLocation";

            if (isAuthorized("TitlingLocation"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Holiday()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Holiday";

            if (isAuthorized("Holiday"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** Start Administration ****/

        /**** Start Settings ****/
        public ActionResult Configuration()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Configuration";

            if (isAuthorized("Configuration"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        public ActionResult EmailTemplate()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/EmailTemplate";

            if (isAuthorized("EmailTemplate"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult SystemParameter()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/SystemParameter";

            if (isAuthorized("SystemParameter"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult TurnoverInterface()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/TurnoverInterface";

            if (isAuthorized("TurnoverInterface"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** End Settings ****/

        public ActionResult BadRequest()
        {
            return Redirect("/Auth/Login");
        }
    }
}
