app.controller("ElectricMeter", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.SalesInfo = null;
    $scope.data1b = {}; // Clients Information
    $scope.ElectricMeter = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, MeterDepositAmount: null, ApplicationProcessStatus: null, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: null, RFPRushTicketNos: null, RFPRushTicketDate: null, RFPRushTicketRemarks: null, IsReceivedCheck: null, ReceivedCheckRemarks: null, WithUnpaidBills: null, UnpaidBillPostedDate: null, IsPaidSettled: null, PaidSettledPostedDate: null, DepositApplicationRemarks: null, MeralcoSubmittedDate: null, MeralcoSubmittedRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null, Remarks: null, ReasonForChange: null };
    $scope.DefaultElectricMeter = [];
    $scope.HolidayWeekenDays = [];
    $scope.SalesInfo = null;
    $scope.jomel = false;
    $scope.totalItems = 0;
    $scope.loading = false;
    $scope.disableButton = true;
    $scope.withUrlVariable = false;
    $scope.ShowReasonChange = false;

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl, Variable) {
        $scope.paramInfo.PageUrl = PageUrl;

        // initialize URL Variable
        if (Variable != null && Variable && Variable !== '') {
            $scope.withUrlVariable = true;

            $scope.paramInfo.CompanyCode = Variable.split('|')[0];
            $scope.paramInfo.ProjectCode = Variable.split('|')[1];
            $scope.paramInfo.UnitCategory = Variable.split('|')[2];
            $scope.paramInfo.UnitNos = Variable.split('|')[3];
            $scope.paramInfo.CustomerNos = Variable.split('|')[4];

            // Search field Population
            $scope.selectedProject = Variable.split('|')[5];
            if ($scope.paramInfo.UnitCategory === 'UN')
                $scope.selectedUnitCategory = 'Residential Unit';
            else if ($scope.paramInfo.UnitCategory === 'PK')
                $scope.selectedUnitCategory = 'Parking Unit';

            $scope.selectedUnit = Variable.split('|')[6];
            $scope.selectedCustomer = $scope.paramInfo.CustomerNos;

            $scope.GetUnitInspectData();
        }
    }

    // ************************************************************************************************************************
    // <md-autocomplete> is a special input component with a drop-down of all possible matches to a custom query. 
    // This component allows you to provide real - time suggestions as the user types in the input area.
    // ************************************************************************************************************************

    $scope.simulateQuery = false;
    /**
     * Search for ProjectLists... use $timeout to simulate
     * remote dataservice call.
     */
    $scope.querySearch = function (query, source) {

        if (source === 'Project')
            var results = query ? $scope.ProjectList().filter(createFilterFor(query)) : $scope.ProjectList(), deferred;
        else if (source === 'UnitCategory')
            results = query ? $scope.UnitCategoryList().filter(createFilterFor(query)) : $scope.UnitCategoryList(), deferred;
        else if (source === 'Unit')
            results = query ? $scope.UnitList().filter(createFilterFor(query)) : $scope.UnitList(), deferred;
        else if (source === 'Customer')
            results = query ? $scope.CustomerList().filter(createFilterFor(query)) : $scope.CustomerList(), deferred;

        if ($scope.simulateQuery) {
            deferred = $q.defer();
            $timeout(function () { deferred.resolve(results); }, Math.random() * 1000, false);
            return deferred.promise;
        } else {
            return results;
        }
    }

    $scope.searchTextChange = function (text, source) {
        //$log.info('Text changed to ' + text + ' ' + source)
        // make sure to remove query string if new search happen
        if (window.location.search.match("flx10ms="))
            window.history.replaceState(null, "", window.location.href.split("?")[0]);

        if (source === 'Project') {
            var item = $scope.Projects.find(x => x.BusinessEntity.toLowerCase() === text.toLowerCase());
            if (item) {
                $scope.selectedUnitCategory = null;
                $scope.searchUnitCategory = "";
                $scope.selectedUnit = null;
                $scope.searchUnit = "";
                $scope.selectedCustomer = null;
                $scope.searchCustomer = "";

                $scope.paramInfo.ProjectID = item.Id;
                $scope.paramInfo.CompanyCode = item.CompanyCode
                $scope.paramInfo.ProjectCode = item.ProjectCode;
                $scope.paramInfo.ProjectLocation = item.ProjectLocation
            }
        } else if (source === 'UnitCategory') {
            var unitcategories = [
            { 'name': 'Residential Unit', 'code': 'UN' },
            { 'name': 'Parking Unit', 'code': 'PK' }
            ];

            var item = unitcategories.find(x => x.name.toLowerCase() === text.toLowerCase());
            if (item) {
                $scope.selectedUnit = null;
                $scope.searchUnit = "";
                $scope.selectedCustomer = null;
                $scope.searchCustomer = "";

                $scope.paramInfo.UnitCategory = item.code;
            }
        } else if (source === 'Unit') {
            var item = $scope.Units.find(x => x.RefNos.toLowerCase() === text.toLowerCase());
            $scope.paramInfo.UnitNos = '';
            if (item)
                $scope.paramInfo.UnitNos = item.UnitNos;

        } else if (source === 'Customer') {
            var item = $scope.Customers.find(x => x.CustomerNos.toLowerCase() === text.toLowerCase());
            $scope.paramInfo.CustomerNos = '';
            if (item)
                $scope.paramInfo.CustomerNos = item.CustomerNos;
        }
        //$scope.GetSearchData();
    }

    $scope.selectedItemChange = function (item, source) {
        //$log.info('Item changed to ' + JSON.stringify(item));
        // make sure to remove query string if new search happen
        if (window.location.search.match("flx10ms="))
            window.history.replaceState(null, "", window.location.href.split("?")[0]);

        if ($scope.jomel) $scope.resetData();

        if (source === 'Project') {
            $scope.selectedUnitCategory = null;
            $scope.searchUnitCategory = "";
            $scope.selectedUnit = null;
            $scope.searchUnit = "";
            $scope.selectedCustomer = null;
            $scope.searchCustomer = "";

            $scope.paramInfo.ProjectID = (item) ? item.Id : 0;
            $scope.paramInfo.CompanyCode = (item) ? item.CompanyCode : '';
            $scope.paramInfo.ProjectCode = (item) ? item.ProjectCode : '';
            $scope.paramInfo.ProjectLocation = (item) ? item.ProjectLocation : '';
        } else if (source === 'UnitCategory') {
            $scope.selectedUnit = null;
            $scope.searchUnit = "";
            $scope.selectedCustomer = null;
            $scope.searchCustomer = "";

            $scope.paramInfo.UnitCategory = (item) ? item.code : '';
        } else if (source === 'Unit') {
            $scope.paramInfo.UnitNos = (item) ? item.UnitNos : $scope.paramInfo.UnitNos;
        } else if (source === 'Customer') {
            $scope.paramInfo.CustomerNos = (item) ? item.CustomerNos : $scope.paramInfo.CustomerNos;
        }

        $scope.disableButton = true;
        $scope.GetSearchData();
    }

    /**Build `ProjectLists` list of key/value pairs**/
    $scope.ProjectList = function () {
        if (typeof ($scope.Projects) !== 'undefined') {
            return $scope.Projects.map(function (item) {
                item.value = item.ProjectCodeName.toLowerCase();
                return item;
            });
        }
        return false;
    }

    /**Build `UnitLists` list of key/value pairs**/
    $scope.UnitCategoryList = function () {
        var unitcategories = [
            { 'name': 'Residential Unit', 'code': 'UN' },
            { 'name': 'Parking Unit', 'code': 'PK' }
        ];
        return unitcategories.map(function (item) {
            item.value = item.name.toLowerCase();
            return item;
        });
    }

    /**Build `UnitLists` list of key/value pairs**/
    $scope.UnitList = function () {
        if (typeof ($scope.Units) !== 'undefined') {
            return $scope.Units.map(function (item) {
                item.value = item.RefNos.toLowerCase();
                return item;
            });
        }
        return false;
    }

    /**Build `Customer` list of key/value pairs**/
    $scope.CustomerList = function () {
        if (typeof ($scope.Customers) !== 'undefined') {
            return $scope.Customers.map(function (item) {
                item.value = item.CustomerNos.toLowerCase();
                return item;
            });
        }
        return false;
    }

    /** Create filter function for a query string **/
    function createFilterFor(query) {
        var lowercaseQuery = query.toLowerCase();

        return function filterFn(field) {
            //return (field.value.indexOf(lowercaseQuery) === 0);
            return field.value.toLowerCase().includes(lowercaseQuery);
        };
    }
    // ************************************************************************************************************************
    // <md-autocomplete> is a special input component with a drop-down of all possible matches to a custom query. 
    // This component allows you to provide real - time suggestions as the user types in the input area.
    // ************************************************************************************************************************

    $scope.clearData = function () {
        $scope.loading = false;
        document.body.style.cursor = 'default';
        $scope.data = {};
        $scope.data.nvFabIcon = "fa-bars";
    };

    $scope.resetData = function () {
        $scope.SalesInfo = null;
        $scope.ElectricMeter = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, MeterDepositAmount: null, ApplicationProcessStatus: null, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: null, RFPRushTicketNos: null, RFPRushTicketDate: null, RFPRushTicketRemarks: null, IsReceivedCheck: null, ReceivedCheckRemarks: null, WithUnpaidBills: null, UnpaidBillPostedDate: null, IsPaidSettled: null, PaidSettledPostedDate: null, DepositApplicationRemarks: null, MeralcoSubmittedDate: null, MeralcoSubmittedRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null, Remarks: null, ReasonForChange: null };
        $scope.totalItems = 0;
        $scope.disableButton = true;
        $scope.disableClientButton = true;
        $('#custom-content-below-tab li:first-child a').tab('show') // Select first tab
    }

    $scope.clearSearch = function () {
        $scope.selectedProject = null;
        $scope.searchProject = "";
        $scope.selectedUnitCategory = null;
        $scope.searchUnitCategory = "";
        $scope.selectedUnit = null;
        $scope.searchUnit = "";
        $scope.selectedCustomer = null;
        $scope.searchCustomer = "";

        $scope.paramInfo.ProjectID = 0;
        $scope.paramInfo.CompanyCode = '';
        $scope.paramInfo.ProjectCode = '';
        $scope.paramInfo.UnitNos = '';
        $scope.paramInfo.UnitCategory = '';
        $scope.paramInfo.CustomerNos = '';
        $scope.paramInfo.ProjectLocation = '';

        $scope.resetData();
        $scope.SalesInfo = null;

        $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: false }
        $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }

        $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
        $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }

        $scope.RASFields = { WithUnpaidBills: null, UnpaidBillPostedDate: null, IsPaidSettled: null, PaidSettledPostedDate: null, DepositApplicationRemarks: null, MeralcoSubmittedDate: null, MeralcoSubmittedRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null }
        $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }

        $scope.ShowReasonChange = false;

        // make sure to remove query string if new search happen
        if (window.location.search.match("flx10ms="))
            window.history.replaceState(null, "", window.location.href.split("?")[0]);
    };

    //-----------------------------------------------------------
    // Set parameter for search field
    $scope.paramInfo = { ProjectID: '0', CompanyCode: '', ProjectCode: '', UnitNos: '', UnitCategory: '', CustomerNos: '', ProjectLocation: '', PageUrl: '' };

    // Set Button Controls
    $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: false }
    $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }

    $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
    $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }

    $scope.RASFields = { WithUnpaidBills: null, UnpaidBillPostedDate: null, IsPaidSettled: null, PaidSettledPostedDate: null, DepositApplicationRemarks: null, MeralcoSubmittedDate: null, MeralcoSubmittedRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null }
    $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }

    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/ElectricMeter/GetSearchData';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.Projects = response.data.PROJECTLIST;
                $scope.Units = response.data.UNITLIST;
                $scope.Customers = response.data.CUSTOMERLIST;
                $scope.Documents = response.data.DOCUMENTLIST;
            }
            $scope.clearData();
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
            $scope.resetData();
        });
    };

    $scope.GetUnitInspectData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $('#custom-content-below-tab li:first-child a').tab('show') // Select first tab

        $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: false }
        $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

        $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
        $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

        $scope.RASFields = { WithUnpaidBills: null, UnpaidBillPostedDate: null, IsPaidSettled: null, PaidSettledPostedDate: null, DepositApplicationRemarks: null, MeralcoSubmittedDate: null, MeralcoSubmittedRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null }
        $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }

        $scope.ShowReasonChange = false;

        if ($scope.paramInfo.CompanyCode === '0' || $scope.paramInfo.ProjectCode === '' || ($scope.paramInfo.UnitNos === '' && $scope.paramInfo.CustomerNos === '')) {
            swal(
                'Error Message',
                'Unit has NO or inactive Customer Number',
                'error'
            );
            return false;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/ElectricMeter/GetElectricMeter';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.SalesInfo = response.data.SALESINFO;
                $scope.Ctrl = response.data.CONTROLS;

                // Check if the data is coming from the URL Variable
                if ($scope.withUrlVariable) {
                    $scope.paramInfo.ProjectLocation = $scope.SalesInfo.ProjectLocation;
                }

                if ($scope.SalesInfo != null) {
                    $scope.jomel = true; // hit the search button

                    $scope.TitlingLocation = response.data.TITLINGLOCATIONLIST;
                    $scope.ElectricMeter = response.data.ELECTRICMETER;
                    $scope.DefaultElectricMeter = response.data.DEFELECTRICMETER;
                    $scope.ServiceDepositAmnt = response.data.METERDEPOSITAMNT;
                    $scope.IsServiceDepositAmntEditable = response.data.ISMETERDEPOSITAMNTEDITABLE;
                    $scope.Documents = response.data.DOCUMENTLIST;
                    $scope.HolidayWeekenDays = response.data.EXCEPTIONDAYS;
                    $scope.CurUser = response.data.CURUSER;
                    $scope.SysParam = response.data.SYSPARAM;
                    
                    $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                    $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                    $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[Start Document Requirement & Application Status Tab]------------------------------------------ //
                    // --------------------------------------------------------------------------------------------------------------------------------- //

                    // Check if has record
                    if ($scope.ElectricMeter && $scope.ElectricMeter.Id !== 0)
                    {
                        if ($scope.ElectricMeter.DocumentaryCompletedDate !== null) {
                            $scope.ElectricMeter.DocumentaryCompletedDate = new Date($scope.ElectricMeter.DocumentaryCompletedDate);
                            $scope.DefaultElectricMeter.DocumentaryCompletedDate = new Date($scope.DefaultElectricMeter.DocumentaryCompletedDate);
                        }

                        if ($scope.ElectricMeter.DocumentaryLastModifedDate !== null) {
                            $scope.ElectricMeter.DocumentaryLastModifedDate = new Date($scope.ElectricMeter.DocumentaryLastModifedDate);
                            $scope.DefaultElectricMeter.DocumentaryLastModifedDate = new Date($scope.DefaultElectricMeter.DocumentaryLastModifedDate);
                        }

                        if ($scope.ElectricMeter.RFPRushTicketDate !== null) {
                            $scope.ElectricMeter.RFPRushTicketDate = new Date($scope.ElectricMeter.RFPRushTicketDate);
                            $scope.DefaultElectricMeter.RFPRushTicketDate = new Date($scope.DefaultElectricMeter.RFPRushTicketDate);
                        }

                        if ($scope.ElectricMeter.UnpaidBillPostedDate !== null) {
                            $scope.ElectricMeter.UnpaidBillPostedDate = new Date($scope.ElectricMeter.UnpaidBillPostedDate);
                            $scope.DefaultElectricMeter.UnpaidBillPostedDate = new Date($scope.DefaultElectricMeter.UnpaidBillPostedDate);
                        }

                        if ($scope.ElectricMeter.PaidSettledPostedDate !== null) {
                            $scope.ElectricMeter.PaidSettledPostedDate = new Date($scope.ElectricMeter.PaidSettledPostedDate);
                            $scope.DefaultElectricMeter.PaidSettledPostedDate = new Date($scope.DefaultElectricMeter.PaidSettledPostedDate);
                        }

                        if ($scope.ElectricMeter.MeralcoSubmittedDate !== null) {
                            $scope.ElectricMeter.MeralcoSubmittedDate = new Date($scope.ElectricMeter.MeralcoSubmittedDate);
                            $scope.DefaultElectricMeter.MeralcoSubmittedDate = new Date($scope.DefaultElectricMeter.MeralcoSubmittedDate);

                            // Min date Receipt from Meralco
                            $scope.dateReceiptMeralco.minDate = $scope.ElectricMeter.MeralcoSubmittedDate;
                        }

                        if ($scope.ElectricMeter.MeralcoReceiptDate !== null) {
                            $scope.ElectricMeter.MeralcoReceiptDate = new Date($scope.ElectricMeter.MeralcoReceiptDate);
                            $scope.DefaultElectricMeter.MeralcoReceiptDate = new Date($scope.DefaultElectricMeter.MeralcoReceiptDate);

                            // Min date Receipt of Unit Owner
                            $scope.dateReceiptUnitOwner.minDate = $scope.ElectricMeter.MeralcoReceiptDate;
                        }

                        if ($scope.ElectricMeter.UnitOwnerReceiptDate !== null) {
                            $scope.ElectricMeter.UnitOwnerReceiptDate = new Date($scope.ElectricMeter.UnitOwnerReceiptDate);
                            $scope.DefaultElectricMeter.UnitOwnerReceiptDate = new Date($scope.DefaultElectricMeter.UnitOwnerReceiptDate);
                        }

                        // Check Remarks Field
                        if ($scope.ElectricMeter.Remarks === null)
                            $scope.ElectricMeter.Remarks = "";

                        //---------------------------------- Tab Document Requirements and Application Status
                        if ($scope.DefaultElectricMeter.MeterDepositAmount !== null || $scope.DefaultElectricMeter.IsMeterDepositAmountEditable === false)
                            $scope.DASFields.MeterDepositAmount = false;
                        else
                            $scope.DASFields.MeterDepositAmount = true;

                        if ($scope.DefaultElectricMeter.DocumentaryRemarks !== null)
                            $scope.DASFields.DocumentaryRemarks = false;
                        else
                            $scope.DASFields.DocumentaryRemarks = true;


                        // check Document Requirements and Application Status
                        if ($scope.DefaultElectricMeter.MeterDepositAmount !== null && $scope.DefaultElectricMeter.DocumentaryRemarks !== null && $scope.Documents.filter(e => e.isChecked === false).length === 0) {
                            $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                        } else if ($scope.DefaultElectricMeter.MeterDepositAmount === null && $scope.DefaultElectricMeter.DocumentaryRemarks === null && $scope.Documents.filter(e => e.isChecked === false).length > 0) {
                            $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                        } else {
                            $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                        }

                        //---------------------------------- Tab RFP Status
                        if ($scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                            $scope.TASFields.RFPRushTicketNos = false;
                        else
                            $scope.TASFields.RFPRushTicketNos = true;

                        if ($scope.DefaultElectricMeter.RFPRushTicketDate !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                            $scope.TASFields.RFPRushTicketDate = false;
                        else
                            $scope.TASFields.RFPRushTicketDate = true;

                        if ($scope.DefaultElectricMeter.RFPRushTicketRemarks !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                            $scope.TASFields.RFPRushTicketRemarks = false;
                        else
                            $scope.TASFields.RFPRushTicketRemarks = true;

                        if ($scope.DefaultElectricMeter.IsReceivedCheck !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                            $scope.TASFields.IsReceivedCheck = false;
                        else
                            $scope.TASFields.IsReceivedCheck = true;

                        if ($scope.DefaultElectricMeter.ReceivedCheckRemarks !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                            $scope.TASFields.ReceivedCheckRemarks = false;
                        else
                            $scope.TASFields.ReceivedCheckRemarks = true;

                        if ($scope.DefaultElectricMeter.IsReceivedCheck !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                            $scope.TASFields.IsReceivedCheck = false;
                        else
                            $scope.TASFields.IsReceivedCheck = true;
                            
                        // check RFP Status
                        if ($scope.DefaultElectricMeter.RFPRushTicketNos !== null && $scope.DefaultElectricMeter.RFPRushTicketDate !== null && $scope.DefaultElectricMeter.RFPRushTicketRemarks !== null &&
                            $scope.DefaultElectricMeter.IsReceivedCheck !== null && $scope.DefaultElectricMeter.ReceivedCheckRemarks !== null) {

                            $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                        } else if ($scope.DefaultElectricMeter.RFPRushTicketNos === null && $scope.DefaultElectricMeter.RFPRushTicketDate === null && $scope.DefaultElectricMeter.RFPRushTicketRemarks === null &&
                            $scope.DefaultElectricMeter.IsReceivedCheck === null && $scope.DefaultElectricMeter.ReceivedCheckRemarks === null) {

                            $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                        } else {
                            $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                        }

                        //---------------------------------- Tab Meralco Service Transfer Status
                        if ($scope.DefaultElectricMeter.IsPaidSettled !== null)
                            $scope.RASFields.IsPaidSettled = false;
                        else
                            $scope.RASFields.IsPaidSettled = true;

                        if ($scope.DefaultElectricMeter.WithUnpaidBills !== null) {
                            $scope.RASFields.WithUnpaidBills = false;

                            if ($scope.DefaultElectricMeter.WithUnpaidBills === false)
                                $scope.RASFields.IsPaidSettled = false;
                            else {
                                $scope.RASFields.IsPaidSettled = true;

                                if ($scope.DefaultElectricMeter.IsPaidSettled !== null)
                                    $scope.RASFields.IsPaidSettled = false;
                                else
                                    $scope.RASFields.IsPaidSettled = true;
                            }
                        } else {
                            $scope.RASFields.WithUnpaidBills = true;
                            $scope.RASFields.IsPaidSettled = false;
                        }

                        if ($scope.DefaultElectricMeter.DepositApplicationRemarks !== null)
                            $scope.RASFields.DepositApplicationRemarks = false;
                        else
                            $scope.RASFields.DepositApplicationRemarks = true;

                        if ($scope.DefaultElectricMeter.MeralcoSubmittedDate !== null)
                            $scope.RASFields.MeralcoSubmittedDate = false;
                        else
                            $scope.RASFields.MeralcoSubmittedDate = true;

                        if ($scope.DefaultElectricMeter.MeralcoSubmittedRemarks !== null)
                            $scope.RASFields.MeralcoSubmittedRemarks = false;
                        else
                            $scope.RASFields.MeralcoSubmittedRemarks = true;

                        if ($scope.DefaultElectricMeter.MeralcoReceiptDate !== null || $scope.DefaultElectricMeter.MeralcoSubmittedDate === null)
                            $scope.RASFields.MeralcoReceiptDate = false;
                        else
                            $scope.RASFields.MeralcoReceiptDate = true;

                        if ($scope.DefaultElectricMeter.MeralcoReceiptRemarks !== null || $scope.DefaultElectricMeter.MeralcoSubmittedDate === null)
                            $scope.RASFields.MeralcoReceiptRemarks = false;
                        else
                            $scope.RASFields.MeralcoReceiptRemarks = true;

                        if ($scope.DefaultElectricMeter.UnitOwnerReceiptDate !== null || $scope.DefaultElectricMeter.MeralcoReceiptDate === null)
                            $scope.RASFields.UnitOwnerReceiptDate = false;
                        else
                            $scope.RASFields.UnitOwnerReceiptDate = true;

                        if ($scope.DefaultElectricMeter.UnitOwnerReceiptRemarks !== null || $scope.DefaultElectricMeter.MeralcoReceiptDate === null)
                            $scope.RASFields.UnitOwnerReceiptRemarks = false;
                        else
                            $scope.RASFields.UnitOwnerReceiptRemarks = true;

                        // check Title Details and Release Endrosement Button Controls Status
                        if ($scope.DefaultElectricMeter.WithUnpaidBills !== null && $scope.DefaultElectricMeter.IsPaidSettled !== null && 
                            $scope.DefaultElectricMeter.MeralcoSubmittedDate !== null && $scope.DefaultElectricMeter.MeralcoSubmittedRemarks !== null &&
                            $scope.DefaultElectricMeter.MeralcoReceiptDate !== null && $scope.DefaultElectricMeter.MeralcoReceiptRemarks !== null &&
                            $scope.DefaultElectricMeter.UnitOwnerReceiptDate !== null && $scope.DefaultElectricMeter.UnitOwnerReceiptRemarks !== null) {

                            $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                        } else if ($scope.DefaultElectricMeter.WithUnpaidBills === null && $scope.DefaultElectricMeter.IsPaidSettled === null && 
                            $scope.DefaultElectricMeter.MeralcoSubmittedDate === null && $scope.DefaultElectricMeter.MeralcoSubmittedRemarks === null &&
                            $scope.DefaultElectricMeter.MeralcoReceiptDate === null && $scope.DefaultElectricMeter.MeralcoReceiptRemarks === null &&
                            $scope.DefaultElectricMeter.UnitOwnerReceiptDate === null && $scope.DefaultElectricMeter.UnitOwnerReceiptRemarks === null) {

                            $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                        } else {
                            $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                        }

                        // Business Rules UD-FRD_TOMS_Unit Titling&Electric Mtr Monitoring-Electric Meter&Service Deposit Status_Add_ver00-draft1c.pdf
                        // 5.1.1 If List of Requirements field from the Document Requirement Details are NOT completely tagged/
                        // check marked, system will not allow
                        if (!$scope.DefaultElectricMeter.IsDocumentCompleted) {
                            $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                        }

                        // Business Rules UD-FRD_TOMS_Unit Titling&Electric Mtr Monitoring-Electric Meter&Service Deposit Status_Add_ver00-draft1c.pdf
                        // 6.1.1 If Received Check? field on RFP Status is NOT yet tagged as YES, system will not allow tagging on
                        // any fields on the Meralco Servic
                        if (!$scope.DefaultElectricMeter.IsReceivedCheck) {
                            $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
                            $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                        }

                        // Business Rules UD-FRD_TOMS_Unit Titling&Electric Mtr Monitoring-Titling Status_EDIT-rev02.pdf
                        // 5.4.1.3 Once “Date Application Submitted to Meralco” field from Meralco Service Transfer Status transaction TAB is already tagged or with posted date already, system will NO longer allow editing / updating of any information on this transaction tab or editing will already be locked. 
                        if ($scope.DefaultElectricMeter.MeralcoSubmittedDate != null) {
                            $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: false }
                            $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                            $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                        }

                        // 5.4.3.1 After 15 working days from the actual date posting/ tagging of “Date Receipt of Unit Owner” field for the new service contract, 
                        // system will NO longer allow editing / updating of any information on the said transaction tab or editing will already be locked. 
                        if ($scope.ElectricMeter.UnitOwnerReceiptStatus === 'Beyond') {
                            // Default for Document Status Fields
                            $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: false }
                            $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                            // Default for RFP Status Fields
                            $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                            // Default for Meralco Service Status
                            $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
                            $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                        }

                        // wait unit variable tower populated
                        $scope.$watch("DefaultElectricMeter", function (item) {
                            if (item) {
                                $scope.ResetElectricMeterFields();
                            }
                        }, true);
                    } else {
                        $scope.ElectricMeter = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, MeterDepositAmount: $scope.ServiceDepositAmnt, ApplicationProcessStatus: null, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: null, RFPRushTicketNos: null, RFPRushTicketDate: null, RFPRushTicketRemarks: null, IsReceivedCheck: null, ReceivedCheckRemarks: null, WithUnpaidBills: null, IsPaidSettled: null, DepositApplicationRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null, Remarks: null, ReasonForChange: null };

                        // Default for Document Status Fields
                        $scope.DASFields = { MeterDepositAmount: true, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: true }
                        $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }

                        // Default for RFP Status Fields
                        $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                        $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                        // Default for Meralco Service Status
                        $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
                        $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                        if ($scope.IsServiceDepositAmntEditable === false)
                            $scope.DASFields.MeterDepositAmount = false;
                        else
                            $scope.DASFields.MeterDepositAmount = true;
                    }
                } else {
                    $scope.ElectricMeter = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, MeterDepositAmount: null, ApplicationProcessStatus: null, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: null, RFPRushTicketNos: null, RFPRushTicketDate: null, RFPRushTicketRemarks: null, IsReceivedCheck: null, ReceivedCheckRemarks: null, WithUnpaidBills: null, UnpaidBillPostedDate: null, IsPaidSettled: null, PaidSettledPostedDate: null, DepositApplicationRemarks: null, MeralcoSubmittedDate: null, MeralcoSubmittedRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null, Remarks: null, ReasonForChange: null };

                    // Default for Document Status Fields
                    $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: false }
                    $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                    // Default for RFP Status Fields
                    $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                    $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                    // Default for Meralco Service Status
                    $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
                    $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                }
          
                $scope.disableButton = false;
                if ($scope.SalesInfo === null) {
                    swal(
                        'Error Message',
                        'Unit has NO or inactive Customer Number',
                        'error'
                    );
                    $scope.disableButton = true;
                }
            }

            $timeout(function () {
                $scope.clearData();
            }, 1000);
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

    //// -------------------------------------- Document Requirement & Application Status
    $scope.SaveDocReqModal = function (type) {
        $scope.$broadcast('show-errors-event');
        if ($scope.docreqForm.$invalid) {
            if ($scope.docreqForm.$error['dateDisabled'] && $scope.docreqForm.$error['dateDisabled'].length) {
                swal(
                    'Error Message',
                    'Selected Date is not allowed.',
                    'error'
                );
            } else {
                swal(
                    'Error Message',
                    'Please fill-out valid data or tag mandatory fields to be able to proceed.',
                    'error'
                );
            }

            return false;
        } else {
            $scope.ElectricMeter.Transaction = type;
            $('#applyChangesModal1a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    // -------------------------------------- RFP Status
    $scope.SaveRFPModal = function (type) {
        $scope.$broadcast('show-errors-event');
        if ($scope.rfpForm.$invalid) {
            if ($scope.rfpForm.$error['dateDisabled'] && $scope.rfpForm.$error['dateDisabled'].length) {
                swal(
                    'Error Message',
                    'Selected Date is not allowed.',
                    'error'
                );
            } else {
                swal(
                    'Error Message',
                    'Please fill-out valid data or tag mandatory fields to be able to proceed.',
                    'error'
                );
            }

            return false;
        } else {
            $scope.ElectricMeter.Transaction = type;
            $('#applyChangesModal1a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    $scope.CheckRFP = function () {
        $scope.ResetElectricMeterFields();

        if (!$scope.SalesInfo) {
            $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }
            return;
        }

        //if ($scope.SalesInfo.TaxDeclarationTransferredDate == null) {
        //    swal(
        //        'Alert',
        //        'Tax Declaration Transferred Date must be posted on SAP to be able to proceed with this transaction.',
        //        'error'
        //    );
        //    $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
        //    $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

        //    return;
        //}
    }

    // -------------------------------------- Meralco Service & Deposit Status  
    $scope.ChangeWithUnpaidBills = function (val) {
        $scope.ElectricMeter.IsPaidSettled = null;
        $scope.RASFields.IsPaidSettled = val;
    }

    $scope.SaveElectricTransferModal = function (type) {
        $scope.$broadcast('show-errors-event');
        if ($scope.electrictransferForm.$invalid) {
            if ($scope.electrictransferForm.$error['dateDisabled'] && $scope.electrictransferForm.$error['dateDisabled'].length) {
                swal(
                    'Error Message',
                    'Selected Date is not allowed.',
                    'error'
                );
            } else {
                swal(
                    'Error Message',
                    'Please fill-out valid data or tag mandatory fields to be able to proceed.',
                    'error'
                );
            }

            return false;
        } else {
            $scope.ElectricMeter.Transaction = type;
            $('#applyChangesModal1a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    // -------------------------------------- Start Reset Cancel Button Actions -------------------------------------- //
    $scope.CancelButtonModal = function () {
        $('#applyChangesModal1b').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    }

    $scope.CancelButton = function () {
        $scope.ResetElectricMeterFields();
        $('#applyChangesModal1b').modal('hide');
    };
    // -------------------------------------- End Reset Cancel Button Actions -------------------------------------- //

    // -------------------------------------- Start Reset Title Status Fields on Button Actions -------------------------------------- //
    $scope.EnableElectricMeterAction = function () {
        $scope.ResetElectricMeterFields();
        $scope.ElectricMeter.ReasonForChange = "";
        $scope.ShowReasonChange = true;

        if ($scope.DefaultElectricMeter) {
            if ($scope.DefaultElectricMeter.Id !== 0) {
                angular.forEach($scope.DefaultElectricMeter, function (value, key) {
                    $scope.ElectricMeter[key] = value;
                });

                //---------------------------------- Tab Document Requirements and Application Status
                if ($scope.DefaultElectricMeter.MeterDepositAmount !== null && $scope.DefaultElectricMeter.IsMeterDepositAmountEditable === true)
                    $scope.DASFields.MeterDepositAmount = true;
                else
                    $scope.DASFields.MeterDepositAmount = false;
                
                if ($scope.DefaultElectricMeter.DocumentaryRemarks !== null)
                    $scope.DASFields.DocumentaryRemarks = true;
                else
                    $scope.DASFields.DocumentaryRemarks = false;
                
                //disable the rest once editing
                angular.forEach($scope.Documents, function (value, key) {
                    value.isChecked = value.resetChecked;
                    value.isDisabled = !value.isDisabled;
                });

                //---------------------------------- Tab RFP Status
                if ($scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                    $scope.TASFields.RFPRushTicketNos = true;
                else
                    $scope.TASFields.RFPRushTicketNos = false;

                if ($scope.DefaultElectricMeter.RFPRushTicketDate !== null && $scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                    $scope.TASFields.RFPRushTicketDate = true;
                else
                    $scope.TASFields.RFPRushTicketDate = false;

                if ($scope.DefaultElectricMeter.RFPRushTicketRemarks !== null && $scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                    $scope.TASFields.RFPRushTicketRemarks = true;
                else
                    $scope.TASFields.RFPRushTicketRemarks = false;

                if ($scope.DefaultElectricMeter.IsReceivedCheck !== null && $scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                    $scope.TASFields.IsReceivedCheck = true;
                else
                    $scope.TASFields.IsReceivedCheck = false;

                if ($scope.DefaultElectricMeter.ReceivedCheckRemarks !== null && $scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                    $scope.TASFields.ReceivedCheckRemarks = true;
                else
                    $scope.TASFields.ReceivedCheckRemarks = false;

                if ($scope.DefaultElectricMeter.IsReceivedCheck !== null && $scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                    $scope.TASFields.IsReceivedCheck = true;
                else
                    $scope.TASFields.IsReceivedCheck = false;

                //---------------------------------- Tab Meralco Service Transfer Status
                if ($scope.DefaultElectricMeter.WithUnpaidBills !== null)
                    $scope.RASFields.WithUnpaidBills = true;
                else
                    $scope.RASFields.WithUnpaidBills = false;

                if ($scope.DefaultElectricMeter.IsPaidSettled !== null && $scope.DefaultElectricMeter.WithUnpaidBills == true)
                    $scope.RASFields.IsPaidSettled = true;
                else
                    $scope.RASFields.IsPaidSettled = false;

                if ($scope.DefaultElectricMeter.DepositApplicationRemarks !== null)
                    $scope.RASFields.DepositApplicationRemarks = true;
                else
                    $scope.RASFields.DepositApplicationRemarks = false;

                if ($scope.DefaultElectricMeter.MeralcoSubmittedDate !== null)
                    $scope.RASFields.MeralcoSubmittedDate = true;
                else
                    $scope.RASFields.MeralcoSubmittedDate = false;

                if ($scope.DefaultElectricMeter.MeralcoSubmittedRemarks !== null)
                    $scope.RASFields.MeralcoSubmittedRemarks = true;
                else
                    $scope.RASFields.MeralcoSubmittedRemarks = false;

                if ($scope.DefaultElectricMeter.MeralcoReceiptDate !== null && $scope.DefaultElectricMeter.MeralcoSubmittedDate !== null)
                    $scope.RASFields.MeralcoReceiptDate = true;
                else
                    $scope.RASFields.MeralcoReceiptDate = false;

                if ($scope.DefaultElectricMeter.MeralcoReceiptRemarks !== null && $scope.DefaultElectricMeter.MeralcoSubmittedDate !== null)
                    $scope.RASFields.MeralcoReceiptRemarks = true;
                else
                    $scope.RASFields.MeralcoReceiptRemarks = false;

                if ($scope.DefaultElectricMeter.UnitOwnerReceiptDate !== null && $scope.DefaultElectricMeter.MeralcoReceiptDate !== null)
                    $scope.RASFields.UnitOwnerReceiptDate = true;
                else
                    $scope.RASFields.UnitOwnerReceiptDate = false;

                if ($scope.DefaultElectricMeter.UnitOwnerReceiptRemarks !== null && $scope.DefaultElectricMeter.MeralcoReceiptDate !== null)
                    $scope.RASFields.UnitOwnerReceiptRemarks = true;
                else
                    $scope.RASFields.UnitOwnerReceiptRemarks = false;

                 $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
            } else {
                $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }
            }
        } else {
            $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
            $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
            $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }
        }
    };
    // -------------------------------------- End Reset Title Status Fields on Button Actions -------------------------------------- //

    // -------------------------------------- Start Reset Title Status Fields -------------------------------------- //
    $scope.ResetElectricMeterFields = function () {
        $scope.ElectricMeter.ReasonForChange = "";
        $scope.ShowReasonChange = false;

        if ($scope.SalesInfo != null) {
            if ($scope.DefaultElectricMeter && $scope.DefaultElectricMeter != null && $scope.DefaultElectricMeter.Id !== 0) {  
                angular.forEach($scope.DefaultElectricMeter, function (value, key) {
                    $scope.ElectricMeter[key] = value;
                });

                //---------------------------------- Tab Document Requirements and Application Status
                if ($scope.DefaultElectricMeter.MeterDepositAmount !== null || $scope.DefaultElectricMeter.IsMeterDepositAmountEditable === false)
                    $scope.DASFields.MeterDepositAmount = false;
                else
                    $scope.DASFields.MeterDepositAmount = true;

                if ($scope.DefaultElectricMeter.DocumentaryRemarks !== null)
                    $scope.DASFields.DocumentaryRemarks = false;
                else
                    $scope.DASFields.DocumentaryRemarks = true;

                angular.forEach($scope.Documents, function (value, key) {
                    value.isChecked = value.isDisabled = value.resetChecked;
                });

                // check Document Requirements and Application Status
                if ($scope.DefaultElectricMeter.TaxDecNos !== null && $scope.DefaultElectricMeter.DocumentaryRemarks !== null && $scope.Documents.filter(e => e.isChecked === false).length === 0) {
                    $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                } else if ($scope.DefaultElectricMeter.TaxDecNos === null && $scope.DefaultElectricMeter.DocumentaryRemarks === null && $scope.Documents.filter(e => e.isChecked === false).length > 0) {
                    $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                } else {
                    $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                }

                //---------------------------------- Tab RFP Status
                if ($scope.DefaultElectricMeter.RFPRushTicketNos !== null)
                    $scope.TASFields.RFPRushTicketNos = false;
                else
                    $scope.TASFields.RFPRushTicketNos = true;

                if ($scope.DefaultElectricMeter.RFPRushTicketDate !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                    $scope.TASFields.RFPRushTicketDate = false;
                else
                    $scope.TASFields.RFPRushTicketDate = true;

                if ($scope.DefaultElectricMeter.RFPRushTicketRemarks !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                    $scope.TASFields.RFPRushTicketRemarks = false;
                else
                    $scope.TASFields.RFPRushTicketRemarks = true;

                if ($scope.DefaultElectricMeter.IsReceivedCheck !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                    $scope.TASFields.IsReceivedCheck = false;
                else
                    $scope.TASFields.IsReceivedCheck = true;

                if ($scope.DefaultElectricMeter.ReceivedCheckRemarks !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                    $scope.TASFields.ReceivedCheckRemarks = false;
                else
                    $scope.TASFields.ReceivedCheckRemarks = true;

                if ($scope.DefaultElectricMeter.IsReceivedCheck !== null || $scope.DefaultElectricMeter.RFPRushTicketNos === null)
                    $scope.TASFields.IsReceivedCheck = false;
                else
                    $scope.TASFields.IsReceivedCheck = true;
                            
                // check RFP Status
                if ($scope.DefaultElectricMeter.RFPRushTicketNos !== null && $scope.DefaultElectricMeter.RFPRushTicketDate !== null && $scope.DefaultElectricMeter.RFPRushTicketRemarks !== null &&
                    $scope.DefaultElectricMeter.IsReceivedCheck !== null && $scope.DefaultElectricMeter.ReceivedCheckRemarks !== null) {

                    $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                } else if ($scope.DefaultElectricMeter.RFPRushTicketNos === null && $scope.DefaultElectricMeter.RFPRushTicketDate === null && $scope.DefaultElectricMeter.RFPRushTicketRemarks === null &&
                    $scope.DefaultElectricMeter.IsReceivedCheck === null && $scope.DefaultElectricMeter.ReceivedCheckRemarks === null) {

                    $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                } else {
                    $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                }

                //---------------------------------- Tab Meralco Service Transfer Status  
                if ($scope.DefaultElectricMeter.IsPaidSettled !== null)
                    $scope.RASFields.IsPaidSettled = false;
                else
                   $scope.RASFields.IsPaidSettled = true;

                if ($scope.DefaultElectricMeter.WithUnpaidBills !== null) {
                    $scope.RASFields.WithUnpaidBills = false;

                    if ($scope.DefaultElectricMeter.WithUnpaidBills === false)
                        $scope.RASFields.IsPaidSettled = false;
                    else {
                        $scope.RASFields.IsPaidSettled = true;

                        if ($scope.DefaultElectricMeter.IsPaidSettled !== null)
                            $scope.RASFields.IsPaidSettled = false;
                        else
                            $scope.RASFields.IsPaidSettled = true;
                    }
                } else {
                    $scope.RASFields.WithUnpaidBills = true;
                    $scope.RASFields.IsPaidSettled = false;
                }

                if ($scope.DefaultElectricMeter.DepositApplicationRemarks !== null)
                    $scope.RASFields.DepositApplicationRemarks = false;
                else
                    $scope.RASFields.DepositApplicationRemarks = true;

                if ($scope.DefaultElectricMeter.MeralcoSubmittedDate !== null)
                    $scope.RASFields.MeralcoSubmittedDate = false;
                else
                    $scope.RASFields.MeralcoSubmittedDate = true;

                if ($scope.DefaultElectricMeter.MeralcoSubmittedRemarks !== null)
                    $scope.RASFields.MeralcoSubmittedRemarks = false;
                else
                    $scope.RASFields.MeralcoSubmittedRemarks = true;

                if ($scope.DefaultElectricMeter.MeralcoReceiptDate !== null || $scope.DefaultElectricMeter.MeralcoSubmittedDate === null)
                    $scope.RASFields.MeralcoReceiptDate = false;
                else
                    $scope.RASFields.MeralcoReceiptDate = true;

                if ($scope.DefaultElectricMeter.MeralcoReceiptRemarks !== null || $scope.DefaultElectricMeter.MeralcoSubmittedDate === null)
                    $scope.RASFields.MeralcoReceiptRemarks = false;
                else
                    $scope.RASFields.MeralcoReceiptRemarks = true;

                if ($scope.DefaultElectricMeter.UnitOwnerReceiptDate !== null || $scope.DefaultElectricMeter.MeralcoReceiptDate === null)
                    $scope.RASFields.UnitOwnerReceiptDate = false;
                else
                    $scope.RASFields.UnitOwnerReceiptDate = true;

                if ($scope.DefaultElectricMeter.UnitOwnerReceiptRemarks !== null || $scope.DefaultElectricMeter.MeralcoReceiptDate === null)
                    $scope.RASFields.UnitOwnerReceiptRemarks = false;
                else
                    $scope.RASFields.UnitOwnerReceiptRemarks = true;

                // check Title Details and Release Endrosement Button Controls Status
                if ($scope.DefaultElectricMeter.WithUnpaidBills !== null && $scope.DefaultElectricMeter.IsPaidSettled !== null && $scope.DefaultElectricMeter.DepositApplicationRemarks !== null &&
                    $scope.DefaultElectricMeter.MeralcoSubmittedDate !== null && $scope.DefaultElectricMeter.MeralcoSubmittedRemarks !== null &&
                    $scope.DefaultElectricMeter.MeralcoReceiptDate !== null && $scope.DefaultElectricMeter.MeralcoReceiptRemarks !== null &&
                    $scope.DefaultElectricMeter.UnitOwnerReceiptDate !== null && $scope.DefaultElectricMeter.UnitOwnerReceiptRemarks !== null) {

                    $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                } else if ($scope.DefaultElectricMeter.WithUnpaidBills === null && $scope.DefaultElectricMeter.IsPaidSettled === null && $scope.DefaultElectricMeter.DepositApplicationRemarks === null &&
                    $scope.DefaultElectricMeter.MeralcoSubmittedDate === null && $scope.DefaultElectricMeter.MeralcoSubmittedRemarks === null &&
                    $scope.DefaultElectricMeter.MeralcoReceiptDate === null && $scope.DefaultElectricMeter.MeralcoReceiptRemarks === null &&
                    $scope.DefaultElectricMeter.UnitOwnerReceiptDate === null && $scope.DefaultElectricMeter.UnitOwnerReceiptRemarks === null) {

                    $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                } else {
                    $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                }

                // Business Rules UD-FRD_TOMS_Unit Titling&Electric Mtr Monitoring-Electric Meter&Service Deposit Status_Add_ver00-draft1c.pdf
                // 5.1.1 If List of Requirements field from the Document Requirement Details are NOT completely tagged/
                // check marked, system will not allow
                if (!$scope.DefaultElectricMeter.IsDocumentCompleted) {
                    $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                    $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                }

                // Business Rules UD-FRD_TOMS_Unit Titling&Electric Mtr Monitoring-Electric Meter&Service Deposit Status_Add_ver00-draft1c.pdf
                // 6.1.1 If Received Check? field on RFP Status is NOT yet tagged as YES, system will not allow tagging on
                // any fields on the Meralco Servic
                if (!$scope.DefaultElectricMeter.IsReceivedCheck) {
                    $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
                    $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                }

                // Business Rules UD-FRD_TOMS_Unit Titling&Electric Mtr Monitoring-Titling Status_EDIT-rev02.pdf
                // 5.4.1.3 Once “Date Application Submitted to Meralco” field from Meralco Service Transfer Status transaction TAB is already tagged or with posted date already, system will NO longer allow editing / updating of any information on this transaction tab or editing will already be locked. 
                if ($scope.DefaultElectricMeter.MeralcoSubmittedDate != null) {
                    $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: false }
                    $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                    $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                    $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                }

                // 5.4.3.1 After 15 working days from the actual date posting/ tagging of “Date Receipt of Unit Owner” field for the new service contract, 
                // system will NO longer allow editing / updating of any information on the said transaction tab or editing will already be locked. 
                if ($scope.ElectricMeter.UnitOwnerReceiptStatus === 'Beyond') {
                    // Default for Document Status Fields
                    $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: false }
                    $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                    // Default for RFP Status Fields
                    $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                    $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                    // Default for Meralco Service Status
                    $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
                    $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                }
            } else {
                $scope.ElectricMeter = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, MeterDepositAmount: $scope.ServiceDepositAmnt, ApplicationProcessStatus: null, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: null, RFPRushTicketNos: null, RFPRushTicketDate: null, RFPRushTicketRemarks: null, IsReceivedCheck: null, ReceivedCheckRemarks: null, WithUnpaidBills: null, IsPaidSettled: null, DepositApplicationRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null, Remarks: null, ReasonForChange: null };

                // Default for Document Status Fields
                $scope.DASFields = { MeterDepositAmount: true, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: true }
                $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }

                // Default for RFP Status Fields
                $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
                $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                // Default for Meralco Service Status
                $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
                $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }
            }
        } else {
            $scope.ElectricMeter = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, MeterDepositAmount: null, ApplicationProcessStatus: null, DocumentaryCompletedDate: null, DocumentaryLastModifedDate: null, DocumentaryRemarks: null, RFPRushTicketNos: null, RFPRushTicketDate: null, RFPRushTicketRemarks: null, IsReceivedCheck: null, ReceivedCheckRemarks: null, WithUnpaidBills: null, UnpaidBillPostedDate: null, IsPaidSettled: null, PaidSettledPostedDate: null, DepositApplicationRemarks: null, MeralcoSubmittedDate: null, MeralcoSubmittedRemarks: null, MeralcoReceiptDate: null, MeralcoReceiptRemarks: null, UnitOwnerReceiptDate: null, UnitOwnerReceiptRemarks: null, Remarks: null, ReasonForChange: null };

            // Default for Document Status Fields
            $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: false }
            $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

            // Default for RFP Status Fields
            $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

            // Default for Meralco Service Status
            $scope.RASFields = { WithUnpaidBills: false, UnpaidBillPostedDate: false, IsPaidSettled: false, PaidSettledPostedDate: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
            $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }
        }
    };
    // -------------------------------------- End Reset Electric Meter Fields -------------------------------------- //

    // -------------------------------------- Start Enable Field from RFP Status -------------------------------------- //
    $scope.EnableRFPield = function (data) {
        $scope.TASFields.RFPRushTicketDate = false;
        $scope.TASFields.RFPRushTicketRemarks = false;
        $scope.TASFields.IsReceivedCheck = false;
        $scope.TASFields.ReceivedCheckRemarks = false;

        if (data != '') {
            $scope.TASFields.RFPRushTicketDate = true;
            $scope.TASFields.RFPRushTicketRemarks = true;
            $scope.TASFields.IsReceivedCheck = true;
            $scope.TASFields.ReceivedCheckRemarks = true;

            // 5.4.2.1.1 Any modification or posted changes on both RFP Rush Tickets No. and RFP Rush Ticket Date fields, system will automatically unticked or 
            // untagged “Yes” on the “Received Check?’ field, and must be manually tagged by user. 
            var a = $scope.ElectricMeter.RFPRushTicketDate
            RFPRushTicketDate1 = a.getFullYear() + '-' + a.getMonth() + '-' + a.getDate();

            var b = $scope.DefaultElectricMeter.RFPRushTicketDate
            RFPRushTicketDate2 = b.getFullYear() + '-' + b.getMonth() + '-' + b.getDate();

            if ($scope.DefaultElectricMeter.IsReceivedCheck === true) {
                if ($scope.ElectricMeter.RFPRushTicketNos !== $scope.DefaultElectricMeter.RFPRushTicketNos || RFPRushTicketDate1 != RFPRushTicketDate2) {
                    $scope.ElectricMeter.IsReceivedCheck = false;
                    $scope.TASFields.IsReceivedCheck = false;
                } else {
                    $scope.ElectricMeter.IsReceivedCheck = true;
                    $scope.TASFields.IsReceivedCheck = true;
                }
            }
        }
    }
    // -------------------------------------- End Enable Field from RFP Status -------------------------------------- //
    
    // -------------------------------------- Start Enable Field from Electric Meter Status -------------------------------------- //
    $scope.EnableMeralcoApplication = function (data) {
        $scope.RASFields.MeralcoReceiptDate = false;
        $scope.RASFields.MeralcoReceiptRemarks = false;

        if (data != null) {
            $scope.RASFields.MeralcoReceiptDate = true;
            $scope.RASFields.MeralcoReceiptRemarks = true;
        }
    }

    $scope.EnableMeralcoReceipt = function (data) {
        $scope.RASFields.UnitOwnerReceiptDate = false;
        $scope.RASFields.UnitOwnerReceiptRemarks = false;

        if (data != null) {
            $scope.RASFields.UnitOwnerReceiptDate = true;
            $scope.RASFields.UnitOwnerReceiptRemarks = true;
        }
    }

    $scope.UpdateCheckbox = function (data) {
      
    }
    // -------------------------------------- End Enable Field from Electric Meter Status -------------------------------------- //

    // -------------------------------------- Start Saving Title Status -------------------------------------- //
    $scope.SaveElectricMeter = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.ShowReasonChange && $scope.ReasonForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
            return;
        } else {

            $scope.DASFields = { MeterDepositAmount: false, ApplicationProcessStatus: false, DocumentaryCompletedDate: false, DocumentaryLastModifedDate: false, DocumentaryRemarks: false }
            $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }

            $scope.TASFields = { RFPRushTicketNos: false, RFPRushTicketDate: false, RFPRushTicketRemarks: false, IsReceivedCheck: false, ReceivedCheckRemarks: false }
            $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }

            $scope.RASFields = { WithUnpaidBills: false, IsPaidSettled: false, DepositApplicationRemarks: false, MeralcoSubmittedDate: false, MeralcoSubmittedRemarks: false, MeralcoReceiptDate: false, MeralcoReceiptRemarks: false, UnitOwnerReceiptDate: false, UnitOwnerReceiptRemarks: false }
            $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }

            $scope.SaveElectricMeterConfirmed(data);
        }
    }

    $scope.SaveElectricMeterConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        data.CompanyCode = $scope.SalesInfo.CompanyCode;
        data.ProjectCode = $scope.SalesInfo.ProjectCode;
        data.RefNos = $scope.SalesInfo.RefNos;
        data.UnitNos = $scope.SalesInfo.UnitNos;
        data.UnitCategory = $scope.SalesInfo.UnitCategoryCode;
        data.CustomerNos = $scope.SalesInfo.CustomerNos;
        data.SalesDocNos = $scope.SalesInfo.SalesDocNos;
        data.QuotDocNos = $scope.SalesInfo.QuotDocNos;
        data.DocumentList = $scope.Documents;

        urlData = '../api/ElectricMeter/SaveElectricMeter';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitInspectData();

                $('#applyChangesModal1a').modal('hide');

                swal(
                    'System Message Confirmation',
                    'Information provided for the units has been recorded.',
                    'success'
                );

                $scope.ElectricMeter.ReasonForChange = "";
                $scope.ShowReasonChange = false;

                $scope.clearData();
            }
        }, function (response) {
            var obj = response.data.Message;
            if (typeof obj == 'undefined')
                swal('Update Failed!', 'Please check your data!', 'error');
            else
                swal('Error Message', obj, 'error');

            $scope.clearData();
        });
    };
    // -------------------------------------- End Saving Title Status -------------------------------------- //

    // -------------------------------------- Start Change Log -------------------------------------- //
    // call from other controller
    $scope.GetTransactionLog = function () {
        $('#changeLogModal').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });

        $rootScope.$emit("searchObjectType", 'ElectricMeterController');
    }
    // -------------------------------------- End Change Log -------------------------------------- //   

    // -------------------------------------- Start Date Management -------------------------------------- //
    $('[data-toggle="tooltip"]').tooltip();
    $('[data-toggle="popover"]').popover();

    $scope.dateOptionsDefault = {
        formatYear: 'yy',
        startingDay: 1,
        showWeeks: false,
    };

    // Key Transmittal Date
    $scope.dateOptionsCurrent = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    //Date Application Submitted to Meralco
    $scope.dateApplicationMeralco = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    //Date Receipt from Meralco
    $scope.dateReceiptMeralco = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Date Receipt of Unit Owner
    $scope.dateReceiptUnitOwner = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    //var disableDates = ["9-11-2020", "14-11-2020", "15-11-2020", "27-12-2020"];

    // Disable weekend selection
    function disabled(data) {
        var date = data.date,
            mode = data.mode;
        //return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);

        dmy = date.getDate() + "-" + (date.getMonth() + 1) + "-" + date.getFullYear();

        if ($scope.HolidayWeekenDays.indexOf(dmy) !== -1 || (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6))) {
            return true;
        }
        else {
            return false;
        }
    }

    // RFP Rush Ticket Date
    $scope.open1 = function () {
        $scope.popup1.opened = true;
    };

    $scope.popup1 = {
        opened: false
    };

    // Date Application Submitted to Meralco
    $scope.open2 = function () {
        $scope.popup2.opened = true;
    };

    $scope.popup2 = {
        opened: false
    };

    // Date Receipt from Meralco
    $scope.open3 = function () {
        // Min date Receipt from Meralco
        $scope.dateReceiptMeralco.minDate = $scope.ElectricMeter.MeralcoSubmittedDate;
        $scope.popup3.opened = true;
    };

    $scope.popup3 = {
        opened: false
    };

    // Date Receipt of Unit Owner
    $scope.open4 = function () {
        // Min date Receipt of Unit Owner
        $scope.dateReceiptUnitOwner.minDate = $scope.ElectricMeter.MeralcoReceiptDate;
        $scope.popup4.opened = true;
    };

    $scope.popup4 = {
        opened: false
    };

    $scope.dateFormats = ['MM/dd/yyyy', 'dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.dateFormat = $scope.dateFormats[0];
    $scope.altInputFormats = ['M!/d!/yyyy'];

    // -------------------------------------- End Date Management -------------------------------------- //
}]);


