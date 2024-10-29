var top_n = 15;

$('#topicBarRaceChart').attr('viewBox', `0 0 ${width} ${height}`);

// Feel free to change or delete any of the code you see in this editor!
var topicRaceSvg = d3.select("#topicBarRaceChart")
    .append("svg")
    .attr("width", width)
    .attr("height", height);

var tickDuration = 3500;
let barPadding = (height - (margin.bottom + margin.top)) / (top_n * 5);
let year = '2017.10';
let animationCycle = undefined;
let topicRaceticker = undefined;

// Starts a new round of the bar race
async function initTopicRaceChart(data) {
    data.forEach(d => {
        d.value = +d.value,
            d.lastValue = +d.lastValue,
            d.value = isNaN(d.value) ? 0 : d.value,
            d.colour = d3.hsl(Math.random() * 360, 0.75, 0.75)
    });

    let yearSlice = data.filter(d => d.year.toString() == year && !isNaN(d.value))
        .sort((a, b) => b.value - a.value)
        .slice(0, top_n);

    yearSlice.forEach((d, i) => d.rank = i);

    let x = d3.scaleLinear()
        .domain([0, d3.max(yearSlice, d => d.value)])
        .range([0, width - margin.right - 65]);

    let y = d3.scaleLinear()
        .domain([top_n, 0])
        .range([height - margin.top, margin.top + 20]);

    let xAxis = d3.axisTop()
        .scale(x)
        .ticks(width > 500 ? 5 : 2)
        .tickSize(-(height - margin.top - margin.bottom))
        .tickFormat(d => d3.format(',')(d));

    topicRaceSvg.append('g')
        .attr('class', 'axis xAxis')
        .attr('transform', `translate(0, ${margin.top + 20})`)
        .call(xAxis)
        .selectAll('.tick line')
        .classed('origin', d => d == 0);

    topicRaceSvg.selectAll('rect.bar')
        .data(yearSlice, d => d.name)
        .enter()
        .append('rect')
        .attr('class', 'bar')
        .attr('x', x(0) + 1)
        .attr('width', d => x(d.value) - x(0) - 1)
        .attr('y', d => y(d.rank) + 5)
        .attr('height', y(1) - y(0) - barPadding)
        .style('fill', d => d.colour);

    topicRaceSvg.selectAll('text.nameLabel')
        .data(yearSlice, d => d.name)
        .enter()
        .append('text')
        .attr('class', 'nameLabel')
        .attr('x', d => x(d.value) - 8)
        .attr('y', d => y(d.rank) + 5 + ((y(1) - y(0)) / 2) + 1)
        .style('text-anchor', 'end')
        .html(d => d.name);

    topicRaceSvg.selectAll('text.valueLabel')
        .data(yearSlice, d => d.name)
        .enter()
        .append('text')
        .attr('class', 'valueLabel')
        .attr('x', d => x(d.value) + 5)
        .attr('y', d => y(d.rank) + 5 + ((y(1) - y(0)) / 2) + 1)
        .text(d => d.value);

    $('.topic-race-options-container .year').html(year.split('.')[1] + '/' + year.split('.')[0]);

    animationCycle = function (e) {
        var splited = year.split('.');
        var step = splited[1];

        yearSlice = data
            .filter(d => d.year.toString() == year && !isNaN(d.value))
            .sort((a, b) => b.value - a.value)
            .slice(0, top_n);

        $('.topic-race-options-container .year').html(year.split('.')[1] + '/' + year.split('.')[0]);

        if (yearSlice.length > 0) {

            yearSlice.forEach((d, i) => d.rank = i);

            x.domain([0, d3.max(yearSlice, d => d.value)]);

            topicRaceSvg.select('.xAxis')
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .call(xAxis);

            let bars = topicRaceSvg.selectAll('.bar').data(yearSlice, d => d.name);

            bars
                .enter()
                .append('rect')
                .attr('class', d => `bar ${d.name.replace(/\s/g, '_')}`)
                .attr('x', x(0) + 1)
                .attr('width', d => x(d.value) - x(0) - 1)
                .attr('y', d => y(top_n + 1) + 5)
                .attr('height', y(1) - y(0) - barPadding)
                .style('fill', d => d.colour)
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('y', d => y(d.rank) + 5);

            bars
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('width', d => x(d.value) - x(0) - 1)
                .attr('y', d => y(d.rank) + 5);

            bars
                .exit()
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('width', d => x(d.value) - x(0) - 1)
                .attr('y', d => y(top_n + 1) + 5)
                .remove();

            let labels = topicRaceSvg.selectAll('.nameLabel')
                .data(yearSlice, d => d.name);

            labels
                .enter()
                .append('text')
                .attr('class', 'nameLabel')
                .attr('x', d => x(d.value) - 8)
                .attr('y', d => y(top_n + 1) + 5 + ((y(1) - y(0)) / 2))
                .style('text-anchor', 'end')
                .html(d => d.name)
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('y', d => y(d.rank) + 5 + ((y(1) - y(0)) / 2) + 1);

            labels
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('x', d => x(d.value) - 8)
                .attr('y', d => y(d.rank) + 5 + ((y(1) - y(0)) / 2) + 1);

            labels
                .exit()
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('x', d => x(d.value) - 8)
                .attr('y', d => y(top_n + 1) + 5)
                .remove();


            let valueLabels = topicRaceSvg.selectAll('.valueLabel').data(yearSlice, d => d.name);

            valueLabels
                .enter()
                .append('text')
                .attr('class', 'valueLabel')
                .attr('x', d => x(d.value) + 5)
                .attr('y', d => y(top_n + 1) + 5)
                .text(d => d3.format(',.0f')(d.lastValue))
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('y', d => y(d.rank) + 5 + ((y(1) - y(0)) / 2) + 1);

            valueLabels
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('x', d => x(d.value) + 5)
                .attr('y', d => y(d.rank) + 5 + ((y(1) - y(0)) / 2) + 1)
                .tween("text", function (d) {
                    var self = this;
                    let i = d3.interpolateRound(d.lastValue, d.value);
                    return function (t) {
                        self.textContent = d3.format(',')(i(t));
                    };
                });


            valueLabels
                .exit()
                .transition()
                .duration(tickDuration)
                .ease(d3.easeLinear)
                .attr('x', d => x(d.value) + 5)
                .attr('y', d => y(top_n + 1) + 5)
                .remove();
        }

        var today = new Date();
        if (year == `${today.getFullYear()}.${(today.getMonth() + 1)}`) topicRaceticker.stop();

        if (step < 12) year = splited[0] + '.' + (parseInt(step) + 1);
        else year = (parseInt(splited[0]) + 1).toString() + ".1";

        $('.topic-race-options-container .loader').hide();
    }
}

