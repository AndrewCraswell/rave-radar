// <!CDATA[
var raveMap, pinsClusterer, clusterGrid;
var pinSize = 32;
var defaultZoom = 4;

/// *** On DOM Ready ***
$(function () {
    $('#zoomIn').click(function () {
        raveMap.setView({ zoom: raveMap.getZoom() + 1 });
        hideMapSpinner();
    });

    $('#zoomOut').click(function () {
        raveMap.setView({ zoom: raveMap.getZoom() - 1 });
        showMapSpinner();
    });

    $('#locateUser').click(function () {
        centerOnUserPosition(defaultZoom + 10);
    });

    $('#ddlCities a').click(function () {
        $('#ddlCities .active').first().removeClass('active');
        $(this).addClass('active');
        $('#SelectedCity').text($(this).text());

        centerMapOnCoords($(this).data('lat'), $(this).data('long'));
    });

    $('.feedback').click(function () {
        showFeedbackModal();
    });
});


/// *** FUNCTIONS ***
function initRaveRadarMap(bingMapsKey) {

    // Load the rave map
    raveMap = new Microsoft.Maps.Map(document.getElementById("raveMapContainer"), {
        credentials: bingMapsKey,
        mapTypeId: Microsoft.Maps.MapTypeId.road,
        enableClickableLogo: false,
        enableSearchLogo: false,
        showCopyright: false,
        showDashboard: false,
        showScalebar: false,
        center: new Microsoft.Maps.Location(34.82, -95.25),
        zoom: defaultZoom
    });

    // Configure client side pin clustering
    raveMap.entities.clear();
    pinsClusterer = new PinClusterer(raveMap, {
        gridSize: pinSize * 2,
        pinSize: pinSize,
        onClusterToMap: pinManager,
        debug: false
    });

    // Destroy any popovers when the view is changed
    Microsoft.Maps.Events.addHandler(raveMap, 'viewchangestart', function (e) {
        killActivePopovers();
    });

    getEventPins();
}

function centerOnUserPosition(zoom) {
    var geoLocationProvider = new Microsoft.Maps.GeoLocationProvider(raveMap);
    geoLocationProvider.getCurrentPosition({
        maximumAge: 300000, showAccuracyCircle: false, successCallback: function (result) {
            raveMap.setView({ zoom: zoom });
        }
    });
}

function centerMapOnCoords(lat, long) {
    raveMap.setView({ center: new Microsoft.Maps.Location(lat, long), zoom: defaultZoom + 8 });
}

function bindPopovers() {
    $(document).on('mouseover', 'div.pushpin', function () {
        var ids = $(this).data('event-ids');
        $(this).qtip(
            {
                content: {
                    text: 'Loading...', // The text to use whilst the AJAX request is loading
                    ajax: {
                        url: 'Map/Popover/', // URL to the local file
                        type: 'GET', // POST or GET
                        data: { ids: ids }
                    }
                },
                show: {
                    event: 'mouseover',
                    ready: true,
                    solo: true,
                },
                hide: {
                    event: 'click',
                    delay: 300,
                    fixed: true,
                },
                position: {
                    my: 'center left',
                    at: 'right center',
                    target: $('.pushpin-icon', this),
                    viewport: $(window),
                    adjust: {
                        method: 'flip'
                    }
                },
                style: {
                    classes: 'qtip-bootstrap qtip-shadow',
                    tip: {
                        corner: 'left center',
                        width: 10,
                        height: 10,
                        offset: 5
                    }
                }

            }
        );
    });
}

function getRaveIDsFromArray(raves) {
    var ids = '';
    for (i = 0; i < raves.length; i++) {
        ids += raves[i].ID + ',';
    }
    return ids.substring(0, ids.length - 1);
}

function getFacebookProfileUrlById(id) {
    return 'http://www.facebook.com/' + id;
}

function pinManager(pin) {

    // Generate the pin's HTML markup
    var html = '<div class="pushpin" data-event-ids="' + getRaveIDsFromArray(pin.raves) + '">';
    if (pin.raves.length > 1) {
        html += '<div class="alert-notify-circle notify-upper-left">' + pin.raves.length + '</div>';
    }
    html += '<div class="pushpin-icon" style="background-image:url(' + pin.raves[0].PicURL + ');"></div></div>';

    // Generate and set the pin HTML
    pin.setOptions({
        htmlContent: html
    });
}



//*********************************
// Object Comparators for Sorting
//*********************************
function comparePinsByVenueThenStartDate(a, b) {

    if (a.Location == b.Location) {
        if (a.StartTime < b.StartTime) {
            return -1;
        } else if (a.StartTime > b.StartTime) {
            return 1;
        } else {
            return 0;
        }
    } else if (a.Location < b.Location) {
        return -1;
    } else {
        return 1;
    }
}

function isMultipleVenuesInRaveCollection(raves) {
    venueName = raves[0].Location;
    var multipleVenues = false;
    for (i = 1; i < raves.length && multipleVenues == false; i++) {
        multipleVenues = (venueName != raves[i].Location);
    }
    return multipleVenues;
}



//********************************
// MESSAGE/ALERT FUNCTIONS
// These methods are meant to show, 
// hide, and manage all modals, popups,
// and alerts.
//********************************
function showMapErrorAlert() {
    var container = '#ModalContainer';
    var url = $(container).data('map-error-url');
    renderResponse(container, url, function () {
        $(container).find(':first').fadeIn();
    });
}

function showWelcomeModal() {
    var container = '#ModalContainer';
    var url = $(container).data('welcome-url');
    showModal(container, url);
}

function showFeedbackModal() {
    var container = '#ModalContainer';
    var url = $(container).data('feedback-url');
    showModal(container, url);
}

function showEventInfoModal(ids) {
    if (ids != null) {
        var container = '#ModalContainer';
        var url = $(container).data('events-url') + '?ids=' + ids;
        showModal(container, url);
    }
}

function showModal(container, url, callback) {
    renderResponse(container, url, function () {
        $(container).find(':first').modal();
        if (callback != null) {
            callback();
        }
    });
}

function showMapSpinner() {
    $('#raveMapContainer').spin({ zIndex: 1050, radius: 35, length: 25, width: 8, shadow: true});
}

function hideMapSpinner() {
    $('#raveMapContainer').spin(false);
}

// This utility method will render data returned
// from the URL in a container element, then fire the
// callback method.
function renderResponse(container, url, callback) {
    $.get(url, function (data) {
        $(container).html(data);
        if (callback != null) {
            callback();
        }
    });
}

function killActivePopovers() {
    $('.qtip').each(function () {
        $(this).data('qtip').destroy();
    })
}


function getEventPins() {
    $.ajax({
        url: rootUrl + "Map/AJAXGetRavePins", crossDomain: false, async: false, success: function (response) {
            // Place the pins on the map
            pinsClusterer.cluster(response);
        },
        error: showMapErrorAlert,
        beforeSend: showMapSpinner,
        complete: hideMapSpinner
    });
}
// ]]>