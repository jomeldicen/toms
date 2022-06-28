app.controller("OccupancyPermit", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};
    $scope.OccupancyPermit = [];
    $scope.countChecked = 0;
    $scope.loading = false;
    $scope.disableButton = true;
    $scope.selectedAll = false;
    $scope.dt = new Date();

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        $scope.pagingInfo.PageUrl = PageUrl;
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
    $scope.querySearch = function (query) {
        var results = query ? $scope.ProjectList().filter(createFilterFor(query)) : $scope.ProjectList(), deferred;
        if ($scope.simulateQuery) {
            deferred = $q.defer();
            $timeout(function () { deferred.resolve(results); }, Math.random() * 1000, false);
            return deferred.promise;
        } else {
            return results;
        }
    }

    $scope.searchTextChange = function (text) {
        //$log.info('Text changed to ' + text);
    }

    $scope.selectedItemChange = function (item) {
        //$log.info('Item changed to ' + JSON.stringify(item));

        $scope.pagingInfo.multiplesearch.ProjectLocation = (item) ? item.ProjectLocation : '';
        $scope.pagingInfo.multiplesearch.ProjectID = (item) ? item.Id : 0;
        $scope.countChecked = 0;
        $scope.GetOccupancyPermitData();
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
        $scope.data = {};
        $scope.data.nvFabIcon = "fa-bars";
    };

    $scope.clearSearch = function () {
        $scope.selectedProject = null;
        $scope.searchProject = "";

        $scope.selectedAll = false;
        $scope.disableButton = true;
        $scope.pagingInfo.multiplesearch = { ProjectID: '0', OccupancyPermit: false, UnitNos: '0', ProjectLocation: '' };
        $scope.OccupancyPermitData.opd = new Date();
        $scope.OccupancyPermitData.Remarks = "";

        $scope.GetOccupancyPermitData();
    };

    //-----------------------------------------------------------
    $scope.OccupancyPermitData = {
        opd: new Date(), Remarks: ''
    };

    // Set parameter for search, filter and sorting
    $scope.pagingInfo = {
        page: 1,
        itemsPerPage: "10",
        sortBy: 'Id',
        reverse: false,
        search: '',
        multiplesearch: { ProjectID: '0', OccupancyPermit: false, UnitNos: '0', ProjectLocation: '' },
        totalItems: 0,
        PageUrl: ''
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetOccupancyPermitData();
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
        $scope.GetOccupancyPermitData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetOccupancyPermitData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetOccupancyPermitData();
    }
    //-----------------------------------------------------------

    $scope.GetOccupancyPermitData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        $scope.loading = true;

        urlData = '../api/OccupancyPermit/GetOccupancyPermit';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.Projects = response.data.PROJECTLIST;
                $scope.Floors = response.data.FLOORLIST;
                $scope.Ctrl = response.data.CONTROLS;
                $scope.pagingInfo.totalItems = response.data.COUNT;
                $scope.disableButton = ($scope.pagingInfo.totalItems) ? false : true;
            }
            $timeout(function () {
                $scope.loading = false;
            }, 1000);
        }, function (response) {
            document.body.style.cursor = 'default';
            $scope.disableButton = true;
            $scope.loading = false;
        });
    };

    // call from other controller
    $scope.GetTransactionLog = function () {
        $('#changeLogModal').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });

        $rootScope.$emit("searchObjectType", 'OccupancyPermitController');
    }

    // Start Checkbox control
    $scope.selectAll = function () {
        angular.forEach($scope.Floors, function (item) {
            if (item.Available === 'False') {
                item.isChecked = !$scope.selectedAll;
                if (item.isChecked)
                    $scope.countChecked++;
                else
                    $scope.countChecked--;
            }
        });
    };

    $scope.checkSelected = function (item) {
        if (item)
            $scope.countChecked++;
        else
            $scope.countChecked--;
    }
    // End Checkbox control
    
    // Start individual Edit control
    $scope.OccupancyPermitModal = function () {
        if ($scope.pagingInfo.multiplesearch.OccupancyPermit === false || $scope.countChecked <= 0) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
        } else {
            $('#applyChangesModal').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    }

    $scope.SaveOccupancyPermit = function (item) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        var data = [];
        // only we need is all permit not yet tag as available 
        item = item.filter(function (obj) {
            return obj.Available === 'False' && obj.isChecked === true;
        });
        for (var i = 0; i < item.length; i++) {
            if (item[i].isChecked) {
                item[i].ProjectID = $scope.pagingInfo.multiplesearch.ProjectID;
                item[i].PertmitDate = $scope.OccupancyPermitData.opd;
                item[i].Remarks = $scope.OccupancyPermitData.Remarks;
                data.push(item[i]);
            }
        }

        urlData = '../api/OccupancyPermit/SaveOccupancyPermit';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            document.body.style.cursor = 'default';
            if (response.status === 200) {
                $scope.clearSearch();

                $('#applyChangesModal').modal('hide');
                swal(
                    'System Message Confirmation',
                    'Occupancy Permit tagging has been applied to the units',
                    'success'
                );
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            var obj = response.data.Message;
            swal(
                'Error Message', obj, 'error'
            );
        });
    };
    // End individual Edit control

    // Start Date Management

    $('[data-toggle="tooltip"]').tooltip();


    $scope.today = function () {
        $scope.OccupancyPermitData.opd = new Date();
    };
    $scope.today();

    $scope.dateOptionsDefault = {
        formatYear: 'yy',
        //maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    // Occupancy Permit Date
    $scope.open1 = function () {
        $scope.popup1.opened = true;
    };

    $scope.popup1 = {
        opened: false
    };

    $scope.dateFormats = ['MM/dd/yyyy', 'dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.dateFormat = $scope.dateFormats[0];
    $scope.altInputFormats = ['M!/d!/yyyy'];

    // End Date Management

}]);
