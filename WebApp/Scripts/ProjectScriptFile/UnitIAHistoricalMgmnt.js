app.controller("UnitIAHistoricalMgmnt", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data1b = {}; // Clients Information
    $scope.UnitIAHistoricalMgmnt = [];
    $scope.HolidayWeekenDays = [];
    $scope.HistoricalInfo = null;
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
        $scope.HistoricalInfo = null;
        $scope.UnitHistorical = null;
        $scope.UnitHistorical = null;
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
        $scope.HistoricalInfo = null;

        // make sure to remove query string if new search happen
        if (window.location.search.match("flx10ms="))
            window.history.replaceState(null, "", window.location.href.split("?")[0]);
    };

    //-----------------------------------------------------------
    // Set parameter for search field
    $scope.paramInfo = { ProjectID: '0', CompanyCode: '', ProjectCode: '', UnitNos: '', UnitCategory: '', CustomerNos: '', ProjectLocation: '', PageUrl: '' };

    // Set Button Controls
    $scope.TASFields = { TurnoverStatus: false, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: false, UnitAcceptanceDate: false, KeyTransmittalDate: false, ReinspectionDate: false, AdjReinspectionDate: false, RushTicketNos: false, SRRemarks: false }
    $scope.TASButtons = { AddButton: false,  CancelButton: false }

    $scope.DASFields = { DeemedAcceptanceDate: false, EmailDateNoticeSent: false, EmailNoticeRemarks: false, CourierDateNoticeSent: false, CourierDateNoticeReceived: false, CourierReceivedBy: false, CourierNoticeRemarks: false }
    $scope.DASButtons = { AddButton: false, CancelButton: false }
    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/UnitIAHistoricalMgmnt/GetSearchData';
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

        urlData = '../api/UnitIAHistoricalMgmnt/GetUnitIAHistoricalMgmnt';
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
                    
                    $scope.TurnoverStatus = response.data.TURNOVEROPTIONLIST;
                    $scope.PunchlistCategory = response.data.PUNCHLIST;
                    $scope.UnitHistorical = response.data.UNITHISTORICAL;
                    $scope.ClientInfo = response.data.CLIENTINFO;
                    $scope.HolidayWeekenDays = response.data.EXCEPTIONDAYS;
                    $scope.CurUser = response.data.CURUSER; 
                    $scope.SysParam = response.data.SYSPARAM;

                    $scope.totalItems = $scope.ClientInfo.length;

                    // Check if Turnover Schedule & Option has record

                    if ($scope.HistoricalInfo.FinalTurnoverDate !== null)
                        $scope.HistoricalInfo.FinalTurnoverDate = new Date($scope.HistoricalInfo.FinalTurnoverDate);

                    if ($scope.HistoricalInfo.FinalTurnoverTime !== null) 
                        $scope.HistoricalInfo.FinalTurnoverTime = new Date($scope.HistoricalInfo.FinalTurnoverTime);
     
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[Start Turnover & Acceptance Status Tab]------------------------------------------ //
                    // --------------------------------------------------------------------------------------------------------------------------------- //

                    // Check if Turnover & Acceptance Status has record
                    if ($scope.UnitHistorical && $scope.UnitHistorical.Id !== 0) {
                        if ($scope.UnitHistorical.TurnoverStatus !== null)
                            $scope.TASFields.TurnoverStatus = true;
                        else
                           $scope.TASFields.TurnoverStatus = false;

                        if ($scope.UnitHistorical.PunchlistCategory !== null)
                            $scope.TASFields.PunchlistCategory = true;
                        else
                            $scope.TASFields.PunchlistCategory = false;

                        if ($scope.UnitHistorical.PunchlistItems !== null)
                            $scope.TASFields.PunchlistItems = true;
                        else
                            $scope.TASFields.PunchlistItems = false;

                        if ($scope.UnitHistorical.OtherIssues !== null)
                            $scope.TASFields.OtherIssues = true;
                        else
                            $scope.TASFields.OtherIssues = false;

                        if ($scope.UnitHistorical.TSRemarks !== null)
                            $scope.TASFields.TSRemarks = true;
                        else
                            $scope.TASFields.TSRemarks = false;
                                               
                        if ($scope.UnitHistorical.UnitAcceptanceDate !== null) {
                            $scope.UnitHistorical.UnitAcceptanceDate = new Date($scope.UnitHistorical.UnitAcceptanceDate);
                            $scope.TASFields.UnitAcceptanceDate = true;
                        } else {
                            if ($scope.HistoricalInfo.SAPTurnoverDate !== null) {
                                $scope.TASFields.UnitAcceptanceDate = true;
                                $scope.UnitHistorical.UnitAcceptanceDate = new Date($scope.HistoricalInfo.SAPTurnoverDate);
                            } else 
                                $scope.TASFields.UnitAcceptanceDate = false;
                        }

                        if ($scope.UnitHistorical.KeyTransmittalDate !== null) {
                            $scope.UnitHistorical.KeyTransmittalDate = new Date($scope.UnitHistorical.KeyTransmittalDate);
                            $scope.TASFields.KeyTransmittalDate = true;
                        } else
                            $scope.TASFields.KeyTransmittalDate = false;

                        if ($scope.UnitHistorical.ReinspectionDate !== null) {
                            $scope.UnitHistorical.ReinspectionDate = new Date($scope.UnitHistorical.ReinspectionDate);
                            $scope.TASFields.ReinspectionDate = true;
                        } else
                            $scope.TASFields.ReinspectionDate = false;

                        if ($scope.UnitHistorical.AdjReinspectionDate !== null) {
                            $scope.UnitHistorical.AdjReinspectionDate = new Date($scope.UnitHistorical.AdjReinspectionDate);
                            $scope.TASFields.AdjReinspectionDate = true;
                        } else
                            $scope.TASFields.AdjReinspectionDate = false;

                        if ($scope.UnitHistorical.RushTicketNos !== null)
                            $scope.TASFields.RushTicketNos = true;
                        else
                            $scope.TASFields.RushTicketNos = false;

                        if ($scope.UnitHistorical.SRRemarks !== null)
                            $scope.TASFields.SRRemarks = true;
                        else
                            $scope.TASFields.SRRemarks = false;
             
                        if ($scope.UnitHistorical.DeemedAcceptanceDate !== null) {
                            $scope.DASFields.DeemedAcceptanceDate = true;
                            $scope.UnitHistorical.DeemedAcceptanceDate = new Date($scope.UnitHistorical.DeemedAcceptanceDate);
                        } else
                            $scope.DASFields.DeemedAcceptanceDate = false;

                        // Details: Email Notice
                        // Disable Email Date Notice Sent and Remarks if has value
                        if ($scope.UnitHistorical.DAEmailDateNoticeSent !== null) {
                            $scope.DASFields.DAEmailDateNoticeSent = true;
                            $scope.DASFields.DAEmailNoticeRemarks = true;
                            $scope.UnitHistorical.DAEmailDateNoticeSent = new Date($scope.UnitHistorical.DAEmailDateNoticeSent);
                        } else {
                            $scope.DASFields.DAEmailDateNoticeSent = false;
                            $scope.DASFields.DAEmailNoticeRemarks = false;
                        }

                        // Details: Notice thru Courier
                        // Disable Courier Date Notice Sent if has value
                        if ($scope.UnitHistorical.DACourierDateNoticeSent !== null) {
                            $scope.DASFields.DACourierDateNoticeSent = true;
                            $scope.UnitHistorical.DACourierDateNoticeSent = new Date($scope.UnitHistorical.DACourierDateNoticeSent);
                        } else
                            $scope.DASFields.DACourierDateNoticeSent = false;

                        // Disable Courier Date Notice Received if has value
                        if ($scope.UnitHistorical.DACourierDateNoticeReceived !== null) {
                            $scope.DASFields.DACourierDateNoticeReceived = true;
                            $scope.DASFields.DACourierNoticeRemarks = true;
                            $scope.UnitHistorical.DACourierDateNoticeReceived = new Date($scope.UnitHistorical.DACourierDateNoticeReceived);
                        } else {
                            $scope.DASFields.DACourierDateNoticeReceived = false;
                            $scope.DASFields.DACourierNoticeRemarks = false;
                        }

                        // Disable Courier Received By if has value
                        if ($scope.UnitHistorical.DACourierReceivedBy !== null)
                            $scope.DASFields.DACourierReceivedBy = true;
                        else
                            $scope.DASFields.DACourierReceivedBy = false;

                        // Disable Courier Notice Remarks field if all Details on Notice thru Courier has been filled out
                        if ($scope.UnitHistorical.DACourierDateNoticeSent !== null && $scope.UnitHistorical.DACourierDateNoticeReceived !== null && $scope.UnitHistorical.DACourierReceivedBy !== null)
                            $scope.DASFields.DACourierNoticeRemarks = true;
                        else
                            $scope.DASFields.DACourierNoticeRemarks = false;

                        if ($scope.UnitHistorical.DAHandoverAssociate === null)
                            $scope.UnitHistorical.DAHandoverAssociate = $scope.CurUser;

                    } else {
                        if ($scope.HistoricalInfo.SAPTurnoverDate !== null) {
                            $scope.UnitHistorical = { UnitAcceptanceDate: '' }
                            $scope.TASFields.UnitAcceptanceDate = true;
                            $scope.UnitHistorical.UnitAcceptanceDate = new Date($scope.HistoricalInfo.SAPTurnoverDate);
                        }
                    }
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[End Turnover & Acceptance Status Tab]-------------------------------------------- //
                    // --------------------------------------------------------------------------------------------------------------------------------- //

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

    // Reset Punchlist Category based on Turnover Status selection
    $scope.changeTOStatus = function () {
        $scope.UnitHistorical.PunchlistCategory = "";
        $scope.UnitHistorical.PunchlistItem = "";
        $scope.UnitHistorical.OtherIssues = "";

        if ($scope.HistoricalInfo.FinalTurnoverOption !== 'Express') {
            $scope.TASFields = { TurnoverStatus: false, PunchlistCategory: true, PunchlistItems: true, OtherIssues: true, TSRemarks: false }

            // Enable & Mandatory Punchlist Category & Items  if selected value on Turnover Status field is AWP or NAPL
            var turnoverstatus = ['AWP', 'NAPL'];
            if (turnoverstatus.indexOf($scope.UnitHistorical.TurnoverStatus) !== -1) {
                $scope.TASFields.PunchlistCategory = false;
                $scope.TASFields.PunchlistItems = false;
            }

            // Enable & Mandatory Other Issues if selected value on Turnover Status field is NAOI
            var turnoverstatus = ['NAOI'];
            if (turnoverstatus.indexOf($scope.UnitHistorical.TurnoverStatus) !== -1) {
                $scope.TASFields.OtherIssues = false;
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

        $rootScope.$emit("searchObjectType", 'UnitIAHistoricalMgmntController');
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

        data.CompanyCode = $scope.HistoricalInfo.CompanyCode;
        data.ProjectCode = $scope.HistoricalInfo.ProjectCode;
        data.UnitNos = $scope.HistoricalInfo.UnitNos;
        data.UnitCategory = $scope.HistoricalInfo.UnitCategoryCode;
        data.CustomerNos = $scope.HistoricalInfo.CustomerNos;
        data.Transaction = 'TurnoverAcceptanceStatus';

        urlData = '../api/UnitIAHistoricalMgmnt/SaveHistoricalData';
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

        data.CompanyCode = $scope.HistoricalInfo.CompanyCode;
        data.ProjectCode = $scope.HistoricalInfo.ProjectCode;
        data.UnitNos = $scope.HistoricalInfo.UnitNos;
        data.UnitCategory = $scope.HistoricalInfo.UnitCategoryCode;
        data.CustomerNos = $scope.HistoricalInfo.CustomerNos;
        data.Transaction = 'DeemedAcceptanceStatus';

        urlData = '../api/UnitIAHistoricalMgmnt/SaveHistoricalData';
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

    // Re-inspection Date
    $scope.open3 = function () {
        $scope.popup3.opened = true;
    };

    $scope.popup3 = {
        opened: false
    };

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

    // Courier Notice Received Date
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


