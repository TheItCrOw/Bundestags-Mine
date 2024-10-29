// Author: Kevin
var startCommentThreshold = 15;

$('#commentNetworkGraph').attr('viewBox', `0 0 ${width} ${height}`);

commentNetworkZoom = d3.zoom()
    .on('zoom', function () {
        commentNetworkSVG.attr('transform', d3.event.transform);
    })

var commentNetworkSVG = d3.select("#commentNetworkGraph")
    .append("svg")
    .call(commentNetworkZoom)
    .attr("width", width)
    .attr("height", height)
    .call(d3.zoom().on("zoom", function () {
        commentNetworkSVG.attr("transform", d3.event.transform)
    }))
    .append("g")

function getColorOfParty(party) {
    if (party == 'CDU' || party == 'CSU') {
        return 'black';
    } else if (party == 'SPD') {
        return 'red';
    } else if (party == 'FDP') {
        return 'yellow';
    } else if (party == 'AfD') {
        return 'dodgerblue';
    } else if (party == 'DIE LINKE.') {
        return 'purple';
    } else if (party.includes('NDNIS')) {
        return 'green';
    } else {
        return 'gray';
    }
}

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

var scale = 20;

var simulation = d3.forceSimulation()
    .force("link", d3.forceLink().id(function (d) { return d.id; }))
    .force("charge", d3.forceManyBody())
    .force("center", d3.forceCenter(width / (2 * scale), height / (2 * scale)));

async function initCommentNetwork() {
    var graph = await getCommentNetworkData();
    var link = commentNetworkSVG.append("g")
        .attr("class", "links")
        .selectAll("line")
        .data(graph.links)
        .enter().append("line")
        .attr('data-toggle', 'popover')
        .attr("data-trigger", 'hover')
        .attr("data-content", function (d) { return 'Kommentare: ' + d.value; })
        .attr('data-value', function (d) { return d.value })
        .attr('data-sentiment', function (d) { return d.sentiment })
        .attr('data-source', function (d) { return d.source; })
        .attr('data-target', function (d) { return d.target; })
        .attr('stroke-width', function (d) { return d.value / 3 })
        .attr("stroke", function (d) { return getColorOfSentiment(d.sentiment); });

    var node = commentNetworkSVG.append("g")
        .attr("class", "nodes")
        .selectAll("g")
        .data(graph.nodes)
        .enter().append("g")

    var circles = node.append("circle")
        .attr("r", 5 * scale)
        .attr('data-toggle', 'popover')
        .attr("data-trigger", 'hover')
        .attr("data-content", function (d) { return d.name; })
        .attr("data-party", function (d) { return d.party; })
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
        .attr('data-party', function (d) { return d.party })
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
            .attr("x1", function (d) { return d.source.x * scale; })
            .attr("y1", function (d) { return d.source.y * scale; })
            .attr("x2", function (d) { return d.target.x * scale; })
            .attr("y2", function (d) { return d.target.y * scale; });

        node
            .attr("transform", function (d) {
                return "translate(" + (d.x * scale) + "," + (d.y * scale) + ")";
            })
    }

    $('#commentNetworkGraph').find('line').each(function (line) {
        if ($(this).data('value') <= startCommentThreshold) {
            $(this).hide();
        }
    })
    simulation.stop();
    // Reactive the popovers again for the newly added html
    $('[data-toggle="popover"]').popover();
}

function dragstarted(d) {
    return;
    if (!d3.event.active) simulation.alphaTarget(0.3).restart();
    d.fx = d.x;
    d.fy = d.y;
}

function dragged(d) {
    return;
    d.fx = d3.event.x;
    d.fy = d3.event.y;
}

function dragended(d) {
    return;
    if (!d3.event.active) simulation.alphaTarget(0);
    d.fx = null;
    d.fy = null;
}

// Creates the network chart
$(document).ready(function () { })

// Handles the start of the simulation
$('body').on('click', '.network-start-btn', async function () {
    $('.network-loader').show();
    await initCommentNetwork();
    // End the simulation after 3 seconds. Its a bit laggy.
    simulation.alphaTarget(0.3).restart();
    commentNetworkZoom.scaleBy(commentNetworkSVG.transition().duration(750), 0.05);
    setTimeout(function () { simulation.stop(); }, 7500);
    $('.network-overlay').fadeOut(1000);
    $(this).fadeOut(1000);
    $('.network-loader').fadeOut(100);
})

