app.controller("SystemParameter", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    $scope.settings = {};
    $scope.countChecked = 0;
    $scope.loading = false;

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        //$scope.pagingInfo.PageUrl = PageUrl;
    }

    $scope.clearData = function () {
        $scope.loading = false;
        document.body.style.cursor = 'default';
        $scope.data = {};
        $scope.data.nvFabIcon = "fa-bars";
    };

    //-----------------------------------------------------------

    $scope.GetSystemParameterData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        urlData = '../api/SystemParameter';

        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.settings = response.data.SYSTEMSETTING;
                $scope.EmailTemplates = response.data.EMAILTEMPLATELIST;
                $scope.Ctrl = response.data.CONTROLS;     

                if ($scope.settings && $scope.settings.Id !== 0) {
                    if ($scope.settings.BsHrFrm !== null) 
                        $scope.settings.BsHrFrm = new Date($scope.settings.BsHrFrm);

                    if ($scope.settings.BsHrTo !== null)
                        $scope.settings.BsHrTo = new Date($scope.settings.BsHrTo);

                    // Formatting TO Cut-Off date relate field
                    if ($scope.settings.TOCutOffDate !== null)
                        $scope.settings.TOCutOffDate = new Date($scope.settings.TOCutOffDate);

                    // Formatting Titling Status Cut-Off date relate field
                    if ($scope.settings.TSCutOffDate !== null)
                        $scope.settings.TSCutOffDate = new Date($scope.settings.TSCutOffDate);

                    // Formatting Effectivity Date Titling Status
                    if ($scope.settings.TitlingStatusEffectivityDate !== null)
                        $scope.settings.TitlingStatusEffectivityDate = new Date($scope.settings.TitlingStatusEffectivityDate);

                    // Formatting Effectivity Date Electric Meter
                    if ($scope.settings.ElectricMeterEffectivityDate !== null)
                        $scope.settings.ElectricMeterEffectivityDate = new Date($scope.settings.ElectricMeterEffectivityDate);
                }
            }
            $timeout(function () {
                $scope.loading = false;
            }, 1000);
        }, function (response) {
            var obj = response.data.Message;
            swal('Loading Failed!', obj, 'error');
            document.body.style.cursor = 'default';
        });
    };

    $scope.UpdateSystemParameter = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.systemParameterForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed',
                'error'
            );
            return;
        }

        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/SystemParameter/SaveSystemParameter';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                swal({
                    title: 'System Message Confirmation',
                    text: 'System settings successfully updated',
                    icon: 'success',
                    dangerMode: false,
                }).then((willUpdate) => {
                    if (willUpdate) {
                        $scope.clearData();
                        $scope.GetSystemParameterData();
                        window.location.reload();
                    }
                });   
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End individual Edit control

    // Start Date Management
    $scope.dateOptions = {
        formatYear: 'yy',
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

    // Effectivity Date Titling Status
    $scope.open2 = function () {
        $scope.popup2.opened = true;
    };

    $scope.popup2 = {
        opened: false
    };

    // Cutoff Date Titling Status
    $scope.open3 = function () {
        $scope.popup3.opened = true;
    };

    $scope.popup3 = {
        opened: false
    };

    // Effectivity Date Electric Meter
    $scope.open4 = function () {
        $scope.popup4.opened = true;
    };

    $scope.popup4 = {
        opened: false
    };

    $scope.dateFormats = ['MM/dd/yyyy', 'dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.dateFormat = $scope.dateFormats[0];

    // End Date Management
}]);