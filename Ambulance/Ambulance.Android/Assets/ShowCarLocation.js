
//55.030189, 82.919490
var myPlacemark = null;

function CreateBalloon(coords) {
    setBaloon(coords);
}


function setBaloon(coords) {
    if (myPlacemark) {
        myPlacemark.geometry.setCoordinates(coords);
    }
    else {
        myPlacemark = createPlacemark(coords);
        myMap.geoObjects.add(myPlacemark);
    }
    getAddress(coords);
}

function createPlacemark(coords) {
    return new ymaps.Placemark(coords, {
        iconCaption: 'поиск...'
    }, {
        preset: 'islands#violetDotIconWithCaption',
        draggable: false
    });
}

function getAddress(coords) {
    myPlacemark.properties.set('iconCaption', 'поиск...');
    ymaps.geocode(coords).then(function (res) {
        var firstGeoObject = res.geoObjects.get(0);

        myPlacemark.properties
        .set({
            iconCaption: firstGeoObject.properties.get('name'),
            balloonContent: firstGeoObject.properties.get('text')
        });


        CSharp.SetCoords(coords[0].toString(), coords[1].toString());
        CSharp.SetAddress(firstGeoObject.properties.get('text'));

    });
}




