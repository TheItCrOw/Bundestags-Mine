// Author: Kevin

/**
 * Creates a new POS chart and returns it, so we can update it later.
 * */
function buildEmptyPOSChart(chartName) {
    return new Chart($(`#${chartName}`), {
        type: 'bar',
        options: {
            legend: {
                display: false
            },
            plugins: {
                legend: {
                    display: false
                },                
            }
        },
    });
}

// Takes in a pos data dictionary and updates the given chart with it
function updatePOSChart(chart, dataDict) {
    chart.data = {
        labels: Object.keys(dataDict),
        datasets: [
            {
                label: 'POS',
                data: Object.values(dataDict),
                borderColor: "black",
                backgroundColor: "lightblue",
            }
        ]
    }
    chart.update();
}

// Gets the complete pos distribution as a data dict
async function buildPOSDataForChart(allPos) {
    var totalPos = {};
    // Foreach pos of this fraction, fill the totalPos dictionary
    for (var k = 0; k < allPos.length; k++) {
        var curPos = allPos[k];
        if (curPos.element in totalPos) {
            totalPos[curPos.element] = totalPos[curPos.element] + curPos.count;
        } else {
            totalPos[curPos.element] = curPos.count;
        }
    }
    return totalPos;
}
