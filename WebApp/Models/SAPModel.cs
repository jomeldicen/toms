using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace WebApp.Models
{
    public class SAPCompany
    {
        public string BUKRS { get; set; }
        public string BUTXT { get; set; }
        public string ADDRESS { get; set; }
    }

    public class SAPProject
    {
        public string BUKRS { get; set; }
        public string SWENR { get; set; }
        public string XWETEXT { get; set; }
    }

    public class SAPUnitType
    {
        public string MANDT { get; set; }
        public string SPRAS { get; set; }
        public string HOMTYP { get; set; }
        public string DESCR { get; set; }
        public string ZZUNIT_COUNT { get; set; }
        public string ZZPKG_COUNT { get; set; }
        public string ZZABBR { get; set; }
        public SAPUnitMeasurement ZZSALES_UNIT_TYPE { get; set; }
    }

    public class SAPPhase
    {
        public string BUKRS { get; set; }
        public string SWENR { get; set; }
        public string PHASE { get; set; }
    }

    public class SAPUnitMeasurement
    {
        public string UNIT_TYPE_CODE { get; set; }
        public string DESCRIPTION { get; set; }
    }

    /*****************************[ START INVENTORY ]***********************************/
    public class SAPUnitInventory
    {
        public string BUKRS { get; set; }
        public string SWENR { get; set; }
        public string SMENR { get; set; }
        public string REFNO { get; set; }
        public string PHASE { get; set; }
        public SAPUnitType UNIT_TYPE { get; set; }
        public SAPUnitInventoryStatus STATUS { get; set; }
        public SAPUnitInventoryTurnover TURNOVER_DATE { get; set; }
    }

    public class SAPUnitInventoryStatus
    {
        public string OBJNR { get; set; }
        public string TEXT { get; set; }
        public string REBOOK_FLAG { get; set; }
    }

    public class SAPUnitInventoryTurnover
    {
        public string UNIT_AREA { get; set; }
        public string MOVE_IN_DATE { get; set; }
        public string APPROVED_DATE { get; set; }
        public string TURN_OVER_DATE { get; set; }
        public string QCD_TO_CEG { get; set; }
        public string QCDACCEPT_DATE { get; set; }
        public string PROJTOVER_DATE { get; set; }
        public string PROJTOVER_TIME { get; set; }
        public string CMG_COMPLETION_DT { get; set; }
        public string OCC_PER_DATE { get; set; }
        public string TAG_NOSHOW { get; set; }
        public string LTSCOMPL_DATE { get; set; }
        public string LTSEXT_DATE { get; set; }
        public string OOMCACCEPT_DATE { get; set; }
    }
    /*****************************[ END INVENTORY ]***********************************/

    /*****************************[ START CUSTOMER ]***********************************/
    public class SAPCustomer
    {
        public string KUNNR { get; set; }
        public string NAME1 { get; set; }
        public string NAME2 { get; set; }
    }

    public class SAPCustomerData
    {
        public string AUFSD { get; set; }
    }

    public class SAPCustomerPartner
    {
        public string AUFSD { get; set; }
    }

    public class SAPCustomerNotice
    {
        public string PRJ_UPD_DT { get; set; }
    }

    public class SAPCustomerPhone
    {
        public string TEL_NO { get; set; }
    }

    public class SAPCustomerEmail
    {
        public string E_MAIL { get; set; }
    }
    /*****************************[ END CUSTOMER ]***********************************/
    
    public class SAPZcomm
    {
        public string QUOT_NUM { get; set; }
        public string INFO12 { get; set; }
    }
}