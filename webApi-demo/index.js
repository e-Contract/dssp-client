var demo = angular.module('demo', ['ngRoute', 'ngResource', 'ui.bootstrap', 'angular-loading-bar']);

demo.config(function ($routeProvider) {
    $routeProvider
		.when('/', {
		    controller: 'docController',
		    templateUrl: 'docList.html',
		    resolve: {
		        currentDocs: function (documents) {
		            return documents.query();
		        }
		    }
		})
		.otherwise({
		    redirectTo: '/'
		});
});

demo.factory('config', ['$resource', function ($resource) {
    return $resource('api/config');
}]);

demo.factory('documents', ['$resource', function ($resource) {
    return $resource('api/documents/:id');
}]);

demo.factory('files', ['$resource', function ($resource) {
    return $resource('api/files/:id');
}]);

demo.controller('docController', ['$scope', '$modal', 'currentDocs', 'documents', 'config', function ($scope, $modal, currentDocs, documents, config) {

    $scope.docs = currentDocs;

    $scope.refresh = function () {
        documents.query(function (value) {
            $scope.docs = value;
        });
    };

    $scope.open = function (docParam) {
        window.open('api/files/' + docParam.Id, '_blank', '');
    };

    $scope.sign = function (docParam) {
        $modal.open({
            templateUrl: 'signModal.html',
            controller: 'signControler',
            resolve: {
                doc: function () {
                    return docParam;
                }
            }
        });
    };

    $scope.config = function () {
        var modalInstance = $modal.open({
            templateUrl: 'configModal.html',
            controller: 'configControler',
            resolve: {
                currentConfig: function () {
                    return config.get();
                }
            }
        });

        modalInstance.result.then(function (newConfig) {
            config.save(newConfig);
        }, function () {

        });
    };
        
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

demo.controller('configControler', ['$scope', '$modalInstance', 'currentConfig', function ($scope, $modalInstance, currentConfig) {

    $scope.config = currentConfig;

    $scope.save = function () {
        $modalInstance.close($scope.config);
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
}]);