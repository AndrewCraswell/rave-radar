function RaveRadar_Sync() {
  var req = require('request');
  req.get({
        url: "http://raveradar.azurewebsites.net/api/sync"
    },
    function(error, result, body) {
     if (result.statusCode == 200)
     {
        console.info("Raves Synced: " + body);
     } else {
        console.warn("Unable to sync events. Returned status code " + result.statusCode + ".");
     }
    });
}