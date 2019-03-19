
function ShowRoute(maptype) {
  
    var Coords = getCoordsArray();
    Set(Coords);
    // Создаем мультимаршрут и настраиваем его внешний вид с помощью опций.
    var multiRoute = new ymaps.multiRouter.MultiRoute({
        referencePoints: Coords
    }, {
        // Внешний вид путевых точек.
        // Задаем собственную картинку для последней путевой точки.
        wayPointStartIconLayout: "default#image",
        wayPointStartIconImageHref: "BoosterPM.png",
        wayPointStartIconImageSize: [50, 50],
        boundsAutoApply: true
    });

    function customizePoints() {
        multiRoute.model.events.once("requestsuccess", function () {
            points = multiRoute.getWayPoints();
            points.get(0).options.set({
                zIndex: 1000
            });
            points.get(0).events
               .add('click', function (e) {
                   CSharp.SetAddress("");
               });
            for (var i = 1; i < points.getLength(); i++) {

                var currentPoint = points.get(i);

                var color = "islands#redIcon";
                if (getAllUrlParams()["ptstype" + (i - 1).toString()].toString() === "1")
                    color = "islands#orangeIcon";
                if (getAllUrlParams()["ptstype" + (i - 1).toString()].toString() === "2")
                    color = "islands#greenIcon";
                currentPoint.options.set({
                    preset: color,
                    iconContentLayout: ymaps.templateLayoutFactory.createClass(
                        i.toString()
                    )
                });
                
                currentPoint.events
                 .add('click', function (e) {
                     var point = e.get('target');
                     CSharp.OpenPage("btsrorder", point.geometry.getCoordinates()[0].toString(), point.geometry.getCoordinates()[1].toString());
                 });
            }
        });
    }

    myMap = new ymaps.Map('map', {
        center: [getAllUrlParams()["array0"], getAllUrlParams()["array1"]],
        zoom: 12,
        controls: ["zoomControl", "searchControl"]
    }, {
        searchControlProvider: 'yandex#search'
    },
     { suppressMapOpenBlock: true }
    );

    myMap.geoObjects.add(multiRoute);
    customizePoints();

   

    function getCoordsArray() {
        var ArrayCoords = [];
        for (var i = 0; i < 99999; i++) {
            if (getAllUrlParams()["array" + i.toString()] === undefined) return ArrayCoords;
            ArrayCoords.push([getAllUrlParams()["array" + i.toString()], getAllUrlParams()["array" + (i + 1).toString()]]);
            i++;
        }
        return ArrayCoords;
    }


     if (maptype == "neworders") {  
      
        CreatePms();  
    }
  
    function getPmsCoordsArray() {
        var PmsCoords = [];
        for (var i = 0; i < 99999; i++) {
            if (getAllUrlParams()["points" + i.toString()] === undefined) return PmsCoords;
            PmsCoords.push([getAllUrlParams()["points" + i.toString()], getAllUrlParams()["points" + (i + 1).toString()]]);
            i++;
        }
        return PmsCoords;
    }

    function CreatePms()
    {   
        var PmsCoords = getPmsCoordsArray();
      
        for (var i = 0; i < PmsCoords.length; i++) {
            var color = "islands#redDotIcon";
            if (getAllUrlParams()["pmtype" + (i).toString()].toString() === "1")
                color = "islands#orangeDotIcon";
            if (getAllUrlParams()["pmtype" + (i).toString()].toString() === "2")
                color = "islands#greenDotIcon"

            var Pm = new ymaps.Placemark([PmsCoords[i][0], PmsCoords[i][1]], { hasBalloon: false }, { preset: color });
           
            Pm.events
                 .add('click', function (e){
                     var point = e.get('target');
                     CSharp.OpenPage("neworders", point.geometry.getCoordinates()[0].toString(), point.geometry.getCoordinates()[1].toString());
                 });

            myMap.geoObjects.add(
               Pm
                );

        }
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
  

    // Добавляем мультимаршрут на карту.
    myMap.geoObjects.add(multiRoute);



 /*  ymaps.route(getCoordsArray(), {
          mapStateAutoApply: true
   }).then(function (route) {
       myMap.geoObjects.add(route);

   });



    function getCoordsArray() {
        var ArrayCoords = [];
        for (var i = 0; i < 99999; i++) {
            if (getAllUrlParams()["array" + i.toString()] === undefined) return ArrayCoords;
            ArrayCoords.push([getAllUrlParams()["array" + i.toString()], getAllUrlParams()["array" + (i + 1).toString()]]);
            i++;
        }
        return ArrayCoords;
    }
    



/*
function SetSimpleRoute() {

    //var flag = true;
    /*  ymaps.route(getCoordsArray(), {
          mapStateAutoApply: true
      }).then(function (route) {
          myMap.geoObjects.add(route);
          var points = route.getWayPoints(),
              lastPoint = points.getLength() - 1;
          points.options.set('preset', 'islands#redStretchyIcon');
          CSharp.SetAddress(route.getLength().toString());

    Set(getCoordsArray());



    function getCoordsArray() {
        var ArrayCoords = [];
        for (var i = 0; i < 99999; i++) {
            if (getAllUrlParams()["array" + i.toString()] === undefined) return ArrayCoords;
            ArrayCoords.push([getAllUrlParams()["array" + i.toString()], getAllUrlParams()["array" + (i + 1).toString()]]);

        }
        return ArrayCoords;
    }



    function Set(ArrayCoords) {
      for (var i = 0; i < ArrayCoords.length; i++)
            for (var j = 0; j < ArrayCoords.length; j++)
                //if (i != j)
            {
                 ymaps.route([[ArrayCoords[i][0], ArrayCoords[i][1]], [ArrayCoords[j][0], ArrayCoords[j][1]]], {
                    mapStateAutoApply: true
                }
             ).then(function (route) {
                 //myMap.geoObjects.add(route);
                 CSharp.SetAddress(route.getLength().toString(), i.toString(), j.toString());
                
             });
            }
    }


}
.then(function (route) {
                 //myMap.geoObjects.add(route);
                 CSharp.SetAddress(route.getLength().toString());
                
             });
*/




/*
function SetSimpleRoute() {

    //var flag = true;
    /*  ymaps.route(getCoordsArray(), {
          mapStateAutoApply: true
      }).then(function (route) {
          myMap.geoObjects.add(route);
          var points = route.getWayPoints(),
              lastPoint = points.getLength() - 1;
          points.options.set('preset', 'islands#redStretchyIcon');
          CSharp.SetAddress(route.getLength().toString());

    Set(getCoordsArray());



    function getCoordsArray() {
        var ArrayCoords = [];
        for (var i = 0; i < 99999; i++) {
            if (getAllUrlParams()["array" + i.toString()] === undefined) return ArrayCoords;
            ArrayCoords.push([getAllUrlParams()["array" + i.toString()], getAllUrlParams()["array" + (i + 1).toString()]]);

        }
        return ArrayCoords;
    }



    function Set(ArrayCoords) {
      for (var i = 0; i < ArrayCoords.length; i++)
            for (var j = 0; j < ArrayCoords.length; j++)
                //if (i != j)
            {
                 ymaps.route([[ArrayCoords[i][0], ArrayCoords[i][1]], [ArrayCoords[j][0], ArrayCoords[j][1]]], {
                    mapStateAutoApply: true
                }
             ).then(function (route) {
                 //myMap.geoObjects.add(route);
                 CSharp.SetAddress(route.getLength().toString(), i.toString(), j.toString());
                
             });
            }
    }


}
.then(function (route) {
                 //myMap.geoObjects.add(route);
                 CSharp.SetAddress(route.getLength().toString());
                
             });
*/


