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
    [RoutePrefix("api/SystemParameter")]
    public class SystemParameterController : ApiController
    {
        private string PageUrl = "/Admin/SystemParameter";
        private string ApiName = "System Parameter";

        string timezone = "";

        private SystemParameterController()
        {
            this.timezone = "Taipei Standard Time";
        }
        private CustomControl GetPermissionControl()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                var cId = User.Identity.GetUserId();
                var roleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;

                return db.Database.SqlQuery<CustomControl>("EXEC spPermissionControls {0}, {1}", roleId, PageUrl).SingleOrDefault();
            }
        }

        public async Task<IHttpActionResult> Get()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl();

                    // Get List of Turnover Options
                    var emailtemplates = await db.EmailTemplates.Where(x => x.Published == true).Select(x => new { x.Id, x.Name }).OrderBy(x => x.Id).ToListAsync();
                   
         
                    CustomSystemParameter systemSettings = await (from sp in db.SystemParameters
                                                                  where sp.Id == 1
                                                                  select new CustomSystemParameter
                                                                  {
                                                                    Id = sp.Id,
                                                                    Title = sp.Title,
                                                                    TurnoverMaxDays = sp.TurnoverMaxDays,
                                                                    TurnoverMaxDaysCT = sp.TurnoverMaxDaysCT,
                                                                    TurnoverMaxDays2 = sp.TurnoverMaxDays2,
                                                                    TurnoverMaxDays2CT = sp.TurnoverMaxDays2CT,
                                                                    NoticeNosDays1 = sp.NoticeNosDays1,
                                                                    NoticeSendRecipient1 = sp.NoticeSendRecipient1,
                                                                    NoticeRecipientEmail1 = sp.NoticeRecipientEmail1,
                                                                    NoticeTemplateID1 = sp.NoticeTemplateID1.ToString(),
                                                                    NoticeCT1 = sp.NoticeCT1,
                                                                    NoticeNosDays2 = sp.NoticeNosDays2,
                                                                    NoticeSendRecipient2 = sp.NoticeSendRecipient2,
                                                                    NoticeRecipientEmail2 = sp.NoticeRecipientEmail2,
                                                                    NoticeTemplateID2 = sp.NoticeTemplateID2.ToString(),
                                                                    NoticeCT2 = sp.NoticeCT2,
                                                                    BusinessHourFrom = sp.BusinessHourFrom,
                                                                    BusinessHourTo = sp.BusinessHourTo,
                                                                    ReschedMaxDays1 = sp.ReschedMaxDays1,
                                                                    ReschedMaxDays1CT = sp.ReschedMaxDays1CT,
                                                                    ReschedMaxDays2 = sp.ReschedMaxDays2,
                                                                    ReschedMaxDays2CT = sp.ReschedMaxDays2CT,
                                                                    TurnoverStatusTAT = sp.TurnoverStatusTAT,
                                                                    TurnoverStatusTATCT = sp.TurnoverStatusTATCT,
                                                                    PunchlistDateTAT = sp.PunchlistDateTAT,
                                                                    PunchlistDateTATCT = sp.PunchlistDateTATCT,
                                                                    ReinspectionDateTAT = sp.ReinspectionDateTAT,
                                                                    ReinspectionDateTATCT = sp.ReinspectionDateTATCT,
                                                                    AdjReinspectionTAT = sp.AdjReinspectionTAT,
                                                                    AdjReinspectionTATCT = sp.AdjReinspectionTATCT,
                                                                    AdjReinspectionMaxDays = sp.AdjReinspectionMaxDays,
                                                                    AdjReinspectionMaxDaysCT = sp.AdjReinspectionMaxDaysCT,
                                                                    RushTicketMaxTAT = sp.RushTicketMaxTAT,
                                                                    RushTicketMaxTATCT = sp.RushTicketMaxTATCT,
                                                                    UnitAcceptanceTAT = sp.UnitAcceptanceTAT,
                                                                    UnitAcceptanceTATCT = sp.UnitAcceptanceTATCT,
                                                                    EnableTOStatusMax = sp.EnableTOStatusMax,
                                                                    EnableTOStatusMaxCT = sp.EnableTOStatusMaxCT,
                                                                    DeemedDaysTAT1 = sp.DeemedDaysTAT1,
                                                                    DeemedDaysTAT1CT = sp.DeemedDaysTAT1CT,
                                                                    DeemedDaysTAT2 = sp.DeemedDaysTAT2,
                                                                    DeemedDaysTAT2CT = sp.DeemedDaysTAT2CT,
                                                                    DeemedEmailDateSentMaxDays = sp.DeemedEmailDateSentMaxDays,
                                                                    DeemedEmailDateSentMaxDaysCT = sp.DeemedEmailDateSentMaxDaysCT,
                                                                    TitleInProcessTAT1 = sp.TitleInProcessTAT1,
                                                                    TitleInProcessTAT2 = sp.TitleInProcessTAT2,
                                                                    TitleInProcessTAT3 = sp.TitleInProcessTAT3,
                                                                    TitleTransferredTAT = sp.TitleTransferredTAT,
                                                                    TitleClaimedTAT = sp.TitleClaimedTAT,
                                                                    TaxDeclarationTransferredTAT = sp.TaxDeclarationTransferredTAT,
                                                                    TaxDeclarationClaimedTAT = sp.TaxDeclarationClaimedTAT,
                                                                    LiquidationEndorsedTAT = sp.LiquidationEndorsedTAT,
                                                                    TitleReleaseEndorsedTAT = sp.TitleReleaseEndorsedTAT,
                                                                    BankReleasedTAT = sp.BankReleasedTAT,
                                                                    BuyerReleasedTAT = sp.BuyerReleasedTAT,
                                                                    TitleInProcessTATCT = sp.TitleInProcessTATCT,
                                                                    TitleTransferredTATCT = sp.TitleTransferredTATCT,
                                                                    TitleClaimedTATCT = sp.TitleClaimedTATCT,
                                                                    TaxDeclarationTransferredTATCT = sp.TaxDeclarationTransferredTATCT,
                                                                    TaxDeclarationClaimedTATCT = sp.TaxDeclarationClaimedTATCT,
                                                                    LiquidationEndorsedTATCT = sp.LiquidationEndorsedTATCT,
                                                                    TitleReleaseEndorsedTATCT = sp.TitleReleaseEndorsedTATCT,
                                                                    BuyerReleasedTATCT = sp.BuyerReleasedTATCT,
                                                                    BankReleasedTATCT = sp.BankReleasedTATCT,
                                                                    Published = sp.Published.ToString(),
                                                                    Applicability = sp.Applicability,
                                                                    TitlingStatusEffectivityDate = sp.TitlingStatusEffectivityDate,
                                                                    EnableTOCutOffDate = sp.EnableTOCutOffDate,
                                                                    TOCutOffDate = sp.TOCutOffDate,
                                                                    EnableTSCutOffDate = sp.EnableTSCutOffDate,
                                                                    DocCompletionTAT = sp.DocCompletionTAT,
                                                                    DocCompletionTATCT = sp.DocCompletionTATCT,
                                                                    RFPCreationTAT = sp.RFPCreationTAT,
                                                                    RFPCreationTATCT = sp.RFPCreationTATCT,
                                                                    CheckPaymentReleaseTAT = sp.CheckPaymentReleaseTAT,
                                                                    CheckPaymentReleaseTATCT = sp.CheckPaymentReleaseTATCT,
                                                                    MeralcoSubmissionTAT = sp.MeralcoSubmissionTAT,
                                                                    MeralcoSubmissionTATCT = sp.MeralcoSubmissionTATCT,
                                                                    TransferElectricServTAT = sp.TransferElectricServTAT,
                                                                    TransferElectricServTATCT = sp.TransferElectricServTATCT,
                                                                    UnitOwnerReceiptTAT = sp.UnitOwnerReceiptTAT,
                                                                    UnitOwnerReceiptTATCT = sp.UnitOwnerReceiptTATCT,
                                                                    ElectricMeterEffectivityDate = sp.ElectricMeterEffectivityDate,
                                                                    TSCutOffDate = sp.TSCutOffDate,
                                                                    ModifiedByPK = sp.ModifiedByPK,
                                                                    ModifiedDate = sp.ModifiedDate,
                                                                    CreatedByPK = sp.CreatedByPK,
                                                                    CreatedDate = sp.CreatedDate
                                                                  }).FirstOrDefaultAsync();


                    if(systemSettings != null)
                    {
                        systemSettings.BsHrFrm = new DateTime(2020, 01, 01, systemSettings.BusinessHourFrom.Hours, systemSettings.BusinessHourFrom.Minutes, systemSettings.BusinessHourFrom.Seconds);
                        systemSettings.BsHrTo = new DateTime(2020, 01, 01, systemSettings.BusinessHourTo.Hours, systemSettings.BusinessHourTo.Minutes, systemSettings.BusinessHourTo.Seconds);
                    }

                    var data = new { SYSTEMSETTING = systemSettings, EMAILTEMPLATELIST = emailtemplates, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("SaveSystemParameter")]
        public async Task<IHttpActionResult> SaveSystemParameter(CustomSystemParameter data)
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
                        if (data.Id == 0)
                            return BadRequest("System Parameter is required. Please contact your System Administrator");

                        var cId = User.Identity.GetUserId();
                        SystemParameter sp = new SystemParameter();

                        sp.Id = data.Id;
                        sp.Title = data.Title;
                        sp.TurnoverMaxDays = data.TurnoverMaxDays;
                        sp.TurnoverMaxDaysCT = data.TurnoverMaxDaysCT;
                        sp.TurnoverMaxDays2 = data.TurnoverMaxDays2;
                        sp.TurnoverMaxDays2CT = data.TurnoverMaxDays2CT;
                        sp.NoticeNosDays1 = data.NoticeNosDays1;
                        sp.NoticeSendRecipient1 = data.NoticeSendRecipient1;
                        sp.NoticeRecipientEmail1 = data.NoticeRecipientEmail1;
                        sp.NoticeTemplateID1 = Convert.ToInt16(data.NoticeTemplateID1);
                        sp.NoticeCT1 = data.NoticeCT1;
                        sp.NoticeNosDays2 = data.NoticeNosDays2;
                        sp.NoticeSendRecipient2 = data.NoticeSendRecipient2;
                        sp.NoticeRecipientEmail2 = data.NoticeRecipientEmail2;
                        sp.NoticeTemplateID2 = Convert.ToInt16(data.NoticeTemplateID2);
                        sp.NoticeCT2 = data.NoticeCT2;
                        sp.ReschedMaxDays1 = data.ReschedMaxDays1;
                        sp.ReschedMaxDays1CT = data.ReschedMaxDays1CT;
                        sp.ReschedMaxDays2 = data.ReschedMaxDays2;
                        sp.ReschedMaxDays2CT = data.ReschedMaxDays2CT;
                        sp.TurnoverStatusTAT = data.TurnoverStatusTAT;
                        sp.TurnoverStatusTATCT = data.TurnoverStatusTATCT;
                        sp.PunchlistDateTAT = data.PunchlistDateTAT;
                        sp.PunchlistDateTATCT = data.PunchlistDateTATCT;

                        sp.ReinspectionDateTAT = data.ReinspectionDateTAT;
                        sp.ReinspectionDateTATCT = data.ReinspectionDateTATCT;
                        sp.AdjReinspectionTAT = data.AdjReinspectionTAT;
                        sp.AdjReinspectionTATCT = data.AdjReinspectionTATCT;
                        sp.AdjReinspectionMaxDays = data.AdjReinspectionMaxDays;
                        sp.AdjReinspectionMaxDaysCT = data.AdjReinspectionMaxDaysCT;
                        sp.RushTicketMaxTAT = data.RushTicketMaxTAT;
                        sp.RushTicketMaxTATCT = data.RushTicketMaxTATCT;
                        sp.TurnoverStatusTAT = data.TurnoverStatusTAT;
                        sp.TurnoverStatusTATCT = data.TurnoverStatusTATCT;
                        sp.UnitAcceptanceTAT = data.UnitAcceptanceTAT;
                        sp.UnitAcceptanceTATCT = data.UnitAcceptanceTATCT;
                        sp.EnableTOStatusMax = data.EnableTOStatusMax;
                        sp.EnableTOStatusMaxCT = data.EnableTOStatusMaxCT;

                        sp.DeemedDaysTAT1 = data.DeemedDaysTAT1;
                        sp.DeemedDaysTAT1CT = data.DeemedDaysTAT1CT;
                        sp.DeemedDaysTAT2 = data.DeemedDaysTAT2;
                        sp.DeemedDaysTAT2CT = data.DeemedDaysTAT2CT;
                        sp.DeemedEmailDateSentMaxDays = data.DeemedEmailDateSentMaxDays;
                        sp.DeemedEmailDateSentMaxDaysCT = data.DeemedEmailDateSentMaxDaysCT;

                        sp.Published = (data.Published == "True") ? true : false;
                        sp.Applicability = data.Applicability;

                        data.BsHrFrm = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.BsHrFrm.ToLocalTime(), timezone);
                        data.BsHrTo = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.BsHrTo.ToLocalTime(), timezone);

                        sp.BusinessHourFrom = new TimeSpan(data.BsHrFrm.Hour, data.BsHrFrm.Minute, data.BsHrFrm.Second);
                        sp.BusinessHourTo = new TimeSpan(data.BsHrTo.Hour, data.BsHrTo.Minute, data.BsHrTo.Second);

                        sp.EnableTOCutOffDate = data.EnableTOCutOffDate;
                        sp.TOCutOffDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TOCutOffDate.ToLocalTime(), timezone);
                        sp.EnableTSCutOffDate = data.EnableTSCutOffDate;
                        sp.TSCutOffDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TSCutOffDate.ToLocalTime(), timezone);

                        sp.TitleInProcessTAT1 = data.TitleInProcessTAT1;
                        sp.TitleInProcessTAT2 = data.TitleInProcessTAT2;
                        sp.TitleInProcessTAT3 = data.TitleInProcessTAT3;
                        sp.TitleTransferredTAT = data.TitleTransferredTAT;
                        sp.TitleClaimedTAT = data.TitleClaimedTAT;
                        sp.TaxDeclarationTransferredTAT = data.TaxDeclarationTransferredTAT;
                        sp.TaxDeclarationClaimedTAT = data.TaxDeclarationClaimedTAT;
                        sp.LiquidationEndorsedTAT = data.LiquidationEndorsedTAT;
                        sp.TitleReleaseEndorsedTAT = data.TitleReleaseEndorsedTAT;
                        sp.BankReleasedTAT = data.BankReleasedTAT;
                        sp.BuyerReleasedTAT = data.BuyerReleasedTAT;

                        sp.TitleInProcessTATCT = data.TitleInProcessTATCT;
                        sp.TitleTransferredTATCT = data.TitleTransferredTATCT;
                        sp.TitleClaimedTATCT = data.TitleClaimedTATCT;
                        sp.TaxDeclarationTransferredTATCT = data.TaxDeclarationTransferredTATCT;
                        sp.TaxDeclarationClaimedTATCT = data.TaxDeclarationClaimedTATCT;
                        sp.LiquidationEndorsedTATCT = data.LiquidationEndorsedTATCT;
                        sp.TitleReleaseEndorsedTATCT = data.TitleReleaseEndorsedTATCT;
                        sp.BankReleasedTATCT = data.BankReleasedTATCT;
                        sp.BuyerReleasedTATCT = data.BuyerReleasedTATCT;
                        sp.TitlingStatusEffectivityDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.TitlingStatusEffectivityDate.ToLocalTime(), timezone);
                        
                        sp.DocCompletionTAT = data.DocCompletionTAT;
                        sp.DocCompletionTATCT = data.DocCompletionTATCT;
                        sp.RFPCreationTAT = data.RFPCreationTAT;
                        sp.RFPCreationTATCT = data.RFPCreationTATCT;
                        sp.CheckPaymentReleaseTAT = data.CheckPaymentReleaseTAT;
                        sp.CheckPaymentReleaseTATCT = data.CheckPaymentReleaseTATCT;
                        sp.MeralcoSubmissionTAT = data.MeralcoSubmissionTAT;
                        sp.MeralcoSubmissionTATCT = data.MeralcoSubmissionTATCT;
                        sp.TransferElectricServTAT = data.TransferElectricServTAT;
                        sp.TransferElectricServTATCT = data.TransferElectricServTATCT;
                        sp.UnitOwnerReceiptTAT = data.UnitOwnerReceiptTAT;
                        sp.UnitOwnerReceiptTATCT = data.UnitOwnerReceiptTATCT;

                        sp.ElectricMeterEffectivityDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(data.ElectricMeterEffectivityDate.ToLocalTime(), timezone);
                        sp.ModifiedByPK = cId;
                        sp.ModifiedDate = DateTime.Now;
                        sp.CreatedByPK = data.CreatedByPK;
                        sp.CreatedDate = data.CreatedDate;

                        db.Entry(sp).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        dbContextTransaction.Commit();

                        // Update System Parameter base on applicability
                        db.spTransactionApplicability(1, "SysParam");

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(sp);
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
                        db.SystemParameters.RemoveRange(db.SystemParameters.Where(x => x.Id == ID));
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
    }
}