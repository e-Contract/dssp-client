var demo = angular.module('demo', ['ngRoute']);

demo.config(function ($routeProvider) {
    $routeProvider
		.when('/', {
		    controller: 'docController',
		    templateUrl: 'docList.html'
		})
		.otherwise({
		    redirectTo: '/'
		});
});

demo.controller('docController', ['$scope', '$http', function($scope, $http) {

    $scope.docs = [];

    $scope.refresh = function () {

        $http.get('api/documents')
            .success(function(data, status, headers, config) {
                $scope.docs = data;
            })
            .error(function(data, status, headers, config) {
                alert(status);
            });
    };

    $scope.refresh();
        
}]);