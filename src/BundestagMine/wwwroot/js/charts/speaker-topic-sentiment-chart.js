/**
 * Creates a new Sentiment Radar chart and returns it, so we can update it later.
 * */
function buildEmptySentimentSpeakerTopicRadarChart() {
    return new Chart($(`.speaker-topic-sentiment-chart`), {
        type: 'bar', // TODO: polarArea is cool
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
function updateSentimentSpeakerTopicRadarChart(chart, dataDist) {
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
async function buildSentimentSpeakerTopicDataForChart(sentiments) {
    return distribution = {
        "pos": sentiments.find(s => s.value == 'pos')?.count,
        "neu": sentiments.find(s => s.value == 'neu')?.count,
        "neg": sentiments.find(s => s.value == 'neg')?.count
    };
}