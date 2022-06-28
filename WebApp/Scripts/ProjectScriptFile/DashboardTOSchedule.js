app.controller("DashboardTOSchedule", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = { 'OptionIDs': [], 'OptionIDs1': [], 'OptionIDs2': [] };
    $scope.DashboardTOSchedule = [];
    $scope.ProjectName = '';
    $scope.loading = false;
    $scope.viewRecord = false;
    $scope.disableButton = true;

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
        $scope.data = { 'OptionIDs': [], 'OptionIDs1': [], 'OptionIDs2': [] };
        $scope.TOSchedule = null;
        $scope.Ctrl = null;
        $scope.viewRecord = false;
        $scope.pagingInfo.DateFrom = new Date();
        $scope.pagingInfo.DateTo = new Date();
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
        multiplesearch: { Published: 'Yes' },
        totalItems: 0,
        PageUrl: '',
        searchbykey1: [],
        searchbykey2: [],
        searchbykey3: [],
        DateFrom: new Date(),
        DateTo: new Date()
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetDashboardTOScheduleData();
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
        $scope.GetDashboardTOScheduleData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetDashboardTOScheduleData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetDashboardTOScheduleData();
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

        urlData = '../api/DashboardTOSchedule/GetSearchData';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.HandoverAssocs = response.data.HANDOVERASSOCLIST;
                $scope.AccountTypes = response.data.ACCTTYPELIST;
                $scope.TurnoverOptions = response.data.TOOPTIONLIST;
            }
            $scope.clearData();
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

    $scope.GetDashboardTOScheduleData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/DashboardTOSchedule/GetDashboardTOSchedule';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.TOSchedule = response.data.QUALIFIEDTOSCHEDULE;
                $scope.Ctrl = response.data.CONTROLS;
                $scope.pagingInfo.totalItems = response.data.COUNT;
                $scope.disableButton = ($scope.pagingInfo.totalItems) ? false : true;
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

    // Get Detail by Criteria (Handover Associate, Turnover Option, Account Type)
    $scope.GetDetailByCriteria = function (list, list2, list3) {

        $scope.disableButton = false;
        if (list.length === 0 || list2.length === 0 || list3.length === 0 || !$scope.pagingInfo.DateFrom || !$scope.pagingInfo.DateTo) {
            swal(
                'System Message Confirmation',
                'Please first make a selection from search criteria',
                'warning'
            );
            $scope.disableButton = true;
            return false;
        }

        if ($scope.pagingInfo.DateFrom && $scope.pagingInfo.DateTo) {

            if ($scope.pagingInfo.DateFrom > $scope.pagingInfo.DateTo) {
                swal(
                    'System Message Confirmation',
                    'Period Date (To) must be the same or after the Period Date (From)',
                    'warning'
                );
                $scope.disableButton = true;
                return false;
            }
        }

        $scope.pagingInfo.search = '';
        $scope.pagingInfo.searchbykey1 = list.map(function (x) { return x.id; });
        $scope.pagingInfo.searchbykey2 = list2.map(function (x) { return x.id; });
        $scope.pagingInfo.searchbykey3 = list3.map(function (x) { return x.id; });

        $scope.GetDashboardTOScheduleData();
    };

    // Report Export
    $scope.ExportReport = function (control) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        var reportParameter = [];

        if ($scope.pagingInfo.searchbykey1.length === 0)
            $scope.pagingInfo.searchbykey1 = [0];

        if ($scope.pagingInfo.searchbykey2.length === 0)
            $scope.pagingInfo.searchbykey2 = [0];

        if ($scope.pagingInfo.searchbykey3.length === 0)
            $scope.pagingInfo.searchbykey3 = [0];

        reportParameter.push({
            param1: $scope.pagingInfo.searchbykey1.join("|"),
            param2: $scope.pagingInfo.searchbykey2.join("|"),
            param3: $scope.pagingInfo.searchbykey3.join("|"),
            param4: $scope.pagingInfo.search,
            dt1: $scope.pagingInfo.DateFrom,
            dt2: $scope.pagingInfo.DateTo
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

        window.open(path + '?rep=t2xf1F10jklxM30923llkj&contype=001x&json=' + json,'_blank');
    };

    $('[data-toggle="tooltip"]').tooltip();

    // Start Date Management
    $scope.dateOptionsCurrent = {
        formatYear: 'yy',
        minDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    $scope.dateOptionsCurrent2 = {
        formatYear: 'yy',
        minDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Period From
    $scope.open1 = function () {
        $scope.popup1.opened = true;
    };

    $scope.popup1 = {
        opened: false
    };

    // Period To
    $scope.open2 = function () {
        $scope.popup2.opened = true;
        $scope.dateOptionsCurrent2.minDate = $scope.pagingInfo.DateFrom;
    };

    $scope.popup2 = {
        opened: false
    };

    $scope.dateFormats = ['MM/dd/yyyy', 'dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.dateFormat = $scope.dateFormats[0];

    // End Date Management
}]);
