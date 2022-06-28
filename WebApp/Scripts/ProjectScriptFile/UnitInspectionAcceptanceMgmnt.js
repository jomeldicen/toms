app.controller("UnitInspectionAcceptanceMgmnt", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data1b = {}; // Clients Information
    $scope.UnitInspectionAcceptanceMgmnt = [];
    $scope.HolidayWeekenDays = [];
    $scope.AcceptanceInfo = null;
    $scope.jomel = false;
    $scope.totalItems = 0;
    $scope.loading = false;
    $scope.disableButton = true;
    $scope.tsButton = false; // Button behavior is based on Turover Status (Express)
    $scope.dmButton = false; // Button behavior by default is disabled on Deemed Acceptance Status (Standard/ Online Assisted)
    $scope.adButton = false; // Button behavior by defualt is disable on Acceptance Details, Service Request 
    $scope.withUrlVariable = false;

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
        $scope.AcceptanceInfo = null;
        $scope.TOAcceptanceInfo = null;
        $scope.DeemedAcceptanceInfo = null;
        $scope.ClientInfo = null;
        $scope.totalItems = 0;
        $scope.disableButton = true;
        $scope.disableClientButton = true;
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
        $scope.AcceptanceInfo = null;

        // make sure to remove query string if new search happen
        if (window.location.search.match("flx10ms="))
            window.history.replaceState(null, "", window.location.href.split("?")[0]);
    };

    //-----------------------------------------------------------
    // Set parameter for search field
    $scope.paramInfo = { ProjectID: '0', CompanyCode: '', ProjectCode: '', UnitNos: '', UnitCategory: '', CustomerNos: '', ProjectLocation: '', PageUrl: '' };

    // Set Button Controls
    $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
    $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }

    $scope.DASFields = { EmailDateNoticeSent: true, EmailNoticeRemarks: true, CourierDateNoticeSent: true, CourierDateNoticeReceived: true, CourierReceivedBy: true, CourierNoticeRemarks: true }
    $scope.DASButtons = { AddButton: false, CancelButton: false }
    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';

        urlData = '../api/UnitInspectionAcceptanceMgmnt/GetSearchData';
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

        if ($scope.paramInfo.CompanyCode === '0' || $scope.paramInfo.ProjectCode === '' || $scope.paramInfo.UnitCategory === '' ||
            ($scope.paramInfo.UnitNos === '' && $scope.paramInfo.CustomerNos === '')) {
            swal(
                'Error Message',
                'Record not found. Please complete search criteria',
                'error'
            );
            return false;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/UnitInspectionAcceptanceMgmnt/GetUnitInspectionAcceptanceMgmnt';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.AcceptanceInfo = response.data.ACCEPTANCEINFO;
                $scope.Ctrl = response.data.CONTROLS;

                // Check if the data is coming from the URL Variable
                if ($scope.withUrlVariable) {
                    $scope.paramInfo.ProjectLocation = $scope.AcceptanceInfo.ProjectLocation;
                }

                if ($scope.AcceptanceInfo != null) {
                    $scope.jomel = true; // hit the search button

                    $scope.TurnoverStatus = response.data.TURNOVEROPTIONLIST;
                    $scope.PunchlistCategory = response.data.PUNCHLIST;
                    $scope.TOAcceptanceInfo = response.data.TOACCEPTANCEINFO;
                    $scope.DeemedAcceptanceInfo = response.data.DEEMEDACCEPTANCEINFO;
                    $scope.ClientInfo = response.data.CLIENTINFO;
                    $scope.HolidayWeekenDays = response.data.EXCEPTIONDAYS;
                    $scope.CurUser = response.data.CURUSER;
                    $scope.SysParam = response.data.SYSPARAM;

                    $scope.totalItems = $scope.ClientInfo.length;
                    
                    $scope.AcceptanceInfo.AllowedTOStatusUpdate = ($scope.AcceptanceInfo.AllowedTOStatusUpdate === 1) ? true : false;

                    // Check if Turnover Schedule & Option has record
                    if ($scope.AcceptanceInfo.TOScheduleID !== 0) {
                        if ($scope.AcceptanceInfo.FinalTurnoverDate !== null)
                            $scope.AcceptanceInfo.FinalTurnoverDate = new Date($scope.AcceptanceInfo.FinalTurnoverDate);

                        if ($scope.AcceptanceInfo.FinalTurnoverTime !== null) {
                            $scope.AcceptanceInfo.FinalTurnoverTime = new Date($scope.AcceptanceInfo.FinalTurnoverTime);
                        }
                    }

                    // Re-inspection Date Convert to System Date
                    if ($scope.AcceptanceInfo.ReinspectionDate !== null)
                        $scope.AcceptanceInfo.ReinspectionDate = new Date($scope.AcceptanceInfo.ReinspectionDate);

                    // Unit Acceptance Date Convert to System Date
                    if ($scope.AcceptanceInfo.UnitAcceptanceDate !== null)
                        $scope.AcceptanceInfo.UnitAcceptanceDate = new Date($scope.AcceptanceInfo.UnitAcceptanceDate);

                    // Key Transmittal Date Convert to System Date
                    if ($scope.AcceptanceInfo.KeyTransmittalDate !== null)
                        $scope.AcceptanceInfo.KeyTransmittalDate = new Date($scope.AcceptanceInfo.KeyTransmittalDate);

                    // Adjusted Reinspection Date Convert to System Date
                    if ($scope.AcceptanceInfo.AdjReinspectionDate !== null)
                        $scope.AcceptanceInfo.AdjReinspectionDate = new Date($scope.AcceptanceInfo.AdjReinspectionDate);
                                                
                    // If value on the Turnover Option field for the unit is Express, system will automatically populate / tag the
                    // following fields with value / data which cannot be changed by the user(disabled)
                    // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.3)
                    if ($scope.AcceptanceInfo.FinalTurnoverOption === 'Express') {
                        if ($scope.TOAcceptanceInfo === null) {
                            $scope.TOAcceptanceInfo = { TurnoverStatus: '', UnitAcceptanceDate: '', KeyTransmittalDate: '' }
                            $scope.TOAcceptanceInfo = { TurnoverStatus: 'AWOP', UnitAcceptanceDate: $scope.AcceptanceInfo.FinalTurnoverDate, KeyTransmittalDate: $scope.AcceptanceInfo.FinalTurnoverDate }
                        }
                        $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                        $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }
                    } else {
                        $scope.TASFields = { TurnoverStatus: false, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: false, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                        $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }    
                    }

                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[Start Turnover & Acceptance Status Tab]------------------------------------------ //
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // Check if Turnover & Acceptance Status has record
                    if ($scope.TOAcceptanceInfo) {
                        $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                        $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                        if ($scope.TOAcceptanceInfo.UnitAcceptanceDate !== null)
                            $scope.TOAcceptanceInfo.UnitAcceptanceDate = new Date($scope.TOAcceptanceInfo.UnitAcceptanceDate);

                        if ($scope.TOAcceptanceInfo.KeyTransmittalDate !== null)
                            $scope.TOAcceptanceInfo.KeyTransmittalDate = new Date($scope.TOAcceptanceInfo.KeyTransmittalDate);

                        if ($scope.TOAcceptanceInfo.ReinspectionDate !== null)
                            $scope.TOAcceptanceInfo.ReinspectionDate = new Date($scope.TOAcceptanceInfo.ReinspectionDate);

                        if ($scope.TOAcceptanceInfo.AdjReinspectionDate !== null)
                            $scope.TOAcceptanceInfo.AdjReinspectionDate = new Date($scope.TOAcceptanceInfo.AdjReinspectionDate);

                        if ($scope.TOAcceptanceInfo.AdjReinspectionMaxDate !== null) {
                            $scope.TOAcceptanceInfo.AdjReinspectionMaxDate = new Date($scope.TOAcceptanceInfo.AdjReinspectionMaxDate);
                            $scope.dateAdjReins.maxDate = $scope.TOAcceptanceInfo.AdjReinspectionMaxDate
                        }

                        // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.2)
                        // Re-inspection Date - Blank disabled if Turnover Option is Online Assisted / 
                        if ($scope.AcceptanceInfo.FinalTurnoverOption !== 'Standard') {
                            $scope.TOAcceptanceInfo.ReinspectionDate = null;
                        }

                        // Applicable on for Turnover Option Standard and Online-Assisted
                        if ($scope.AcceptanceInfo.FinalTurnoverOption !== 'Express') {
                            $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                            $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: true }
                            
                            // Enable Apply button if Turnover Status has no value
                            if ($scope.TOAcceptanceInfo.TurnoverStatus === '') {
                                $scope.TASFields = { TurnoverStatus: false, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: false, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                                $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                            }

                            // Disable Turnover & Acceptance Status Fields if Turnover Status is "No Show"
                            if ($scope.TOAcceptanceInfo.TurnoverStatus === 'No Show') {
                                $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                                $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: true }
                            }
                        }

                        // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.5.3.1.1.2 )
                        // Beyond thirty (30) calendar days from the Turnover Date field,
                        // system will NOT allow changes on status and will not accept punchlist details.                        
                        turnoverstatus = ['AWOP', 'AWP'];
                        if (!$scope.AcceptanceInfo.AllowedTOStatusUpdate && turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1) {
                            $scope.TASFields.TurnoverStatus = true;
                            $scope.TASFields.PunchlistCategory = true;
                            $scope.TASFields.PunchlistItems = true;
                        }
                        
                        // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.8)
                        // If ALL information, as applicable, on Turnover & Acceptance Status tab are completely posted, or
                        // transaction is with posted either Unit Acceptance Date, Key Transmittal Date or Deemed Acceptance
                        // Date fields, whichever comes later, system will display the details of transaction for VIEWING only, UPDATE
                        // and APPLY buttons will already be disabled.
                        if ($scope.AcceptanceInfo.KeyTransmittalDate !== null && $scope.AcceptanceInfo.UnitAcceptanceDate !== null) {
                            $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: false }
                        }
                    }
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[End Turnover & Acceptance Status Tab]-------------------------------------------- //
                    // --------------------------------------------------------------------------------------------------------------------------------- //


                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[Start Deemed Acceptance & Details Tab]------------------------------------------- //
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // Check if Deemed Acceptance & Details has record
                    if ($scope.DeemedAcceptanceInfo && $scope.DeemedAcceptanceInfo.Id !== 0) {
                        // Disable Deemed Acceptance Status Button and Field by default
                        $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                        $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

                        $scope.DASFields = { EmailDateNoticeSent: false, EmailNoticeRemarks: false, CourierDateNoticeSent: false, CourierDateNoticeReceived: false, CourierReceivedBy: false, CourierNoticeRemarks: false }
                        $scope.DASButtons = { AddButton: false, CancelButton: false }

                        if ($scope.DeemedAcceptanceInfo.DeemedAcceptanceDate !== null) 
                            $scope.DeemedAcceptanceInfo.DeemedAcceptanceDate = new Date($scope.DeemedAcceptanceInfo.DeemedAcceptanceDate);

                        // Details: Email Notice
                        // Disable Email Date Notice Sent and Remarks if has value
                        if ($scope.DeemedAcceptanceInfo.EmailDateNoticeSent !== null) {
                            $scope.DASFields.EmailDateNoticeSent = true;
                            $scope.DASFields.EmailNoticeRemarks = true;
                            $scope.DeemedAcceptanceInfo.EmailDateNoticeSent = new Date($scope.DeemedAcceptanceInfo.EmailDateNoticeSent);
                        }

                        // Details: Notice thru Courier
                        // Disable Courier Date Notice Sent if has value
                        if ($scope.DeemedAcceptanceInfo.CourierDateNoticeSent !== null) {
                            $scope.DASFields.CourierDateNoticeSent = true;
                            $scope.DeemedAcceptanceInfo.CourierDateNoticeSent = new Date($scope.DeemedAcceptanceInfo.CourierDateNoticeSent);
                        }

                        // Disable Courier Date Notice Received if has value
                        if ($scope.DeemedAcceptanceInfo.CourierDateNoticeReceived !== null) {
                            $scope.DASFields.CourierDateNoticeReceived = true;
                            $scope.DASFields.CourierNoticeRemarks = true;
                            $scope.DeemedAcceptanceInfo.CourierDateNoticeReceived = new Date($scope.DeemedAcceptanceInfo.CourierDateNoticeReceived);
                        }

                        // Disable Courier Received By if has value
                        if ($scope.DeemedAcceptanceInfo.CourierReceivedBy !== null) 
                            $scope.DASFields.CourierReceivedBy = true;

                        // Disable Courier Notice Remarks field if all Details on Notice thru Courier has been filled out
                        if ($scope.DeemedAcceptanceInfo.CourierDateNoticeSent !== null && $scope.DeemedAcceptanceInfo.CourierDateNoticeReceived !== null && $scope.DeemedAcceptanceInfo.CourierReceivedBy !== null)
                            $scope.DASFields.CourierNoticeRemarks = true;

                        if ($scope.DeemedAcceptanceInfo.HandoverAssociate === null)
                            $scope.DeemedAcceptanceInfo.HandoverAssociate = $scope.CurUser;

                        // Disable Deemed Acceptance Status Button if all fields has been filled out
                        if ($scope.DeemedAcceptanceInfo.EmailDateNoticeSent !== null && $scope.DeemedAcceptanceInfo.CourierDateNoticeSent !== null && $scope.DeemedAcceptanceInfo.CourierDateNoticeReceived !== null && $scope.DeemedAcceptanceInfo.CourierReceivedBy !== null)
                            $scope.DASButtons = { AddButton: true, CancelButton: true }

                        // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.8)
                        //If ALL information, as applicable, on Turnover & Acceptance Status tab are completely posted, or
                        //transaction is with posted either Unit Acceptance Date, Key Transmittal Date or Deemed Acceptance
                        //Date fields, whichever comes later, system will display the details of transaction for VIEWING only, UPDATE
                        //and APPLY buttons will already be disabled.
                        if ($scope.DeemedAcceptanceInfo.DeemedAcceptanceDate !== null) {
                            $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: true, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: false }
                        }

                    } else {
                        $scope.DeemedAcceptanceInfo = { HandoverAssociate: $scope.CurUser };
                        // If Not for Deemed Acceptance Date then disable all fields
                        $scope.DASFields = { EmailDateNoticeSent: true, EmailNoticeRemarks: true, CourierDateNoticeSent: true, CourierDateNoticeReceived: true, CourierReceivedBy: true, CourierNoticeRemarks: true }
                        $scope.DASButtons = { AddButton: true, CancelButton: true }
                    }
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[End Deemed Acceptance & Details Tab]--------------------------------------------- //
                    // --------------------------------------------------------------------------------------------------------------------------------- //

                    if ($scope.ClientInfo) {
                        Object.keys($scope.ClientInfo).forEach(function (key) {
                            $scope.ClientInfo[key].CreatedDate = new Date($scope.ClientInfo[key].CreatedDate);
                        });
                    }
                }
                $scope.disableClientButton = true;
                $scope.disableButton = false;
                if ($scope.AcceptanceInfo === null) {
                    swal(
                        'Error Message',
                        'Criteria is not met or not yet qualified for turnover',
                        'error'
                    );
                    $scope.disableButton = true;
                }
                $scope.data4b = { CustomerNos: '0', ClientRemarks: '', ClientAttachment: '' };
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

    // Reset Punchlist Category based on Turnover Status selection
    $scope.changeTOStatus = function () {
        $scope.TOAcceptanceInfo.PunchlistCategory = "";
        $scope.TOAcceptanceInfo.PunchlistItem = "";
        $scope.TOAcceptanceInfo.OtherIssues = "";
        $scope.TOAcceptanceInfo.UnitAcceptanceDate = "";
        $scope.TOAcceptanceInfo.KeyTransmittalDate = "";
        $scope.TOAcceptanceInfo.ReinspectionDate = "";
        $scope.TOAcceptanceInfo.AdjReinspectionDate = "";
        $scope.TOAcceptanceInfo.RushTicketNos = "";

        if ($scope.AcceptanceInfo.FinalTurnoverOption !== 'Express') {
            $scope.TASFields = { TurnoverStatus: false, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: false, UnitAcceptanceDate: true, KeyTransmittalDate: false, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }

            // Enable & Mandatory Punchlist Category & Items  if selected value on Turnover Status field is AWP or NAPL
            var turnoverstatus = ['AWP', 'NAPL'];
            if (turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TASFields.PunchlistCategory = false;
                $scope.TASFields.PunchlistItems = false;
            }

            // Enable & Mandatory Other Issues if selected value on Turnover Status field is NAOI
            var turnoverstatus = ['NAOI'];
            if (turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TASFields.OtherIssues = false;
            }

            // Enable Unit Acceptance Date if value on the Turnover Status is NAPL or NAOI
            turnoverstatus = ['NAOI', 'NAPL'];
            if (turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TASFields.UnitAcceptanceDate = false;
            }

            // Re-inspection Date Applicable, if value on Turnover Option field is Standard and selected value on Turnover Status field is NAPL
            turnoverstatus = ['NAPL'];
            if (turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1 && $scope.AcceptanceInfo.FinalTurnoverOption === 'Statndard') {
                $scope.TASFields.ReinspectionDate = false; 
            }

            // Enable Adjusted Reinspection Date if value on the Turnover Status field is NAPL and Reinspection Date field has value/posted already
            turnoverstatus = ['NAPL'];
            if (turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1 && $scope.AcceptanceInfo.ReinspectionDate !== null) {
                $scope.TASFields.AdjReinspectionDate = false; 
            }

            // Enable Rush.Net Ticket # if selected value on Turnover Status field is AWP, NAPL or NAOI
            turnoverstatus = ['NAOI', 'NAPL', 'AWP'];
            if (turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TASFields.RushTicketNos = false; 
            }
            
            // If Turnover Status is AWOP, AWP or Warranty Claim, autopopulated with the value on the Turnover Date field but Unit Acceptance Date is disabled
            var turnoverstatus = ['AWOP', 'AWP', 'Warranty Claims'];
            if (turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TOAcceptanceInfo.UnitAcceptanceDate = $scope.AcceptanceInfo.FinalTurnoverDate;
            }

            // Blank disabled if Turnover Option is Online Assisted / 
            if ($scope.AcceptanceInfo.FinalTurnoverOption !== 'Standard') {
                $scope.TOAcceptanceInfo.ReinspectionDate = "";
            }
        }
    };

    // Compute Inspection TAT based on Parameter
    $scope.ChangePunchlistCategory = function () {
        $scope.TOAcceptanceInfo.ReinspectionDate = "";

        // Re-inspection Date: Applicable, if value on Turnover Option field is Standard and selected value on Turnover Status field is NAPL
        if ($scope.AcceptanceInfo.FinalTurnoverOption === 'Standard' && $scope.TOAcceptanceInfo.TurnoverStatus === 'NAPL') {

            // Find TAT based on the Punchlist Category field 
            var obj = $scope.PunchlistCategory.find(obj => {
                return obj.Name === $scope.TOAcceptanceInfo.PunchlistCategory
            });

            // Applicable, if value on Turnover Option field is Standard and selected value on Turnover Status field is NAPL
            // System-generated date based on the TAT for the selected value on the Punchlist Category field, for NAPL Status 
            if (obj) {
                // Get only date based on calendar days 
                if (obj.CalendarType === 'CD') {
                    var reinspectionDate = new Date();
                    reinspectionDate.setDate(reinspectionDate.getDate() + obj.TurnaroundTime);
                    $scope.TOAcceptanceInfo.ReinspectionDate = reinspectionDate;
                } else {
                    var token = sessionStorage.getItem(tokenKey);
                    var headers = {};
                    if (token) {
                        headers.Authorization = 'Bearer ' + token;
                    }

                    urlData = '../api/UnitInspectionAcceptanceMgmnt/GetWorkingDate';
                    $http({
                        method: 'GET',
                        url: urlData,
                        params: { 'TAT': obj.TurnaroundTime },
                        headers: headers,
                    }).then(function (response) {
                        if (response.status === 200) {
                            $scope.TOAcceptanceInfo.ReinspectionDate = new Date(response.data.ADJDATE);
                        }
                    }, function (response) {
                        var obj = response.data.Message;
                        swal('Error Message', obj, 'error');
                    });
                }
            }
        }
    };    

    // call from other controller
    $scope.GetTransactionLog = function () {
        $('#changeLogModal').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });

        $rootScope.$emit("searchObjectType", 'UnitInspectionAcceptanceMgmntController');
    }

    // Start individual Edit control
    // -------------------------------------- Turnover & Acceptance Status

    $scope.UpdateTurnoverAcceptance = function () {
        // If value on the Turnover Option field for the unit is Express, system will automatically populate / tag the
        // following fields with value / data which cannot be changed by the user(disabled)
        // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.3)
        if ($scope.AcceptanceInfo.FinalTurnoverOption !== 'Express' && $scope.TOAcceptanceInfo) {
            $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
            $scope.TASFields = { TurnoverStatus: true, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: true, UnitAcceptanceDate: true, KeyTransmittalDate: false, ReinspectionDate: true, AdjReinspectionDate: true, RushTicketNos: true, SRRemarks: true }
                        

            //// Enable & Mandatory Other Issues if selected value on Turnover Status field is NAOI
            //var turnoverstatus = ['NAOI'];
            //if (turnoverstatus.indexOf($scope.AcceptanceInfo.TurnoverStatus) !== -1) {
            //    $scope.TASFields.OtherIssues = false;
            //}

            // Enable Unit Acceptance Date if value on the Turnover Status is NAPL or NAOI
            turnoverstatus = ['NAOI', 'NAPL'];
            if (turnoverstatus.indexOf($scope.AcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TASFields.UnitAcceptanceDate = false;
            }

            //// Re-inspection Date Applicable, if value on Turnover Option field is Standard and selected value on Turnover Status field is NAPL
            //turnoverstatus = ['NAPL'];
            //if (turnoverstatus.indexOf($scope.AcceptanceInfo.TurnoverStatus) !== -1 && $scope.AcceptanceInfo.FinalTurnoverOption === 'Statndard') {
            //    $scope.TASFields.ReinspectionDate = false;
            //}

            // Enable Adjusted Reinspection Date if value on the Turnover Status field is NAPL and Reinspection Date field has value/posted already
            turnoverstatus = ['NAPL'];
            if (turnoverstatus.indexOf($scope.AcceptanceInfo.TurnoverStatus) !== -1 && $scope.AcceptanceInfo.ReinspectionDate !== null) {
                $scope.TASFields.AdjReinspectionDate = false;
            }

            // Enable Rush.Net Ticket # if selected value on Turnover Status field is AWP, NAPL or NAOI
            turnoverstatus = ['NAOI', 'NAPL', 'AWP'];
            if (turnoverstatus.indexOf($scope.AcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TASFields.RushTicketNos = false;
            }

            // Blank disabled if Turnover Option is Online Assisted / 
            if ($scope.AcceptanceInfo.FinalTurnoverOption === 'Online-Assisted') {
                $scope.TOAcceptanceInfo.ReinspectionDate = "";
            }

            // Reference FRD: Transaction > Unit Inspection & Acceptance Management > Turnover & Acceptance Status (5.5.3.1.1.2 )
            // Beyond thirty (30) calendar days from the Turnover Date field,
            // system will NOT allow changes on status and will not accept punchlist details.        
            if ($scope.TOAcceptanceInfo.TurnoverStatus === 'AWOP' && $scope.AcceptanceInfo.AllowedTOStatusUpdate) {
                $scope.TASFields.TurnoverStatus = false;

                // Enable & Mandatory Punchlist Category & Items  if selected value on Turnover Status field is AWP or NAPL
                var turnoverstatus = ['AWP'];
                if (turnoverstatus.indexOf($scope.AcceptanceInfo.TurnoverStatus) !== -1) {
                    $scope.TASFields.PunchlistCategory = false;
                    $scope.TASFields.PunchlistItems = false;
                } else {
                    $scope.TASFields.PunchlistCategory = true;
                    $scope.TASFields.PunchlistItems = true;
                }

            } else {
                $scope.TASFields.TurnoverStatus = true;
            }

            turnoverstatus = ['AWOP', 'AWP'];
            if (!$scope.AcceptanceInfo.AllowedTOStatusUpdate && turnoverstatus.indexOf($scope.TOAcceptanceInfo.TurnoverStatus) !== -1) {
                $scope.TASFields.TurnoverStatus = true;
                $scope.TASFields.PunchlistCategory = true;
                $scope.TASFields.PunchlistItems = true;
            }  

            // For checking, remove this line if already checked
            // Deemed Acceptance transaction is not applicable for Lease To Own(LTO) account type.
            if ($scope.AcceptanceInfo.AccountTypeCode === 'L' && $scope.TOAcceptanceInfo.TurnoverStatus === 'No Show') {
                $scope.TASFields.UnitAcceptanceDate = false;

                if ($scope.TOAcceptanceInfo.UnitAcceptanceDate !== null)
                    $scope.TASFields.UnitAcceptanceDate = true;

                if ($scope.TOAcceptanceInfo.KeyTransmittalDate !== null)
                    $scope.TASFields.KeyTransmittalDate = true;
            }
        }
    }

    $scope.SaveTurnoverAcceptanceModal = function () {
        $scope.$broadcast('show-errors-event');
        if ($scope.toacceptstatusForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'error'
            );
        } else {
            $('#applyChangesModal1a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    $scope.SaveTurnoverAcceptance = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        data.SalesDocNos = $scope.AcceptanceInfo.SalesDocNos;
        data.QuotDocNos = $scope.AcceptanceInfo.QuotDocNos;
        data.CompanyCode = $scope.AcceptanceInfo.CompanyCode;
        data.ProjectCode = $scope.AcceptanceInfo.ProjectCode;
        data.UnitNos = $scope.AcceptanceInfo.UnitNos;
        data.UnitCategory = $scope.AcceptanceInfo.UnitCategoryCode;
        data.CustomerNos = $scope.AcceptanceInfo.CustomerNos;
        data.TOScheduleID = $scope.AcceptanceInfo.TOScheduleID;
        data.AccountTypeCode = $scope.AcceptanceInfo.AccountTypeCode;
        data.FinalTurnoverOption = $scope.AcceptanceInfo.FinalTurnoverOption;
        data.TORule2 = $scope.AcceptanceInfo.TORule2;
        data.FinalTurnoverDate = new Date($scope.AcceptanceInfo.FinalTurnoverDate);

        urlData = '../api/UnitInspectionAcceptanceMgmnt/SaveTurnoverAcceptance';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitInspectData();
                $scope.clearData();

                $('#applyChangesModal1a').modal('hide');
                swal(
                    'System Message Confirmation',
                    'Information provided for the units has been recorded.',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

    $scope.CancelTurnoverAcceptanceModal = function () {
        $('#applyChangesModal1b').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    }

    $scope.CancelTurnoverAcceptance = function () {
        $scope.GetUnitInspectData();
        $('#applyChangesModal1b').modal('hide');
    };

    // -------------------------------------- Deemed Acceptance Details
    $scope.SaveDeemedAcceptanceModal = function () {
        $scope.$broadcast('show-errors-event');
        if ($scope.deemedacceptForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out valid data or tag mandatory fields to be able to proceed.',
                'error'
            );
        } else {
            $('#applyChangesModal2a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    $scope.SaveDeemedAcceptance = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        data.SalesDocNos = $scope.AcceptanceInfo.SalesDocNos;
        data.QuotDocNos = $scope.AcceptanceInfo.QuotDocNos;
        data.CompanyCode = $scope.AcceptanceInfo.CompanyCode;
        data.ProjectCode = $scope.AcceptanceInfo.ProjectCode;
        data.UnitNos = $scope.AcceptanceInfo.UnitNos;
        data.UnitCategory = $scope.AcceptanceInfo.UnitCategoryCode;
        data.CustomerNos = $scope.AcceptanceInfo.CustomerNos;
        data.DeemedAcceptanceID = $scope.AcceptanceInfo.DeemedAcceptanceID;
        data.AccountTypeCode = $scope.AcceptanceInfo.AccountTypeCode;
        data.FinalTurnoverOption = $scope.AcceptanceInfo.FinalTurnoverOption;
        data.TORule2 = $scope.AcceptanceInfo.TORule2;

        urlData = '../api/UnitInspectionAcceptanceMgmnt/SaveDeemedAcceptance';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitInspectData();
                $scope.clearData();

                $('#applyChangesModal2a').modal('hide');
                swal(
                    'System Message Confirmation',
                    'Information provided for the units has been recorded.',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

    $scope.CancelDeemedAcceptanceModal = function () {
        $('#applyChangesModal2b').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    }

    $scope.CancelDeemedAcceptance = function () {
        $scope.GetUnitInspectData();
        $('#applyChangesModal2b').modal('hide');
    };
    // End individual Edit control

    // Start Date Management
    $('[data-toggle="tooltip"]').tooltip();

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

    $scope.dateOptionsWorkingDays = {
        dateDisabled: disabled,
        formatYear: 'yy',
        startingDay: 1,
        showWeeks: false,
    };

    // Adjusted Reinspection Date
    $scope.dateAdjReins = {
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

    // Unit Acceptance Date
    $scope.open1 = function () {
        $scope.popup1.opened = true;
    };

    $scope.popup1 = {
        opened: false
    };

    // Key Transmittal Date
    $scope.open2 = function () {
        $scope.popup2.opened = true;
    };

    $scope.popup2 = {
        opened: false
    };

    //// Re-inspection Date
    //$scope.open3 = function () {
    //    $scope.popup3.opened = true;
    //};

    //$scope.popup3 = {
    //    opened: false
    //};

    // Adj. Re-inspection Date
    $scope.open4 = function () {
        $scope.popup4.opened = true;
    };

    $scope.popup4 = {
        opened: false
    };

    // Date Email Notice Sent 
    $scope.open5 = function () {
        $scope.popup5.opened = true;
    };

    $scope.popup5 = {
        opened: false
    };

    // Courier Notice Sent Date
    $scope.open6 = function () {
        $scope.popup6.opened = true;
    };

    $scope.popup6 = {
        opened: false
    };

    // Courier Notice Received Date
    $scope.open7 = function () {
        $scope.popup7.opened = true;
    };

    $scope.popup7 = {
        opened: false
    };

    $scope.dateFormats = ['MM/dd/yyyy', 'dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.dateFormat = $scope.dateFormats[0];
    $scope.altInputFormats = ['M!/d!/yyyy'];
    // End Date Management
}]);


