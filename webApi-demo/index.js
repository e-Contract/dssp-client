var demo = angular.module('demo', ['ngRoute', 'ui.bootstrap']);

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

demo.controller('docController', ['$scope', '$http', '$modal', function($scope, $http, $modal) {

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

    $scope.sign = function (docParam) {
        $modal.open({
            templateUrl: 'signDialog.html',
            controller: 'signControler',
            resolve: {
                doc: function () {
                    return docParam;
                }
            }
        });
    }

    $scope.refresh();
        
}]);

demo.controller('signControler', ['$scope', '$modalInstance', 'doc', function ($scope, $modalInstance, doc) {

    $scope.doc = doc;
    $scope.props = {
        location: undefined,
        role: undefined
    };

    $scope.fill = function () {
        $scope.props.location = "Denderleeuw";
        $scope.props.role = "Zaakvoerder";
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
}]);