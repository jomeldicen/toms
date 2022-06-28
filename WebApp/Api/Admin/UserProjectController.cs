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
    [RoutePrefix("api/UserProject")]
    public class UserProjectController : ApiController
    {
        private string PageUrl = "/Admin/UserProject";
        //private string ApiName = "User Project";

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

        [Route("GetUsers")]
        public async Task<IHttpActionResult> GetUsers([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var Id = db.AspNetUserRoles.Where(x => x.RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03").FirstOrDefault().UserId;
                    Id = (User.Identity.GetUserId() == Id) ? null : Id;

                    IEnumerable<CustomUsers> source = null;
                    source = await (from usr in db.AspNetUsers
                                    where usr.Id != Id
                                    select new CustomUsers
                                    {
                                        Id = usr.Id,
                                        Email = usr.Email,
                                        NosProjects = db.AspNetUsersProjects.Where(x => x.UserID == usr.Id).Count(),
                                        RoleName = usr.AspNetUserRoles.FirstOrDefault().AspNetRole.Name,
                                        FullName = usr.AspNetUsersProfile.vFirstName + " " + usr.AspNetUsersProfile.vLastName,
                                        Photo = usr.AspNetUsersProfile.vPhoto,
                                        Company = usr.AspNetUsersProfile.vCompany,
                                        Department = usr.AspNetUsersProfile.vDepartment,
                                        Position = usr.AspNetUsersProfile.vPosition,
                                        Rank = usr.AspNetUsersProfile.vRank
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.FullName.ToLower().Contains(param.search) || x.RoleName.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomUsers).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), USERLIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetUserProject")]
        public async Task<IHttpActionResult> GetUserProject(string ID)
        {            
            using (WebAPPv0MainEntities db = new WebAPPv0MainEntities())
            {
                try
                {                    
                    var source = await db.Projects.Where(x => x.Published == true).ToListAsync();
                    WebAppEntities db1 = new WebAppEntities();
                    List<CustomUserProject> UserProj = new List<CustomUserProject>();
                    foreach (var item in source)
                    {
                        CustomUserProject up = new CustomUserProject();
                        up.ProjectID = item.Id;
                        up.UserID = ID;
                        up.ProjectCode = item.ProjectCode;
                        up.BusinessEntity = item.BusinessEntity;
                        up.isChecked = (db1.AspNetUsersProjects.Where(x => x.UserID == ID && x.ProjectID == item.Id).Count() == 0) ? false : true;
                        UserProj.Add(up);
                    }
                    db1.Dispose();

                    var data = new { PROJECTLIST = UserProj };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveUserProject")]
        public async Task<IHttpActionResult> SaveUserProject(List<CustomUserProject> data)
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
                        var sql = "DELETE FROM AspNetUsersProject WHERE UserID = {0}";
                        await db.Database.ExecuteSqlCommandAsync(sql, data.FirstOrDefault().UserID);

                        data = data.Where(x => x.isChecked == true).ToList();
                        foreach (var ds in data)
                        {
                            AspNetUsersProject up = new AspNetUsersProject();

                            up.vUserProjId = Guid.NewGuid().ToString();
                            up.ProjectID = ds.ProjectID;
                            up.UserID = ds.UserID;

                            db.AspNetUsersProjects.Add(up);
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

        //[Route("RemoveData")]
        //public IHttpActionResult RemoveData(int ID)
        //{
        //    using (WebAppEntities db = new WebAppEntities())
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                db.UserProjects.RemoveRange(db.UserProjects.Where(x => x.Id == ID));
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
        //public async Task<IHttpActionResult> RemoveRecords(CustomUserProject data)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    using (WebAppEntities db = new WebAppEntities())
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                foreach (var ds in data.dsList)
        //                {
        //                    var sql = "DELETE FROM UserProject WHERE Id = {0}";
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