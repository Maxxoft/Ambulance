
function OptimalRoute() {

   
    Set(getCoordsArray());

    function getCoordsArray() {
        var ArrayCoords = [];
        for (var i = 0; i < 99999; i++) {
            if (getAllUrlParams()["array" + i.toString()] === undefined) return ArrayCoords;
            ArrayCoords.push([getAllUrlParams()["array" + i.toString()], getAllUrlParams()["array" + (i + 1).toString()]]);
            i++;
        }
        return ArrayCoords;
    }

    function Set(ArrayCoords) {
        for (var i = 0; i < ArrayCoords.length; i++)
            for (var j = 0; j < ArrayCoords.length; j++) {

                ymaps.route([[ArrayCoords[i][0], ArrayCoords[i][1]], [ArrayCoords[j][0], ArrayCoords[j][1]]], {
                    mapStateAutoApply: true
                }
            ).then(function (route) {
                var points = route.getWayPoints();
                CSharp.GetAllRoutes(
                    route.getLength().toString(),
                    points.get(0).geometry.getCoordinates()[0].toString(),
                    points.get(0).geometry.getCoordinates()[1].toString(),
                    points.get(1).geometry.getCoordinates()[0].toString(),
                    points.get(1).geometry.getCoordinates()[1].toString(),
                    route.getJamsTime().toString()
                );
            });

            }


    }



}

/*


    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
     /*  ymaps.route([[ArrayCoords[i][0], ArrayCoords[i][1]], [ArrayCoords[j][0], ArrayCoords[j][1]]]
                        ).then(function (route, i) {
                            var points = route.getWayPoints();
                            var point0 = points.get(0).geometry.getCoordinates();
                            var point1 = points.get(1).geometry.getCoordinates();
                            CSharp.GetAllRoutes(
                                  i.toString(),
                                  point0[0].toString(),
                                  point0[1].toString(),
                                  point1[0].toString(),
                                  point1[1].toString()
                          );
                        });
// sleep();
   
   ymaps.route([[ArrayCoords[i][0], ArrayCoords[i][1]], [ArrayCoords[j][0], ArrayCoords[j][1]]]
            ).then(function (route) {
                var points = route.getWayPoints();
                CSharp.GetAllRoutes(
                    route.getLength().toString(),
                    points.get(0).geometry.getCoordinates()[0].toString(),
                    points.get(0).geometry.getCoordinates()[1].toString(),
                    points.get(1).geometry.getCoordinates()[0].toString(),
                    points.get(1).geometry.getCoordinates()[1].toString()
                );
            });
*/