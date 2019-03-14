ymaps.ready(init);

function init() {
    if (getAllUrlParams().maptype == "optimalroute") {
        OptimalRoute();
    }
    else {
        if (getAllUrlParams().maptype == "neworders") {
            ShowRoute(getAllUrlParams().maptype);
        }
        if (getAllUrlParams().maptype == "showroute") {
            ShowRoute(getAllUrlParams().maptype);
        }
        if (getAllUrlParams().maptype == "carlocation") {
            geolocation = ymaps.geolocation,
             bascoords = [getAllUrlParams().longitude, getAllUrlParams().latitude],
              myMap = new ymaps.Map('map', {
                  center: bascoords,
                  zoom: 12,
                  controls: ["zoomControl", "searchControl"]
              }, {
                  searchControlProvider: 'yandex#search'
              },
     { suppressMapOpenBlock: true }
     );

            CreateBalloon(bascoords);

        }
        CSharp.MapLoaded();


    }



}