using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Collections.Generic;
using WebApp.Helper;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Data.Entity;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/CronJob")]
    public class CronJobController : ApiController
    {
        private string PageUrl = "/Admin/CronJobController";

        private string Domain = "";
        private string SMTPHost = "";

        private string SAPUser = "";
        private string SAPPass = "";
        private string SAPClient = "";
        private string SAPIP = "";
        private string SAPPort = "";

        IEnumerable<string> cookies = new List<string>();
        CookieContainer cookieJar = new CookieContainer();

        public CronJobController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    this.Domain = db.Settings.Where(x => x.vSettingID == "D000434T-8C18-4W37-N868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // SMTP Host               
                    this.SMTPHost = db.Settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // SMTP Host 

                    this.SAPUser = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B16D224B-1C28-4A37-B767-B15C089JOMEL").FirstOrDefault().vSettingOption); // SAP User
                    this.SAPPass = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B23D124B-0D28-4A37-A867-D15C089DICEN").FirstOrDefault().vSettingOption); // SAP Pass
                    this.SAPClient = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B33D124B-0D28-4A37-A867-D15C08900001").FirstOrDefault().vSettingOption); // SAP Client

                    this.SAPIP = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B34D124B-0D28-4A37-A867-D15C08900002").FirstOrDefault().vSettingOption); // SAP IP
                    this.SAPPort = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B35D124B-0D28-4A37-A867-D15C08900003").FirstOrDefault().vSettingOption); // SAP Port

                    // Process all notification for email queuing
                    db.spProcessEmailQueueing();

                    // Applying Business Rules for Transactions
                    db.spProcessBusinessRules();
                }
                catch (Exception)
                {
                }
            }
        }

        [Route("GetProcessEmailQueueing")]
        public async Task<IHttpActionResult> GetProcessEmailQueueing()
        {
            string htmlbody = string.Empty;

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Get all Queue Email Notification
                        var emailQueue = db.EmailSendingQueues.Where(x => x.IsEmailSent == false).ToList();
                        if (emailQueue != null)
                        {
                            foreach (var em in emailQueue)
                            {
                                // Email Sending
                                EmailSender sendmail = new EmailSender();
                                sendmail.MailSubject = em.Subject;
                                sendmail.ToEmail = em.Email;
                                htmlbody = em.Message;

                                sendmail.ComposeMessage(htmlbody);

                                // Update Email after Succesful Notification
                                var sql = "Update EmailSendingQueue SET IsEmailSent = {1}, DateSent = {2} WHERE Id = {0}";
                                await db.Database.ExecuteSqlCommandAsync(sql, em.Id, true, DateTime.Now);

                                dbContextTransaction.Commit();

                                // ---------------- Start Transaction Activity Logs ------------------ //
                                AuditTrail log = new AuditTrail();
                                log.EventType = "NOTIFICATION";
                                log.Description = "Send Email Notification on pending notices";
                                log.PageUrl = this.PageUrl;
                                log.ObjectType = this.GetType().Name;
                                log.EventName = "Email Notification";
                                log.ContentDetail = JsonConvert.SerializeObject(emailQueue);
                                log.SaveTransactionLogs();
                                // ---------------- End Transaction Activity Logs -------------------- //
                            }
                        }

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        // Fetch record from SAP
        [Route("GetFetchAllSAPAccountWithTOAS")]
        public async Task<IHttpActionResult> GetFetchAllSAPAccountWithTOAS()
        {
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        string crftoken = "";
                        var data = new { COUNT = 0 };

                        // Credentials Assignment
                            HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(this.SAPUser, this.SAPPass) };

                        using (var client = new HttpClient(handler))
                        {
                            // Define Base PATH URL
                            client.BaseAddress = new Uri(string.Concat(this.SAPIP, ":", this.SAPPort));

                            // Headers
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Add("sap-client", this.SAPClient);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                            // Api End Point to get TOAS
                            var api = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C81D134B-2C19-5B37-9968-A18C089JDIEN").FirstOrDefault().vSettingOption);
                            HttpResponseMessage response = client.GetAsync(api).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                HttpContent content = response.Content;

                                // ... Read the string.
                                string result = await content.ReadAsStringAsync();

                                // Load result string into XmlDocument
                                XmlDocument xmldoc = new XmlDocument();
                                xmldoc.LoadXml(result);

                                // Parse Xml Document to make it LinqXML
                                XDocument xdoc = new XDocument();
                                xdoc = XDocument.Parse(xmldoc.InnerXml);

                                // Get only data with valid date and valid quotnum
                                var source = xdoc.Descendants().Where(d => d.Name == "item" && d.Descendants().Any(g => g.Name == "QUOT_NUM" && g.Value != "") && d.Descendants().Any(g => g.Name == "INFO12" && g.Value != "0000-00-00")).
                                             Select(c => new SAPZcomm { QUOT_NUM = c.Element("QUOT_NUM").Value, INFO12 = c.Element("INFO12").Value }).ToList();

                                // We have to get only projects with active company
                                var sapdata = (from w in source
                                               join x in db.SalesInventories on w.QUOT_NUM equals x.QuotDocNos
                                               join y in db.Projects on x.ProjectCode equals y.ProjectCode
                                               join z in db.Companies on y.CompanyCode equals z.Code
                                               where y.Published == true && z.Published == true && x.TOAS == null
                                               select new SAPZcomm { QUOT_NUM = w.QUOT_NUM, INFO12 = w.INFO12 }).ToList();

                                if (sapdata.Count > 0)
                                {
                                    // Update Sales Inventory Table with Valid TOAS
                                    foreach (var ds in sapdata)
                                    {
                                        var sql = "Update SalesInventory SET TOAS = {1} WHERE QuotDocNos = {0} AND TOAS is null ";
                                        await db.Database.ExecuteSqlCommandAsync(sql, ds.QUOT_NUM, ds.INFO12);
                                    }

                                    dbContextTransaction.Commit();
                                }

                                // ---------------- Start Transaction Activity Logs ------------------ //
                                AuditTrail log = new AuditTrail();
                                log.EventType = "FETCH";
                                log.Description = String.Concat("Fetch Valid TOAS Date from SAP.", sapdata.Count, " record(s) affected.");
                                log.PageUrl = this.PageUrl;
                                log.ObjectType = this.GetType().Name;
                                log.EventName = "Fetch data from SAP";
                                log.ContentDetail = JsonConvert.SerializeObject(sapdata);
                                log.SaveTransactionLogs();
                                // ---------------- End Transaction Activity Logs -------------------- //
                                data = new { COUNT = 1 };
                            } 
                            
                            // ---------------- Start Http Client Logs ------------------ //
                            AuditTrail httplog = new AuditTrail();
                            httplog.SaveHttpClientLogs(response.RequestMessage.Method.Method, response.RequestMessage.RequestUri.AbsoluteUri, crftoken, response.StatusCode.ToString(), response.IsSuccessStatusCode, response.ReasonPhrase, "GetFetchAllSAPAccountWithTOAS");
                            // ---------------- End Http Client Logs -------------------- //
                           
                            return Ok();
                        }
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        // Get Update Titlig Status Dashboard Summary
        [Route("GetUpdatedTitlingStatus")]
        public async Task<IHttpActionResult> GetUpdatedTitlingStatus()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var ProcessJobTitling = await db.ProcessJobs.Where(x => x.StatusCode == "OK" && x.IsSuccessStatusCode == true && x.ObjectType == "TitlingStatusDashboard").OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                    
                    // Process Job for spDashboardProcessingJob
                    db.spDashboardProcessingJob();

                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        // Post Parameter from TOMS to SAP
        [Route("GetSendTurnoverDateToSAP")]
        public async Task<IHttpActionResult> GetSendTurnoverDateToSAP()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    // attempt to download JSON data as a string
                    try
                    {
                        string crftoken = "";

                        // Credentials Assignment
                        HttpClientHandler handler = new HttpClientHandler()
                        {
                            Credentials = new NetworkCredential(this.SAPUser, this.SAPPass)
                        };

                        using (var client = new HttpClient(handler))
                        {
                            var basaUrl = string.Concat(this.SAPIP, ":", this.SAPPort);
                            client.BaseAddress = new Uri(basaUrl);

                            // Get all pending Unit Acceptance Date for SAP Synching
                            var unitAcceptance = db.UnitID_TOAcceptance.Where(x => x.IsUnitAcceptanceDateSAPSync == 0 && x.UnitAcceptanceDate != null).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitNos, x.UnitAcceptanceDate }).ToList();
                            if (unitAcceptance != null)
                            {
                                // SAP API Acceptance Date End Point
                                var apiEndPoint = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C82D134B-2C19-6B37-9968-A18C089JDIEN").FirstOrDefault().vSettingOption);
                                foreach (var ua in unitAcceptance)
                                {
                                    var api = apiEndPoint;
                                    // Replace Keyword with actual value
                                    api = api.Replace("{BUKRS}", ua.CompanyCode);
                                    api = api.Replace("{SWENR}", ua.ProjectCode);
                                    api = api.Replace("{SMENR}", ua.UnitNos);

                                    client.DefaultRequestHeaders.Clear();
                                    client.DefaultRequestHeaders.Add("x-csrf-token", "fetch");
                                    client.DefaultRequestHeaders.Add("sap-client", this.SAPClient);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    HttpResponseMessage response = client.GetAsync(api).Result;
                                   
                                    // Get x-csrf-token from previous Get Request 
                                    crftoken = response.Headers.GetValues("x-csrf-token").FirstOrDefault();

                                    client.DefaultRequestHeaders.Clear();
                                    client.DefaultRequestHeaders.Add("x-csrf-token", crftoken);
                                    client.DefaultRequestHeaders.Add("sap-client", this.SAPClient);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    // Serialize parameter
                                    string unitAcceptanceDate = ua.UnitAcceptanceDate.GetValueOrDefault().ToString("yyyyMMdd");

                                    var httpContent = new StringContent("{\"date\":" + unitAcceptanceDate + "}", Encoding.UTF8, "application/json");
                                    HttpResponseMessage result = client.PutAsync(api, httpContent).Result;
                                    if (result.IsSuccessStatusCode)
                                    {
                                        HttpContent content = result.Content;
                                        // ... Read the string.
                                        string resultStr = await content.ReadAsStringAsync();

                                        // Update Unit Acceptance Date after SAP Sync Process
                                        var sql = "Update UnitID_TOAcceptance SET IsUnitAcceptanceDateSAPSync = {1}, UnitAcceptanceDateSyncDate = {2} WHERE Id = {0}";
                                        await db.Database.ExecuteSqlCommandAsync(sql, ua.Id, 1, DateTime.Now);
                                    }
                                    // ---------------- Start Http Client Logs ------------------ //
                                    HttpClientLog log = new HttpClientLog(); // Change Log Table

                                    log.Method = result.RequestMessage.Method.Method;
                                    log.AbsoluteUri = result.RequestMessage.RequestUri.AbsoluteUri;
                                    log.CrfToken = crftoken;
                                    log.StatusCode = result.StatusCode.ToString();
                                    log.IsSuccessStatusCode = result.IsSuccessStatusCode;
                                    log.ReasonPhrase = result.ReasonPhrase;
                                    log.CreatedDate = DateTime.Now;
                                    log.ObjectType = "GetSendTurnoverDateToSAP";
                                    log.ContentDetail = string.Concat("Company: ", ua.CompanyCode, " | Project: ", ua.ProjectCode, " | Unit Nos: ", ua.UnitNos, " | Unit Acceptance Date: ", unitAcceptanceDate);

                                    db.HttpClientLogs.Add(log);
                                    db.SaveChanges();
                                    // ---------------- End Http Client Logs -------------------- //
                                }
                            }

                            // Get all pending Deemed Acceptance Date for SAP Synching
                            var deemedAcceptance = db.UnitID_DeemedAcceptance.Where(x => x.IsDeemedAcceptanceDateSAPSync == 0 && x.DeemedAcceptanceDate != null).Select(x => new { x.Id, x.CompanyCode, x.ProjectCode, x.UnitNos, x.DeemedAcceptanceDate }).ToList();
                            if (deemedAcceptance != null)
                            {
                                // SAP API Acceptance Date End Point         
                                var apiEndPoint = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C82D134B-2C19-6B37-9968-A18C089JDIEN").FirstOrDefault().vSettingOption);
                                foreach (var ua in deemedAcceptance)
                                {
                                    var api = apiEndPoint;
                                    // Replace Keyword with actual value
                                    api = api.Replace("{BUKRS}", ua.CompanyCode);
                                    api = api.Replace("{SWENR}", ua.ProjectCode);
                                    api = api.Replace("{SMENR}", ua.UnitNos);

                                    client.DefaultRequestHeaders.Clear();
                                    client.DefaultRequestHeaders.Add("x-csrf-token", "fetch");
                                    client.DefaultRequestHeaders.Add("sap-client", this.SAPClient);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    HttpResponseMessage response = client.GetAsync(api).Result;

                                    // Get x-csrf-token from previous Get Request 
                                    crftoken = response.Headers.GetValues("x-csrf-token").FirstOrDefault();

                                    client.DefaultRequestHeaders.Clear();
                                    client.DefaultRequestHeaders.Add("x-csrf-token", crftoken);
                                    client.DefaultRequestHeaders.Add("sap-client", this.SAPClient);
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    // Serialize parameter
                                    string deemedAcceptanceDate = ua.DeemedAcceptanceDate.GetValueOrDefault().ToString("yyyyMMdd");

                                    var httpContent = new StringContent("{\"date\":" + deemedAcceptanceDate + "}", Encoding.UTF8, "application/json");
                                    HttpResponseMessage result = client.PutAsync(api, httpContent).Result;
                                    if (result.IsSuccessStatusCode)
                                    {
                                        HttpContent content = result.Content;
                                        // ... Read the string.
                                        string resultStr = await content.ReadAsStringAsync();

                                        // Update Unit Acceptance Date after SAP Sync Process
                                        var sql = "Update UnitID_DeemedAcceptance SET IsDeemedAcceptanceDateSAPSync = {1}, DeemedAcceptanceDateSyncDate = {2} WHERE Id = {0}";
                                        await db.Database.ExecuteSqlCommandAsync(sql, ua.Id, 1, DateTime.Now);
                                    }
                                    // ---------------- Start Http Client Logs ------------------ //
                                    HttpClientLog log = new HttpClientLog(); // Change Log Table

                                    log.Method = result.RequestMessage.Method.Method;
                                    log.AbsoluteUri = result.RequestMessage.RequestUri.AbsoluteUri;
                                    log.CrfToken = crftoken;
                                    log.StatusCode = result.StatusCode.ToString();
                                    log.IsSuccessStatusCode = result.IsSuccessStatusCode;
                                    log.ReasonPhrase = result.ReasonPhrase;
                                    log.CreatedDate = DateTime.Now;
                                    log.ObjectType = "GetSendTurnoverDateToSAP";
                                    log.ContentDetail = string.Concat("Company: ", ua.CompanyCode, "| Project: ", ua.ProjectCode, "| Unit Nos: ", ua.UnitNos, "| Deemed Acceptance Date: ", deemedAcceptanceDate);

                                    db.HttpClientLogs.Add(log);
                                    db.SaveChanges();
                                    // ---------------- End Http Client Logs -------------------- //
                                }
                            }

                            dbContextTransaction.Commit();
                            return Ok();
                        }
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        // The .NET Framework gives you different ways to call a Web Service.
        //private void SomeNotes()
        //{
        //    // Using WebClient
        //    using (var wc = new WebClient())
        //    {
        //        wc.Credentials = new NetworkCredential(this.SAPUser, this.SAPPass);
        //        wc.Headers.Add("Accept", "application/json");
        //        wc.Headers.Add("sap-client", this.SAPClient);

        //        // default value is to retrieve all with TOAS
        //        var api = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C81D134B-2C19-5B37-9968-A18C089JDIEN").FirstOrDefault().vSettingOption);
        //        var url = string.Concat(this.SAPIP, ":", this.SAPPort, api);


        //        // donwload api json string
        //        var json_data = string.Empty;
        //        json_data = wc.DownloadString(url);
        //    }

        //    // Using HttpClient
        //    HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(this.SAPUser, this.SAPPass) };

        //    using (var client = new HttpClient(handler))
        //    {
        //        var api = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C81D134B-2C19-5B37-9968-A18C089JDIEN").FirstOrDefault().vSettingOption);
        //        var basaUrl = string.Concat(this.SAPIP, ":", this.SAPPort);

        //        client.BaseAddress = new Uri(basaUrl);

        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Add("sap-client", this.SAPClient);
        //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        //        //var byteArray = Encoding.ASCII.GetBytes("SAPDEV:xTAasV225jYg2mPy");
        //        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        //        HttpResponseMessage response = client.GetAsync(api).Result;

        //        HttpContent content = response.Content;
        //    }
        //}
    }
}