// Pauses the race
$('body').on('click', '.topic-race-options-container .pause', function () {
    if (topicRaceticker) {
        topicRaceticker.stop();
        topicRaceticker = undefined;
        $('.topic-race-options-container .play').show();
        $('.topic-race-options-container .pause').hide();
    }
})

// Starts the race
$('body').on('click', '.topic-race-options-container .play', function () {
    if (!topicRaceticker) {
        $('.topic-race-options-container .loader').show();
        topicRaceticker = d3.interval(e => animationCycle(), tickDuration);
        $('.topic-race-options-container .play').hide();
        $('.topic-race-options-container .pause').show();
    }
})

// Replay the race
$('body').on('click', '.topic-race-options-container .replay', function () {
    if (!topicRaceticker) {
        $('.topic-race-options-container .loader').show();
        year = '2017.10';
        $('.topic-race-options-container .year').html(year.split('.')[1] + '/' + year.split('.')[0]);
        topicRaceticker = d3.interval(e => animationCycle(), tickDuration);

        $('.topic-race-options-container .play').hide();
        $('.topic-race-options-container .pause').show();
    }
})

// Go one month back
$('body').on('click', '.topic-race-options-container .back', function () {
    if (!topicRaceticker) {
        var splited = year.split('.');
        var step = splited[1];

        // cant go further back
        if (year == '2017.10') return;

        if (step > 1) year = splited[0] + '.' + (parseInt(step) - 1);
        else year = (parseInt(splited[0]) - 1).toString() + ".12";

        $('.topic-race-options-container .year').html(year.split('.')[1] + '/' + year.split('.')[0]);
    }
})

// Go one month forward
$('body').on('click', '.topic-race-options-container .forward', function () {
    if (!topicRaceticker) {
        var splited = year.split('.');
        var step = splited[1];

        // cant go further back
        var today = new Date();
        var limit = today.getFullYear() + '.' + (today.getMonth() + 1);
        if (year == limit) return;

        if (step < 12) year = splited[0] + '.' + (parseInt(step) + 1);
        else year = (parseInt(splited[0]) + 1).toString() + ".1";

        $('.topic-race-options-container .year').html(year.split('.')[1] + '/' + year.split('.')[0]);
    }
})

$(document).ready(async function () {
    if (!window.mobileCheck())
        await initTopicRaceChart(await getTopicBarRaceChartData());
    else
        console.log("Not initing the bar race due to mobile device");
})