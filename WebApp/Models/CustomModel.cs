using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace WebApp.Models
{
    public class CustomChangeLog
    {
        public int Id { get; set; }
        public string vMenuID { get; set; }
        public string MenuName { get; set; }
        public string EventType { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string ContentDetail { get; set; }
        public string Remarks { get; set; }
        public string ObjectType { get; set; }
        public string ObjectID { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public string UserName { get; set; }
    }

    public class CustomRegisterModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordSendMailModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class CustomUserRegisterModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string RoleId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string Company { get; set; }
        [Required]
        public string Rank { get; set; }
        [Required]
        public string Position { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        public string Photo { get; set; }
        [Required]
        public bool EmailVerificationDisabled { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string BlockedAccount { get; set; }
        [Required]
        public string ResetPassword { get; set; }
    }

    public class UpdateUserModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        public string Id { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string RoleId { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Photo { get; set; }
        [Required]
        public string Company { get; set; }
        [Required]
        public string Rank { get; set; }
        [Required]
        public string Position { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string BlockedAccount { get; set; }
        public string ResetPassword { get; set; }
    }

    public class CustomUsers
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public int NosProjects { get; set; }
        public string Photo { get; set; }
        public string Status { get; set; }
        public string BlockedAccount { get; set; }
        public string ResetPassword { get; set; }
        public string Company { get; set; }
        public string Position { get; set; }
        public string Rank { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class Menu
    {
        public string vMenuID { get; set; }
        public string NameWithParent { get; set; }
        public string nvMenuName { get; set; }
        public int iSerialNo { get; set; }
        public string nvFabIcon { get; set; }
        public string vParentMenuID { get; set; }
        public string nvPageUrl { get; set; }
        public List<Menu> Child { get; set; }
        public string PrefixCode { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<Menu> dsList { get; set; }
        public List<IdList> OptionIDs { get; set; }
        public List<IdLabelList> ControlIDs { get; set; }
    }

    public class Role
    {
        public int sl { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string IndexPage { get; set; }
        public int totalUser { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<Role> dsList { get; set; }
        public List<IdList> OptionIDs { get; set; }
    }

    public class CustomApprovalTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Published { get; set; }
        public string Applicability { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomApprovalStage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ApprovalNos { get; set; }
        public int RejectionNos { get; set; }
        public string Published { get; set; }
        public string Applicability { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomUserProject
    {
        public string vUserProjId { get; set; }
        public string UserID { get; set; }
        public int ProjectID { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public bool isChecked { get; set; }
    }

    public class CustomControl
    {
        public int wView { get; set; }
        public int wAdd { get; set; }
        public int wEdit { get; set; }
        public int wDelete { get; set; }
        public int wExtract { get; set; }
        public int wPrint { get; set; }
        public int wDownload { get; set; }
        public int wUpload { get; set; }
        public int wSync { get; set; }
        public int wFetch { get; set; }
    }

    public class CustomRoles
    {
        public Role Role { get; set; }
        public List<Menu> MenuList { get; set; }
    }

    // No need anymore
    public class CustomRole
    {
        public AspNetRole Role { get; set; }
        public List<AspNetUsersMenuPermission> MenuList { get; set; }
    }

    public class CustomSettings
    {
        public bool UserRegister { get; set; }
        public bool EmailVerificationDisable { get; set; }
        public string UserRole { get; set; }
        public bool RecoverPassword { get; set; }
        public bool ChangePassword { get; set; }
        public bool ChangeProfile { get; set; }
        public bool EnableCronJob { get; set; }
        public bool EnableSendMail { get; set; }
        public string SAPAuthUser { get; set; }
        public string SAPAuthPass { get; set; }
        public string SAPClient { get; set; }
        public string SAPIPAddress { get; set; }
        public string SAPPort { get; set; }
        public string SAPAPICompany { get; set; }
        public string SAPAPIProject { get; set; }
        public string SAPAPIUnitType { get; set; }
        public string SAPAPIInventory { get; set; }
        public string SAPAPIPhase { get; set; }
        public string SAPAPICustomer { get; set; }
        public string SAPAPITOAS { get; set; }
        public string SAPAPITO { get; set; }
        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public bool SiteOffline { get; set; }
        public string SiteOfflineMessage { get; set; }
        public bool BodySmallText { get; set; }
        public bool NavbarSmallText { get; set; }
        public bool SidebarNavSmallText { get; set; }
        public bool FooterSmallText { get; set; }
        public bool SidebarNavFlatStyle { get; set; }
        public bool SidebarNavLegacyStyle { get; set; }
        public bool SidebarNavCompact { get; set; }
        public bool SidebarNavChildIndent { get; set; }
        public bool BrandLogoSmallText { get; set; }
        public string NavbarColorVariants { get; set; }
        public string LinkColorVariants { get; set; }
        public string DarkSidebarVariants { get; set; }
        public string LightSidebarVariants { get; set; }
        public string BrandLogoVariants { get; set; }
        public string SiteMetaDescription { get; set; }
        public string SiteMetaKeyword { get; set; }
        public string Robots { get; set; }
        public string SiteAuthor { get; set; }
        public string ContentRight { get; set; }
        public string IPAddress { get; set; }
        public string SystemEnvironment { get; set; }
        public string SecretKey { get; set; }
        public string DefaultIndex { get; set; }
        public string ProjectDirectory { get; set; }
        public string UploadPath { get; set; }
        public string ReportPath { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ReplyToEmail { get; set; }
        public string ReplyToName { get; set; }
        public string Mailer { get; set; }
        public string SMTPHost { get; set; }
        public string SMTPPort { get; set; }
        public string SMTPSecurity { get; set; }
        public bool SMTPAuthentication { get; set; }
        public bool SMTPEnableSSL { get; set; }
        public string Company { get; set; }
        public string CorporateAddress { get; set; }
        public string CorporateContactNos { get; set; }
        public string CorporateFaxNos { get; set; }
        public string CorporateEmail { get; set; }
        public string CorporateWebsite { get; set; }
        public string FacebookPage { get; set; }
        public string TwitterPage { get; set; }
        public string InstagramPage { get; set; }
        public string YoutubePage { get; set; }
        public string LinkedinPage { get; set; }
        public string PinterestPage { get; set; }
        public string ITEmail { get; set; }
    }

    public class CustomSystemParameter
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TurnoverMaxDays { get; set; }
        public string TurnoverMaxDaysCT { get; set; }
        public int TurnoverMaxDays2 { get; set; }
        public string TurnoverMaxDays2CT { get; set; }
        public int NoticeNosDays1 { get; set; }
        public bool NoticeSendRecipient1 { get; set; }
        public string NoticeRecipientEmail1 { get; set; }
        public string NoticeTemplateID1 { get; set; }
        public string NoticeCT1 { get; set; }
        public int NoticeNosDays2{ get; set; }
        public bool NoticeSendRecipient2 { get; set; }
        public string NoticeRecipientEmail2 { get; set; }
        public string NoticeTemplateID2 { get; set; }
        public string NoticeCT2 { get; set; }
        public System.TimeSpan BusinessHourFrom { get; set; }
        public System.TimeSpan BusinessHourTo { get; set; }
        public System.DateTime BsHrFrm { get; set; }
        public System.DateTime BsHrTo { get; set; }
        public int ReschedMaxDays1 { get; set; }
        public string ReschedMaxDays1CT { get; set; }
        public int ReschedMaxDays2 { get; set; }
        public string ReschedMaxDays2CT { get; set; }
        public int TurnoverStatusTAT { get; set; }
        public string TurnoverStatusTATCT { get; set; }
        public int PunchlistDateTAT { get; set; }
        public string PunchlistDateTATCT { get; set; }
        public int ReinspectionDateTAT { get; set; }
        public string ReinspectionDateTATCT { get; set; }
        public int AdjReinspectionMaxDays { get; set; }
        public string AdjReinspectionMaxDaysCT { get; set; }
        public int AdjReinspectionTAT { get; set; }
        public string AdjReinspectionTATCT { get; set; }
        public int RushTicketMaxTAT { get; set; }
        public string RushTicketMaxTATCT { get; set; }
        public int EnableTOStatusMax { get; set; }
        public string EnableTOStatusMaxCT { get; set; }
        public int UnitAcceptanceTAT { get; set; }
        public string UnitAcceptanceTATCT { get; set; }
        public int DeemedDaysTAT1 { get; set; }
        public string DeemedDaysTAT1CT { get; set; }
        public int DeemedDaysTAT2 { get; set; }
        public string DeemedDaysTAT2CT { get; set; }
        public int DeemedEmailDateSentMaxDays { get; set; }
        public string DeemedEmailDateSentMaxDaysCT { get; set; }
        public string Published { get; set; }
        public string Applicability { get; set; }
        public bool EnableTOCutOffDate { get; set; }
        public System.DateTime TOCutOffDate { get; set; }
        public int TitleInProcessTAT1 { get; set; }
        public int TitleInProcessTAT2 { get; set; }
        public int TitleInProcessTAT3 { get; set; }
        public int TitleTransferredTAT { get; set; }
        public int TitleClaimedTAT { get; set; }
        public int TaxDeclarationTransferredTAT { get; set; }
        public int TaxDeclarationClaimedTAT { get; set; }
        public int LiquidationEndorsedTAT { get; set; }
        public int TitleReleaseEndorsedTAT { get; set; }
        public int BuyerReleasedTAT { get; set; }
        public int BankReleasedTAT { get; set; }
        public string TitleInProcessTATCT { get; set; }
        public string TitleTransferredTATCT { get; set; }
        public string TitleClaimedTATCT { get; set; }
        public string TaxDeclarationTransferredTATCT { get; set; }
        public string TaxDeclarationClaimedTATCT { get; set; }
        public string LiquidationEndorsedTATCT { get; set; }
        public string TitleReleaseEndorsedTATCT { get; set; }
        public string BuyerReleasedTATCT { get; set; }
        public string BankReleasedTATCT { get; set; }
        public System.DateTime TitlingStatusEffectivityDate { get; set; }
        public bool EnableTSCutOffDate { get; set; }
        public System.DateTime TSCutOffDate { get; set; }
        public int DocCompletionTAT { get; set; }
        public string DocCompletionTATCT { get; set; }
        public int RFPCreationTAT { get; set; }
        public string RFPCreationTATCT { get; set; }
        public int CheckPaymentReleaseTAT { get; set; }
        public string CheckPaymentReleaseTATCT { get; set; }
        public int MeralcoSubmissionTAT { get; set; }
        public string MeralcoSubmissionTATCT { get; set; }
        public int TransferElectricServTAT { get; set; }
        public string TransferElectricServTATCT { get; set; }
        public int UnitOwnerReceiptTAT { get; set; }
        public string UnitOwnerReceiptTATCT { get; set; }
        public System.DateTime ElectricMeterEffectivityDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    }

    public class CustomCompany
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomProject
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string Published { get; set; }
        public bool TOM { get; set; }
        public bool TSM { get; set; }
        public bool EMM { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomUnitType
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UnitTypeDesc { get; set; }
        public string UnitTypeDesc2 { get; set; }
        public string UnitCategoryCode { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomPhase
    {
        public int Id { get; set; }
        public int CompanyID { get; set; }
        public string CompanyCode { get; set; }
        public int ProjectID { get; set; }
        public string ProjectCode { get; set; }
        public string Project { get; set; }
        public string Phase { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomUnitInventory
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitType { get; set; }
        public string UnitTypeDesc { get; set; }
        public string UnitTypeCode { get; set; }
        public string UnitArea { get; set; }
        public string UnitMsrmnt { get; set; }
        public string UnitCategoryCode { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string RefNos { get; set; }
        public string Phase { get; set; }
        public string SNKS { get; set; }
        public string Intreno { get; set; }
        public string Usr08 { get; set; }
        public string Usr09 { get; set; }
        public string ZoneNos { get; set; }
        public string ZoneType { get; set; }
        public string Floor { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomCustomer
    {
        public int Id { get; set; }
        public string BusPartnerID { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class IdList
    {
        public int id { get; set; }
    }

    public class IdLabelList
    {
        public int id { get; set; }
        public string label { get; set; }
    }

    public class listData
    {
        public int Id { get; set; }
        public bool isChecked { get; set; }
    }

    public class CustomOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OptionGroup { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomDocumentaryRequirement
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OptionGroup { get; set; }
        public string Published { get; set; }
        public int? iSerialNo { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomTurnoverOption
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Published { get; set; }
        public string Applicability { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomTurnoverStatus
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Published { get; set; }
        public string Applicability { get; set; }
        public int TurnaroundTime { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
        public List<IdList> OptionIDs { get; set; }
        public List<IdLabelList> ControlIDs { get; set; }
    }

    public class CustomPunchlistCategory
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Published { get; set; }
        public string Applicability { get; set; }
        public int TurnaroundTime { get; set; }
        public bool isChecked { get; set; }
        public string CalendarType { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomTitlingLocation
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Published { get; set; }
        public string Applicability { get; set; }
        public int TurnaroundTime { get; set; }
        public bool isChecked { get; set; }
        public string CalendarType { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomHoliday
    {
        public int Id { get; set; }
        public int YearCovered { get; set; }
        public System.DateTime TheDate { get; set; }
        public string HolidayText { get; set; }
        public string HolidayType { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomEmailTemplate
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomOccupancyPermit
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string Project { get; set; }
        public string ProjectName { get; set; }
        public string Phase { get; set; }
        public string Available { get; set; }
        public bool isChecked { get; set; }
        public string Remarks { get; set; }
        public string Personnel { get; set; }
        public System.DateTime? PertmitDate { get; set; }
        public System.DateTime? PostedDate { get; set; }
        public string PostedDateStr { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }
    
    public class CustomUnitAcceptance
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string AccountTypeCode { get; set; }
        public System.DateTime? QCDAcceptanceDate { get; set; }
        public System.DateTime? FPMCAcceptanceDate { get; set; }
        public string Remarks { get; set; }
        public bool? IsCancelled { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    }

    public class CustomUnitQD_Qualifylist
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string UnitNos { get; set; }
        public string RefNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName { get; set; }
        public string AccountTypeCode { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
        public string SalesDocStatus { get; set; }
        public System.DateTime TOAS { get; set; }
        public System.DateTime OccupancyPermitDate { get; set; }
        public System.DateTime CMGAcceptanceDate { get; set; }
        public System.DateTime QualificationDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public bool isChecked { get; set; }
        public List<listData> dsList { get; set; }
        public string UniqueHashKey { get; set; }
        public int NoticeTOID { get; set; }
    }

    public class CustomUnitQD_Qualification
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string AccountTypeCode { get; set; }
        public System.DateTime TOAS { get; set; }
        public System.DateTime OccupancyPermitDate { get; set; }
        public System.DateTime CMGAcceptanceDate { get; set; }
        public System.DateTime QualificationDate { get; set; }
        public System.DateTime EmailTurnoverMaxDate { get; set; }
        public System.DateTime EmailNoticeNotifDate1 { get; set; }
        public System.DateTime EmailNoticeNotifDate2 { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public bool isChecked { get; set; }
        public List<listData> dsList { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
    }

    public class CustomUnitQD_NoticeTO
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string AccountTypeCode { get; set; }
        public System.DateTime QualificationDate { get; set; }
        public System.DateTime EmailDateNoticeSent { get; set; }
        public System.DateTime EmailTurnoverDate { get; set; }
        public System.DateTime EmailTurnoverTime { get; set; }
        public string EmailNoticeRemarks { get; set; }
        public string EmailNoticeAttachment { get; set; }
        public System.DateTime? CourierDateNoticeSent { get; set; }
        public System.DateTime? CourierDateNoticeReceived { get; set; }
        public System.DateTime EmailTurnoverMaxDate { get; set; }
        public System.DateTime ScheduleTurnoverMaxDate { get; set; }        
        public System.DateTime ScheduleEmailNotifDate1 { get; set; }
        public System.DateTime ScheduleEmailNotifDate2 { get; set; }
        public string CourierReceivedBy { get; set; }
        public string CourierNoticeRemarks { get; set; }
        public string CourierNoticeAttachment { get; set; }
        public string HandoverAssociate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
    }

    public class CustomUnitQD_TOSchedule
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string AccountTypeCode { get; set; }
        public string TurnoverOption1 { get; set; }
        public System.DateTime TurnoverDate1 { get; set; }
        public System.DateTime TurnoverTime1 { get; set; }
        public System.DateTime ScheduleTurnoverMaxDate { get; set; }
        public string TurnoverRemarks1 { get; set; }
        public string TurnoverAttachment1 { get; set; }
        public string TurnoverOption2 { get; set; }
        public System.DateTime? TurnoverDate2 { get; set; }
        public System.DateTime? TurnoverTime2 { get; set; }
        public string TurnoverRemarks2 { get; set; }
        public string TurnoverAttachment2 { get; set; }
        public bool IsPosted { get; set; }
        public System.DateTime? PostedDate { get; set; }
        public System.DateTime TurnoverStatusTAT { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
    }    

    public class CustomCustomerProfile
    {
        public int Id { get; set; }
        public string CustomerNos { get; set; }
        public string ClientRemarks { get; set; }
        public string ClientAttachment { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    }

    public class CustomUnitID_TOAcceptance
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public int TOScheduleID { get; set; }
        public string FinalTurnoverOption { get; set; }
        public System.DateTime FinalTurnoverDate { get; set; }
        public int TORule2 { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string AccountTypeCode { get; set; }
        public string TurnoverStatus { get; set; }
        public string PunchlistCategory { get; set; }
        public System.DateTime? PunchlistCategoryTAT { get; set; }
        public string PunchlistItem { get; set; }
        public string OtherIssues { get; set; }
        public string TSRemarks { get; set; }
        public string TSAttachment { get; set; }
        public System.DateTime TurnoverStatusDate { get; set; }
        public System.DateTime? UnitAcceptanceDate { get; set; }
        public System.DateTime? UnitAcceptanceDateTAT { get; set; }
        public System.DateTime? KeyTransmittalDate { get; set; }
        public System.DateTime? ReinspectionDate { get; set; }
        public System.DateTime? ReinspectionDateTAT { get; set; }
        public System.DateTime? AdjReinspectionDate { get; set; }
        public System.DateTime? AdjReinspectionDateTAT { get; set; }
        public System.DateTime? AdjReinspectionMaxDate { get; set; }
        public System.DateTime? RushTicketNosTAT { get; set; }
        public System.DateTime? DeemedDateTAT1 { get; set; }
        public System.DateTime? TOAllowableDate { get; set; }
        public string RushTicketNos { get; set; }
        public string SRRemarks { get; set; }
        public string HandoverAssociate { get; set; }
        public int IsUnitAcceptanceDateSAPSync { get; set; }
        public System.DateTime? UnitAcceptanceDateSyncDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
    }

    public class CustomUnitID_DeemedAcceptance
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public int DeemedAcceptanceID { get; set; }
        public string FinalTurnoverOption { get; set; }
        public int TORule2 { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string AccountTypeCode { get; set; }        
        public System.DateTime? DeemedAcceptanceDate { get; set; }
        public string DeemedAcceptanceRemarks { get; set; }
        public System.DateTime EmailDateNoticeSent { get; set; }
        public System.DateTime? EmailDateNoticeSentMaxDate { get; set; }
        public string EmailNoticeAttachment { get; set; }
        public string EmailNoticeRemarks { get; set; }
        public System.DateTime? CourierDateNoticeSent { get; set; }
        public System.DateTime? CourierDateNoticeReceived { get; set; }
        public string CourierReceivedBy { get; set; }
        public string CourierNoticeRemarks { get; set; }
        public string CourierNoticeAttachment { get; set; }
        public string HandoverAssociate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int IsDeemedAcceptanceDateSAPSync { get; set; }
        public System.DateTime? DeemedAcceptanceDateSyncDate { get; set; }
        public string CreatedByUser { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
    }

    public class CustomUnitHD_HistoricalData
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string Transaction { get; set; }
        public System.DateTime? QCDAcceptanceDate { get; set; }
        public System.DateTime? FPMCAcceptanceDate { get; set; }
        public System.DateTime? EmailDateNoticeSent { get; set; }
        public System.DateTime? EmailTurnoverDate { get; set; }
        public System.DateTime? EmailTurnoverTime { get; set; }
        public string EmailNoticeRemarks { get; set; }
        public string EmailNoticeAttachment { get; set; }
        public System.DateTime? CourierDateNoticeSent { get; set; }
        public System.DateTime? CourierDateNoticeReceived { get; set; }
        public string CourierReceivedBy { get; set; }
        public string CourierNoticeRemarks { get; set; }
        public string CourierNoticeAttachment { get; set; }
        public string HandoverAssociate { get; set; }
        public string TurnoverOption1 { get; set; }
        public System.DateTime? TurnoverDate1 { get; set; }
        public System.DateTime? TurnoverTime1 { get; set; }
        public string TurnoverRemarks1 { get; set; }
        public string TurnoverAttachment1 { get; set; }
        public string TurnoverOption2 { get; set; }
        public System.DateTime? TurnoverDate2 { get; set; }
        public System.DateTime? TurnoverTime2 { get; set; }
        public string TurnoverRemarks2 { get; set; }
        public string TurnoverAttachment2 { get; set; }
        public string TurnoverStatus { get; set; }
        public string PunchlistCategory { get; set; }
        public string PunchlistItem { get; set; }
        public string OtherIssues { get; set; }
        public string TSRemarks { get; set; }
        public string TSAttachment { get; set; }
        public System.DateTime? TurnoverStatusDate { get; set; }
        public System.DateTime? UnitAcceptanceDate { get; set; }
        public System.DateTime? KeyTransmittalDate { get; set; }
        public System.DateTime? ReinspectionDate { get; set; }
        public System.DateTime? AdjReinspectionDate { get; set; }
        public string RushTicketNos { get; set; }
        public string SRRemarks { get; set; }
        public System.DateTime? DeemedAcceptanceDate { get; set; }
        public string DeemedAcceptanceRemarks { get; set; }
        public System.DateTime? DAEmailDateNoticeSent { get; set; }
        public string DAEmailNoticeAttachment { get; set; }
        public string DAEmailNoticeRemarks { get; set; }
        public System.DateTime? DACourierDateNoticeSent { get; set; }
        public System.DateTime? DACourierDateNoticeReceived { get; set; }
        public string DACourierReceivedBy { get; set; }
        public string DACourierNoticeRemarks { get; set; }
        public string DACourierNoticeAttachment { get; set; }
        public string DAHandoverAssociate { get; set; }
        public string CreatedByUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
    }

    public class CustomTitlingStatus
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string RefNos { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string Transaction { get; set; }
        public System.DateTime? TitleInProcessDate { get; set; }
        public string TitleInProcessRemarks { get; set; }
        public System.DateTime? TitleTransferredDate { get; set; }
        public string TitleTransferredRemarks { get; set; }
        public System.DateTime? TitleClaimedDate { get; set; }
        public string TitleClaimedRemarks { get; set; }
        public System.DateTime? TaxDeclarationTransferredDate { get; set; }
        public string TaxDeclarationTransferredRemarks { get; set; }
        public System.DateTime? TaxDeclarationClaimedDate { get; set; }
        public string TaxDeclarationClaimedRemarks { get; set; }
        public string TaxDecNos { get; set; }
        public System.DateTime? LiquidationEndorsedDate { get; set; }
        public string LiquidationRushTicketNos { get; set; }
        public string LiquidationEndorsedRemarks { get; set; }
        public System.DateTime? TitleReleaseEndorsedDate { get; set; }
        public string TitleReleaseRushTicketNos { get; set; }
        public string TitleReleaseEndorsedRemarks { get; set; }
        public string TitleLocationID { get; set; }
        public string TitleLocationName { get; set; }
        public string TitleNos { get; set; }
        public string TitleRemarks { get; set; }
        public bool? IsBankReleaseNA { get; set; }
        public System.DateTime? BankReleasedDate { get; set; }
        public string BankReleasedRemarks { get; set; }
        public bool? IsBuyerReleaseNA { get; set; }
        public System.DateTime? BuyerReleasedDate { get; set; }
        public string BuyerReleasedRemarks { get; set; }
        public string TitleStatusType { get; set; }
        public string Remarks { get; set; }
        public string ReasonForChange { get; set; }
        public string DAHandoverAssociate { get; set; }
        public string CreatedByUser { get; set; }
        public int? TitleInProcessTAT1 { get; set; }
        public int? TitleInProcessTAT2 { get; set; }
        public int? TitleInProcessTAT3 { get; set; }
        public int? TitleTransferredTAT { get; set; }
        public int? TitleClaimedTAT { get; set; }
        public int? TaxDeclarationTransferredTAT { get; set; }
        public int? TaxDeclarationClaimedTAT { get; set; }
        public int? LiquidationEndorsedTAT { get; set; }
        public int? TitleReleaseEndorsedTAT { get; set; }
        public string TitleInProcessTATCT { get; set; }
        public string TitleTransferredTATCT { get; set; }
        public string TitleClaimedTATCT { get; set; }
        public string TaxDeclarationTransferredTATCT { get; set; }
        public string TaxDeclarationClaimedTATCT { get; set; }
        public string LiquidationEndorsedTATCT { get; set; }
        public string TitleReleaseEndorsedTATCT { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
    }

    public class CustomTitleRemark
    {
        public int Id { get; set; }
        public int TitleStatusID { get; set; }
        public string TitleStatusType { get; set; }
        public string Remarks { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    }

    public class CustomElectricMeter
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string RefNos { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string Transaction { get; set; }
        public decimal? MeterDepositAmount { get; set; }
        public bool? IsMeterDepositAmountEditable { get; set; }
        public string ApplicationProcessStatus { get; set; }
        public System.DateTime? DocumentaryCompletedDate { get; set; }
        public System.DateTime? DocumentaryLastModifedDate { get; set; }
        public string DocumentaryRemarks { get; set; }
        public System.DateTime? RFPRushTicketDate { get; set; }
        public string RFPRushTicketNos { get; set; }
        public string RFPRushTicketRemarks { get; set; }
        public bool? IsReceivedCheck { get; set; }
        public System.DateTime? ReceivedCheckDate { get; set; }
        public string ReceivedCheckRemarks { get; set; }
        public bool? WithUnpaidBills { get; set; }
        public System.DateTime? UnpaidBillPostedDate { get; set; }
        public bool? IsPaidSettled { get; set; }
        public System.DateTime? PaidSettledPostedDate { get; set; }
        public string DepositApplicationRemarks { get; set; }
        public System.DateTime? MeralcoSubmittedDate { get; set; }
        public string MeralcoSubmittedRemarks { get; set; }
        public System.DateTime? MeralcoReceiptDate { get; set; }
        public string MeralcoReceiptRemarks { get; set; }
        public System.DateTime? UnitOwnerReceiptDate { get; set; }
        public string UnitOwnerReceiptRemarks { get; set; }
        public string Remarks { get; set; }
        public string ReasonForChange { get; set; }
        public string CreatedByUser { get; set; }
        public bool IsDocumentCompleted { get; set; }
        public int? DocCompletionTAT { get; set; }
        public bool? DocumentaryStatus { get; set; }
        public string DocCompletionTATCT { get; set; }
        public int? RFPCreationTAT { get; set; }
        public string RFPCreationTATCT { get; set; }
        public int? CheckPaymentReleaseTAT { get; set; }
        public string CheckPaymentReleaseTATCT { get; set; }
        public int? MeralcoSubmissionTAT { get; set; }
        public string MeralcoSubmissionTATCT { get; set; }
        public int? TransferElectricServTAT { get; set; }
        public string TransferElectricServTATCT { get; set; }
        public int? UnitOwnerReceiptTAT { get; set; }
        public string UnitOwnerReceiptTATCT { get; set; }
        public string UnitOwnerReceiptStatus { get; set; }
        public string SalesElectricDocStatus { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
        public List<CustomElectricMeterDocument> DocumentList { get; set; }
    }

    public class CustomElectricMeterDocument
    {
        public int Id { get; set; }
        public int ElectericMeterID { get; set; }
        public string DocumentName { get; set; }
        public string PaymentTermDesc { get; set; }
        public bool isChecked { get; set; }
        public bool isDisabled { get; set; }
        public bool resetChecked { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomInventoryMeterDeposit
    {
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string CustomerNos { get; set; }
        public string SalesDocNos { get; set; }
        public string QuotDocNos { get; set; }
        public string UnitNos { get; set; }
        public string RefNos { get; set; }
        public string UnitCategory { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string UnitTypeDesc { get; set; }
        public decimal MeterDepositAmount { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public string ReasonForChange { get; set; }
        public int? ElectricMeterId { get; set; }
        public System.DateTime? MeralcoSubmittedDate { get; set; }
        public System.DateTime? MeralcoReceiptDate { get; set; }
        public System.DateTime? UnitOwnerReceiptDate { get; set; }
        public System.DateTime? CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomTOInterface
    {
        public int Id { get; set; }
        public string TOModule { get; set; }
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public string UnitNos { get; set; }
        public string UnitCategory { get; set; }
        public string CustomerNos { get; set; }
        public string TurnoverStatus { get; set; }
        public System.DateTime TurnoverStatusDate { get; set; }
        public System.DateTime? TOAcceptanceDate { get; set; }
        public int? IsTODateSAPSync { get; set; }
        public System.DateTime? TODateSyncDate { get; set; }
        public List<listData2> dsList { get; set; }
        public bool isChecked { get; set; }
    }

    public class listData2
    {
        public int Id { get; set; }
        public string TOModule { get; set; }
        public bool isChecked { get; set; }
    }

    // Dashboard Module
    public class CustomDashboard_TOSchedule
    {
        public System.DateTime? FinalTurnoverDate { get; set; }
        public System.TimeSpan? FinalTurnoverTime { get; set; }
        public string FinalTurnoverOption { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName { get; set; }
        public string ProjectCode { get; set; }
        public string BusinessEntity { get; set; }
        public string UnitType { get; set; }
        public string RefNos { get; set; }
        public string Phase { get; set; }
        public string HandoverAssociate { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
    }

    public class CustomDashboard_PipelineSummary
    {
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public int QualifiedTotal { get; set; }
        public int QualifiedWithinTAT { get; set; }
        public int QualifiedBeyondTAT { get; set; }
        public int ForProcessTotal { get; set; }
        public int ForProcessWithinTAT { get; set; }
        public int ForProcessBeyondTAT { get; set; }
        public int ConfirmedTotal { get; set; }
        public int ConfirmedWithinTAT { get; set; }
        public int ConfirmedBeyondTAT { get; set; }
    }

    public class CustomerDashboard_Pipeline
    {
        public string UniqueHashKey { get; set; }
        public string CompanyCode { get; set; }
        public string UnitNos { get; set; }
        public string ProjectCode { get; set; }
        public string RefNos { get; set; }
        public string Phase { get; set; }
        public string BusinessEntity { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string UnitTypeDesc { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName1 { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public string EmailAdd { get; set; }
        public string ContactPerson { get; set; }
        public string TelNos { get; set; }
        public string QCDAcceptanceDate { get; set; }
        public string FPMCAcceptanceDate { get; set; }
        public string HasOccupancyPermit { get; set; }
        public string HandoverAssociate { get; set; }
        public DateTime? TOAS { get; set; }
        public int EmailNoticeSentAging { get; set; }
        public int EmailNoticeSentAgingDays { get; set; }
        public string EmailDateNoticeSent { get; set; }
        public string EmailTurnoverDate { get; set; }
        public string EmailTurnoverTime { get; set; }
        public string CourierDateNoticeSent { get; set; }
        public string CourierDateNoticeReceived { get; set; }
        public string CourierReceivedBy { get; set; }
        public DateTime? QualificationDate { get; set; }
        public string FinalTurnoverDate { get; set; }
        public string FinalTurnoverTime { get; set; }
        public string FinalTurnoverOption { get; set; }
    }

    public class CustomDashboard_StatusSummary
    {
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public int ForTOStatusTotal { get; set; }
        public int ForTOStatusWithinTAT { get; set; }
        public int ForTOStatusBeyondTAT { get; set; }
        public int ForReinspectionTotal { get; set; }
        public int ForReinspectionWithinTAT { get; set; }
        public int ForReinspectionBeyondTAT { get; set; }
        public int ForDAProcessTotal { get; set; }
        public int ForDAProcessWithinTAT { get; set; }
        public int ForDAProcessBeyondTAT { get; set; }
    }

    public class CustomerDashboard_Status
    {
        public string UniqueHashKey { get; set; }
        public string CompanyCode { get; set; }
        public string UnitNos { get; set; }
        public string ProjectCode { get; set; }
        public string RefNos { get; set; }
        public string BusinessEntity { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string UnitTypeDesc { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName1 { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public string EmailAdd { get; set; }
        public string ContactPerson { get; set; }
        public string TelNos { get; set; }
        public string QCDAcceptanceDate { get; set; }
        public string FPMCAcceptanceDate { get; set; }
        public string HasOccupancyPermit { get; set; }
        public string HandoverAssociate { get; set; }
        public DateTime? TOAS { get; set; }
        public DateTime? QualificationDate { get; set; }
        public DateTime? FinalTurnoverDate { get; set; }
        public string FinalTurnoverOption { get; set; }
        public string TurnoverStatus { get; set; }
        public string PunchlistCategory { get; set; }
        public string PunchlistItem { get; set; }
        public string OtherIssues { get; set; }
        public string RushTicketNos { get; set; }
        public string ReinspectionDate { get; set; }
        public string UnitAcceptanceDate { get; set; }
        public string KeyTransmittalDate { get; set; }
        public string DeemedAcceptanceDate { get; set; }
        public string DAEmailDateNoticeSent { get; set; }
        public string DACourierDateNoticeSent { get; set; }
        public string DACourierDateNoticeReceived { get; set; }
        public string DACourierReceivedBy { get; set; }
        public int TurnoverStatusTATNos { get; set; }
        public int PunchlistDateTATNos { get; set; }
        public int DeemedEmailDateSentTATNos { get; set; }
        public int TOStatusAging { get; set; }
        public int PunchlistAging { get; set; }
        public int DAEmailAging { get; set; }
    }

    // Titling Status Dashboard
    public class CustomDashboard_TitlingStatusSummary
    {
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public int TitleInProcessTotal { get; set; }
        public int TitleInProcessWithinTAT { get; set; }
        public int TitleInProcessBeyondTAT { get; set; }
        public int TitleTransferredTotal { get; set; }
        public int TitleTransferredWithinTAT { get; set; }
        public int TitleTransferredBeyondTAT { get; set; }
        public int TitleClaimedTotal { get; set; }
        public int TitleClaimedWithinTAT { get; set; }
        public int TitleClaimedBeyondTAT { get; set; }
        public int TaxDecTransferredTotal { get; set; }
        public int TaxDecTransferredWithinTAT { get; set; }
        public int TaxDecTransferredBeyondTAT { get; set; }
        public int TaxDecClaimedTotal { get; set; }
        public int TaxDecClaimedWithinTAT { get; set; }
        public int TaxDecClaimedBeyondTAT { get; set; }
        public int EndorsedLiquidationTotal { get; set; }
        public int EndorsedLiquidationWithinTAT { get; set; }
        public int EndorsedLiquidationBeyondTAT { get; set; }
        public int EndorsedTitleReleasedTotal { get; set; }
        public int EndorsedTitleReleasedWithinTAT { get; set; }
        public int EndorsedTitleReleasedBeyondTAT { get; set; }
        public int BankReleasedTotal { get; set; }
        public int BankReleasedWithinTAT { get; set; }
        public int BankReleasedBeyondTAT { get; set; }
        public int BuyerReleasedTotal { get; set; }
        public int BuyerReleasedWithinTAT { get; set; }
        public int BuyerReleasedBeyondTAT { get; set; }
        public int TitleNotDueProcessTotal { get; set; }
    }

    public class CustomerDashboard_TitlingStatus
    {
        public string UniqueHashKey { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string UnitNos { get; set; }
        public string ProjectCode { get; set; }
        public string RefNos { get; set; }
        public string Phase { get; set; }
        public string BusinessEntity { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string UnitTypeDesc { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName1 { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public string EmailAdd { get; set; }
        public string ContactPerson { get; set; }
        public string TelNos { get; set; }
        public DateTime? TOAS { get; set; }
        public DateTime? TurnoverDate { get; set; }
        public DateTime? SAPTurnoverDate { get; set; }
        public DateTime? QualificationDate { get; set; }
        public DateTime? TitleInProcessDate { get; set; }
        public int? TitleInProcessTAT { get; set; }
        public int? TitleInProcessSysTAT { get; set; }
        public string TitleInProcessGroup { get; set; }
        public string TitleInProcessRemarks { get; set; }
        public DateTime? TitleTransferredDate { get; set; }
        public int? TitleTransferredTAT { get; set; }
        public int? TitleTransferredSysTAT { get; set; }
        public string TitleTransferredGroup { get; set; }
        public string TitleTransferredRemarks { get; set; }
        public DateTime? TitleClaimedDate { get; set; }
        public int? TitleClaimedTAT { get; set; }
        public int? TitleClaimedSysTAT { get; set; }
        public string TitleClaimedGroup { get; set; }
        public string TitleClaimedRemarks { get; set; }
        public DateTime? TaxDeclarationTransferredDate { get; set; }
        public int? TaxDeclarationTransferredTAT { get; set; }
        public int? TaxDeclarationTransferredSysTAT { get; set; }
        public string TaxDeclarationTransferredGroup { get; set; }
        public string TaxDeclarationTransferredRemarks { get; set; }
        public DateTime? TaxDeclarationClaimedDate { get; set; }
        public int? TaxDeclarationClaimedTAT { get; set; }
        public int? TaxDeclarationClaimedSysTAT { get; set; }
        public string TaxDeclarationClaimedGroup { get; set; }
        public string TaxDeclarationClaimedRemarks { get; set; }
        public string TaxDecNos { get; set; }
        public DateTime? LiquidationEndorsedDate { get; set; }
        public string LiquidationRushTicketNos { get; set; }
        public int? LiquidationEndorsedTAT { get; set; }
        public int? LiquidationEndorsedSysTAT { get; set; }
        public string LiquidationEndorsedGroup { get; set; }
        public string LiquidationEndorsedRemarks { get; set; }
        public DateTime? TitleReleaseEndorsedDate { get; set; }
        public string TitleReleaseRushTicketNos { get; set; }
        public int? TitleReleaseEndorsedTAT { get; set; }
        public int? TitleReleaseEndorsedSysTAT { get; set; }
        public string TitleReleaseEndorsedGroup { get; set; }
        public string TitleReleaseEndorsedRemarks { get; set; }
        public string TitleLocationID { get; set; }
        public string TitleLocationName { get; set; }
        public string TitleNos { get; set; }
        public string TitleRemarks { get; set; }
        public DateTime? BankReleasedDate { get; set; }
        public int? BankReleasedTAT { get; set; }
        public int? BankReleasedSysTAT { get; set; }
        public string BankReleasedGroup { get; set; }
        public string BankReleasedRemarks { get; set; }
        public DateTime? BuyerReleasedDate { get; set; }
        public int? BuyerReleasedTAT { get; set; }
        public int? BuyerReleasedSysTAT { get; set; }
        public string BuyerReleasedGroup { get; set; }
        public string BuyerReleasedRemarks { get; set; }
        public string TitleStatus { get; set; }
        public DateTime? MeralcoReceiptDate { get; set; }
        public DateTime? UnitOwnerReceiptDate { get; set; }
        public DateTime? MeralcoSubmittedDate { get; set; }
        public int? AgingTOASTaxDecTransfer { get; set; }
        public int? AgingQualifTaxDecTransfer { get; set; }
        public int? AgingTTransferReleaseBank { get; set; }
        public int? AgingTTransferReleaseBuyer { get; set; }
    }

    // Titling Status Dashboard
    public class CustomDashboard_ElectricMeterStatusSummary
    {
        public string CompanyCode { get; set; }
        public string ProjectCode { get; set; }
        public int DocCompletionWithinTAT { get; set; }
        public int DocCompletionBeyondTAT { get; set; }
        public int DocCompletionTotal { get; set; }
        public int RFPCreationWithinTAT { get; set; }
        public int RFPCreationBeyondTAT { get; set; }
        public int RFPCreationTotal { get; set; }
        public int CheckPaymentReleaseWithinTAT { get; set; }
        public int CheckPaymentReleaseBeyondTAT { get; set; }
        public int CheckPaymentReleaseTotal { get; set; }
        public int MeralcoSubmissionWithinTAT { get; set; }
        public int MeralcoSubmissionBeyondTAT { get; set; }
        public int MeralcoSubmissionTotal { get; set; }
        public int TransferElectricServWithinTAT { get; set; }
        public int TransferElectricServBeyondTAT { get; set; }
        public int TransferElectricServTotal { get; set; }
    }

    public class CustomerDashboard_ElecticMeterStatus
    {
        public string UniqueHashKey { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string UnitNos { get; set; }
        public string ProjectCode { get; set; }
        public string RefNos { get; set; }
        public string Phase { get; set; }
        public string BusinessEntity { get; set; }
        public string UnitCategoryDesc { get; set; }
        public string UnitTypeDesc { get; set; }
        public string CustomerNos { get; set; }
        public string CustomerName1 { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountTypeDesc { get; set; }
        public string EmailAdd { get; set; }
        public string ContactPerson { get; set; }
        public string TelNos { get; set; }
        public DateTime? TOAS { get; set; }
        public DateTime? TurnoverDate { get; set; }
        public DateTime? SAPTurnoverDate { get; set; }
        public DateTime? QualificationDate { get; set; }        
        public DateTime? TitleInProcessDate { get; set; }
        public DateTime? TitleTransferredDate { get; set; }
        public DateTime? TaxDeclarationTransferredDate { get; set; }
        public DateTime? LiquidationEndorsedDate { get; set; }
        public DateTime? TitleReleaseEndorsedDate { get; set; }
        public DateTime? TitleClaimedDate { get; set; }
        public DateTime? TaxDeclarationClaimedDate { get; set; }
        public DateTime? BuyerReleasedDate { get; set; }
        public DateTime? BankReleasedDate { get; set; }
        public decimal? MeterDepositAmount { get; set; }
        public string ApplicationProcessStatus { get; set; }
        public DateTime? DocumentaryCompletedDate { get; set; }
        public DateTime? DocumentaryLastModifedDate { get; set; }
        public string DocumentaryRemarks { get; set; }
        public DateTime? CCTReceivedDate { get; set; }
        public int? DocCompletionTAT { get; set; }
        public int? DocCompletionSysTAT { get; set; }
        public string DocCompletionGroup { get; set; }
        public DateTime? RFPRushTicketDate { get; set; }
        public int? RFPCreationTAT { get; set; }
        public int? RFPCreationSysTAT { get; set; }
        public string RFPCreationGroup { get; set; }
        public string RFPRushTicketNos { get; set; }
        public bool? IsReceivedCheck { get; set; }
        public DateTime? ReceivedCheckDate { get; set; }
        public string ReceivedCheckRemarks { get; set; }
        public int? CheckPaymentReleaseTAT { get; set; }
        public int? CheckPaymentReleaseSysTAT { get; set; }
        public string CheckPaymentReleaseGroup { get; set; }
        public string RFPRushTicketRemarks { get; set; }
        public bool? WithUnpaidBills { get; set; }
        public DateTime? UnpaidBillPostedDate { get; set; }
        public bool? IsPaidSettled { get; set; }
        public DateTime? PaidSettledPostedDate { get; set; }
        public string DepositApplicationRemarks { get; set; }
        public DateTime? MeralcoSubmittedDate { get; set; }
        public int? MeralcoSubmissionTAT { get; set; }
        public int? MeralcoSubmissionSysTAT { get; set; }
        public string MeralcoSubmissionGroup { get; set; }
        public string MeralcoSubmittedRemarks { get; set; }
        public DateTime? MeralcoReceiptDate { get; set; }
        public int? TransferElectricServTAT { get; set; }
        public int? TransferElectricServSysTAT { get; set; }
        public string TransferElectricServGroup { get; set; }
        public string MeralcoReceiptRemarks { get; set; }
        public DateTime? UnitOwnerReceiptDate { get; set; }
        public string UnitOwnerReceiptRemarks { get; set; }
        public string ElectricMeterStatus { get; set; }
        public int? AgingRFPRushToCheckRelease { get; set; }
        public int? AgingMeralcoSubmittedToReceipt { get; set; }
    }
}

      
     