app.controller("UnitQualifyMasterlist", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};
    $scope.UnitQualifyMasterlist = [];
    $scope.countChecked = 0;
    $scope.loading = false;
    $scope.disableButton = true;
    $scope.selectedAll = false;

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        $scope.pagingInfo.PageUrl = PageUrl;
    }

    $scope.clearData = function () {
        $scope.loading = false;
        $scope.selectedAll = false;
        document.body.style.cursor = 'default';
        $scope.data = {};
        $scope.data.nvFabIcon = "fa-bars";
    };

    //-----------------------------------------------------------
    // Set parameter for search, filter and sorting
    $scope.pagingInfo = {
        page: 1,
        itemsPerPage: "10",
        sortBy: 'Id',
        reverse: true,
        search: '',
        multiplesearch: { Published: 'Yes' },
        totalItems: 0,
        PageUrl: ''
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetUnitQualifyMasterlistData();
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
        $scope.GetUnitQualifyMasterlistData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetUnitQualifyMasterlistData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetUnitQualifyMasterlistData();
    }
    //-----------------------------------------------------------

    $scope.GetUnitQualifyMasterlistData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/UnitQualifyTurnoverMgmnt/GetUnitQualifyMasterlist';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.UnitQualifyMasterlist = response.data.UnitQualifyMasterlistLIST;
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

    // Hyperlink Redirect to Transaction Module
    $scope.linkRederict = function (UniqueHashKey) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        window.location.href = '../Admin/UnitQualifyTurnoverMgmnt?flx10ms=' + UniqueHashKey;
    };

    // Start Checkbox control
    $scope.selectAll = function () {
        angular.forEach($scope.UnitQualifyMasterlist, function (item) {
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
    
    // Start individual Edit control
    $scope.EditUnitQualifyMasterlistData = function (data) {
        $scope.clearData();
        $scope.data = data;
    };

    $scope.SaveUnitQualifyMasterlist = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.UnitQualifyMasterlistForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
            return;
        } else {
            $scope.SaveUnitQualifyMasterlistConfirmed(data);
        }
    }

    $scope.SaveUnitQualifyMasterlistConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/UnitQualifyMasterlist/SaveUnitQualifyMasterlist';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetUnitQualifyMasterlistData();
                $('#createUnitQualifyMasterlistModal').modal('hide');

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
                'Deleted Failed!',
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

        url = '../api/UnitQualifyTurnoverMgmnt/RemoveRecords';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            var obj = response.data;
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetUnitQualifyMasterlistData();

                swal(
                    'System Message Confirmation',
                    response.data,
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
    $scope.DeleteUnitQualifyMasterlist = function (data) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                $scope.DeleteUnitQualifyMasterlistConfirmed(data);
            }
        });
    }

    $scope.DeleteUnitQualifyMasterlistConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        url = "/api/UnitQualifyTurnoverMgmnt/RemoveData";
        $http({
            method: 'POST',
            url: url,
            params: {'ID' : data.Id},
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetUnitQualifyMasterlistData();

                swal(
                    'System Message Confirmation',
                    'Record successfully deleted',
                    'success'
                )
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End individual Delete control

}]);