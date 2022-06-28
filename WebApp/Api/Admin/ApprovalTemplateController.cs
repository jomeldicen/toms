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
    [RoutePrefix("api/ApprovalTemplate")]
    public class ApprovalTemplateController : ApiController
    {
        private string PageUrl = "/Admin/ApprovalTemplate";

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

        [Route("GetApprovalTemplate")]
        public async Task<IHttpActionResult> GetApprovalTemplate([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {                                  
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomApprovalTemplate> source = null;
                    source = await (from apt in db.ApprovalTemplates
                                          select new CustomApprovalTemplate
                                          {
                                              Id = apt.Id,
                                              Name = apt.Name,
                                              Description = apt.Description,
                                              Published = apt.Published.ToString(),
                                              isChecked = false,
                                              ModifiedByPK = apt.ModifiedByPK,
                                              ModifiedDate = apt.ModifiedDate,
                                              CreatedByPK = apt.CreatedByPK,
                                              CreatedDate = apt.CreatedDate
                                          }).ToListAsync();
                    
                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Name.ToLower().Contains(param.search) || x.Description.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomApprovalTemplate).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), ApprovalTemplateLIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomApprovalTemplate data)
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
                            var sql = "Update ApprovalTemplate SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
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

        [Route("SaveApprovalTemplate")]
        public async Task<IHttpActionResult> SaveApprovalTemplate(ApprovalTemplate data)
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
                        bool isApprovalTemplateExists = db.ApprovalTemplates.Where(x => x.Name == data.Name && x.Id != data.Id).Any();
                        if (isApprovalTemplateExists)
                            return BadRequest("ApprovalTemplate Exists");

                        data.ModifiedByPK = cId;
                        data.ModifiedDate = DateTime.Now;
                        if (data.Id == null)
                        {
                            data.CreatedByPK = cId;
                            data.CreatedDate = DateTime.Now;

                            db.ApprovalTemplates.Add(data);
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
                        db.ApprovalTemplates.RemoveRange(db.ApprovalTemplates.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomApprovalTemplate data)
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
                            var sql = "DELETE FROM ApprovalTemplate WHERE Id = {0}";
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