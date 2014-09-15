var demo = angular.module('demo', ['ngRoute', 'ui.bootstrap']);

demo.config(function ($routeProvider) {
    $routeProvider
		.when('/', {
		    controller: 'docController',
		    templateUrl: 'docList.html',
		    resolve: {
		        docs: function (docSrv) {
		            return docSrv.getDocuments()
		        }
		    }
		})
		.otherwise({
		    redirectTo: '/'
		});
});

demo.run(['$rootScope', function ($root) {
    $root.$on('$routeChangeStart', function (e, curr, prev) {
        if (curr.$$route && curr.$$route.resolve) {
            $root.loadingView = true;
        }
    });
    $root.$on('$routeChangeSuccess', function (e, curr, prev) {
        $root.loadingView = false;
    });
}]);

demo.factory('configSrv', ['$http', '$log', function ($http, $log) {
    var sdo = {
        getConfig : function () {
            return $http.get('api/config')
                .success(function (data, status, headers, config) {
                    $log.debug('Got config: ' + data);
                    return data;
                })
                .error(function (data, status, headers, config) {
                    $log.error('Failed get config ' + status + headers);
                });
        },
        setConfig : function (config) {
            return $http.put('api/config', config)
                .success(function (data, status, headers, config) {
                    $log.debug('Saved config');
                })
                .error(function (data, status, headers, config) {
                    $log.error('Failed save config ' + status + headers);
                });
        }
    }
    return sdo;
}]);

demo.factory('docSrv', ['$http', '$log', function ($http, $log) {
    var sdo = {
        getDocuments: function () {
            return $http.get('api/documents')
                .success(function (data, status, headers, config) {
                    $log.debug('Got documents: ' + data);
                    return data;
                })
                .error(function(data, status, headers, config) {
                    $log.error('Failed get documents ' + status + headers);
                });
        }
    }
    return sdo;
}]);

demo.controller('docController', ['$scope', '$modal', 'docs', 'docSrv', 'configSrv', function ($scope, $modal, docs, docSrv, configSrv) {

    $scope.docs = docs.data;

    $scope.refresh = function () {
        docSrv.getDocuments().then(function (promise) {
            $scope.docs = promise.data;
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

    $scope.config = function (config) {
        var modalInstance = $modal.open({
            templateUrl: 'config.html',
            controller: 'configControler',
            resolve: {
                config: function () {
                    return configSrv.getConfig();
                }
            }
        });

        modalInstance.result.then(function (config) {
            configSrv.setConfig(config);
        }, function () {
            
        });
    }
        
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

demo.controller('configControler', ['$scope', '$modalInstance', '$http', 'config', function ($scope, $modalInstance, $http, config) {

    $scope.config = config.data;

    $scope.save = function () {
        $modalInstance.close($scope.config);
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
}]);