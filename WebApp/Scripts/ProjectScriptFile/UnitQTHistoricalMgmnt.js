app.controller("UnitQTHistoricalMgmnt", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data4b = { CustomerNos: '0', ClientRemarks: '', ClientAttachment: '' }; // Clients Information
    $scope.UnitQTHistoricalMgmnt = [];
    $scope.HolidayWeekenDays = [];
    $scope.HistoricalInfo = null;
    $scope.ToScheduleInfo = null;
    $scope.jomel = false;
    $scope.totalItems = 0;
    $scope.loading = false;
    $scope.disableButton = true;
    $scope.disableClientButton = true;
    $scope.ntoButton = false; // Button behavior by default is disabled on Notice of Unit Turnover
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

            $scope.GetUnitQualifyData();
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
        //$log.info('Item changed to ' + source + ' ' + JSON.stringify(item));

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
        $scope.data2 = {};
        $scope.data.nvFabIcon = "fa-bars";
    };

    $scope.resetData = function () {
        $scope.Ctrl = null;
        $scope.HistoricalInfo = null;
        $scope.UnitHistorical = null;
        $scope.ToScheduleInfo = null;
        $scope.ClientInfo = null;
        $scope.data4b = { CustomerNos: '0', ClientRemarks: '', ClientAttachment: '' };
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
        $scope.HistoricalInfo = null;

        // make sure to remove query string if new search happen
        if (window.location.search.match("flx10ms="))
            window.history.replaceState(null, "", window.location.href.split("?")[0]);
    };

    //-----------------------------------------------------------
    // Set parameter for search field
    $scope.paramInfo = { ProjectID: '0', CompanyCode: '', ProjectCode: '', UnitNos: '', UnitCategory: '', CustomerNos: '', ProjectLocation: '', PageUrl: '' };

    $scope.NUTFields = { EmailDateNoticeSent: true, EmailTurnoverDate: true, EmailTurnoverTime: true, EmailNoticeRemarks: true, CourierDateNoticeSent: true, CourierDateNoticeReceived: true, CourierReceivedBy: true, CourierNoticeRemarks: true }
    $scope.NUTButtons = { AddButton: true, CancelButton: true }

    $scope.TSOFields = { TurnoverDate1: true, TurnoverTime1: true, TurnoverRemarks1: true, TurnoverOption1: true, TurnoverDate2: true, TurnoverTime2: true, TurnoverRemarks2: true, TurnoverOption2: true }
    $scope.TSOButtons = { AddButton: true, CancelButton: true }
    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';

        urlData = '../api/UnitQTHistoricalMgmnt/GetSearchData';
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

    $scope.GetUnitQualifyData = function () {
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

        urlData = '../api/UnitQTHistoricalMgmnt/GetQTUnitHistoricalMgmnt';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.HistoricalInfo = response.data.HISTORICALINFO;
                $scope.Ctrl = response.data.CONTROLS;

                // Check if the data is coming from the URL Variable
                if ($scope.withUrlVariable) {
                    $scope.paramInfo.ProjectLocation = $scope.HistoricalInfo.ProjectLocation;
                }

                if ($scope.HistoricalInfo != null) {
                    $scope.jomel = true; // hit the search button
                    // Populate Data once Header Data is found!
                    $scope.TurnoverOptions = response.data.TURNOVEROPTIONLIST;
                    $scope.UnitHistorical = response.data.UNITHISTORICAL;
                    $scope.ClientInfo = response.data.CLIENTINFO;
                    $scope.HolidayWeekenDays = response.data.EXCEPTIONDAYS;
                    $scope.CurUser = response.data.CURUSER;
                    $scope.SysParam = response.data.SYSPARAM;

                    $scope.totalItems = $scope.ClientInfo.length;

                    // Formatting TOAS date relate field
                    if ($scope.HistoricalInfo.TOAS !== null)
                        $scope.HistoricalInfo.TOAS = new Date($scope.HistoricalInfo.TOAS);

                    // Check if Notice of Unit Turnover has record
                    if ($scope.UnitHistorical && $scope.UnitHistorical.Id !== 0) {

                        // --------------------------------------------------------------------------------------------------------------------------------- //
                        // -----------------------------------------------[Start Notice of Unit Turnover Tab]----------------------------------------------- //
                        // --------------------------------------------------------------------------------------------------------------------------------- //
                        // FPMC Acceptance Date
                        if ($scope.UnitHistorical.FPMCAcceptanceDate !== null) {
                            $scope.NUTFields.FPMCAcceptanceDate = true;
                            $scope.UnitHistorical.FPMCAcceptanceDate = new Date($scope.UnitHistorical.FPMCAcceptanceDate);
                        } else {
                            $scope.NUTFields.FPMCAcceptanceDate = false;
                        }

                        // QCD Acceptance Date
                        if ($scope.UnitHistorical.QCDAcceptanceDate !== null) {
                            $scope.NUTFields.QCDAcceptanceDate = true;
                            $scope.UnitHistorical.QCDAcceptanceDate = new Date($scope.UnitHistorical.QCDAcceptanceDate);
                        } else {
                            $scope.NUTFields.QCDAcceptanceDate = false;
                        }

                        // Details: Email Notice
                        // Disable Email Notice Sent if has value
                        if ($scope.UnitHistorical.EmailDateNoticeSent !== null) {
                            $scope.NUTFields.EmailDateNoticeSent = true;
                            $scope.UnitHistorical.EmailDateNoticeSent = new Date($scope.UnitHistorical.EmailDateNoticeSent);

                            // Turnover Schedule Date should not be beyond x (calendar type) days from the Date of Email Notice Sent
                            // Reference FRD: Transaction > Unit Qualification & Turnover Management > Turnover Schedule & Option (5.2)
                            $scope.dateTSOTurnover.minDate = $scope.UnitHistorical.EmailDateNoticeSent;
                        } else {
                            $scope.NUTFields.EmailDateNoticeSent = false;
                        }

                        // Disable Email Turnover Date if has value
                        if ($scope.UnitHistorical.EmailTurnoverDate !== null) {
                            $scope.NUTFields.EmailTurnoverDate = true;
                            $scope.NUTFields.EmailTurnoverTime = true;
                            var nudt1 = new Date($scope.UnitHistorical.EmailTurnoverDate);
                            $scope.UnitHistorical.EmailTurnoverDate = new Date($scope.UnitHistorical.EmailTurnoverDate);
                            $scope.UnitHistorical.EmailTurnoverTime = new Date(1970, 0, 1, nudt1.getHours(), nudt1.getMinutes(), nudt1.getSeconds());
                        } else {
                            $scope.NUTFields.EmailTurnoverDate = false;
                            $scope.NUTFields.EmailTurnoverTime = false;
                        }

                        // Disable Email Notice Remarks field if all Details on Email Notice has been filled out
                        if ($scope.UnitHistorical.EmailDateNoticeSent !== null && $scope.UnitHistorical.EmailTurnoverDate !== null)
                            $scope.NUTFields.EmailNoticeRemarks = true;
                        else
                            $scope.NUTFields.EmailNoticeRemarks = false;

                        // Details: Notice thru Courier
                        // Disable Courier Date Notice Sent field if has value
                        if ($scope.UnitHistorical.CourierDateNoticeSent !== null) {
                            $scope.NUTFields.CourierDateNoticeSent = true;
                            $scope.NUTFields.CourierDateNoticeReceived = false;
                            $scope.NUTFields.CourierReceivedBy = false;
                            $scope.UnitHistorical.CourierDateNoticeSent = new Date($scope.UnitHistorical.CourierDateNoticeSent);
                        }
                        else {
                            $scope.NUTFields.CourierDateNoticeSent = false;
                            $scope.NUTFields.CourierDateNoticeReceived = true;
                            $scope.NUTFields.CourierReceivedBy = true;
                        }

                        // Disable Courier Date Notice Received field if has value
                        if ($scope.UnitHistorical.CourierDateNoticeReceived !== null) {
                            $scope.NUTFields.CourierDateNoticeReceived = true;
                            $scope.UnitHistorical.CourierDateNoticeReceived = new Date($scope.UnitHistorical.CourierDateNoticeReceived);
                        }

                        // Disable Courier Received By field if has value
                        if ($scope.UnitHistorical.CourierReceivedBy !== null)
                            $scope.NUTFields.CourierReceivedBy = true;

                        // Disable Courier Notice Remarks field if all Details on Notice thru Courier has been filled out
                        if ($scope.UnitHistorical.CourierDateNoticeSent !== null && $scope.UnitHistorical.CourierDateNoticeReceived !== null && $scope.UnitHistorical.CourierReceivedBy !== null)
                            $scope.NUTFields.CourierNoticeRemarks = true;
                        else
                            $scope.NUTFields.CourierNoticeRemarks = false;

                        // Disable Notice of Unit Turnover Button if all fields has been filled out
                        if ($scope.UnitHistorical.EmailDateNoticeSent !== null && $scope.UnitHistorical.EmailTurnoverDate !== null && $scope.UnitHistorical.CourierDateNoticeSent !== null && $scope.UnitHistorical.CourierDateNoticeReceived !== null && $scope.UnitHistorical.CourierReceivedBy !== null)
                            $scope.NUTButtons = { AddButton: true, CancelButton: true }
                        else
                            $scope.NUTButtons = { AddButton: false, CancelButton: false }
                        // ------------------------------------------------------------------------------------------------------------------------------- //
                        // -----------------------------------------------[End Notice of Unit Turnover Tab]----------------------------------------------- //
                        // ------------------------------------------------------------------------------------------------------------------------------- //
                        // ------------------------------------------------------------------------------------------------------------------------------------ //
                        // -----------------------------------------------[Start Turnover Schedule & Option Tab]----------------------------------------------- //
                        // ------------------------------------------------------------------------------------------------------------------------------------ //
                        // Enable Turnover Schedule and Option Button and Field by default   
                        $scope.TSOFields = { TurnoverDate1: false, TurnoverTime1: false, TurnoverRemarks1: false, TurnoverOption1: false, TurnoverDate2: false, TurnoverTime2: false, TurnoverRemarks2: false, TurnoverOption2: false }
                        $scope.TSOButtons = { AddButton: false, CancelButton: false }

                        // Details: Confirmed Turnover Schedule & Option (1st)
                        // Disable 1st Turnover Date if has value
                        if ($scope.UnitHistorical.TurnoverDate1 !== null) {
                            $scope.TSOFields.TurnoverDate1 = true;
                            $scope.UnitHistorical.TurnoverDate1 = new Date($scope.UnitHistorical.TurnoverDate1);
                        }

                        // Disable 1st Turnover Time if has value
                        if ($scope.UnitHistorical.TurnoverTime1 !== null) {
                            $scope.TSOFields.TurnoverTime1 = true;
                            var tot1 = new Date($scope.UnitHistorical.TurnoverTime1);
                            $scope.UnitHistorical.TurnoverTime1 = new Date(1970, 0, 1, tot1.getHours(), tot1.getMinutes(), tot1.getSeconds());
                        }

                        // Disable 1st Turnover Option if has value
                        if ($scope.UnitHistorical.TurnoverOption1 !== null)
                            $scope.TSOFields.TurnoverOption1 = true;

                        // Disable 1st Turnover Remarks if all fields on 1st Turnover Schedule & Option has been filled out
                        if ($scope.UnitHistorical.TurnoverDate1 !== null && $scope.UnitHistorical.TurnoverTime1 !== null && $scope.UnitHistorical.TurnoverOption1 !== null)
                            $scope.TSOFields.TurnoverRemarks1 = true;

                        // Details: Confirmed Turnover Schedule & Option (re-schedule)
                        // Disable re-schedule Turnover Date if has value
                        if ($scope.UnitHistorical.TurnoverDate2 !== null) {
                            $scope.TSOFields.TurnoverDate2 = true;
                            $scope.UnitHistorical.TurnoverDate2 = new Date($scope.UnitHistorical.TurnoverDate2);
                        }

                        // Disable re-schedule Turnover Time if has value
                        if ($scope.UnitHistorical.TurnoverTime2 !== null) {
                            $scope.TSOFields.TurnoverTime2 = true;
                            var tot2 = new Date($scope.UnitHistorical.TurnoverTime2);
                            $scope.UnitHistorical.TurnoverTime2 = new Date(1970, 0, 1, tot2.getHours(), tot2.getMinutes(), tot2.getSeconds());
                        }

                        // Disable re-schedule Turnover Option if has value
                        if ($scope.UnitHistorical.TurnoverOption2 !== null)
                            $scope.TSOFields.TurnoverOption2 = true;

                        // Disable re-schedule Turnover Remarks if all fields on re-schedule Turnover Schedule & Option has been filled out
                        if ($scope.UnitHistorical.TurnoverDate2 !== null && $scope.UnitHistorical.TurnoverTime2 !== null && $scope.UnitHistorical.TurnoverOption2 !== null)
                            $scope.TSOFields.TurnoverRemarks2 = true;

                        //---------------------------------------------------------------------------------------------------------------------------------- //
                        //-----------------------------------------------[End Turnover Schedule & Option Tab]----------------------------------------------- //
                        //---------------------------------------------------------------------------------------------------------------------------------- //
                    } else {

                        $scope.UnitHistorical = { HandoverAssociate: $scope.CurUser, FPMCAcceptanceDate : '', QCDAcceptanceDate: '' };

                        // Enable Notice of Unit Turnover Button and Field by default
                        $scope.NUTFields = { EmailDateNoticeSent: false, EmailTurnoverDate: false, EmailTurnoverTime: false, EmailNoticeRemarks: false, CourierDateNoticeSent: false, CourierDateNoticeReceived: false, CourierReceivedBy: false, CourierNoticeRemarks: false }
                        $scope.NUTButtons = { AddButton: false, CancelButton: false }

                        // Enable 1st Turnover Schedule and Option Button and Field by default           
                        $scope.TSOFields = { TurnoverDate1: false, TurnoverTime1: false, TurnoverRemarks1: false, TurnoverOption1: false, TurnoverDate2: true, TurnoverTime2: true, TurnoverRemarks2: true, TurnoverOption2: true }
                        $scope.TSOButtons = { AddButton: false, CancelButton: false }


                        // get Default value from unitacceptance if already set
                        if ($scope.HistoricalInfo.FPMCAcceptanceDate != null) {
                            $scope.UnitHistorical.FPMCAcceptanceDate = new Date($scope.HistoricalInfo.FPMCAcceptanceDate);
                        }

                        if ($scope.HistoricalInfo.QCDAcceptanceDate != null) {
                            $scope.UnitHistorical.QCDAcceptanceDate = new Date($scope.HistoricalInfo.QCDAcceptanceDate);
                        }
                    }

                    // Check if Client Information has record
                    if ($scope.ClientInfo) {
                        Object.keys($scope.ClientInfo).forEach(function (key) {
                            $scope.ClientInfo[key].CreatedDate = new Date($scope.ClientInfo[key].CreatedDate);
                        });
                    }
                }

                $scope.disableClientButton = true;
                $scope.disableButton = false;
                if ($scope.HistoricalInfo === null) {
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

    // call from other controller
    $scope.GetTransactionLog = function () {
        $('#changeLogModal').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });

        $rootScope.$emit("searchObjectType", 'UnitHistoricalMgmntController');
    }

    // Start individual Edit control
    // -------------------------------------- Notice Unit Turnover
    $scope.SaveNoticeUnitTurnoverModal = function () {
        $scope.$broadcast('show-errors-event');
        if ($scope.noticeunittoForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'error'
            );
            return;
        } else {
            $('#applyChangesModal1a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    $scope.SaveNoticeUnitTurnover = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        data.CompanyCode = $scope.HistoricalInfo.CompanyCode;
        data.ProjectCode = $scope.HistoricalInfo.ProjectCode;
        data.UnitNos = $scope.HistoricalInfo.UnitNos;
        data.UnitCategory = $scope.HistoricalInfo.UnitCategoryCode;
        data.CustomerNos = $scope.HistoricalInfo.CustomerNos;
        data.Transaction = 'NoticeUnitTurnover';

        urlData = '../api/UnitQTHistoricalMgmnt/SaveHistoricalData';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitQualifyData();
                $scope.clearData();

                $scope.data4b = { CustomerNos: '0', ClientRemarks: '', ClientAttachment: '' };
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

    $scope.CancelNoticeUnitTurnoverModal = function () {
        $('#applyChangesModal1b').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    }

    $scope.CancelNoticeUnitTurnover = function () {
        $scope.GetUnitQualifyData();
        $('#applyChangesModal1b').modal('hide');
    };

    // -------------------------------------- Turnover Schedule
    $scope.SaveTurnoverScheduleModal = function (item) {
        $scope.$broadcast('show-errors-event');
        if ($scope.turnoverscheduleForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out valid data or tag mandatory fields to be able to proceed.',
                'error'
            );
        } else {
            if (item) {
                $('#applyChangesModal2a2').modal({
                    backdrop: 'static',
                    keyboard: false,
                    show: true
                });
            } else {
                $('#applyChangesModal2a').modal({
                    backdrop: 'static',
                    keyboard: false,
                    show: true
                });
            }
        }
    }

    $scope.SaveTurnoverSchedule = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        data.CompanyCode = $scope.HistoricalInfo.CompanyCode;
        data.ProjectCode = $scope.HistoricalInfo.ProjectCode;
        data.UnitNos = $scope.HistoricalInfo.UnitNos;
        data.UnitCategory = $scope.HistoricalInfo.UnitCategoryCode;
        data.CustomerNos = $scope.HistoricalInfo.CustomerNos;
        data.Transaction = 'TurnoverSchedule';

        urlData = '../api/UnitQTHistoricalMgmnt/SaveHistoricalData';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitQualifyData();
                $scope.clearData();

                $('#applyChangesModal2a').modal('hide');
                $('#applyChangesModal2a2').modal('hide');
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

    $scope.CancelTurnoverScheduleModal = function () {
        $('#applyChangesModal2b').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    }

    $scope.CancelTurnoverSchedule = function () {
        $scope.GetUnitQualifyData();
        $('#applyChangesModal2b').modal('hide');
    };

    // -------------------------------------- Client Information
    $scope.UpdateClientRemarks = function () {
        $scope.disableClientButton = false;
    };

    $scope.SaveClientInfoModal = function () {
        $scope.$broadcast('show-errors-event');
        if ($scope.clientForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out valid data or tag mandatory fields to be able to proceed.',
                'error'
            );
        } else {
            $('#applyChangesModal3a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    $scope.SaveClientInfo = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';
        data.CustomerNos = $scope.HistoricalInfo.CustomerNos;

        urlData = '../api/UnitQTHistoricalMgmnt/SaveCustomerProfile';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitQualifyData();
                $scope.clearData();

                $scope.data4b = { CustomerNos: '0', ClientRemarks: '', ClientAttachment: '' };
                $('#applyChangesModal3a').modal('hide');
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

    $scope.CancelClientInfoModal = function () {
        $('#applyChangesModal3b').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    }

    $scope.CancelClientInfo = function () {
        $scope.GetUnitQualifyData();
        $('#applyChangesModal3b').modal('hide');
    };

    // End individual Edit control
    // Start Date Management
    $('[data-toggle="tooltip"]').tooltip();

    $scope.SetCourierDateNotice = function (d) {
        $scope.NUTFields.CourierDateNoticeReceived = true;
        $scope.NUTFields.CourierReceivedBy = true;
        if (d !== null) {
            $scope.NUTFields.CourierDateNoticeReceived = false;
            $scope.NUTFields.CourierReceivedBy = false;
        }
    }

    $scope.dateOptionsDefault = {
        formatYear: 'yy',
        startingDay: 1,
        showWeeks: false,
    };

    $scope.dateOptionsCurrent = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Notice of Unit Turnover Email & Courier Date Notice Sent
    $scope.dateNoticeCurrent = {
        formatYear: 'yy',
        minDate: new Date(),
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Notice of Unit Turnover Turnover Date
    $scope.dateNUTTurnover = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Notice of Unit Turnover Date Notice Received
    $scope.dateNUTsReceived = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Turnover Schedule & Option Turnover Date 1st
    $scope.dateTSOTurnover = {
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

    // Date Email Notice Sent 
    $scope.open1 = function () {
        $scope.popup1.opened = true;

        $scope.UnitHistorical.EmailTurnoverDate = null;
    };

    $scope.popup1 = {
        opened: false
    };

    // Date Email Turnover Date
    $scope.open2 = function () {
        $scope.popup2.opened = true;
        $scope.dateNUTTurnover.minDate = $scope.UnitHistorical.EmailDateNoticeSent;
    };

    $scope.popup2 = {
        opened: false
    };

    // Date Courier Notice Sent
    $scope.open3 = function () {
        $scope.popup3.opened = true;
    };

    $scope.popup3 = {
        opened: false
    };

    // Date Courier Notice Received
    $scope.open4 = function () {
        $scope.popup4.opened = true;
    };

    $scope.popup4 = {
        opened: false
    };

    // 1st Turnover Date 
    $scope.open5 = function () {
        $scope.popup5.opened = true;
    };

    $scope.popup5 = {
        opened: false
    };

    // Reschedule Turnover Date 
    $scope.open6 = function () {
        $scope.popup6.opened = true;
    };

    $scope.popup6 = {
        opened: false
    };

    // QCDAcceptanceDate
    $scope.open7 = function () {
        $scope.popup7.opened = true;
    };

    $scope.popup7 = {
        opened: false
    };

    // FPMCAcceptanceDate
    $scope.open8 = function () {
        $scope.popup8.opened = true;
    };

    $scope.popup8 = {
        opened: false
    };

    $scope.dateFormats = ['MM/dd/yyyy', 'dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.dateFormat = $scope.dateFormats[0];
    $scope.altInputFormats = ['M!/d!/yyyy'];

    // End Date Management
}]);