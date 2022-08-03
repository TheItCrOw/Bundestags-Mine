// Author: Kevin

/**
 * Creates a new speaker chart and returns it, so we can update it later.
 * */
function buildEmptySpeakerChart(chartName) {
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
        },
    });
}

// Takes in a token data dictionary and updates the given chart with it
function updateSpeakerChart(chart, dataDict) {
    chart.data = {
        labels: Object.keys(dataDict),
        datasets: [
            {
                label: 'Redner(innen)',
                data: Object.values(dataDict),
                borderColor: "black",
                backgroundColor: "steelblue",
            }
        ]
    }

    chart.options = {
        legend: {
            display: false
        },
        plugins: {
            legend: {
                display: false
            },
            tooltip: {
                // Disable the on-canvas tooltip
                enabled: false,

                external: async function (context) {
                    // Tooltip Element
                    let tooltipEl = document.getElementById('speaker-tooltip');
                    // make the tooltip visible
                    $(tooltipEl).show();

                    // Hide if no tooltip
                    const tooltipModel = context.tooltip;
                    if (tooltipModel.opacity === 0) {
                        tooltipEl.style.opacity = 0;
                        return;
                    }

                    // Set caret Position
                    tooltipEl.classList.remove('above', 'below', 'no-transform');
                    if (tooltipModel.yAlign) {
                        tooltipEl.classList.add(tooltipModel.yAlign);
                    } else {
                        tooltipEl.classList.add('no-transform');
                    }

                    function getBody(bodyItem) {
                        return bodyItem.lines;
                    }

                    // This is the info from the chart!
                    const titleLines = tooltipModel.title;
                    const bodyLines = tooltipModel.body.map(getBody);

                    const position = context.chart.canvas.getBoundingClientRect();

                    // Display, position, and set styles for font
                    tooltipEl.style.opacity = 1;
                    tooltipEl.style.position = 'absolute';
                    tooltipEl.style.left = position.left + window.pageXOffset + tooltipModel.caretX + 'px';
                    tooltipEl.style.top = position.top + window.pageYOffset + tooltipModel.caretY + 'px';
                    tooltipEl.style.pointerEvents = 'none';

                    // Set the content of the tooltip
                    var name = titleLines.toString().split('/')[0];
                    var id = titleLines.toString().split('/')[1];
                    var portrait = await getSpeakerPortrait(id);
                    $(tooltipEl).find('.portrait').attr('src', portrait);
                    $(tooltipEl).find('.name').html(name);
                }
            }
        },
        scales: {
            x: {
                ticks: {
                    // Include a dollar sign in the ticks
                    callback: function (value, index, ticks) {
                        // The labels are: fullname/id
                        // We want to show only the fullname, without the id. Thats what we do here.
                        return chart.data.labels[index].split('/')[0];
                    }
                }
            }
        }
    }
    chart.update();
}

// Gets the complete token distribution as a data dict
async function buildSpeakerDataForChart(speakers) {
    var totalSpeaker = {};

    // We save the id with the name but delete the id for the labeling.
    for (var k = 0; k < speakers.length; k++) {
        var curSpeaker = speakers[k];
        var fullname = curSpeaker.firstName + ' ' + curSpeaker.lastName + '/' + curSpeaker.id;
        totalSpeaker[fullname] = curSpeaker.speakerCount;
    }

    return totalSpeaker;
}
