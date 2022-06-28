using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Collections;
using System.Collections.Generic;
using WebApp.Helper;
using Newtonsoft.Json;
using System.Net;
using EntityFramework.BulkInsert.Extensions;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/Customer")]
    public class CustomerController : ApiController
    {
        private string PageUrl = "/Admin/Customer";
        //private string ApiName = "Customer";

        private CustomControl GetPermissionControl()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                var cId = User.Identity.GetUserId();
                var roleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;

                return db.Database.SqlQuery<CustomControl>("EXEC spPermissionControls {0}, {1}", roleId, PageUrl).SingleOrDefault();
            }
        }

        //[Route("SAPCustomer")]
        //public async Task<IHttpActionResult> SAPCustomer()
        //{
        //    using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
        //    {
        //        // attempt to download JSON data as a string
        //        try
        //        {
        //            using (var w = new WebClient())
        //            {
        //                // default value is to retrieve all companies
        //                var ip = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B34D124B-0D28-4A37-A867-D15C08900002").FirstOrDefault().vSettingOption);
        //                var port = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B35D124B-0D28-4A37-A867-D15C08900003").FirstOrDefault().vSettingOption);
        //                var api = Convert.ToString(db.Settings.Where(x => x.vSettingID == "C80D134B-1C19-4B37-9968-A18C089JDIEN").FirstOrDefault().vSettingOption);

        //                var url = string.Concat(ip, ":", port, api, "?page=1&limit=100");
        //                var user = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B16D224B-1C28-4A37-B767-B15C089JOMEL").FirstOrDefault().vSettingOption);
        //                var pass = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B23D124B-0D28-4A37-A867-D15C089DICEN").FirstOrDefault().vSettingOption);
        //                var client = Convert.ToString(db.Settings.Where(x => x.vSettingID == "B33D124B-0D28-4A37-A867-D15C08900001").FirstOrDefault().vSettingOption);
        //                var json_data = string.Empty;

        //                w.Credentials = new NetworkCredential(user, pass);
        //                w.Headers.Add("Accept", "application/json");
        //                w.Headers.Add("sap-client", client);

        //                // donwload api json string
        //                json_data = w.DownloadString(url);

        //                // if string with JSON data is not empty, deserialize it to class and return its instance 
        //                var sapdata = JsonConvert.DeserializeObject<List<SAPCustomer>>(json_data);

        //                // We have to get only records which is not currently exist on our database
        //                // intersect 2 objects and get only what we needed
        //                var keys = db.Customers.Select(x => new { x.BusPartnerID });
        //                sapdata.RemoveAll(x => keys.Any(k => k.BusPartnerID == x.KUNNR));

        //                using (var dbContextTransaction = db.Database.BeginTransaction())
        //                {
        //                    try 
        //                    {
        //                        var cId = User.Identity.GetUserId();

        //                        IEnumerable<Customer> source = null;
        //                        source = sapdata.Select(a => 
        //                                new Customer() 
        //                                { 
        //                                    BusPartnerID = a.KUNNR, 
        //                                    Name1 = a.NAME1, 
        //                                    Name2 = a.NAME2,
        //                                    Published = false,
        //                                    ModifiedByPK = cId,
        //                                    ModifiedDate = DateTime.Now,
        //                                    CreatedByPK = cId,
        //                                    CreatedDate = DateTime.Now
        //                                }).ToList();

        //                        await db.BulkInsertAsync<Customer>(source);
        //                        dbContextTransaction.Commit();

        //                        var data = new { RecordCount = source.Count() };
        //                        return Ok(data);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        dbContextTransaction.Rollback();
        //                        return BadRequest(ex.Message);
        //                    }
        //                }
        //            }
        //        } 
        //        catch (Exception ex)
        //        {
        //            return BadRequest("" + ex.Message);
        //        }
        //    }
        //}

        //[Route("GetCustomer")]
        //public async Task<IHttpActionResult> GetCustomer([FromUri] FilterModel param)
        //{
        //    using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
        //    {
        //        try
        //        {                                  
        //            var permissionCtrl = this.GetPermissionControl();

        //            IEnumerable<CustomCustomer> source = null;
        //            source = await (from cu in db.Customers
        //                                  select new CustomCustomer
        //                                  {
        //                                      Id = cu.Id,
        //                                      BusPartnerID = cu.BusPartnerID,
        //                                      Name1 = cu.Name1,
        //                                      Name2 = cu.Name2,
        //                                      Published = cu.Published.ToString(),
        //                                      isChecked = false,
        //                                      ModifiedByPK = cu.ModifiedByPK,
        //                                      ModifiedDate = cu.ModifiedDate,
        //                                      CreatedByPK = cu.CreatedByPK,
        //                                      CreatedDate = cu.CreatedDate
        //                                  }).ToListAsync();
                    
        //            // searching
        //            if (!string.IsNullOrWhiteSpace(param.search))
        //            {
        //                param.search = param.search.ToLower();
        //                source = source.Where(x => x.BusPartnerID.ToLower().Contains(param.search) || x.Name1.ToLower().Contains(param.search));
        //            }

        //            // sorting
        //            var sortby = typeof(CustomCustomer).GetProperty(param.sortby);
        //            switch (param.reverse)
        //            {
        //                case true:
        //                    source = source.OrderByDescending(s => sortby.GetValue(s, null));
        //                    break;
        //                case false:
        //                    source = source.OrderBy(s => sortby.GetValue(s, null));
        //                    break;
        //            }

        //            // paging
        //            var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);

        //            var data = new { COUNT = source.Count(), CustomerLIST = sourcePaged, CONTROLS = permissionCtrl };
        //            return Ok(data);
        //        }
        //        catch (Exception ex)
        //        {
        //            return BadRequest("" + ex.Message);
        //        }
        //    }
        //}

        //[Route("UpdateStatus")]
        //public async Task<IHttpActionResult> UpdateStatus(CustomCustomer data)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                foreach (var ds in data.dsList)
        //                {
        //                    var sql = "Update Customer SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
        //                    await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.Published, User.Identity.GetUserId(), DateTime.Now);
        //                }

        //                dbContextTransaction.Commit();
        //                return Ok();
        //            }
        //            catch (Exception ex)
        //            {
        //                dbContextTransaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }
        //        }
        //    }
        //}

        //[Route("SaveCustomer")]
        //public async Task<IHttpActionResult> SaveCustomer(Customer data)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                var cId = User.Identity.GetUserId();
        //                bool isCustomerExists = db.Customers.Where(x => x.BusPartnerID == data.BusPartnerID && x.Id != data.Id).Any();
        //                if (isCustomerExists)
        //                    return BadRequest("Customer Exists");

        //                data.ModifiedByPK = cId;
        //                data.ModifiedDate = DateTime.Now;
        //                if (data.Id == 0)
        //                {
        //                    data.CreatedByPK = cId;
        //                    data.CreatedDate = DateTime.Now;

        //                    db.Customers.Add(data);
        //                    await db.SaveChangesAsync();
        //                }
        //                else
        //                {
        //                    db.Entry(data).State = EntityState.Modified;
        //                    await db.SaveChangesAsync();
        //                }

        //                dbContextTransaction.Commit();
        //                return Ok();
        //            }
        //            catch (Exception ex)
        //            {
        //                dbContextTransaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }
        //        }
        //    }
        //}

        //[Route("RemoveData")]
        //public IHttpActionResult RemoveData(int ID)
        //{
        //    using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                db.Customers.RemoveRange(db.Customers.Where(x => x.Id == ID));
        //                db.SaveChanges();

        //                dbContextTransaction.Commit();
        //                return Ok();
        //            }
        //            catch (Exception ex)
        //            {
        //                dbContextTransaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }
        //        }
        //    }
        //}

        //[Route("RemoveRecords")]
        //public async Task<IHttpActionResult> RemoveRecords(CustomCustomer data)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                foreach (var ds in data.dsList)
        //                {
        //                    var sql = "DELETE FROM Customer WHERE Id = {0}";
        //                    await db.Database.ExecuteSqlCommandAsync(sql, ds.Id);
        //                }

        //                dbContextTransaction.Commit();
        //                return Ok();
        //            }
        //            catch (Exception ex)
        //            {
        //                dbContextTransaction.Rollback();
        //                return BadRequest(ex.Message);
        //            }
        //        }
        //    }
        //}
    }
}