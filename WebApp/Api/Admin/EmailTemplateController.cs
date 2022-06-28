﻿using System;
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
using WebApp.Helper;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/EmailTemplate")]
    public class EmailTemplateController : ApiController
    {
        private string PageUrl = "/Admin/EmailTemplate";
        private string ApiName = "Email Template";

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

        [Route("GetEmailTemplate")]
        public async Task<IHttpActionResult> GetEmailTemplate([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {                                  
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomEmailTemplate> source = null;
                    source = await (from et in db.EmailTemplates
                                          select new CustomEmailTemplate
                                          {
                                              Id = et.Id,
                                              Code = et.Code,
                                              Name = et.Name,
                                              Description = et.Description,
                                              EmailSubject = et.EmailSubject,
                                              EmailBody = et.EmailBody,
                                              Published = et.Published.ToString(),
                                              isChecked = false,
                                              ModifiedByPK = et.ModifiedByPK,
                                              ModifiedDate = et.ModifiedDate,
                                              CreatedByPK = et.CreatedByPK,
                                              CreatedDate = et.CreatedDate
                                          }).ToListAsync();
                    
                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Code.ToLower().Contains(param.search) || x.Name.ToLower().Contains(param.search) || x.Description.ToLower().Contains(param.search) || x.EmailSubject.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomEmailTemplate).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), EmailTemplateLIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomEmailTemplate data)
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
                        var cd = db.EmailTemplates.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.Description, x.EmailSubject, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update EmailTemplate SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
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

        [Route("SaveEmailTemplate")]
        public async Task<IHttpActionResult> SaveEmailTemplate(EmailTemplate data)
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
                        var nwe = false;
                        var cId = User.Identity.GetUserId();

                        bool isEmailTemplateExists = db.EmailTemplates.Where(x => x.Id != data.Id && x.Name == data.Name).Any();
                        if (isEmailTemplateExists)
                            return BadRequest("Exists");

                        data.ModifiedByPK = cId;
                        data.ModifiedDate = DateTime.Now;
                        if (data.Id == 0)
                        {
                            nwe = true;
                            data.CreatedByPK = cId;
                            data.CreatedDate = DateTime.Now;

                            db.EmailTemplates.Add(data);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.Entry(data).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create LOV " + this.ApiName : "Update LOV " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(data);
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

        [Route("RemoveData")]
        public IHttpActionResult RemoveData(int ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cd = db.EmailTemplates.Where(x => x.Id == ID).Select(x => new { x.Id, x.Name, x.Description, x.EmailSubject, x.EmailBody, Published = x.Published.ToString() }).SingleOrDefault();

                        db.EmailTemplates.RemoveRange(db.EmailTemplates.Where(x => x.Id == ID));
                        db.SaveChanges();

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete single LOV " + this.ApiName;
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

        [Route("RemoveRecords")]
        public async Task<IHttpActionResult> RemoveRecords(CustomEmailTemplate data)
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
                        var cd = db.EmailTemplates.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.Description, x.EmailSubject, x.EmailBody, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM EmailTemplate WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete list LOV " + this.ApiName;
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
    }
}