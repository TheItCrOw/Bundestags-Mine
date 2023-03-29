// Builds a named entiti with sentiment stacked bar chart into the given element of the graphId
async function buildTopicSentimentStackedBarChart(data, graphId) {
    var chart = new Chart($(`#${graphId}`), {
        type: 'bar',
        options: {
            legend: {
                display: false
            },
            plugins: {
                legend: {
                    display: false
                },
            },
            scales: {
                x: {
                    stacked: true,
                    grid: {
                        display: false
                    }
                },
                y: {
                    stacked: true,
                }
            }
        },
    });

    // We need a dataset foreach sentiment type: neg, neu, pos
    var datasets = [];
    var negDataset = {
        data: data.map(a => a.sentiments.find(s => s.value == 'neg')?.count),
        backgroundColor: '#e85a5a',
        borderColor: 'gray',
        label: 'Negativ'
    }
    var neuDataset = {
        data: data.map(a => a.sentiments.find(s => s.value == 'neu')?.count),
        backgroundColor: '#607da1',
        borderColor: 'gray',
        label: 'Neutral'
    }
    var posDataset = {
        data: data.map(a => a.sentiments.find(s => s.value == 'pos')?.count),
        backgroundColor: '#69946a',
        borderColor: 'gray',
        label: 'Positiv'
    }
    datasets.push(negDataset);
    datasets.push(neuDataset);
    datasets.push(posDataset);

    chart.data = {
        labels: data.map(a => a.namedEntity),
        datasets,
    }
    chart.update();
}