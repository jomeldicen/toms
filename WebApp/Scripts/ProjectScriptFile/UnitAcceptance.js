app.controller("UnitAcceptance", ['$scope', '$http', '$window', '$timeout', '$q', '$log', '$rootScope', function ($scope, $http, $window, $timeout, $q, $log, $rootScope) {
    
    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.UnitAcceptance = [];
    $scope.UnitInfo = null;
    $scope.jomel = false;
    $scope.loading = false;
    $scope.totalItems = 0;
    $scope.disableButton = true;
    $scope.selectedAll = false;
    $scope.dt = new Date();

    // $.fn.datepicker.defaults.format = "mm/dd/yyyy";

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        $scope.paramInfo.PageUrl = PageUrl;
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

            $scope.paramInfo.ProjectID = (item) ? item.Id : 0;
            $scope.paramInfo.CompanyCode = (item) ? item.CompanyCode : '';
            $scope.paramInfo.ProjectCode = (item) ? item.ProjectCode : '';
            $scope.paramInfo.ProjectLocation = (item) ? item.ProjectLocation : '';
        } else if (source === 'UnitCategory') {
            $scope.selectedUnit = null;
            $scope.searchUnit = "";

            $scope.paramInfo.UnitCategory = (item) ? item.code : '';
        } else if (source === 'Unit') {
            $scope.paramInfo.UnitNos = (item) ? item.UnitNos : '';
            $scope.paramInfo.CustomerNos = (item) ? item.CustomerNos : '';
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
            {'name': 'Residential Unit', 'code': 'UN'},
            {'name': 'Parking Unit', 'code': 'PK'}
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
        $scope.UnitInfo = null;
        $scope.UnitAcceptanceInfo = null;
        $scope.totalItems = 0;
        $scope.disableButton = true;
        $scope.disableClientButton = true;
        $scope.UAButtons = { AddButton: true, EditButton: true, CancelButton: true }
    }

    $scope.clearSearch = function () {
        $scope.selectedProject = null;
        $scope.searchProject = "";
        $scope.selectedUnitCategory = null;
        $scope.searchUnitCategory = "";
        $scope.selectedUnit = null;
        $scope.searchUnit = "";

        $scope.paramInfo.ProjectID = 0;
        $scope.paramInfo.CompanyCode = '';
        $scope.paramInfo.ProjectCode = '';
        $scope.paramInfo.UnitNos = '';
        $scope.paramInfo.UnitCategory = '';
        $scope.paramInfo.ProjectLocation = '';
        $scope.paramInfo.CustomerNos = '';

        $scope.Ctrl = false;
        $scope.resetData();
        $scope.UnitInfo = null;

    };

    //-----------------------------------------------------------
    // Set parameter for search field
    $scope.paramInfo = { ProjectID: '0', CompanyCode: '', ProjectCode: '', UnitNos: '', UnitCategory: '', CustomerNos: '', ProjectLocation: '', PageUrl: '' };

    //-----------------------------------------------------------
    $scope.GetSearchData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';

        urlData = '../api/UnitAcceptance/GetSearchData';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
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

    // Buttons
    $scope.UAButtons = { AddButton: false, EditButton: true, CancelButton: false }

    $scope.GetUnitAcceptanceData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        if ($scope.paramInfo.CompanyCode === '0' || $scope.paramInfo.ProjectCode === '' || $scope.paramInfo.UnitNos === '' || $scope.paramInfo.UnitCategory === '') {
            swal(
                'Error Message',
                'Record not found. Please complete search criteria',
                'error'
            );
            return false;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/UnitAcceptance/GetUnitAcceptance';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.paramInfo,
            headers: headers,
        }).then(function (response) {

            if (response.status === 200) {
                $scope.UnitInfo = response.data.UNITINFO;
                $scope.Ctrl = response.data.CONTROLS;

                $scope.UAButtons = { AddButton: false, EditButton: true, CancelButton: false }

                if ($scope.UnitInfo != null) {
                    $scope.jomel = true; // hit the search button
                    $scope.UnitAcceptanceInfo = response.data.UNITACCEPTANCEINFO;
                    $scope.CurUser = response.data.CURUSER;

                    if ($scope.UnitAcceptanceInfo) {
                        if ($scope.UnitAcceptanceInfo.QCDAcceptanceDate !== null)
                            $scope.UnitAcceptanceInfo.QCDAcceptanceDate = new Date($scope.UnitAcceptanceInfo.QCDAcceptanceDate);

                        if ($scope.UnitAcceptanceInfo.FPMCAcceptanceDate !== null)
                            $scope.UnitAcceptanceInfo.FPMCAcceptanceDate = new Date($scope.UnitAcceptanceInfo.FPMCAcceptanceDate);

                        if ($scope.UnitAcceptanceInfo.CreatedDate !== null)
                            $scope.UnitAcceptanceInfo.CreatedDate = new Date($scope.UnitAcceptanceInfo.CreatedDate);

                        $scope.UAButtons = { AddButton: true, EditButton: false, CancelButton: true }
                    }
                } 


                $scope.disableButton = false;
                if ($scope.UnitInfo === null) {
                    swal(
                        'System Message Confirmation',
                        'No record found!',
                        'error'
                    );
                    $scope.UAButtons = { AddButton: false, EditButton: true, CancelButton: false }
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

    // call from other controller
    $scope.GetTransactionLog = function () {
        $('#changeLogModal').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });

        $rootScope.$emit("searchObjectType", 'UnitAcceptanceController');
    }    

    $scope.UpdateAcceptance = function () {
        $scope.UAButtons = { AddButton: false, EditButton: true, CancelButton: false }
    }

    $scope.CancelUnitAcceptanceModal = function () {
        $('#applyChangesModal1b').modal({
            backdrop: 'static',
            keyboard: false,
            show: true
        });
    }

    $scope.CancelUnitAcceptance = function () {
        $scope.GetUnitAcceptanceData();
        $('#applyChangesModal1b').modal('hide');
    };

    // Start individual Edit control
    $scope.UnitAcceptanceModal = function () {
        $scope.$broadcast('show-errors-event');
        if ($scope.acceptanceForm.$invalid) {
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

    $scope.SaveUnitAcceptance = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        data.CompanyCode = $scope.UnitInfo.CompanyCode;
        data.ProjectCode = $scope.UnitInfo.ProjectCode;
        data.UnitNos = $scope.UnitInfo.UnitNos;
        data.UnitCategory = $scope.UnitInfo.UnitCategoryCode;
        data.CustomerNos = $scope.UnitInfo.CustomerNos;
        
        urlData = '../api/UnitAcceptance/SaveUnitAcceptance';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetUnitAcceptanceData();
                $scope.clearData();

                $('#applyChangesModal').modal('hide');
                swal(
                    'System Message Confirmation',
                    'Acceptance Date from CMG has been applied to the unit',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            $('#applyChangesModal').modal('hide');
            swal('Error Message', obj, 'error');
            $scope.clearData(); 
        });
    };
    // End individual Edit control

    // Start Date Management
    $('[data-toggle="tooltip"]').tooltip();


    $scope.dateOptionsDefault = {
        formatYear: 'yy',
        maxDate: new Date(),
        startingDay: 1,
        showWeeks: false,
    };

    //QCD Acceptance Date
    $scope.open1 = function () {
        $scope.popup1.opened = true;
    };

    $scope.popup1 = {
        opened: false
    };

    //FPMC Acceptance Date
    $scope.open2 = function () {
        $scope.popup2.opened = true;
    };

    $scope.popup2 = {
        opened: false
    };

    $scope.dateFormats = ['MM/dd/yyyy', 'dd-MMMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
    $scope.dateFormat = $scope.dateFormats[0];
    $scope.altInputFormats = ['M!/d!/yyyy'];
    // End Date Management
}]);
