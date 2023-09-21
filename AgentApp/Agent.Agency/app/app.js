﻿
//Angular app
var bookingApp = angular.module('bookingApp', ['ngRoute', 'ngResource', 'ngAnimate', 'ngCookies', 'ngSanitize', 'angular-loading-bar',
    'ngClientPaginate', 'ngSectionHeader', 'ngBookingGrid', 'ngBookingSorter', 'ngTitle',
    'ngModelState', 'ui.bootstrap' 
]);
bookingApp.constant('_', window._); //lodash :)
bookingApp.config(['$routeProvider', '$httpProvider', 'cfpLoadingBarProvider', function ($routeProvider, $httpProvider, cfpLoadingBarProvider) {
    $httpProvider.interceptors.push('customHttpInterceptor');
    //#region Routes
    $routeProvider
        .when('/',
            {
                controller: 'homeCtrl',
                templateUrl: 'app/views/home.html',
                resolve: {}
            })
        .when('/login',
            {
                controller: 'loginCtrl',
                templateUrl: 'app/views/login.html',
                resolve: {}
            })
        .when('/register',
            {
                controller: 'registerCtrl',
                templateUrl: 'app/views/register.html',
                resolve: {}
            })
        .when('/booking',
            {
                controller: 'bookingCtrl',
                templateUrl: 'app/views/booking.html',
                resolve: {}
            })
        .when('/accomodations/create',
            {
                controller: 'createAcomodationCtrl',
                templateUrl: 'app/views/create_accomodation.html',
                resolve: {}
            })
        .when('/accomodations/edit/:accomodationId',
            {
                controller: 'editAcomodationCtrl',
                templateUrl: 'app/views/edit_accomodation.html',
                resolve: {}
            })
        .when('/accomodations/:locationId',
            {
                controller: 'accomodationsCtrl',
                templateUrl: 'app/views/accomodations.html',
                resolve: {}
            })
        .when('/comments/:accomodationId',
            {
                controller: 'commentsPreviewCtrl',
                templateUrl: 'app/views/commentsPreview.html',
                resolve: {}
            })
        .when('/makereservation/:accomodationId',
            {
                controller: 'selfReservationCtrl',
                templateUrl: 'app/views/selfReservation.html',
                resolve: {}
            })
        .when('/selectedaccomodation/:accomodationId',
            {
                controller: 'selectedAccomodationCtrl',
                templateUrl: 'app/views/selectedAccomodation.html',
                resolve: {}
            })
        .when('/agentsettings',
            {
                controller: 'agentSettingsCtrl',
                templateUrl: 'app/views/agentSettings.html',
                resolve: {}
            })
        .when('/messages',
            {
                controller: 'messagesCtrl',
                templateUrl: 'app/views/messages.html',
                resolve: {}
            })

        .otherwise({ redirectTo: '/login' });
    //#endregion
}]);
bookingApp.factory('customHttpInterceptor', ['$q', '$rootScope', '$location', '$cookieStore', 'flashService', 'appService', function ($q, $rootScope, $location, $cookieStore, flashService, appService) {
    return {
        response: function (res) {
            $rootScope.modelStateErrors = {};
            if (res.data.status) {
                if (res.data.status.code) {
                    if (res.data.status.code == 200) {
                        flashService.ok(res.data.status);
                    }
                }
            }
            return res;
        },
        responseError: function (res) {
            if ($rootScope.arrReq.length != 0) {
                $rootScope.arrReq.shift();
            }
            if ($rootScope.arrReq.length == 0) {
                appService.refreshScroll(true);
            }

            $rootScope.modelStateErrors = {};

            if (res.status) {
                if (res.status == 0) {
                    flashService.err({
                        code: 599,
                        title: 'offline',
                        messages: { message: 'You are not logged in' }
                    });
                    return $q.reject(res);
                }
                if (res.data) {
                    if (res.data.status) {
                        if (res.data.status.code == 401) {
                            flashService.err(res.data.status);
                            $location.path('/login');
                        }
                        $rootScope.modelStateErrors = res.data.status.errors;
                        res.data.status = {
                            errors: ['Error occured', ' Please correct your input and try again']
                        };
                        flashService.err(res.data.status);
                    }
                    else {
                        if (res.data.Message) {
                            res.data.status = {
                                code: 500,
                                message: res.data.Message,
                                messages: [],
                                errors: [res.data.Message, res.data.ExceptionMessage]
                            };
                            flashService.err(res.data.status);
                            $location.path('/login');
                        }
                    }
                }
            }
            else {
                flashService.err({ code: 406, title: 'Status error', messages: { message: 'res.status is undefined/null' } });
            }

            return $q.reject(res);
        }
    };
}]);
bookingApp.run(['$q', '$resource', '$rootScope', '$templateCache', '$cookieStore', '$window', '$timeout', '$locale', '$location', '$routeParams', 'appService', 'dataService', function ($q, $resource, $rootScope, $templateCache, $cookieStore, $window, $timeout, $locale, $location, $routeParams, appService, dataService) {
    //-----------------------------------------------------------------------------------------------------
    // Init all
    appService.init();
    //-----------------------------------------------------------------------------------------------------
    $rootScope.$on('loading', function (event, data) {
        $rootScope.loading = data.show;
    });
    //-----------------------------------------------------------------------------------------------------
    $rootScope.$on('$locationChangeStart', function () {
        $rootScope.modelStateErrors = {};
        $rootScope.route = $location.path();

        if ($rootScope.loggedUser.UserId == '') {
            //$rootScope.changeView('/login');
        }
    });
    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
    });
    $rootScope.$on('cfpLoadingBar:loading', function (event, current, previous) {
        $rootScope.$broadcast('loading', { show: true });
    });
    $rootScope.$on('cfpLoadingBar:completed', function (event, current, previous) {
        $rootScope.$broadcast('loading', { show: false });
    });
    //cfpLoadingBar:loaded
    //-----------------------------------------------------------------------------------------------------
    $rootScope.$on('$viewContentLoaded', function () {
        appService.refreshScroll();
    });
    //-----------------------------------------------------------------------------------------------------
    $rootScope.logout = function () {
        dataService.logout();
    };
    //-----------------------------------------------------------------------------------------------------
}]);
//---------------------------------------------------------------------------------------------------------