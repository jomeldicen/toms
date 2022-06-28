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
using Newtonsoft.Json;
using System.Net;
using EntityFramework.BulkInsert.Extensions;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/ApprovalStage")]
    public class ApprovalStageController : ApiController
    {
        private string PageUrl = "/Admin/ApprovalStages";

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

        [Route("GetApprovalStage")]
        public async Task<IHttpActionResult> GetApprovalStage([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {                                  
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomApprovalStage> source = null;
                    source = await (from aps in db.ApprovalStages
                                          select new CustomApprovalStage
                                          {
                                              Id = aps.Id,
                                              Name = aps.Name,
                                              Description = aps.Description,
                                              ApprovalNos = aps.ApprovalNos,
                                              RejectionNos = aps.RejectionNos,
                                              Published = aps.Published.ToString(),
                                              isChecked = false,
                                              ModifiedByPK = aps.ModifiedByPK,
                                              ModifiedDate = aps.ModifiedDate,
                                              CreatedByPK = aps.CreatedByPK,
                                              CreatedDate = aps.CreatedDate
                                          }).ToListAsync();
                    
                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Name.ToLower().Contains(param.search) || x.Description.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomApprovalStage).GetProperty(param.sortby);
                    switch (param.reverse)
                    {
                        case true:
                            source = source.OrderByDescending(s => sortby.GetValue(s, null));
                            break;
                        case false:
                            source = source.OrderBy(s => sortby.GetValue(s, null));
                            break;
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);

                    var data = new { COUNT = source.Count(), ApprovalStageLIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomApprovalStage data)
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
                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update ApprovalStages SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.Published, User.Identity.GetUserId(), DateTime.Now);
                        }

                        dbContextTransaction.Commit();
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

        [Route("SaveApprovalStage")]
        public async Task<IHttpActionResult> SaveApprovalStage(ApprovalStage data)
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
                        var cId = User.Identity.GetUserId();
                        bool isApprovalStageExists = db.ApprovalStages.Where(x => x.Name == data.Name && x.Id != data.Id).Any();
                        if (isApprovalStageExists)
                            return BadRequest("ApprovalStage Exists");

                        data.ModifiedByPK = cId;
                        data.ModifiedDate = DateTime.Now;
                        if (data.Id == null)
                        {
                            data.CreatedByPK = cId;
                            data.CreatedDate = DateTime.Now;

                            db.ApprovalStages.Add(data);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(data).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();
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

        [Route("RemoveData")]
        public IHttpActionResult RemoveData(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.ApprovalStages.RemoveRange(db.ApprovalStages.Where(x => x.Id == ID));
                        db.SaveChanges();

                        dbContextTransaction.Commit();
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

        [Route("RemoveRecords")]
        public async Task<IHttpActionResult> RemoveRecords(CustomApprovalStage data)
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
                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM ApprovalStages WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id);
                        }

                        dbContextTransaction.Commit();
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