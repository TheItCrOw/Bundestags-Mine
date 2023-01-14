var DailyPaperHandler = (function (data, graphId) {

    // The default value of how many searchresults we want to show per page.
    //GlobalSearchHandler.prototype.defaultTakeSize = 30;

    // Constructor
    function DailyPaperHandler() { }

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
            labels: data.map(a => a.namedEntity),
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

    // Builds a stacked horizontal poll bar chart
    DailyPaperHandler.prototype.buildPollBarChart = async function (data, graphId) {
        data = JSON.parse(data);
        var chart = new Chart($(`#${graphId}`), {
            type: 'bar',
            options: {
                events: [], // We dont want onhover label effects
                indexAxis: 'y', //makes the bar horizontal
                maintainAspectRatio: false, // we set the hight in css
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
                            display: false,
                            drawBorder: false
                        },
                        ticks: {
                            display: false
                        },
                        stacked: true
                    },
                    y: {
                        grid: {
                            display: false,
                            drawBorder: false
                        },
                        ticks: {
                            display: false
                        },
                        stacked: true
                    }
                }
            },
        });

        // We need a dataset foreach poll result type: yes, no, abstention
        var datasets = [];
        var yesDataset = {
            data: [data.find(s => s.pollResult == 'Ja')?.count],
            backgroundColor: '#8acd00',
            barPercentage: 6,
        }
        var noDataset = {
            data: [data.find(s => s.pollResult == 'Nein')?.count],
            backgroundColor: '#e70097',
            barPercentage: 6,
        }
        var abstentionDataset = {
            data: [data.find(s => s.pollResult == 'Enthalten')?.count],
            backgroundColor: '#00b2dc',
            barPercentage: 6,
        }
        var notSubmittedDataset = {
            data: [data.find(s => s.pollResult == 'Nicht abg.')?.count],
            backgroundColor: '#b1b3b4',
            barPercentage: 6,
        }

        datasets.push(yesDataset);
        datasets.push(noDataset);
        datasets.push(abstentionDataset);
        datasets.push(notSubmittedDataset);
        chart.data = {
            labels: data.map(a => a.pollResult),
            datasets
        }
        chart.update();
    }

    // Builds the a sentiment polarArea chart
    DailyPaperHandler.prototype.buildSentimentChart = async function (data, graphId) {
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
                data: [data.find(s => s.value == 'pos').count,
                data.find(s => s.value == 'neu').count,
                data.find(s => s.value == 'neg').count],
                backgroundColor: ['rgba(37, 180, 11, 0.5)', 'rgba(19, 91, 143, 0.6)', 'rgba(196, 24, 24, 0.6)'],
                borderColor: 'rgba(0,0,0,1)',
                borderWidth: '0.4',
                hoverBackgroundColor: ['#69946a', '#607da1', '#e85a5a'],
                hoverBorderColor: "rgba(234, 236, 244, 1)",
            }],
        }
        chart.update();
    }

    // Builds the comment network
    DailyPaperHandler.prototype.buildCommentNetwork = async function (data, graphId) {
        data = JSON.parse(data);
        // We need this for coloring the nodes
        function getColorOfParty(party) {
            if (party.toLowerCase().includes('cdu') || party.toLowerCase().includes('csu')) {
                return 'black';
            } else if (party == 'SPD') {
                return 'red';
            } else if (party == 'FDP') {
                return 'yellow';
            } else if (party == 'AfD') {
                return 'dodgerblue';
            } else if (party.toLowerCase().includes('linke')) {
                return 'purple';
            } else if (party.toLowerCase().includes('ndnis')) {
                return 'green';
            } else {
                return 'gray';
            }
        }

        // Needed for coloring the links
        function getColorOfSentiment(sentiment) {
            if (sentiment == 0.0) {
                return 'steelblue';
            } else if (sentiment < 0 && sentiment > -1) {
                return 'red'
            } else if (sentiment <= -1) {
                return 'darkred';
            } else if (sentiment > 0 && sentiment < 1) {
                return 'green'
            } else if (sentiment >= 1) {
                return 'limegreen';
            }
        }

        // Create the svg where we draw in
        var svg = d3.select(`#${graphId}`).append("svg")
            .call(d3.zoom().on("zoom", function () {
                svg.attr("transform", d3.event.transform)
            }))
            .append("g");

        // The graph is nested in a relative div and rendered into the svg.
        // read the width and height of them to determine the width and heigth for the calcs
        var el = document.getElementById(graphId).getElementsByTagName('svg')[0];
        var dimenson = el.getBoundingClientRect();

        // Set widht and height for calcs
        width = +dimenson.width;
        height = +dimenson.height;

        var simulation = d3.forceSimulation()
            .force("link", d3.forceLink().id(function (d) { return d.id; }))
            .force("charge", d3.forceManyBody())
            .force("center", d3.forceCenter(width / 2, height / 2));

        graph = data; // sets the data here

        var link = svg.append("g")
            .attr("class", "links")
            .selectAll("line")
            .data(graph.links)
            .enter().append("line")
            .attr("stroke-width", function (d) { return Math.sqrt(d.value); })
            .attr("stroke", function (d) { return getColorOfSentiment(d.sentiment); });

        var node = svg.append("g")
            .attr("class", "nodes")
            .selectAll("g")
            .data(graph.nodes)
            .enter().append("g")

        var circles = node.append("circle")
            .attr("r", 5)
            .attr("fill", function (d) { return getColorOfParty(d.party); });

        // Create a drag handler and append it to the node object instead
        var drag_handler = d3.drag()
            .on("start", dragstarted)
            .on("drag", dragged)
            .on("end", dragended);

        drag_handler(node);

        var lables = node.append("text")
            .text(function (d) {
                return d.name;
            })
            .attr('x', 6)
            .attr('y', 3);

        node.append("title")
            .text(function (d) { return d.name; });

        simulation
            .nodes(graph.nodes)
            .on("tick", ticked);

        simulation.force("link")
            .links(graph.links);

        function ticked() {
            link
                .attr("x1", function (d) { return d.source.x; })
                .attr("y1", function (d) { return d.source.y; })
                .attr("x2", function (d) { return d.target.x; })
                .attr("y2", function (d) { return d.target.y; });

            node
                .attr("transform", function (d) {
                    return "translate(" + d.x + "," + d.y + ")";
                })
        }

        function dragstarted(d) {
            if (!d3.event.active) simulation.alphaTarget(0.3).restart();
            d.fx = d.x;
            d.fy = d.y;
        }

        function dragged(d) {
            d.fx = d3.event.x;
            d.fy = d3.event.y;
        }

        function dragended(d) {
            if (!d3.event.active) simulation.alphaTarget(0);
            d.fx = null;
            d.fy = null;
        }
    }

    // Loads a new daily paper and shows it
    DailyPaperHandler.prototype.loadNewDailyPaper = async function (period, protocolNumber) {
        // Show loading
        $('#dailyPaperContent .loader').fadeIn(250);
        // Get the new rendered daily paper
        var view = await getDailyPaper(period, protocolNumber);
        // add the html
        $('#dailyPaperContent .actual-paper-container').html(view);
        // disable loader
        $('#dailyPaperContent .loader').fadeOut(250);
    }

    // Inits the handler. That is the up most paper in the dropdown
    DailyPaperHandler.prototype.init = async function () {
        // We build the newest daily paper in the init
        var selected = $('#dailyPaperContent .daily-papers-select').find("option:selected");
        var param = selected.data('param').split(',');
        var period = param[0];
        var protocolNumber = param[1];

        dailyPaperHandler.loadNewDailyPaper(period, protocolNumber);
    }

    // Handles the adding of a subscription
    DailyPaperHandler.prototype.dailyPaperSubscribe = async function (mail) {
        console.log(mail);

        if (!validateEmail(mail)) {
            $('#dailyPaperMailingListModal .error-message').html("Die E-Mail Adresse ist ungültig.");
            return;
        }

        var post = await postDailyPaperSubscription(mail);
        if (post.status == 200) {
            $('#dailyPaperMailingListModal .info-message').html(post.result);
        } else {
            $('#dailyPaperMailingListModal .error-message').html(post.message);
        }
    }

    return DailyPaperHandler;
}());

