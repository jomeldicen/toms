app.controller("InventoryMeterDeposit", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};
    $scope.InventoryMeterDeposit = [];
    $scope.countChecked = 0;
    $scope.loading = false;
    $scope.disableButton = true;
    $scope.selectedAll = false; 
    $scope.ShowReasonChange = false;

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
    $scope.querySearch = function (query, source) {

        if (source === 'Project')
            var results = query ? $scope.ProjectList().filter(createFilterFor(query)) : $scope.ProjectList(), deferred;
        else if (source === 'UnitCategory')
            results = query ? $scope.UnitCategoryList().filter(createFilterFor(query)) : $scope.UnitCategoryList(), deferred;
        else if (source === 'Unit')
            results = query ? $scope.UnitList().filter(createFilterFor(query)) : $scope.UnitList(), deferred;

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

    $scope.selectedItemChange = function (item, source) {
        //$log.info('Item changed to ' + JSON.stringify(item));
        if ($scope.jomel) $scope.resetData();

        if (source === 'Project') {
            $scope.selectedUnitCategory = null;
            $scope.searchUnitCategory = "";
            $scope.selectedUnit = null;
            $scope.searchUnit = "";

            $scope.pagingInfo.multiplesearch.ProjectID = (item) ? item.Id : 0;
            $scope.pagingInfo.multiplesearch.CompanyCode = (item) ? item.CompanyCode : '';
            $scope.pagingInfo.multiplesearch.ProjectCode = (item) ? item.ProjectCode : '';
            $scope.pagingInfo.multiplesearch.ProjectLocation = (item) ? item.ProjectLocation : '';
        } else if (source === 'UnitCategory') {
            $scope.selectedUnit = null;
            $scope.searchUnit = "";

            $scope.pagingInfo.multiplesearch.UnitCategory = (item) ? item.code : '';
        } else if (source === 'Unit') {
            $scope.pagingInfo.multiplesearch.UnitNos = (item) ? item.UnitNos : '';
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
        return $scope.Units.map(function (item) {
            item.value = item.RefNos.toLowerCase();
            return item;
        });
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
        $scope.data.nvFabIcon = "fa-bars";
    };

    $scope.resetData = function () {
        $scope.totalItems = 0;
        $scope.disableButton = true;
        $scope.disableClientButton = true;
    };

    $scope.clearSearch = function () {
        $scope.selectedProject = null;
        $scope.searchProject = "";
        $scope.selectedUnitCategory = null;
        $scope.searchUnitCategory = "";
        $scope.selectedUnit = null;
        $scope.searchUnit = "";

        $scope.pagingInfo.multiplesearch.ProjectID = 0;
        $scope.pagingInfo.multiplesearch.CompanyCode = '';
        $scope.pagingInfo.multiplesearch.ProjectCode = '';
        $scope.pagingInfo.multiplesearch.UnitNos = '';
        $scope.pagingInfo.multiplesearch.UnitCategory = '';
        $scope.pagingInfo.multiplesearch.ProjectLocation = '';

        $scope.resetData();
        $scope.UnitInfo = null;
        $scope.ShowReasonChange = false;

    };

    //-----------------------------------------------------------
    // Set parameter for search, filter and sorting
    $scope.pagingInfo = {
        page: 1,
        itemsPerPage: "10",
        sortBy: 'Id',
        reverse: true,
        search: '',
        multiplesearch: { ProjectID: '0', CompanyCode: '', ProjectCode: '', UnitNos: '', UnitCategory: '', CustomerNos: '', ProjectLocation: '' },
        totalItems: 0,
        PageUrl: ''
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetInventoryMeterDepositData();
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
        $scope.GetInventoryMeterDepositData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetInventoryMeterDepositData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetInventoryMeterDepositData();
    }

    //-----------------------------------------------------------
    // Set parameter for search field

    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';

        urlData = '../api/InventoryMeterDeposit/GetSearchData';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo.multiplesearch,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.Projects = response.data.PROJECTLIST;
                $scope.Units = response.data.UNITLIST;
            }
            $scope.clearData();
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    //-----------------------------------------------------------

    $scope.GetInventoryMeterDepositData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        $scope.ShowReasonChange = false;
        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/InventoryMeterDeposit/GetInventoryMeterDeposit';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.InventoryMeterDeposit = response.data.InventoryMeterDepositLIST;
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

    // Start Checkbox control
    $scope.selectAll = function () {
        angular.forEach($scope.InventoryMeterDeposit, function (item) {
            item.isChecked = !$scope.selectedAll;
            if (item.isChecked)
                $scope.countChecked++;
            else
                $scope.countChecked--;
        });

    };

    $scope.checkSelected = function (item) {
        if (item)
            $scope.countChecked++;
        else
            $scope.countChecked--;
    }
    // End Checkbox control

    // Start Create control
    $scope.AddData = function () {
        $scope.clearData();
        $scope.data = {
            Id: 0,
            Published: "True"
        };
    };
    // End Create control

    // Start Activate and Deactivate control
    $scope.StatusData = function (data, a) {
        if ($scope.countChecked === 0) {
            swal(
                'Update Failed!',
                'Please first make a selection from the list.',
                'warning'
            );
        } else {
            swal({
                title: "Update Confirmation",
                text: "You are going to " + ((a) ? "Activate" : "Deactivate") + " this record! Do you want to proceed?",
                icon: "warning",
                buttons: true,
                dangerMode: true,
            })
            .then((willUpdate) => {
                if (willUpdate) {
                    $scope.StatusDataConfirmed(data, a);
                }
            });
        }
    }

    $scope.StatusDataConfirmed = function (item, published) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        var data = {};
        data.dsList = [];
        data.Published = published;
        for (var i = 0; i < item.length; i++) {
            if (item[i].isChecked)
                data.dsList.push(item[i]);
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        url = '../api/InventoryMeterDeposit/UpdateStatus';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetInventoryMeterDepositData();
                swal(
                    'System Message Confirmation',
                    'Record successfully updated',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End Activate and Deactivate control

    // Start individual Edit control
    $scope.EditInventoryMeterDepositData = function (data) {
        $scope.clearData();
        $scope.data = data;

        $scope.ShowReasonChange = false;
        if(data.Id !== 0)
            $scope.ShowReasonChange = true;
    };

    $scope.SaveInventoryMeterDeposit = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.ShowReasonChange && $scope.InventoryMeterDepositForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
            return;
        } else {
            $scope.SaveInventoryMeterDepositConfirmed(data);
        }
    }

    $scope.SaveInventoryMeterDepositConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/InventoryMeterDeposit/SaveInventoryMeterDeposit';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                data.ReasonForChange = "";
                $scope.ShowReasonChange = false;

                $scope.clearData();
                $scope.GetInventoryMeterDepositData();
                $('#UpdateMeterDepositModal').modal('hide');

                swal(
                    'System Message Confirmation',
                    'Record successfully saved',
                    'success'
                    );


            }
        }, function (response) {
            var obj = response.data.Message;
            if (obj === "Exists")
                swal('Error Message', 'Record already exist', 'error');
            else
                swal('Error Message', obj, 'error');

            document.body.style.cursor = 'default';
            $scope.loading = false;             
        });
    };
    // End individual Edit control

    // Start Delete control
    $scope.DeleteData = function (data) {
        if ($scope.countChecked === 0) {
            swal(
                'Delete Failed!',
                'Please first make a selection from the list.',
                'warning'
            );
        } else {
            swal({
                title: "Are you sure?",
                text: "Once deleted, you will not be able to recover this!",
                icon: "warning",
                buttons: true,
                dangerMode: true,
            })
            .then((willDelete) => {
                if (willDelete) {
                    $scope.DeleteDataConfirmed(data);
                }
            });
        }
    }

    $scope.DeleteDataConfirmed = function (item) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        var data = {};
        data.dsList = [];
        for (var i = 0; i < item.length; i++) {
            if (item[i].isChecked)
                data.dsList.push(item[i]);
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        url = '../api/InventoryMeterDeposit/RemoveRecords';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetInventoryMeterDepositData();

                swal(
                    'System Message Confirmation',
                    'Record successfully deleted',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End Delete control

    // Start individual Delete control
    $scope.DeleteInventoryMeterDeposit = function (data) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                $scope.DeleteInventoryMeterDepositConfirmed(data);
            }
        });
    }

    $scope.DeleteInventoryMeterDepositConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        url = "/api/InventoryMeterDeposit/RemoveData";
        $http({
            method: 'POST',
            url: url,
            params: {'ID' : data.Id},
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetInventoryMeterDepositData();

                swal(
                    'System Message Confirmation',
                    'Record successfully deleted',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End individual Delete control

    // -------------------------------------- Start Change Log -------------------------------------- //
    // call from other controller
    $scope.GetTransactionLog = function () {
        $('#changeLogModal').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });

        $rootScope.$emit("searchObjectType", 'InventoryMeterDepositController');
    }
    // -------------------------------------- End Change Log -------------------------------------- //
}]);