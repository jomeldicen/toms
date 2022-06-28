app.controller("DashboardTOStatus", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = { 'OptionIDs': [] };
    $scope.DashboardTOStatus = [];
    $scope.ProjectName = '';
    $scope.loading = false;
    $scope.viewRecord = false;
    $scope.disableButton = true;
    $scope.formatPlaceholder = 'search here...';
    $scope.maindashloading1 = true;
    $scope.maindashloading2 = true;

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        $scope.pagingInfo.PageUrl = PageUrl;
    }

    $scope.setting1 = {
        scrollableHeight: '300px',
        scrollable: true,
        enableSearch: true
    };

    $scope.clearData = function () {
        $scope.loading = false;
        document.body.style.cursor = 'default';
        $scope.data.nvFabIcon = "fa-bars";
    };

    $scope.clearSearch = function () {
        $scope.data = { 'OptionIDs': [] };
        $scope.DashboardTOStatus = null;
        $scope.DashboardSummary2 = null;
        $scope.Ctrl = null;
        $scope.viewRecord = false;
        $scope.pagingInfo.search = '';
        $scope.pagingInfo.search2 = '';

    };
    //-----------------------------------------------------------
    // Set parameter for search field
    $scope.paramInfo = { ProjectID: '0', CompanyCode: '', ProjectCode: '', UnitNos: '', UnitCategory: '', CustomerNos: '', ProjectLocation: '', PageUrl: '' };

    //-----------------------------------------------------------
    // Set parameter for search, filter and sorting
    $scope.pagingInfo = {
        page: 1,
        itemsPerPage: "10",
        sortBy: 'default',
        reverse: true,
        search: '',
        search2: '',
        search3: '',
        multiplesearch: { Published: 'Yes' },
        totalItems: 0,
        PageUrl: '',
        searchbyids: [],
        searchcol: '0'
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetDashboardTOStatusData();
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
        $scope.GetDashboardTOStatusData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetDashboardTOStatusData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetDashboardTOStatusData();
    }

    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/DashboardTOStatus/GetSearchData';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.Projects = response.data.PROJECTLIST;
            }
            $scope.clearData();
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

    $scope.GetDashboardTOStatusData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/DashboardTOStatus/GetDashboardTOStatus';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.DashboardTOStatus = response.data.QUALIFIEDTOSTATUS;
                $scope.ProjSelected = response.data.PROJSELECTED;
                $scope.DateSync = new Date(response.data.LASTDATESYNC);
                $scope.Ctrl = response.data.CONTROLS;
                $scope.pagingInfo.totalItems = response.data.COUNT;
                $scope.disableButton = ($scope.pagingInfo.totalItems) ? false : true;
                $scope.viewRecord = true;

                if ($scope.ProjSelected.length !== 0) {
                    if ($scope.ProjSelected.length === 1) {
                        $scope.ProjectName = $scope.ProjSelected[0].BusinessEntity;
                    } else {
                        $scope.ProjectName = "Multiple Projects Selection";
                    }
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
    
    // Get Statistics data on Main Summary
    $scope.GetMainSummary = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.maindashloading1 = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/DashboardTOStatus/GetMainSummary';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.DashboardSummary = response.data.DASHBOARDSUMMARY;
                $scope.DateSync = new Date(response.data.LASTDATESYNC);

                $scope.maindashloading1 = false;
                document.body.style.cursor = 'default';
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.maindashloading1 = false;
            document.body.style.cursor = 'default';
        });
    };

    // Get Statistics data on Detailed Summary
    $scope.GetDetailSummary = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.maindashloading2 = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/DashboardTOStatus/GetDetailSummary';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.DashboardSummary2 = response.data.DASHBOARDSUMMARY2;

                $scope.maindashloading2 = false;
                document.body.style.cursor = 'default';
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.maindashloading1 = false;
            document.body.style.cursor = 'default';
        });
    };

    // Get Detail by Project
    $scope.GetDetailByProject = function (list) {
        $scope.disableButton = false;
        if (list.length === 0) {
            swal(
                'System Message Confirmation',
                'Please first make a selection from Project Field',
                'warning'
            );
            $scope.disableButton = true;
            return false;
        }

        $scope.pagingInfo.search = '';
        $scope.pagingInfo.search2 = '';
        $scope.pagingInfo.search3 = '';
        $scope.pagingInfo.searchbyids = list.map(function (x) { return x.id; });
        $scope.GetDashboardTOStatusData();
        $scope.GetDetailSummary();
    };

    // Get Detail by Summary
    $scope.GetDetailBySummary = function (type) {
        $scope.disableButton = false;
        if (type === '') {
            swal(
                'System Message Confirmation',
                'Please first make a selection from Project Summary',
                'warning'
            );
            $scope.disableButton = true;
            return false;
        }

        $scope.pagingInfo.search = '';
        $scope.pagingInfo.search3 = '';
        $scope.data = { 'OptionIDs': [] };
        $scope.pagingInfo.search2 = type;
        $scope.pagingInfo.searchbyids = [];
        $scope.GetDashboardTOStatusData();
        $scope.GetDetailSummary();
    };

    // Fetch Updated TOAS
    $scope.FetchUpdatedTOAS = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/CronJob/GetFetchAllSAPAccountWithTOAS';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.pagingInfo.search3 = 'Ok';
                swal('System Message Confirmation', 'Record successfully synced', 'success');
                $scope.GetMainSummary();

                $timeout(function () {
                    $scope.clearData();
                }, 1000);
            }
        }, function (response) {
            $scope.pagingInfo.search3 = 'Ok';
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

    // Format Placeholder based on Column Selection
    $scope.updatePlaceholder = function () {
        $scope.formatPlaceholder = 'search here...';

        if ($scope.pagingInfo.searchcol === '8')
            $scope.formatPlaceholder = "ex. MM/dd/yyyy";
        else
            $scope.formatPlaceholder = 'search here...';
    };

    // Hyperlink Redirect to Transaction Module
    $scope.linkRederict = function (UniqueHashKey) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        window.location.href = '../Admin/UnitInspectionAcceptanceMgmnt?flx10ms=' + UniqueHashKey;
    };

    // Report Export
    $scope.ExportReport = function (control) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        var reportParameter = [];
        if ($scope.pagingInfo.searchbyids.length === 0)
            $scope.pagingInfo.searchbyids = [0];

        reportParameter.push({
            param1: $scope.pagingInfo.searchbyids.join("|"), // project list
            param2: $scope.pagingInfo.search2, // type of column in Summary ex (Title In-Process, Title Transferred, etc..)
            param3: $scope.pagingInfo.searchcol, // column to search
            param4: $scope.pagingInfo.search,  // string data if not date
            dt1: $scope.pagingInfo.DateFrom,  // date from if searchcol is related to date
            dt2: $scope.pagingInfo.DateTo // date to if searchcol is related to date
        });

        if ($scope.pagingInfo.DateFrom === null || $scope.pagingInfo.DateTo === null) {
            var reportParameter = [];
            reportParameter.push({
                param1: $scope.pagingInfo.searchbyids.join("|"), // project list
                param2: $scope.pagingInfo.search2, // type of column in Summary ex (Title In-Process, Title Transferred, etc..)
                param3: $scope.pagingInfo.searchcol, // column to search
                param4: $scope.pagingInfo.search,  // string data if not date
            });
        }

        var json = JSON.stringify(reportParameter);

        var path = "../Report/ExportReport";
        if (control === 'Download')
            path = "../Report/ExportReport";
        else if (control === 'Print')
            path = "../Reports/ReportViewer.aspx";

        window.open(path + '?rep=t3xf2E20iklxL40123lssj&contype=001x&json=' + json, '_blank');
    };

    $('[data-toggle="tooltip"]').tooltip();
}]);