// Use this item to call all globalsearch logic
var dailyPaperHandler = new DailyPaperHandler();

// Handle the exapnding of the paper to fullscreen
$('body').on('click', '#dailyPaperContent .fullscreen-btn', function () {
    // For that, we just disable the sidemenu for now
    var menu = $('.sidebar');
    if (menu.hasClass('toggled')) {
        menu.removeClass('toggled');
        menu.show();
    } else {
        menu.addClass('toggled');
        menu.hide();
    }
})

// Handle the opening of the mailing list
$('body').on('click', '#dailyPaperContent .mail-list-btn', function () {
    // For that, we just disable the sidemenu for now
    $('#dailyPaperMailingListModal .error-message').html("");
    $('#dailyPaperMailingListModal .info-message').html("");
    $('#dailyPaperMailingListModal').modal('show');
})

// Handle the subscribe to the mailing list
$('body').on('click', '#dailyPaperMailingListModal .submit-mail-btn', function () {
    // We build the newest daily paper in the init
    // Clear all messages
    $('#dailyPaperMailingListModal .error-message').html("");
    $('#dailyPaperMailingListModal .info-message').html("");
    // Check mail
    var mail = $('#dailyPaperMailingListModal .mail-input').val();
    dailyPaperHandler.dailyPaperSubscribe(mail);
})

// Handle the switching of the daily papers
$('body').on('change', '#dailyPaperContent .daily-papers-select', async function () {
    var selected = $(this).find("option:selected");
    var param = selected.data('param').split(',');
    var period = param[0];
    var protocolNumber = param[1];

    dailyPaperHandler.loadNewDailyPaper(period, protocolNumber);
})

// Handle the changing of the font size
$('body').on('input', '#dailyPaperContent .font-size-range', function () {
    var val = $(this).val();
    $('#dailyPaperContent .actual-paper-container').find('p').css('font-size', parseInt(val));
    $('#dailyPaperContent .actual-paper-container').find('p').css('line-height', (parseInt(val) + 1).toString() + "px");
})