// This holds all filters
var filter = {
    'MW': startCommentThreshold, 'NEG': true, 'NEU': true, 'NEG': true,
    'CDU': true, 'SPD': true, 'FDP': true, 'AfD': true,
    'LINKE': true, 'GRU': true
};

// This gets called whenever a filter gets changed. We reapply all active filters then.
async function handleFilterChanged() {

    // Handle all line filters
    $('#commentNetworkGraph').find('line').each(function () {
        var val = $(this).data('value');
        var sentiment = $(this).data('sentiment');

        // Comment threshhold
        if (val < filter['MW']) { $(this).hide(); return; }

        // sentiments filter
        if (filter['NEG'] == false && sentiment < 0) { $(this).hide(); return; }
        if (filter['NEU'] == false && sentiment == 0) { $(this).hide(); return; }
        if (filter['POS'] == false && sentiment > 0) { $(this).hide(); return; }

        // party filter
        var s = allSpeaker.find(sp => sp.speakerId == $(this).data('source'));
        var t = allSpeaker.find(sp => sp.speakerId == $(this).data('target'));
        if (s != undefined && t != undefined) {
            if (filter['CDU'] == false && (s.party == 'CDU' || t.party == 'CDU' || s.party == 'CSU' || t.party == 'CSU')) { $(this).hide(); return; }
            if (filter['SPD'] == false && (s.party == 'SPD' || t.party == 'SPD')) { $(this).hide(); return; }
            if (filter['FDP'] == false && (s.party == 'FDP' || t.party == 'FDP')) { $(this).hide(); return; }
            if (filter['AfD'] == false && (s.party == 'AfD' || t.party == 'AfD')) { $(this).hide(); return; }
            if (filter['LINKE'] == false && (s.party == 'DIE LINKE.' || t.party == 'DIE LINKE.')) { $(this).hide(); return; }
            if (filter['GRU'] == false && (s.party.includes('DNIS') || t.party.includes('DNIS'))) { $(this).hide(); return; }
        }

        // If we reach this, show the line.
        $(this).show();
    })

    // Handle the nodes
    $('#commentNetworkGraph').find('circle').each(function () {
        var party = $(this).data('party');

        if (filter['CDU'] == false && (party == 'CDU' || party == 'CSU')) { $(this).hide(); return; }
        if (filter['SPD'] == false && party == 'SPD') { $(this).hide(); return; }
        if (filter['FDP'] == false && party == 'FDP') { $(this).hide(); return; }
        if (filter['AfD'] == false && party == 'AfD') { $(this).hide(); return; }
        if (filter['LINKE'] == false && party == 'DIE LINKE.') { $(this).hide(); return; }
        if (filter['GRU'] == false && party.includes('DNIS')) { $(this).hide(); return; }

        $(this).show();
    });

    // Handle the text
    $('#commentNetworkGraph').find('text').each(function () {
        var party = $(this).data('party');

        if (filter['CDU'] == false && (party == 'CDU' || party == 'CSU')) { $(this).hide(); return; }
        if (filter['SPD'] == false && party == 'SPD') { $(this).hide(); return; }
        if (filter['FDP'] == false && party == 'FDP') { $(this).hide(); return; }
        if (filter['AfD'] == false && party == 'AfD') { $(this).hide(); return; }
        if (filter['LINKE'] == false && party == 'DIE LINKE.') { $(this).hide(); return; }
        if (filter['GRU'] == false && party.includes('DNIS')) { $(this).hide(); return; }

        $(this).show();
    });
}

// Handles the changing of the minimum comment threshhold
$('body').on('change', '.comment-threshold-input', async function () {
    // End the simulation after 3 seconds. Its a bit laggy.
    var val = $(this).val()
    $(this).attr('data-content', val);
    filter['MW'] = val;
    handleFilterChanged();
})

// Handles the showing of neg, neu, pos
$('body').on('change', '.change-sentiment-inputs', async function () {
    var type = $(this).data('type');
    var val = $(this).prop('checked');
    filter[type] = val;
    handleFilterChanged();
})

// Handles the showing parties
$('body').on('change', '.change-party-inputs', async function () {
    var type = $(this).data('type');
    var val = $(this).prop('checked');
    filter[type] = val;
    handleFilterChanged();
})