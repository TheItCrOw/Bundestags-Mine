// Author: Kevin

/**
 * Creates a new token chart and returns it, so we can update it later.
 * */
function buildEmptyTokenChart(chartName) {
    return new Chart($(`#${chartName}`), {
        type: 'line',
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

// Takes in a token data dictionary and updates the given chart with it
function updateTokenChart(chart, dataDict) {
    chart.data = {
        labels: Object.keys(dataDict),
        datasets: [
            {
                label: 'Token',
                data: Object.values(dataDict),
                borderColor: "gold",
                backgroundColor: "gray",
            }
        ]
    }
    chart.update();
}

// Gets the complete token distribution as a data dict
async function buildTokenDataForChart(tokens) {
    var totalToken = {};
    // To get all token, we fetch the REST data foreach fraction
    // Foreach token of this fraction, fill the totalToken dictionary
    for (var k = 0; k < tokens.length; k++) {
        var curToken = tokens[k];
        if (curToken.element in totalToken) {
            totalToken[curToken.element] = totalToken[curToken.element] + curToken.count;
        } else {
            totalToken[curToken.element] = curToken.count;
        }
    }
    return totalToken;
}
