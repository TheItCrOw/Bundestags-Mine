var DailyPaperHandler = (function (data, graphId) {

    // The default value of how many searchresults we want to show per page.
    //GlobalSearchHandler.prototype.defaultTakeSize = 30;

    // Constructor
    function DailyPaperHandler() { }

    // Builds the main big topic bar chart
    DailyPaperHandler.prototype.buildTopicBarChart = async function (data, graphId) {
        data = JSON.parse(data);

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
                        grid: {
                            display: false
                        }
                    }
                }
            },
        });

        chart.data = {
            labels: data.map(a => a.value),
            datasets: [{
                data: data.map(a => a.count),
                backgroundColor: ['gold'].concat(data.map(d => 'lightgray')),
                borderColor: 'gray',
                borderWidth: '1'
            }],
        }
        chart.update();
    }

    // Builds the main big stacked topic bar chart
    DailyPaperHandler.prototype.buildTopicSentimentStackedBarChart = async function (data, graphId) {
        data = JSON.parse(data);

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
            labels: data.map(a => a.ne),
            datasets,
        }
        chart.update();
    }

    // Builds the agenda vertical bar chart
    DailyPaperHandler.prototype.buildAgendaBarChart = async function (data, graphId) {
        data = JSON.parse(data);

        var chart = new Chart($(`#${graphId}`), {
            type: 'bar',
            options: {
                indexAxis: 'y', //makes the bar horizontal
                maintainAspectRatio: false,
                legend: {
                    display: false
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function (tooltipItem) {
                                return tooltipItem.label; // show the agendaitem name on hover
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            display: false
                        }
                    }
                }
            },
        });

        chart.data = {
            labels: data.map(a => a.value),
            datasets: [{
                data: data.map(a => a.count),
                backgroundColor: data.map(d => 'lightgray'),
                borderColor: 'gray',
                borderWidth: '1',
            }],
        }
        chart.update();
    }

    // Builds the a sentiment polarArea chart
    DailyPaperHandler.prototype.buildTotalSentimentChartData = async function (data, graphId) {
        data = JSON.parse(data);
        var chart = new Chart($(`#${graphId}`), {
            type: 'polarArea',
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

        chart.data = {
            labels: ["Positiv", "Neutral", "Negativ"],
            datasets: [{
                data: [data.find(s => s.element == 'pos').count,
                    data.find(s => s.element == 'neu').count,
                    data.find(s => s.element == 'neg').count],
                backgroundColor: ['rgba(37, 180, 11, 0.5)', 'rgba(19, 91, 143, 0.6)', 'rgba(196, 24, 24, 0.6)'],
                borderColor: 'rgba(0,0,0,1)',
                borderWidth: '0.4',
                hoverBackgroundColor: ['#69946a', '#607da1', '#e85a5a'],
                hoverBorderColor: "rgba(234, 236, 244, 1)",
            }],
        }
        chart.update();
    }

    return DailyPaperHandler;
}());

// Use this item to call all globalsearch logic
var dailyPaperHandler = new DailyPaperHandler();