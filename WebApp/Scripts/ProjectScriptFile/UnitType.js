app.controller("UnitType", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};
    $scope.unittype = [];
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
        multiplesearch: { Published: 'Yes'},
        totalItems: 0,
        PageUrl: ''
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetUnitTypeData();
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
        $scope.GetUnitTypeData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetUnitTypeData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetUnitTypeData();
    }
    //-----------------------------------------------------------

    $scope.GetUnitTypeData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        $scope.loading = true;

        urlData = '../api/UnitType/GetUnitType';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.UnitType = response.data.UNITYPELIST;
                $scope.Ctrl = response.data.CONTROLS;
                $scope.pagingInfo.totalItems = response.data.COUNT;
                $scope.disableButton = ($scope.pagingInfo.totalItems) ? false : true;
            }
            $timeout(function () {
                $scope.loading = false;
            }, 1000);
        }, function (response) {
            var obj = response.data.Message;
            swal('Loading Failed!', obj, 'error');
            document.body.style.cursor = 'default';
            $scope.disableButton = true;
            $scope.loading = false;
        });
    };

    // Start Checkbox control
    $scope.selectAll = function () {
        angular.forEach($scope.UnitType, function (item) {
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
        var data = {};
        data.dsList = [];
        data.Published = published;
        for (var i = 0; i < item.length; i++) {
            if (item[i].isChecked)
                data.dsList.push(item[i]);
        }

        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';
        url = '../api/UnitType/UpdateStatus';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            var obj = response.data;
            if (response.status === 200) {
                document.body.style.cursor = 'default';
                $scope.selectedAll = false;
                $scope.GetUnitTypeData();
                swal(
                    'Successfully updated',
                    'Record was updated successfully',
                    'success'
                );
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            var obj = response;
            swal(
                'Update Failed!',
                'Please check your data!',
                'error'
            );
        });

    };
    // End Activate and Deactivate control

    // Start individual Edit control
    $scope.EditUnitTypeData = function (data) {
        $scope.clearData();
        $scope.data = data;
    };

    $scope.SaveUnitType = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/UnitType/SaveUnitType';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            document.body.style.cursor = 'default';
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetUnitTypeData();
                $('#createUnitTypeModal').modal('hide');
                swal(
                    'Save Successful',
                    'UnitType Saved Successfully',
                    'success'
                    );
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            if (response.data.Message === "Unit Code Exists") {
            swal(
                'Unit Code Exists',
                'Change Unit Code',
                'error'
                );
            }
        });
    };

    // Start Fetch control
    $scope.FetchUnitType = function () {
        $scope.clearData();
        $scope.FetchUnitTypeConfirmed();
    }

    $scope.FetchUnitTypeConfirmed = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        var $modal = $('.js-loading-bar'),
            $bar = $modal.find('.progress-bar');

        $bar.css({ width: "70%" });

        urlData = "../api/UnitType/GetSAPUnitType";
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            $scope.res = response;

            $bar.css({ width: "90%" });
            if (response.status === 200) {
                $bar.css({ width: "100%" });
                setTimeout(function () {
                    document.body.style.cursor = 'default';
                    $scope.clearData();
                    $scope.GetUnitTypeData();
                    $('#fetchUnitTypeModal').modal('hide');
                    swal(
                        'Successfully Sync.',
                        'Information Sync Successfully',
                        'success'
                    );
                }, 1500);
            }
        }, function (response) {
            var obj = response.data.Message;
            swal(
                'Error',
                'You can not fetch this data because Unit Type data exist.',
                'error'
            );
        });
    };
    // End Fetch control

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
                title: "Delete Confirmation",
                text: "You are going to delete this record! Do you want to proceed?",
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
        var data = {};
        data.dsList = [];
        for (var i = 0; i < item.length; i++) {
            if (item[i].isChecked)
                data.dsList.push(item[i]);
        }

        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';
        url = '../api/UnitType/RemoveRecords';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            var obj = response.data;
            if (response.status === 200) {
                document.body.style.cursor = 'default';
                $scope.selectedAll = false;
                $scope.GetUnitTypeData();
                swal(
                    'Successfully deleted',
                    'Record was deleted successfully',
                    'success'
                );
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            var obj = response;
            swal(
                'Update Failed!',
                'Please check your data!',
                'error'
            );
        });
    };
    // End Delete control

    // Start individual Delete control
    $scope.DeleteUnitType = function (data) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                $scope.DeleteUnitTypeConfirmed(data);
            }
        });
    }

    $scope.DeleteUnitTypeConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        url = "/api/UnitType/RemoveData";
        $http({
            method: 'POST',
            url: url,
            params: { 'Id': data.Id},
            headers: headers,
        }).then(function (response) {
            $scope.res = response;
            if (response.status === 200) {
                swal(
                'Successfully Deleted.',
                'Record was deleted successfully',
                'success'
                );
                $scope.clearData();
                $scope.GetUnitTypeData();
            }
        }, function (response) {
            var obj = response.data.Message;
            swal(
                'Error',
                'You can not delete this data because related data exist.',
                'error'
                );
        });
    };
    // End individual Delete control

}]);
