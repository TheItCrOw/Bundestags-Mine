// Author: Kevin

/**
 * Creates a new Sentiment Radar chart and returns it, so we can update it later.
 * */
function buildEmptySentimentRadarChart(chartName) {
    return new Chart($(`#${chartName}`), {
        type: 'polarArea', // TODO: polarArea is cool
        options: {
            legend: {
                display: false
            },
            plugins: {
                legend: {
                    display: false
                },
            },
        },
    });
}

// Takes in a sentiment data and updates the given chart with it
function updateSentimentRadarChart(chart, dataDist) {
    // Determine the background color
    chart.data = {
        labels: ["Positiv", "Neutral", "Negativ"],
        datasets: [{
            data: [dataDist["pos"], dataDist["neu"], dataDist["neg"]],
            backgroundColor: ['rgba(37, 180, 11, 0.5)', 'rgba(19, 91, 143, 0.6)', 'rgba(196, 24, 24, 0.6)'],
            borderColor: 'rgba(0,0,0,1)',
            borderWidth: '0.4',
            hoverBackgroundColor: ['#69946a', '#607da1', '#e85a5a'],
            hoverBorderColor: "rgba(234, 236, 244, 1)",
        }],
    }
    chart.update();
}

// Takes in the sentiments data from an API fetch and prepares the dictionary for it.
function buildSentimentDataForChart(sentiments) {
    return distribution = {
        "neg": sentiments.find(s => s.value == 'neg')?.count,
        "neu": sentiments.find(s => s.value == 'neu')?.count,
        "pos": sentiments.find(s => s.value == 'pos')?.count
    };

    var i;
    for (i = 0; i < sentiments.length; i++) {
        var sentiment = parseFloat(sentiments[i].sentiment); // This is the value
        var count = parseInt(sentiments[i].count);

        if (sentiment < 0) {
            distribution["neg"] = distribution["neg"] + count;
        } else if (sentiment == 0) {
            distribution["neu"] = distribution["neu"] + count;
        } else if (sentiment > 0) {
            distribution["pos"] = distribution["pos"] + count;
        }
    }
    return distribution;
}
