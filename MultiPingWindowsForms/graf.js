// Denne loader inn noen google libraries for chart plotting
google.charts.load('current', {
  packages: ['corechart', 'line']
});
// Denne starter funksjonen vÃ¥r nÃ¥r libraries (eller hele websiden?) er ferdig loadet
google.charts.setOnLoadCallback(startChart);

// Her er kanalen min med data fra vÃ¦rstasjonen
thingspeakURL = "http://2.79-160-77.customer.lyse.net:8080/last";
//thingspeakURL = "http://79.160.77.2:8080/last";
//thingspeakURL = "http://localhost:8080/last";


function startChart() {
  var data = new google.visualization.DataTable();
  data.addColumn('datetime', 'Time');
  /*data.addColumn('number', 'Temperature');
  data.addColumn('number', 'Humidity');*/

    var options = {
        series: {
            0: { targetAxisIndex: 0 }
            // 4: {targetAxisIndex: 4}
        },
        hAxis: {
            title: 'Time',
            format: 'HH:mm',
            gridlines: {
                color: '#333'
            }
        },
        vAxis: {

            // Adds titles to each axis.
            0: {
                title: 'Temps (Celsius)',
                gridlines: {
                    color: '#333'
                }
            },
            /*4: {title: 'Daylight',
            gridlines: {
              color: '#333'
        }}    ,*/
            gridlines: {
                color: '#333'
            },


        },
        backgroundColor: '#000000',
        width: '100%',
        height: '100%',
        interpolateNulls: 'false',
        chartArea: { left: '5%', top: '5%', width: '80%', height: '90%' }
    };

  // Denne oppretter et 'google LineChart'
  var chart = new google.visualization.LineChart(document.getElementById('chart_div'));

    chart.draw(data, options);

    // Denne oppretter en ny funksjon, som skal gÃ¥ hver 5 sek og hente inn 1 rad med data
    (function updatethingspeakgauges() {

      $.getJSON(thingspeakURL, function(json) {

        // Set the variables from the results array
          if (data.getNumberOfColumns() > Object.entries(json).length + 1) { // +1 cause col[0]=timestamp
              data.removeColumn(data.getNumberOfColumns() - 1);
              Object.entries(json).forEach(([key, value]) => {
                  value = null;
              });
          }

		var i=0;
        Object.entries(json).forEach(([key, value]) => {
    			// do something with key and val
          if (data.wg.length<=i+1)  {
			  /*if (key.includes("%"))
				  options.series.push({targetAxisIndex: i});*/
			  data.addColumn('number', value + " " + key, key);
		  }
           else
           	data.setColumnLabel(i+1, value + " " + key)
          i++;				
		});

		if (i>data.getNumberOfColumns())
			removeColumn(i-1);

	    var myVal = [];
        myVal=Object.values(json);
        myVal.unshift(new Date(Date.now()));
        console.log(myVal);
        data.addRow(myVal);
      });

	// Fjern første rad om den er eldre enn en time
      var now = new Date(Date.now());
      now.setHours(now.getHours()-6);
      while (data.getNumberOfRows()>0 &&
			 now > data.getValue(0,0))
		data.removeRow(0);

      chart.draw(data, options);

      // Denne setter timeout pÃ¥ denne funksjonen, dvs gjentar den etter 5 sek
      setTimeout(updatethingspeakgauges, 5000);
    })();
}