app.controller("TitlingStatus", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data1b = {}; // Clients Information
    $scope.TitleStatus = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, TaxDecNos: null, LiquidationEndorsedDate: null, LiquidationRushTicketNos: null, LiquidationEndorsedRemarks: null, TitleReleaseEndorsedDate: null, TitleReleaseRushTicketNos: null, TitleReleaseEndorsedRemarks: null, TitleLocationID: null, TitleNos: null, TitleRemarks: null, BankReleasedDate: null, BankReleasedRemarks: null, BuyerReleasedDate: null, BuyerReleasedRemarks: null, IsBankReleaseNA: null, IsBuyerReleaseNA: null, TitleStatusType: null, Remarks: null, ReasonForChange: null };
    $scope.DefaultTitleStatus = null;
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
        $scope.TitleStatus = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, TaxDecNos: null, LiquidationEndorsedDate: null, LiquidationRushTicketNos: null, LiquidationEndorsedRemarks: null, TitleReleaseEndorsedDate: null, TitleReleaseRushTicketNos: null, TitleReleaseEndorsedRemarks: null, TitleLocationID: null, TitleNos: null, TitleRemarks: null, BankReleasedDate: null, BankReleasedRemarks: null, BuyerReleasedDate: null, BuyerReleasedRemarks: null, IsBankReleaseNA: null, IsBuyerReleaseNA: null, TitleStatusType: null, Remarks: null, ReasonForChange: null };
        $scope.DefaultTitleStatus = $scope.TitleStatus;

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

        $('#custom-content-below-tab li:first-child a').tab('show') // Select first tab

        $scope.DASFields = { TaxDecNos: false }
        $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }

        $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
        $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }

        $scope.RASFields = { TitleLocationID: false, TitleNos: false, TitleRemarks: false, BankReleasedDate: false, BankReleasedRemarks: false, BuyerReleasedDate: false, BuyerReleasedRemarks: false, IsBankReleaseNA: null, IsBuyerReleaseNA: null }
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
    $scope.DASFields = { TaxDecNos: false }
    $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }

    $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
    $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }

    $scope.RASFields = { TitleLocationID: false, TitleNos: false, TitleRemarks: false, BankReleasedDate: false, BankReleasedRemarks: false, BuyerReleasedDate: false, BuyerReleasedRemarks: false, IsBankReleaseNA: null, IsBuyerReleaseNA: null }
    $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }

    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/TitlingStatus/GetSearchData';
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

        $('#custom-content-below-tab li:first-child a').tab('show') // Select first tab
        
        $scope.DASFields = { TaxDecNos: false }
        $scope.DASButtons = { AddButton: true, EditButton: true, CancelButton: true }

        $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
        $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

        $scope.RASFields = { TitleLocationID: false, TitleNos: false, TitleRemarks: false, BankReleasedDate: false, BankReleasedRemarks: false, BuyerReleasedDate: false, BuyerReleasedRemarks: false, IsBankReleaseNA: null, IsBuyerReleaseNA: null }
        $scope.RASButtons = { AddButton: true, EditButton: true, CancelButton: true }

        $scope.ShowReasonChange = false;

        if ($scope.paramInfo.CompanyCode === '0' || $scope.paramInfo.ProjectCode === '' ||  ($scope.paramInfo.UnitNos === '' && $scope.paramInfo.CustomerNos === '')) {
            swal(
                'Error Message',
                'Unit has NO or inactive Customer Number',
                'error'
            );
            return false;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/TitlingStatus/GetTitlingStatus';
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
                    $scope.TitleStatus = response.data.TITLESTATUS;
                    $scope.DefaultTitleStatus = response.data.DEFTITLESTATUS;
                    $scope.HolidayWeekenDays = response.data.EXCEPTIONDAYS;
                    $scope.CurUser = response.data.CURUSER; 
                    $scope.SysParam = response.data.SYSPARAM;
                    
                    if ($scope.SalesInfo.TitleInProcessDate !== null)
                        $scope.SalesInfo.TitleInProcessDate = new Date($scope.SalesInfo.TitleInProcessDate);

                    if ($scope.SalesInfo.TitleTransferredDate !== null)
                        $scope.SalesInfo.TitleTransferredDate = new Date($scope.SalesInfo.TitleTransferredDate);

                    if ($scope.SalesInfo.TitleClaimedDate !== null)
                        $scope.SalesInfo.TitleClaimedDate = new Date($scope.SalesInfo.TitleClaimedDate);

                    if ($scope.SalesInfo.TaxDeclarationTransferredDate !== null)
                        $scope.SalesInfo.TaxDeclarationTransferredDate = new Date($scope.SalesInfo.TaxDeclarationTransferredDate);

                    if ($scope.SalesInfo.TaxDeclarationClaimedDate !== null)
                        $scope.SalesInfo.TaxDeclarationClaimedDate = new Date($scope.SalesInfo.TaxDeclarationClaimedDate);
                         
                    $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                    $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                    $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                    
                    // Min date for Endorsed Liquidation Date
                    if ($scope.SalesInfo.TaxDeclarationTransferredDate !== null) {
                        $scope.dateEndorseLiquidation.minDate = new Date($scope.SalesInfo.TaxDeclarationTransferredDate);
                    }
                    // --------------------------------------------------------------------------------------------------------------------------------- //
                    // -----------------------------------------------[Start Title Transfer and Tax Dec Tab]------------------------------------------ //
                    // --------------------------------------------------------------------------------------------------------------------------------- //

                    // Check if Turnover & Acceptance Status has record
                    if ($scope.TitleStatus && $scope.TitleStatus.Id !== 0) {
                        if ($scope.TitleStatus.LiquidationEndorsedDate !== null) {
                            $scope.TitleStatus.LiquidationEndorsedDate = new Date($scope.TitleStatus.LiquidationEndorsedDate);
                            $scope.DefaultTitleStatus.LiquidationEndorsedDate = new Date($scope.DefaultTitleStatus.LiquidationEndorsedDate);
                                
                            // Min date for Title Released Endorse
                            $scope.dateTitleReleaseEndorse.minDate = new Date($scope.TitleStatus.LiquidationEndorsedDate);
                        }

                        if ($scope.TitleStatus.TitleReleaseEndorsedDate !== null) {
                            $scope.TitleStatus.TitleReleaseEndorsedDate = new Date($scope.TitleStatus.TitleReleaseEndorsedDate);
                            $scope.DefaultTitleStatus.TitleReleaseEndorsedDate = new Date($scope.DefaultTitleStatus.TitleReleaseEndorsedDate);

                            // Min date for Released to Bank & Buyer
                            //$scope.dateTitleReleaseBuyerBank.minDate = new Date($scope.TitleStatus.TitleReleaseEndorsedDate);
                            //$scope.dateTitleReleaseBuyerBank.minDate = new Date($scope.TitleStatus.TitleReleaseEndorsedDate);
                        }

                        if ($scope.TitleStatus.BankReleasedDate !== null) {
                            $scope.DefaultTitleStatus.BankReleasedDate = new Date($scope.TitleStatus.BankReleasedDate);
                            //$scope.DefaultTitleStatus.BankReleasedDate = new Date($scope.DefaultTitleStatus.BankReleasedDate);
                        }

                        if ($scope.TitleStatus.BuyerReleasedDate !== null) {
                            $scope.DefaultTitleStatus.BuyerReleasedDate = new Date($scope.TitleStatus.BuyerReleasedDate);
                            //$scope.DefaultTitleStatus.BuyerReleasedDate = new Date($scope.DefaultTitleStatus.BuyerReleasedDate);
                        }

                        // Check Remarks Field
                        if ($scope.TitleStatus.TitleStatusType === null)
                            $scope.TitleStatus.TitleStatusType = "";

                        if ($scope.TitleStatus.Remarks === null)
                            $scope.TitleStatus.Remarks = "";

                        //---------------------------------- Tab Title Status
                        if ($scope.DefaultTitleStatus.TaxDecNos !== null)
                            $scope.DASFields.TaxDecNos = false;
                        else
                            $scope.DASFields.TaxDecNos = true;

                        // check Title Status
                        if ($scope.DefaultTitleStatus.TaxDecNos !== null) {
                            $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                        } else if ($scope.DefaultTitleStatus.TaxDecNos === null) {
                            $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                        } else {
                            $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                        }

                        //---------------------------------- Tab Title Endrosement Status
                        if ($scope.DefaultTitleStatus.LiquidationEndorsedDate !== null)
                            $scope.TASFields.LiquidationEndorsedDate = false;
                        else
                            $scope.TASFields.LiquidationEndorsedDate = true;

                        if ($scope.DefaultTitleStatus.LiquidationRushTicketNos !== null || $scope.DefaultTitleStatus.LiquidationEndorsedDate === null)
                            $scope.TASFields.LiquidationRushTicketNos = false;
                        else
                            $scope.TASFields.LiquidationRushTicketNos = true;

                        if ($scope.DefaultTitleStatus.LiquidationEndorsedRemarks !== null || $scope.DefaultTitleStatus.LiquidationEndorsedDate === null)
                            $scope.TASFields.LiquidationEndorsedRemarks = false;
                        else
                            $scope.TASFields.LiquidationEndorsedRemarks = true;

                        if ($scope.DefaultTitleStatus.TitleReleaseEndorsedDate !== null || $scope.DefaultTitleStatus.LiquidationRushTicketNos === null)
                            $scope.TASFields.TitleReleaseEndorsedDate = false;
                        else
                            $scope.TASFields.TitleReleaseEndorsedDate = true;

                        if ($scope.DefaultTitleStatus.TitleReleaseRushTicketNos !== null || $scope.DefaultTitleStatus.TitleReleaseEndorsedDate === null)
                            $scope.TASFields.TitleReleaseRushTicketNos = false;
                        else
                            $scope.TASFields.TitleReleaseRushTicketNos = true;

                        if ($scope.DefaultTitleStatus.TitleReleaseEndorsedRemarks !== null || $scope.DefaultTitleStatus.TitleReleaseEndorsedDate === null)
                            $scope.TASFields.TitleReleaseEndorsedRemarks = false;
                        else
                            $scope.TASFields.TitleReleaseEndorsedRemarks = true;

                        // check Title Endrosement Button Controls Status
                        if ($scope.DefaultTitleStatus.LiquidationEndorsedDate !== null && $scope.DefaultTitleStatus.LiquidationRushTicketNos !== null && $scope.DefaultTitleStatus.LiquidationEndorsedRemarks !== null &&
                            $scope.DefaultTitleStatus.TitleReleaseEndorsedDate !== null && $scope.DefaultTitleStatus.TitleReleaseRushTicketNos !== null && $scope.DefaultTitleStatus.TitleReleaseEndorsedRemarks !== null) {

                            $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                        } else if ($scope.DefaultTitleStatus.LiquidationEndorsedDate === null && $scope.DefaultTitleStatus.LiquidationRushTicketNos === null && $scope.DefaultTitleStatus.LiquidationEndorsedRemarks === null &&
                            $scope.DefaultTitleStatus.TitleReleaseEndorsedDate === null && $scope.DefaultTitleStatus.TitleReleaseRushTicketNos === null && $scope.DefaultTitleStatus.TitleReleaseEndorsedRemarks === null) {

                            $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                        } else {
                            $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                        }

                        //---------------------------------- Tab Title Details and Release Endrosement Status
                        if ($scope.DefaultTitleStatus.TitleLocationID !== null)
                            $scope.RASFields.TitleLocationID = false;
                        else
                            $scope.RASFields.TitleLocationID = true;

                        if ($scope.DefaultTitleStatus.TitleNos !== null)
                            $scope.RASFields.TitleNos = false;
                        else
                            $scope.RASFields.TitleNos = true;

                        if ($scope.DefaultTitleStatus.TitleRemarks !== null)
                            $scope.RASFields.TitleRemarks = false;
                        else
                            $scope.RASFields.TitleRemarks = true;

                        if ($scope.DefaultTitleStatus.BankReleasedDate !== null || $scope.DefaultTitleStatus.IsBankReleaseNA === true)
                            $scope.RASFields.BankReleasedDate = false;
                        else
                            $scope.RASFields.BankReleasedDate = true;

                        if ($scope.DefaultTitleStatus.BankReleasedRemarks !== null)
                            $scope.RASFields.BankReleasedRemarks = false;
                        else
                            $scope.RASFields.BankReleasedRemarks = true;

                        if ($scope.DefaultTitleStatus.BuyerReleasedDate !== null || $scope.DefaultTitleStatus.IsBuyerReleaseNA === true)
                            $scope.RASFields.BuyerReleasedDate = false;
                        else
                            $scope.RASFields.BuyerReleasedDate = true;

                        if ($scope.DefaultTitleStatus.BuyerReleasedRemarks !== null)
                            $scope.RASFields.BuyerReleasedRemarks = false;
                        else
                            $scope.RASFields.BuyerReleasedRemarks = true;

                        if ($scope.DefaultTitleStatus.IsBuyerReleaseNA !== null)
                            $scope.RASFields.IsBuyerReleaseNA = false;
                        else
                            $scope.RASFields.IsBuyerReleaseNA = true;

                        if ($scope.DefaultTitleStatus.IsBankReleaseNA !== null)
                            $scope.RASFields.IsBankReleaseNA = false;
                        else
                            $scope.RASFields.IsBankReleaseNA = true;

                        // check Title Details and Release Endrosement Button Controls Status
                        if ($scope.DefaultTitleStatus.TitleLocationID !== null && $scope.DefaultTitleStatus.TitleNos !== null && $scope.DefaultTitleStatus.TitleRemarks !== null &&
                         $scope.DefaultTitleStatus.IsBankReleaseNA !== null && $scope.DefaultTitleStatus.IsBuyerReleaseNA !== null &&
                         $scope.DefaultTitleStatus.BankReleasedDate !== null && $scope.DefaultTitleStatus.BankReleasedRemarks !== null &&
                         $scope.DefaultTitleStatus.BuyerReleasedDate !== null && $scope.DefaultTitleStatus.BuyerReleasedRemarks !== null) {

                            $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                        } else if ($scope.DefaultTitleStatus.TitleLocationID === null && $scope.DefaultTitleStatus.TitleNos === null && $scope.DefaultTitleStatus.TitleRemarks === null &&
                                    $scope.DefaultTitleStatus.IsBankReleaseNA === null && $scope.DefaultTitleStatus.IsBuyerReleaseNA === null &&
                                    $scope.DefaultTitleStatus.BankReleasedDate === null && $scope.DefaultTitleStatus.BankReleasedRemarks === null &&
                                    $scope.DefaultTitleStatus.BuyerReleasedDate === null && $scope.DefaultTitleStatus.BuyerReleasedRemarks === null) {
                            $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                        } else {
                            $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                        }

                        // wait unit variable tower populated
                        $scope.$watch("DefaultTitleStatus", function (item) {
                            if (item) {
                                $scope.ResetTitleStatusFields();
                            }
                        }, true);
                    } else {
                        // Check if Tab Title Status
                        $scope.TitleStatus = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, TaxDecNos: null, LiquidationEndorsedDate: null, LiquidationRushTicketNos: null, LiquidationEndorsedRemarks: null, TitleReleaseEndorsedDate: null, TitleReleaseRushTicketNos: null, TitleReleaseEndorsedRemarks: null, TitleLocationID: null, TitleNos: null, TitleRemarks: null, BankReleasedDate: null, BankReleasedRemarks: null, BuyerReleasedDate: null, BuyerReleasedRemarks: null, IsBankReleaseNA: null, IsBuyerReleaseNA: null, TitleStatusType: null, Remarks: null, ReasonForChange: null };

                        // Default for Title Status Fields
                        $scope.DASFields = { TaxDecNos: true }
                        $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }

                        // Default for Title Endorsement Status Fields
                        $scope.TASFields = { LiquidationEndorsedDate: true, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
                        $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }

                        // Default for Title Details and Release Endrosement Fields
                        $scope.RASFields = { TitleLocationID: true, TitleNos: true, TitleRemarks: true, BankReleasedDate: true, BankReleasedRemarks: true, BuyerReleasedDate: true, BuyerReleasedRemarks: true, IsBankReleaseNA: true, IsBuyerReleaseNA: true }
                        $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                    }
                } else {
                    // Check if Tab Title Status
                    $scope.TitleStatus = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, TaxDecNos: null, LiquidationEndorsedDate: null, LiquidationRushTicketNos: null, LiquidationEndorsedRemarks: null, TitleReleaseEndorsedDate: null, TitleReleaseRushTicketNos: null, TitleReleaseEndorsedRemarks: null, TitleLocationID: null, TitleNos: null, TitleRemarks: null, BankReleasedDate: null, BankReleasedRemarks: null, BuyerReleasedDate: null, BuyerReleasedRemarks: null, IsBankReleaseNA: null, IsBuyerReleaseNA: null, TitleStatusType: null, Remarks: null, ReasonForChange: null };

                    // Default for Title Status Fields
                    $scope.DASFields = { TaxDecNos: false }
                    $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }

                    // Default for Title Endorsement Status Fields
                    $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
                    $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }

                    // Default for Title Details and Release Endrosement Fields
                    $scope.RASFields = { TitleLocationID: false, TitleNos: false, TitleRemarks: false, BankReleasedDate: false, BankReleasedRemarks: false, BuyerReleasedDate: false, BuyerReleasedRemarks: false, IsBankReleaseNA: false, IsBuyerReleaseNA: false }
                    $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
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

    //// -------------------------------------- Title Transfer and Tax Dec
    $scope.SaveTitlingStatusModal = function (type) {
        $scope.$broadcast('show-errors-event');
        if ($scope.titleTransferTaxForm.$invalid) {
            if ($scope.titleTransferTaxForm.$error['dateDisabled'] && $scope.titleTransferTaxForm.$error['dateDisabled'].length) {
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
            $scope.TitleStatus.Transaction = type;
            $('#applyChangesModal1a').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }  

    // -------------------------------------- Title Endorsement Status
    $scope.CheckTitleEndorsement = function () {
        $scope.ResetTitleStatusFields();

        if (!$scope.SalesInfo) {
            $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }
            return;
        }

        if ($scope.SalesInfo.TaxDeclarationTransferredDate == null) {
            swal(
                'Alert',
                'Tax Declaration Transferred Date must be posted on SAP to be able to proceed with this transaction.',
                'error'
            );
            $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
            $scope.TASButtons = { AddButton: true, EditButton: true, CancelButton: true }

            return;
        }
    }

    $scope.SaveTitleEndorsementModal = function (type) {
        $scope.$broadcast('show-errors-event');
        if ($scope.titleendorsementForm.$invalid) {
            if ($scope.titleendorsementForm.$error['dateDisabled'] && $scope.titleendorsementForm.$error['dateDisabled'].length) {
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
            if ($scope.TitleStatus.LiquidationEndorsedDate > $scope.TitleStatus.TitleReleaseEndorsedDate && $scope.TitleStatus.TitleReleaseEndorsedDate != null) {
                swal(
                    'Error Message',
                    'Liquidation Endorsed Date should not be greater thand Title Release Endorsed Date',
                    'warning'
                );
                return;
            } else {
                $scope.TitleStatus.Transaction = type;
                $('#applyChangesModal1a').modal({
                    backdrop: 'static',
                    keyboard: false,
                    show: true
                });
            }
        }
    }
        
    // -------------------------------------- Title Details and Release Status    
    $scope.SaveTitlingDetailsReleaseModal = function (type) {
        $scope.$broadcast('show-errors-event');
        if ($scope.titlereleaseForm.$invalid) {
            if ($scope.titlereleaseForm.$error['dateDisabled'] && $scope.titlereleaseForm.$error['dateDisabled'].length) {
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
            $scope.TitleStatus.Transaction = type;
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
        $scope.ResetTitleStatusFields();
        $('#applyChangesModal1b').modal('hide');
    };
    // -------------------------------------- End Reset Cancel Button Actions -------------------------------------- //

    // -------------------------------------- Start Reset Title Status Fields on Button Actions -------------------------------------- //
    $scope.EnableTitleStatusAction = function () {
        $scope.TitleStatus.ReasonForChange = "";
        $scope.ShowReasonChange = true;

        if ($scope.DefaultTitleStatus) {
            if ($scope.DefaultTitleStatus.Id !== 0) {
                angular.forEach($scope.DefaultTitleStatus, function (value, key) {
                    $scope.TitleStatus[key] = value;
                });
                //---------------------------------- Tab Title Status
                if ($scope.DefaultTitleStatus.TaxDecNos !== null)
                    $scope.DASFields.TaxDecNos = true;
                else
                    $scope.DASFields.TaxDecNos = false;

                //---------------------------------- Tab Title Endrosement Status
                if ($scope.DefaultTitleStatus.LiquidationEndorsedDate !== null)
                    $scope.TASFields.LiquidationEndorsedDate = true;
                else
                    $scope.TASFields.LiquidationEndorsedDate = false;

                if ($scope.DefaultTitleStatus.LiquidationRushTicketNos !== null && $scope.DefaultTitleStatus.LiquidationEndorsedDate !== null)
                    $scope.TASFields.LiquidationRushTicketNos = true;
                else
                    $scope.TASFields.LiquidationRushTicketNos = false;

                if ($scope.DefaultTitleStatus.LiquidationEndorsedRemarks !== null && $scope.DefaultTitleStatus.LiquidationEndorsedDate !== null)
                    $scope.TASFields.LiquidationEndorsedRemarks = true;
                else
                    $scope.TASFields.LiquidationEndorsedRemarks = false;

                if ($scope.DefaultTitleStatus.TitleReleaseEndorsedDate !== null && $scope.DefaultTitleStatus.LiquidationRushTicketNos !== null)
                    $scope.TASFields.TitleReleaseEndorsedDate = true;
                else
                    $scope.TASFields.TitleReleaseEndorsedDate = false;

                if ($scope.DefaultTitleStatus.TitleReleaseRushTicketNos !== null && $scope.DefaultTitleStatus.TitleReleaseEndorsedDate !== null)
                    $scope.TASFields.TitleReleaseRushTicketNos = true;
                else
                    $scope.TASFields.TitleReleaseRushTicketNos = false;

                if ($scope.DefaultTitleStatus.TitleReleaseEndorsedRemarks !== null && $scope.DefaultTitleStatus.TitleReleaseEndorsedDate !== null)
                    $scope.TASFields.TitleReleaseEndorsedRemarks = true;
                else
                    $scope.TASFields.TitleReleaseEndorsedRemarks = false;

                // Check if Tab Title Endrosement Status
                if ($scope.DefaultTitleStatus.TitleLocationID !== null)
                    $scope.RASFields.TitleLocationID = true;
                else
                    $scope.RASFields.TitleLocationID = false;

                if ($scope.DefaultTitleStatus.TitleNos !== null)
                    $scope.RASFields.TitleNos = true;
                else
                    $scope.RASFields.TitleNos = false;

                if ($scope.DefaultTitleStatus.TitleRemarks !== null)
                    $scope.RASFields.TitleRemarks = true;
                else
                    $scope.RASFields.TitleRemarks = false;

                if ($scope.DefaultTitleStatus.BankReleasedDate !== null)
                    $scope.RASFields.BankReleasedDate = true;
                else
                    $scope.RASFields.BankReleasedDate = false;

                if ($scope.DefaultTitleStatus.BankReleasedRemarks !== null)
                    $scope.RASFields.BankReleasedRemarks = true;
                else
                    $scope.RASFields.BankReleasedRemarks = false;

                if ($scope.DefaultTitleStatus.BuyerReleasedDate !== null)
                    $scope.RASFields.BuyerReleasedDate = true;
                else
                    $scope.RASFields.BuyerReleasedDate = false;

                if ($scope.DefaultTitleStatus.BuyerReleasedRemarks !== null)
                    $scope.RASFields.BuyerReleasedRemarks = true;
                else
                    $scope.RASFields.BuyerReleasedRemarks = false;
                
                if ($scope.DefaultTitleStatus.IsBuyerReleaseNA !== null)
                    $scope.RASFields.IsBuyerReleaseNA = true;
                else
                    $scope.RASFields.IsBuyerReleaseNA = false;

                if ($scope.DefaultTitleStatus.IsBankReleaseNA !== null)
                    $scope.RASFields.IsBankReleaseNA = true;
                else
                    $scope.RASFields.IsBankReleaseNA = false;

                $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
            } else {
                $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }

                $scope.RASFields.IsBuyerReleaseNA = false;
                $scope.RASFields.IsBankReleaseNA = false;
            }
        } else {
            $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
            $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
            $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }

            $scope.RASFields.IsBuyerReleaseNA = false;
            $scope.RASFields.IsBankReleaseNA = false;
        }
    };
    // -------------------------------------- End Reset Title Status Fields on Button Actions -------------------------------------- //

    // -------------------------------------- Start Reset Title Status Fields -------------------------------------- //
    $scope.ResetTitleStatusFields = function () {
        $scope.TitleStatus.ReasonForChange = "";
        $scope.ShowReasonChange = false;

        if ($scope.SalesInfo != null) {
            if ($scope.DefaultTitleStatus && $scope.DefaultTitleStatus != null && $scope.DefaultTitleStatus.Id !== 0) {                
                angular.forEach($scope.DefaultTitleStatus, function (value, key) {
                    $scope.TitleStatus[key] = value;
                });

                // Check if Tab Title Status
                if ($scope.DefaultTitleStatus.TaxDecNos !== null)
                    $scope.DASFields.TaxDecNos = false;
                else
                    $scope.DASFields.TaxDecNos = true;

                if ($scope.DefaultTitleStatus.TaxDecNos !== null) {
                    $scope.DASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                } else if ($scope.DefaultTitleStatus.TaxDecNos === null) {
                    $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                } else {
                    $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                }

                // Check if Tab Title Endrosement Status
                if ($scope.DefaultTitleStatus.LiquidationEndorsedDate !== null)
                    $scope.TASFields.LiquidationEndorsedDate = false;
                else
                    $scope.TASFields.LiquidationEndorsedDate = true;

                if ($scope.DefaultTitleStatus.LiquidationRushTicketNos !== null || $scope.DefaultTitleStatus.LiquidationEndorsedDate === null)
                    $scope.TASFields.LiquidationRushTicketNos = false;
                else
                    $scope.TASFields.LiquidationRushTicketNos = true;

                if ($scope.DefaultTitleStatus.LiquidationEndorsedRemarks !== null || $scope.DefaultTitleStatus.LiquidationEndorsedDate === null)
                    $scope.TASFields.LiquidationEndorsedRemarks = false;
                else
                    $scope.TASFields.LiquidationEndorsedRemarks = true;

                if ($scope.DefaultTitleStatus.TitleReleaseEndorsedDate !== null || $scope.DefaultTitleStatus.LiquidationRushTicketNos === null)
                    $scope.TASFields.TitleReleaseEndorsedDate = false;
                else
                    $scope.TASFields.TitleReleaseEndorsedDate = true;

                if ($scope.DefaultTitleStatus.TitleReleaseRushTicketNos !== null || $scope.DefaultTitleStatus.TitleReleaseEndorsedDate === null)
                    $scope.TASFields.TitleReleaseRushTicketNos = false;
                else
                    $scope.TASFields.TitleReleaseRushTicketNos = true;

                if ($scope.DefaultTitleStatus.TitleReleaseEndorsedRemarks !== null || $scope.DefaultTitleStatus.TitleReleaseEndorsedDate === null)
                    $scope.TASFields.TitleReleaseEndorsedRemarks = false;
                else
                    $scope.TASFields.TitleReleaseEndorsedRemarks = true;

                if ($scope.DefaultTitleStatus.LiquidationEndorsedDate !== null && $scope.DefaultTitleStatus.LiquidationRushTicketNos !== null && $scope.DefaultTitleStatus.LiquidationEndorsedRemarks !== null &&
                    $scope.DefaultTitleStatus.TitleReleaseEndorsedDate !== null && $scope.DefaultTitleStatus.TitleReleaseRushTicketNos !== null && $scope.DefaultTitleStatus.TitleReleaseEndorsedRemarks !== null) {

                    $scope.TASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                } else if ($scope.DefaultTitleStatus.LiquidationEndorsedDate === null && $scope.DefaultTitleStatus.LiquidationRushTicketNos === null && $scope.DefaultTitleStatus.LiquidationEndorsedRemarks === null &&
                            $scope.DefaultTitleStatus.TitleReleaseEndorsedDate === null && $scope.DefaultTitleStatus.TitleReleaseRushTicketNos === null && $scope.DefaultTitleStatus.TitleReleaseEndorsedRemarks === null) {

                    $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                } else {
                    $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                }

                // Check if Tab Title Details and Release Status
                if ($scope.DefaultTitleStatus.TitleLocationID !== null)
                    $scope.RASFields.TitleLocationID = false;
                else
                    $scope.RASFields.TitleLocationID = true;

                if ($scope.DefaultTitleStatus.TitleNos !== null)
                    $scope.RASFields.TitleNos = false;
                else
                    $scope.RASFields.TitleNos = true;

                if ($scope.DefaultTitleStatus.TitleRemarks !== null)
                    $scope.RASFields.TitleRemarks = false;
                else
                    $scope.RASFields.TitleRemarks = true;

                if ($scope.DefaultTitleStatus.BankReleasedDate !== null || $scope.DefaultTitleStatus.IsBankReleaseNA === true)
                    $scope.RASFields.BankReleasedDate = false;
                else
                    $scope.RASFields.BankReleasedDate = true;

                if ($scope.DefaultTitleStatus.BankReleasedRemarks !== null)
                    $scope.RASFields.BankReleasedRemarks = false;
                else
                    $scope.RASFields.BankReleasedRemarks = true;

                if ($scope.DefaultTitleStatus.BuyerReleasedDate !== null || $scope.DefaultTitleStatus.IsBuyerReleaseNA === true)
                    $scope.RASFields.BuyerReleasedDate = false;
                else
                    $scope.RASFields.BuyerReleasedDate = true;

                if ($scope.DefaultTitleStatus.BuyerReleasedRemarks !== null)
                    $scope.RASFields.BuyerReleasedRemarks = false;
                else
                    $scope.RASFields.BuyerReleasedRemarks = true;

                if ($scope.DefaultTitleStatus.IsBuyerReleaseNA !== null)
                    $scope.RASFields.IsBuyerReleaseNA = false;
                else
                    $scope.RASFields.IsBuyerReleaseNA = true;

                if ($scope.DefaultTitleStatus.IsBankReleaseNA !== null)
                    $scope.RASFields.IsBankReleaseNA = false;
                else
                    $scope.RASFields.IsBankReleaseNA = true;

                if ($scope.DefaultTitleStatus.TitleRemarks !== null)
                    $scope.RASFields.TitleRemarks = false;
                else
                    $scope.RASFields.TitleRemarks = true;

                //if ($scope.DefaultTitleStatus.TitleLocationID !== null && $scope.DefaultTitleStatus.TitleNos !== null && $scope.DefaultTitleStatus.TitleRemarks !== null &&
                //    (($scope.DefaultTitleStatus.IsBankReleaseNA === true) || ($scope.DefaultTitleStatus.IsBankReleaseNA === false && $scope.DefaultTitleStatus.BankReleasedDate !== null && $scope.DefaultTitleStatus.BankReleasedRemarks !== null)) &&
                //    (($scope.DefaultTitleStatus.IsBuyerReleaseNA === true) || ($scope.DefaultTitleStatus.IsBuyerReleaseNA === false && $scope.DefaultTitleStatus.BuyerReleasedDate !== null && $scope.DefaultTitleStatus.BuyerReleasedRemarks !== null))) {
                //} else if ($scope.DefaultTitleStatus.TitleLocationID === null && $scope.DefaultTitleStatus.TitleNos === null && $scope.DefaultTitleStatus.TitleRemarks === null &&
                //         $scope.DefaultTitleStatus.IsBankReleaseNA === null && $scope.DefaultTitleStatus.IsBuyerReleaseNA === null) {

                if ($scope.DefaultTitleStatus.TitleLocationID !== null && $scope.DefaultTitleStatus.TitleNos !== null && $scope.DefaultTitleStatus.TitleRemarks !== null &&
                    $scope.DefaultTitleStatus.IsBankReleaseNA !== null && $scope.DefaultTitleStatus.IsBuyerReleaseNA !== null &&
                    $scope.DefaultTitleStatus.BankReleasedDate !== null && $scope.DefaultTitleStatus.BankReleasedRemarks !== null &&
                    $scope.DefaultTitleStatus.BuyerReleasedDate !== null && $scope.DefaultTitleStatus.BuyerReleasedRemarks !== null) {

                    $scope.RASButtons = { AddButton: true, EditButton: false, CancelButton: false }
                } else if ($scope.DefaultTitleStatus.TitleLocationID === null && $scope.DefaultTitleStatus.TitleNos === null && $scope.DefaultTitleStatus.TitleRemarks === null &&
                            $scope.DefaultTitleStatus.IsBankReleaseNA === null && $scope.DefaultTitleStatus.IsBuyerReleaseNA === null &&
                            $scope.DefaultTitleStatus.BankReleasedDate === null && $scope.DefaultTitleStatus.BankReleasedRemarks === null &&
                            $scope.DefaultTitleStatus.BuyerReleasedDate === null && $scope.DefaultTitleStatus.BuyerReleasedRemarks === null) {
                    $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
                } else {
                    $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
                }
            } else {
                // Check if Tab Title Status
                $scope.TitleStatus = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, TaxDecNos: null, LiquidationEndorsedDate: null, LiquidationRushTicketNos: null, LiquidationEndorsedRemarks: null, TitleReleaseEndorsedDate: null, TitleReleaseRushTicketNos: null, TitleReleaseEndorsedRemarks: null, TitleLocationID: null, TitleNos: null, TitleRemarks: null, BankReleasedDate: null, BankReleasedRemarks: null, BuyerReleasedDate: null, BuyerReleasedRemarks: null, IsBankReleaseNA: null, IsBuyerReleaseNA: null, TitleStatusType: null, Remarks: null, ReasonForChange: null };

                // Default for Title Status Fields
                $scope.DASFields = { TaxDecNos: true }
                $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }

                // Default for Title Endorsement Status Fields
                $scope.TASFields = { LiquidationEndorsedDate: true, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
                $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }

                // Default for Title Details and Release Endrosement Fields
                $scope.RASFields = { TitleLocationID: true, TitleNos: true, TitleRemarks: true, BankReleasedDate: true, BankReleasedRemarks: true, BuyerReleasedDate: true, BuyerReleasedRemarks: true, IsBankReleaseNA: true, IsBuyerReleaseNA: true }
                $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }
            }
        } else {
            // Check if Tab Title Status
            $scope.TitleStatus = { Id: 0, CompanyCode: null, ProjectCode: null, UnitNos: null, UnitCategory: null, CustomerNos: null, QuotDocNos: null, SalesDocNos: null, TaxDecNos: null, LiquidationEndorsedDate: null, LiquidationRushTicketNos: null, LiquidationEndorsedRemarks: null, TitleReleaseEndorsedDate: null, TitleReleaseRushTicketNos: null, TitleReleaseEndorsedRemarks: null, TitleLocationID: null, TitleNos: null, TitleRemarks: null, BankReleasedDate: null, BankReleasedRemarks: null, BuyerReleasedDate: null, BuyerReleasedRemarks: null, IsBankReleaseNA: null, IsBuyerReleaseNA: null, TitleStatusType: null, Remarks: null, ReasonForChange: null };
           
            // Default for Title Status Fields
            $scope.DASFields = { TaxDecNos: false }
            $scope.DASButtons = { AddButton: false, EditButton: false, CancelButton: false }

            // Default for Title Endorsement Status Fields
            $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
            $scope.TASButtons = { AddButton: false, EditButton: false, CancelButton: false }

            // Default for Title Details and Release Endrosement Fields
            $scope.RASFields = { TitleLocationID: false, TitleNos: false, TitleRemarks: false, BankReleasedDate: false, BankReleasedRemarks: false, BuyerReleasedDate: false, BuyerReleasedRemarks: false, IsBankReleaseNA: false, IsBuyerReleaseNA: false }
            $scope.RASButtons = { AddButton: false, EditButton: false, CancelButton: false }
        }
    };
    // -------------------------------------- End Reset Title Status Fields -------------------------------------- //

    // -------------------------------------- Start Enable Field from Title Status -------------------------------------- //
    $scope.EnableLiquidationField = function (data) {
        $scope.TASFields.TitleReleaseEndorsedDate = false;
        $scope.TASFields.TitleReleaseRushTicketNos = false;
        $scope.TASFields.TitleReleaseEndorsedRemarks = false;
        $scope.TASFields.LiquidationRushTicketNos = false;
        $scope.TASFields.LiquidationEndorsedRemarks = false;

        if (data != null) {
            $scope.TASFields.TitleReleaseEndorsedDate = true;
            $scope.TASFields.TitleReleaseRushTicketNos = true;
            $scope.TASFields.TitleReleaseEndorsedRemarks = true;
            $scope.TASFields.LiquidationRushTicketNos = true;
            $scope.TASFields.LiquidationEndorsedRemarks = true;
        }

        if ($scope.TitleStatus.LiquidationEndorsedDate > $scope.TitleStatus.TitleReleaseEndorsedDate && $scope.TitleStatus.TitleReleaseEndorsedDate != null) {
            swal(
                'Error Message',
                'Liquidation Endorsed Date should not be greater thand Title Release Endorsed Date',
                'warning'
            );
            $scope.TitleStatus.LiquidationEndorsedDate = null;
            $scope.TASButtons.AddButton = true;
            return;
        }
        else {
            $scope.TASButtons.AddButton = false;
        }
    }

    $scope.EnableTitleReleaseEndorseField = function (data) {
        $scope.TASFields.TitleReleaseRushTicketNos = false;
        $scope.TASFields.TitleReleaseEndorsedRemarks = false;

        if (data != null) {
            $scope.TASFields.TitleReleaseRushTicketNos = true;
            $scope.TASFields.TitleReleaseEndorsedRemarks = true;
        }
    }

    $scope.EnableBankBuyerReleaseField = function (data, type) {
        if (type === 'Buyer') {
            $scope.TitleStatus.BuyerReleasedDate = null;
            //$scope.TitleStatus.BuyerReleasedRemarks = null;
            if(data) {
                $scope.RASFields.BuyerReleasedDate = false;
               // $scope.RASFields.BuyerReleasedRemarks = false;
            } else {
                $scope.RASFields.BuyerReleasedDate = true;
                //$scope.RASFields.BuyerReleasedRemarks = true;
            }
        }

        if (type === 'Bank') {
            $scope.TitleStatus.BankReleasedDate = null;
            //$scope.TitleStatus.BankReleasedRemarks = null;
            if(data) {
                $scope.RASFields.BankReleasedDate = false;
                //$scope.RASFields.BankReleasedRemarks = false;
            } else {
                $scope.RASFields.BankReleasedDate = true;
                //$scope.RASFields.BankReleasedRemarks = true;
            }
        }
    }
    // -------------------------------------- End Enable Field from Title Status -------------------------------------- //
    
    // -------------------------------------- Start Saving Title Status -------------------------------------- //
    $scope.SaveTitlingStatus = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.ShowReasonChange && $scope.ReasonForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
            return;
        } else {
            $scope.DASFields = { TaxDecNos: false }
            $scope.DASButtons = { AddButton: false, EditButton: true, CancelButton: false }

            $scope.TASFields = { LiquidationEndorsedDate: false, LiquidationRushTicketNos: false, LiquidationEndorsedRemarks: false, TitleReleaseEndorsedDate: false, TitleReleaseRushTicketNos: false, TitleReleaseEndorsedRemarks: false }
            $scope.TASButtons = { AddButton: false, EditButton: true, CancelButton: false }

            $scope.RASFields = { TitleLocationID: false, TitleNos: false, TitleRemarks: false, BankReleasedDate: false, BankReleasedRemarks: false, BuyerReleasedDate: false, BuyerReleasedRemarks: false, IsBankReleaseNA: null, IsBuyerReleaseNA: null }
            $scope.RASButtons = { AddButton: false, EditButton: true, CancelButton: false }

            $scope.SaveTitlingStatusConfirmed(data);
        }
    }  

    $scope.SaveTitlingStatusConfirmed = function (data) {
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
        data.TitleInProcessDate = $scope.SalesInfo.TitleInProcessDate;
        data.TitleTransferredDate = $scope.SalesInfo.TitleTransferredDate;
        data.TitleClaimedDate = $scope.SalesInfo.TitleClaimedDate;
        data.TaxDecTransferredDate = $scope.SalesInfo.TaxDecTransferredDate;
        data.TaxDecClaimedDate = $scope.SalesInfo.TaxDecClaimedDate;

        urlData = '../api/TitlingStatus/SaveTitlingStatus';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitInspectData();

                $('#applyChangesModal1a').modal('hide');
                $('#applyChangesModal2a').modal('hide');
                $('#applyChangesModal3a').modal('hide');
                $('#createTitlingRemarksModal').modal('hide');
                swal(
                    'System Message Confirmation',
                    'Information provided for the units has been recorded.',
                    'success'
                );

                $scope.TitleStatus.ReasonForChange = "";
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

        $rootScope.$emit("searchObjectType", 'TitlingStatusController');
    }
    // -------------------------------------- End Change Log -------------------------------------- //
    
    // -------------------------------------- Start Add Titling Remarks -------------------------------------- //
    // Set parameter for search, filter and sorting
    $scope.pagingInfo = {
        page: 1,
        itemsPerPage: "10",
        sortBy: 'Id',
        reverse: true,
        search: '',
        multiplesearch: { Published: 'Yes' },
        totalItems: 0,
        PageUrl: '',

    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetTitlingRemarksData();
    };

    // Sorting of content
    $scope.sort = function (sortBy) {
        if (sortBy === $scope.pagingInfo.sortBy) {
            $scope.pagingInfo.reverse = !$scope.pagingInfo.reverse;
        } else {
            $scope.pagingInfo.sortBy = sortBy;
            $scope.pagingInfo.reverse = false;
        }
        $scope.pagingInfo.page = 1;
        $scope.GetTitlingRemarksData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetTitlingRemarksData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetTitlingRemarksData();
    }
    //-----------------------------------------------------------

    $scope.GetTitlingRemarksData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/TitlingStatus/GetTitlingRemarks';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.TitlingRemarks = response.data.TitlingRemarksLIST;
                $scope.pagingInfo.totalItems = response.data.COUNT;
                //$scope.disableButton = ($scope.pagingInfo.totalItems) ? false : true;
                $scope.clearData();

                $scope.ResetTitleStatusFields();
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
        });
    };

    $scope.AddTitlingRemarks = function (type) {
        $scope.ResetTitleStatusFields();

        if ($scope.TitleStatus == null)          
            $scope.TitleStatus = { TitleStatusType : '', Remarks: ''};

        $scope.TitleStatus.TitleStatusType = type;
        $scope.TitleStatus.Remarks = "";
    };

    $scope.ViewTitlingRemarks = function (type) {
        $scope.pagingInfo.search1 = type;
        $scope.pagingInfo.search2 = $scope.TitleStatus.Id;

        $scope.search();
    };

    $scope.SaveTitlingRemarks = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.TitlingRemarksForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
            return;
        } else {

            $scope.SaveTitlingStatus(data);
        }
    }
    // -------------------------------------- Start Add Titling Remarks -------------------------------------- //

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

    $scope.dateOptionsWorkingDays = {
        dateDisabled: disabled,
        formatYear: 'yy',
        startingDay: 1,
        showWeeks: false,
    };

    // Liquidation Endorsed Date
    $scope.dateEndorseLiquidation = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Title Release Endorsed Date
    $scope.dateTitleReleaseEndorse = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Title Release to Buyer and Bank
    $scope.dateTitleReleaseBuyerBank = {
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

    // Liquidation Endorsed Date
    $scope.open1 = function () {
        // Min date for Endorsed Liquidation 
        $scope.dateEndorseLiquidation.minDate = $scope.SalesInfo.TaxDeclarationTransferredDate;
        $scope.popup1.opened = true;
    };

    $scope.popup1 = {
        opened: false
    };

    // Title Release Endorsed Date
    $scope.open2 = function () {
        // Min date for Title Release Endorsed
        $scope.dateTitleReleaseEndorse.minDate = $scope.TitleStatus.LiquidationEndorsedDate;
        $scope.popup2.opened = true;
    };

    $scope.popup2 = {
        opened: false
    };

    // Date Released to Bank
    $scope.open3 = function () {
        // Min date for Released to Buyer & Bank
        // $scope.dateTitleReleaseBuyerBank.minDate = $scope.TitleStatus.TitleReleaseEndorsedDate;
        $scope.popup3.opened = true;
    };

    $scope.popup3 = {
        opened: false
    };

    // Date Released to Buyer
    $scope.open4 = function () {
        // Min date for Released to Buyer & Bank
        // $scope.dateTitleReleaseBuyerBank.minDate = $scope.TitleStatus.TitleReleaseEndorsedDate;
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


