var demo = angular.module('demo', ['ngRoute']);

demo.config(function ($routeProvider) {
    $routeProvider
		.when('/', {
		    controller: 'docController',
		    templateUrl: 'docs.html'
		})
		.when('/startSign/:docId', {
		    controller: 'signController',
		    templateUrl: 'startSign.html'
		})
        .when('/finishedSign', {
            controller: 'signController',
            templateUrl: 'finishedSign.html'
        })
		.otherwise({
		    redirectTo: '/'
		});
});

demo.controller('docController', ['$scope', '$http', function($scope, $http) {

    $scope.docs = [];

    $scope.refresh = function () {

        $http.get('/api/documents')
            .success(function(data, status, headers, config) {
                $scope.docs = data;
            })
            .error(function(data, status, headers, config) {
                alert(status);
            });

    };

    $scope.refresh();
        
}]);

demo.controller('signController', ['$scope', '$http', '$routeParams', function ($scope, $http, $routeParams) {

    $scope.init = function () {
        $http.post('/api/documents/' + $routeParams.docId + "/signing")
            .success(function (data, status, headers, config) {
                $scope.SignRequest = data.SignRequest;
            })
            .error(function (data, status, headers, config) {
                alert(status);
            });
    
    }

    $scope.init();

}]);