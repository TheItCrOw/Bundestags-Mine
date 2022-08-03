// Author: Kevin

/**
 * Creates a new Named Entity chart and returns it, so we can update it later.
 * */
function buildEmptyNamedEntityChart(chartName) {
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
            },
            scales: {
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                }
            }
        },
    });
}

// Takes in a named enity data and updates the given chart with it
function updateNamedEntityChart(chart, dataDict) {
    chart.data = {
        labels: Array.from(Array(Object.keys(dataDict[0]).length).keys()),
        datasets: [
            {
                labels: Object.keys(dataDict[0]),
                data: Object.values(dataDict[0]),
                borderColor: "lightblue",
                backgroundColor: "lightblue",
            },
            {
                labels: Object.keys(dataDict[1]),
                data: Object.values(dataDict[1]),
                borderColor: "gold",
                backgroundColor: "gold",
            },
            {
                labels: Object.keys(dataDict[2]),
                data: Object.values(dataDict[2]),
                borderColor: "limegreen",
                backgroundColor: "limegreen",
            },
        ],
    }
    chart.options = {
        plugins: {
            legend: {
                display: false,
            },
            tooltip: {
                callbacks: {
                    label: function (tooltipItem) {
                        return tooltipItem.dataset.labels[tooltipItem.parsed.x];
                    }
                }
            }
        },
        //animation: {
        //    onComplete: function () {
        //        var ctx = chart.ctx;
        //        var height = chart.boxes[0].bottom;
        //        ctx.textAlign = "center";
        //        Chart.helpers.each(this.data.datasets.forEach(function (dataset, i) {
        //            var meta = chart.getDatasetMeta(i);
        //            Chart.helpers.each(meta.data.forEach(function (bar, index) {
        //                console.log(bar);
        //                console.log(index);
        //                console.log(dataset.labels);
        //                ctx.fillText(dataset.labels[index], bar.x, height - ((height - bar.y) / 2) - 10);
        //            }), this)
        //        }), this);
        //    }
        //}
    }
    chart.update();
}

// Gets the complete named entity distribution as a data dict
async function buildNamedEntityDataForChart(namedEntities) {
    var totalEntities = [];
    var personDict = {};
    var orgsDict = {};
    var locationsDict = {};

    // LOC
    namedEntities.find(n => n.type == 'LOC').value.forEach(function (entity) {
        locationsDict[entity.value] = entity.count;
    });

    // PER
    namedEntities.find(n => n.type == 'PER').value.forEach(function (entity) {
        personDict[entity.value] = entity.count;
    });

    // ORG
    namedEntities.find(n => n.type == 'ORG').value.forEach(function (entity) {
        orgsDict[entity.value] = entity.count;
    });

    totalEntities[0] = personDict;
    totalEntities[1] = orgsDict;
    totalEntities[2] = locationsDict;
    return totalEntities;
}
