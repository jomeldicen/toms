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
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Reporting.WebForms;
using System.Web;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/DashboardTOSchedule")]
    public class DashboardTOScheduleController : ApiController
    {
        private string PageUrl = "/Admin/DashboardTOSchedule";
        //private string ApiName = "Dashboard Turnover Schedule";

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
                    // Get List of Handover Associate
                    var HandoverAssocs = db.UnitQD_NoticeTO.GroupBy(x => x.HandoverAssociate).Select(x => new { id = x.Key, label = x.Key }).ToList();//.Select((x, i) => new { id = i + 1, x.label }).ToList();
                   
                    // Get List of Account Type
                    var AccountTypes = await db.Options.Where(x => x.OptionGroup == "Account Type" && x.Published == true).Select(x=> new { id = x.Name, label = x.Description }).ToListAsync();

                    // Get List of Turnover Option
                    var TurnoverOptions = await db.TurnoverOptions.Where(x => x.Published == true).Select(x => new { id = x.Name, label = x.Name }).ToListAsync();
                    
                    var data = new { HANDOVERASSOCLIST = HandoverAssocs, ACCTTYPELIST = AccountTypes, TOOPTIONLIST = TurnoverOptions };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetDashboardTOSchedule")]
        public async Task<IHttpActionResult> GetDashboardTOSchedule([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    // Check if system parameter is properly set
                    var systemParameter = await db.SystemParameters.Where(x => x.Published == true).FirstOrDefaultAsync();
                    if (systemParameter == null)
                        return BadRequest("Please configure system parameter");

                    DateTime dt = DateTime.Today;
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    // Get Current User
                    var cId = User.Identity.GetUserId();
                    var user = db.AspNetUsersProfiles.Where(x => x.Id == cId).Select(x => new { vFullname = x.vFirstName + " " + x.vLastName }).SingleOrDefault().vFullname;

                    // Get Selected Handover Associates
                    if (param.searchbykey1 == null || param.searchbykey2 == null || param.searchbykey3 == null || param.DateFrom == null || param.DateTo == null)
                        return BadRequest("Please first make a selection from search criteria");

                    // Get List of Qualified Clients for Scheduling
                    IQueryable<VW_QualifiedTurnover> source = db.VW_QualifiedTurnover.
                        Where(x => x.TOAS != null && x.FinalTurnoverOption != null && param.searchbykey1.Contains(x.HandoverAssociate) && param.searchbykey2.Contains(x.AccountTypeCode) && 
                                    param.searchbykey3.Contains(x.FinalTurnoverOption) && (x.FinalTurnoverDate >= param.DateFrom.Date && x.FinalTurnoverDate <= param.DateTo.Date)).
                        OrderBy(x => x.FinalTurnoverDate).OrderBy(x => x.FinalTurnoverOption).OrderBy(x => x.AccountTypeDesc).OrderBy(x => x.BusinessEntity).OrderBy(x => x.Phase).OrderBy(x => x.RefNos).OrderBy(x => x.HandoverAssociate);

                    
                    // Business Rule with SAP Cut-off Date based on System Parameter
                    if (systemParameter.EnableTOCutOffDate == true)
                        source = source.Where(x => x.TranClass == "Business Rule 1");

                    // for Searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.ProjectCode.ToLower().Contains(param.search) || x.RefNos.ToLower().Contains(param.search) ||
                                            x.UnitType.ToLower().Contains(param.search) || x.CustomerNos.ToLower().Contains(param.search) ||
                                            x.CustomerName1.ToLower().Contains(param.search) || x.FinalTurnoverOption.ToLower().Contains(param.search) ||
                                            x.AccountTypeDesc.ToLower().Contains(param.search) || x.HandoverAssociate.ToLower().Contains(param.search));
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);

                    // Get the final list base on the define linq queryable parameter
                    var results = sourcePaged.Select(x => new {
                        x.FinalTurnoverDate,
                        x.FinalTurnoverTime,
                        x.FinalTurnoverOption,
                        x.CustomerNos,
                        x.CustomerName1,
                        x.UnitType,
                        x.ProjectCode,
                        x.BusinessEntity,
                        x.RefNos,
                        x.Phase,
                        x.HandoverAssociate,
                        x.AccountTypeDesc,
                        x.TranClass
                    }).ToList();

                    IEnumerable<CustomDashboard_TOSchedule> toSchedule = null;
                    toSchedule = results.Select(x => new CustomDashboard_TOSchedule
                                {
                                    FinalTurnoverDate = x.FinalTurnoverDate.Value.Add(x.FinalTurnoverTime.Value),
                                    FinalTurnoverOption = x.FinalTurnoverOption,
                                    CustomerNos = x.CustomerNos,
                                    CustomerName = x.CustomerName1,
                                    UnitType = x.UnitType,
                                    ProjectCode = x.ProjectCode,
                                    BusinessEntity = x.BusinessEntity,
                                    RefNos = x.RefNos,
                                    Phase = x.Phase,
                                    HandoverAssociate = x.HandoverAssociate,
                                    AccountTypeDesc = x.AccountTypeDesc
                                }).AsEnumerable();

                    // For Sorting
                    if (param.sortby != "default")
                    {
                        var sortby = typeof(CustomDashboard_TOSchedule).GetProperty(param.sortby);
                        switch (param.reverse)
                        {
                            case true:
                                toSchedule = toSchedule.OrderByDescending(s => sortby.GetValue(s, null));
                                break;
                            case false:
                                toSchedule = toSchedule.OrderBy(s => sortby.GetValue(s, null));
                                break;
                        }
                    }
                    else
                    {
                        toSchedule = toSchedule.OrderBy(s => s.FinalTurnoverDate).ThenBy(s => s.FinalTurnoverOption).ThenBy(s => s.AccountTypeDesc).ThenBy(s => s.BusinessEntity).ThenBy(s => s.Phase).ThenBy(s => s.RefNos).ThenBy(s => s.HandoverAssociate);
                    }

                    var data = new { COUNT = source.Count(), QUALIFIEDTOSCHEDULE = toSchedule, CURUSER = user, CONTROLS = permissionCtrl };

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }        
    }
}