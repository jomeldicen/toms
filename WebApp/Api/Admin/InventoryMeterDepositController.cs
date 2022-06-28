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

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/InventoryMeterDeposit")]
    public class InventoryMeterDepositController : ApiController
    {
        private string PageUrl = "/Admin/InventoryMeterDeposit";
        private string ApiName = "Inventory Meter Deposit";

        private CustomControl GetPermissionControl(string PageUrl)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.PageUrl = PageUrl;
                var cId = User.Identity.GetUserId();
                var roleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;

                return db.Database.SqlQuery<CustomControl>("EXEC spPermissionControls {0}, {1}", roleId, PageUrl).SingleOrDefault();
            }
        }

        [Route("GetSearchData")]
        public async Task<IHttpActionResult> GetSearchData([FromUri] SearchData item)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var prj = db.VW_Projects.Where(x => x.EMM == true && x.Id == item.ProjectID).SingleOrDefault();
                    if (prj != null)
                    {
                        item.ProjectCode = prj.ProjectCode;
                        item.CompanyCode = prj.CompanyCode;
                    }
                    // Get List of Active Projects
                    var projects = await db.VW_UnitInventory.Select(x => new { Id = x.ProjectId, x.CompanyCode, x.ProjectCode, x.BusinessEntity, x.ProjectLocation, ProjectCodeName = x.ProjectCode + " : " + x.BusinessEntity }).Distinct().OrderBy(x => x.BusinessEntity).ToListAsync();

                    // Get List of Units Inventory
                    var units = await db.VW_UnitInventory.Where(x => x.CompanyCode == item.CompanyCode && x.ProjectCode == item.ProjectCode && x.UnitCategoryCode == item.UnitCategory).Select(x => new { x.UnitNos, x.RefNos }).Distinct().OrderBy(x => x.UnitNos).ToListAsync();

                    var data = new { PROJECTLIST = projects, UNITLIST = units };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetInventoryMeterDeposit")]
        public async Task<IHttpActionResult> GetInventoryMeterDeposit([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);


                    SearchData searchData = JsonConvert.DeserializeObject<SearchData>(param.multiplesearch[0]);

                    // Get List of Titling Status
                    IQueryable<VW_ElectricMeterInvServiceDeposit> source = db.VW_ElectricMeterInvServiceDeposit.OrderBy(x => x.Id);

                    if (!String.IsNullOrEmpty(searchData.CompanyCode) && !String.IsNullOrEmpty(searchData.ProjectCode) && !String.IsNullOrEmpty(searchData.UnitCategory))
                    {
                        source = source.Where(x => x.CompanyCode == searchData.CompanyCode && x.ProjectCode == searchData.ProjectCode && x.UnitCategoryCode == searchData.UnitCategory);

                        // if unit nos is set, include in the criteria
                        if (!String.IsNullOrEmpty(searchData.UnitNos))
                            source = source.Where(x => x.UnitNos == searchData.UnitNos);
                    }

                    // searching
                    if (!String.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.CompanyCode.ToLower().Contains(param.search) || x.CompanyName.ToLower().Contains(param.search) || x.ProjectCode.ToLower().Contains(param.search) ||
                                                   x.BusinessEntity.ToLower().Contains(param.search) || x.RefNos.ToLower().Contains(param.search) || x.UnitCategoryDesc.ToLower().Contains(param.search));
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);
                    // Get the final list base on the define linq queryable parameter
                    var results = await sourcePaged.ToListAsync();

                    IEnumerable<CustomInventoryMeterDeposit> inventoryDeposit = null;
                    inventoryDeposit = results.Select(x => new CustomInventoryMeterDeposit {
                        Id = Convert.ToInt16(x.MeterDepositId),
                        CompanyCode = x.CompanyCode,
                        CompanyName = x.CompanyName,
                        ProjectCode = x.ProjectCode,
                        BusinessEntity = x.BusinessEntity,
                        UnitCategory = x.UnitCategoryCode,
                        UnitCategoryDesc = x.UnitCategoryDesc,
                        RefNos = x.RefNos,
                        UnitNos = x.UnitNos,
                        CustomerNos = x.CustomerNos,
                        QuotDocNos = x.QuotDocNos,
                        SalesDocNos = x.SalesDocNos,
                        ElectricMeterId = x.ElectricMeterId,
                        MeralcoSubmittedDate = x.MeralcoSubmittedDate,
                        UnitOwnerReceiptDate = x.UnitOwnerReceiptDate,
                        MeralcoReceiptDate = x.MeralcoReceiptDate,
                        MeterDepositAmount = x.MeterDepositAmount,
                        CreatedByPK = x.CreatedByPK,
                        CreatedDate = x.CreatedDate                 
                    }).AsEnumerable();

                    // sorting
                    var sortby = typeof(CustomInventoryMeterDeposit).GetProperty(param.sortby);
                    switch (param.reverse)
                    {
                        case true:
                            inventoryDeposit = inventoryDeposit.OrderByDescending(s => sortby.GetValue(s, null));
                            break;
                        case false:
                            inventoryDeposit = inventoryDeposit.OrderBy(s => sortby.GetValue(s, null));
                            break;
                    }

                    var data = new { COUNT = source.Count(), InventoryMeterDepositLIST = inventoryDeposit, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomInventoryMeterDeposit data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = db.ElectricMeterInvServiceDeposits.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.CompanyCode, x.CompanyName, x.ProjectCode, x.ProjectName, x.UnitCategory, x.UnitCategoryDesc, x.UnitNos, x.RefNos, x.MeterDepositAmount, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update ElectricMeterInvServiceDeposit SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.Published, User.Identity.GetUserId(), DateTime.Now);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = (data.Published == "True") ? "Activate list of LOV " + this.ApiName : "Deactivate list of LOV " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

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

        [Route("SaveInventoryMeterDeposit")]
        public async Task<IHttpActionResult> SaveInventoryMeterDeposit(CustomInventoryMeterDeposit data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        var cId = User.Identity.GetUserId();

                        ElectricMeterInvServiceDeposit emsd = new ElectricMeterInvServiceDeposit();

                        emsd.Id = data.Id;
                        emsd.CompanyCode = data.CompanyCode;
                        emsd.CompanyName = data.CompanyName;
                        emsd.ProjectCode = data.ProjectCode;
                        emsd.ProjectName = data.BusinessEntity;
                        emsd.TowerCode = data.ProjectCode;
                        emsd.TowerName = data.BusinessEntity;
                        emsd.UnitCategory = data.UnitCategory;
                        emsd.UnitCategoryDesc = data.UnitCategoryDesc;
                        emsd.UnitNos = data.UnitNos;
                        emsd.RefNos = data.RefNos;
                        emsd.MeterDepositAmount = data.MeterDepositAmount;
                        emsd.Published = true;

                        emsd.ModifiedByPK = cId;
                        emsd.ModifiedDate = DateTime.Now;

                        if (data.Id == 0)
                        {
                            nwe = true;
                            emsd.CreatedByPK = cId;
                            emsd.CreatedDate = DateTime.Now;

                            db.ElectricMeterInvServiceDeposits.Add(emsd);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            emsd.CreatedByPK = data.CreatedByPK;
                            emsd.CreatedDate = Convert.ToDateTime(data.CreatedDate);

                            db.Entry(emsd).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        
                        var sql = "Update ElectricMeter SET MeterDepositAmount = {0} WHERE Id = {1}";
                        await db.Database.ExecuteSqlCommandAsync(sql, data.MeterDepositAmount, data.ElectricMeterId);

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD" : "UPDATE";
                        log.Description = ((string.IsNullOrEmpty(data.ReasonForChange)) ? "ADD " : "UPDATE ") + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.Remarks = data.ReasonForChange;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(emsd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

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
    }
}