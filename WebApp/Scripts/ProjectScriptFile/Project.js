app.controller("Project", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};
    $scope.project = [];
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
        $scope.GetProjectData();
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
        $scope.GetProjectData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetProjectData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetProjectData();
    }
    //-----------------------------------------------------------

    $scope.GetProjectData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        $scope.loading = true;

        urlData = '../api/Project/GetProject';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.Project = response.data.PROJLIST;
                $scope.Companys = response.data.COMPANYLIST;
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
        angular.forEach($scope.Project, function (item) {
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

    // Start enabling project according to module
    $scope.UpdateData = function (data) {  
        swal({
            title: "Update Confirmation",
            text: "You are going to update this record! Do you want to proceed?",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willUpdate) => {
            if (willUpdate) {
                $scope.UpdateDataConfirmed(data);
            }
        });
    }

    $scope.UpdateDataConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';
        url = '../api/Project/UpdateModule';
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
                $scope.GetProjectData();
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
    // End enabling project according to module

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
        url = '../api/Project/UpdateStatus';
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
                $scope.GetProjectData();
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
    $scope.EditProjectData = function (data) {
        $scope.clearData();
        $scope.data = data;
    };

    $scope.SaveProject = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/Project/SaveProject';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            document.body.style.cursor = 'default';
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetProjectData();
                $('#createProjectModal').modal('hide');
                swal(
                    'Save Successful',
                    'Project Saved Successfully',
                    'success'
                    );
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            if (response.data.Message === "Project Code Exist") {
                swal(
                'Project Code Exists',
                'Change Project Code',
                'error'
                );
            }
        });
    };
    // End individual Edit control

    // Start Fetch control

    // Start individual Edit control
    $scope.FetchData = function () {
        $scope.clearData();
        $scope.data = { 'Id' : '0' };
    };

    $scope.FetchProject = function (Id) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        $scope.clearData();

        $scope.ro = true;
        document.body.style.cursor = 'wait';

        var $modal = $('.js-loading-bar'),
            $bar = $modal.find('.progress-bar');

        $bar.css({ width: "70%" });

        urlData = "../api/Project/SAPProject";
        $http({
            method: 'POST',
            url: urlData,
            params: { 'Id': Id },
            headers: headers,
        }).then(function (response) {
            $scope.res = response;

            $bar.css({ width: "95%" });
            if (response.status === 200) {
                $bar.css({ width: "100%" });
                setTimeout(function () {
                    document.body.style.cursor = 'default';
                    $scope.clearData();
                    $scope.GetProjectData();
                    $('#fetchProjectModal').modal('hide');

                    var msg = (response.data.RecordCount !== 0)? 'Information Sync Successfully. Rows Affected: ' + response.data.RecordCount : 'All records are updated!';
                    swal(
                        'Successfully Sync.',
                        msg,
                        'success'
                    );
                    $bar.css({ width: "0%" });
                    $scope.ro = false;
                }, 1500);
            }
        }, function (response) {            
            var obj = response.data.Message;
            swal(
                'Error', obj, 'error'
                );

            document.body.style.cursor = 'default';
            $scope.clearData();
            $('#fetchProjectModal').modal('hide');
            $bar.css({ width: "0%" });
            $scope.ro = false;
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
        url = '../api/Project/RemoveRecords';
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
                $scope.GetProjectData();
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
    $scope.DeleteProject = function (data) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                $scope.DeleteProjectConfirmed(data);
            }
        });
    }

    $scope.DeleteProjectConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        url = "/api/Project/RemoveData";
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
                  'Information Delete Successfully',
                  'success'
                );
                $scope.clearData();
                $scope.GetProjectData();
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
