{
    // This is the sentiment chart foreach speaker.
    var speakerSentimentChart = buildEmptySentimentSpeakerTopicRadarChart();

    function color(depth, name) {
        if (depth == 0) {
            return "rgba(255,255,255,0.8)";
        }
        else if (depth == 1) {
            if (name == 'CDU/CSU') {
                return 'rgba(0,0,0,0.7)';
            } else if (name == 'SPD') {
                return 'rgba(255, 0, 0, 0.8)';
            } else if (name == 'FDP') {
                return 'rgba(222,216,33, 0.9)';
            } else if (name == 'AfD') {
                return 'rgba(0,145,255, 0.8)';
            } else if (name == 'Die Linke') {
                return 'rgba(255,0,166, 0.8)';
            } else if (name.includes('ndnis')) {
                return 'rgba(60,255,0, 0.7)';
            } else {
                return 'gray';
            }
        } else if (depth == 2) {
            return "rgba(125,125,125,0.6)";
        }
    }

    async function initTopicMapChart(root) {
        // Delete the data before.
        $("#topicMapContent .topic-map-container").html("");
        $("#topicMapContent .topic-map-container").append('<svg class="topic-chart-svg"></svg>');

        // Create new
        var docWidth = window.innerWidth - 120; // 120 from the sidemenu
        var docHeight = window.innerHeight;
        var final = 0;
        if (docHeight > docWidth) final = docWidth;
        else final = docHeight;
        $("#topicMapContent").find("svg").attr("height", final);
        $("#topicMapContent").find("svg").attr("width", final);

        // Load it freshly new.
        var topicSvg = d3.select("#topicMapContent svg"),
            topicMargin = 20,
            diameter = +topicSvg.attr("width"),
            g = topicSvg.append("g").attr("transform", "translate(" + diameter / 2 + "," + diameter / 2 + ")");
        topicSvg.enter().append("svg");

        var pack = d3.pack()
            .size([diameter - topicMargin, diameter - topicMargin])
            .padding(2);

        // Starts the charts stuff here
        var selected = root;
        root = d3.hierarchy(root)
            .sum(function (d) {
                return d.value;
            })
            .sort(function (a, b) { return b.value - a.value; });

        var focus = root,
            nodes = pack(root).descendants(),
            view;

        var circle = g.selectAll("circle")
            .data(nodes)
            .enter().append("circle")
            .attr("class", function (d) { return d.parent ? d.children ? "node" : "node node--leaf speakernode" : "node node--root"; })
            .attr("data-d", function (d) { return d.depth; })
            .attr('data-toggle', 'popover')
            .attr("data-trigger", 'hover')
            .attr('data-speakerid', function (d) { return d.data.speakerId; })
            .attr('data-entityvalue', function (d) { return d.data.NamedEntityLemmaValue; })
            .attr("data-content", function (d) { if (d.data.speakerId == null) return 'Erw\u00e4hnungen: ' + d.value.toLocaleString(); })
            .style("fill", function (d) { return d.children ? color(d.depth, d.data.name) : null; })
            .on("click", async function (d) {
                selected = d;
                if (d.data.speakerId != null) {
                    // Show the occurences of the speaker.
                    var window = $('.topic-speaker-window');

                    // Activate the loader
                    window.find('.loader').show();
                    window.find('.chart').hide();

                    // Fill the window with information
                    window.find('.name').html(d.data.name);
                    window.find('.ne').html(d.data.namedEntityLemmaValue);

                    window.hide(0);
                    window.show(100);
                    var left = $(this).offset().left + $(this).attr('r') * 2;
                    var top = $(this).offset().top;
                    window.offset({ top, left });

                    // speaker portrait
                    window.find('.portrait').attr('src', 'img/Unbekannt.jpg');
                    window.find('.portrait').on('error', function () { $(this).attr('src', 'img/Unbekannt.jpg'); });
                    window.find('.portrait').attr('src', await getSpeakerPortrait(d.data.speakerId));

                    // Load the sentiment values of this speaker.
                    var year = $('.topic-map-info .year').html();
                    var from = '';
                    var to = '';
                    if (year == 'Gesamt') { from = "2016-01-01"; to = "2222-12-31"; }
                    else { from = year + "-01-01"; to = year + "-12-31"; }

                    var speakerChartData = await getSentimentOfSpeakerNamedEntity(d.data.speakerId, d.data.namedEntityLemmaValue, from, to);
                    updateSentimentSpeakerTopicRadarChart(speakerSentimentChart, await buildSentimentSpeakerTopicDataForChart(speakerChartData));

                    // Disable the loader
                    window.find('.loader').fadeOut(200);
                    window.find('.chart').show();
                } else {
                    if (focus !== d) zoom(d), d3.event.stopPropagation();
                }
            });

        var text = g.selectAll("text")
            .data(nodes)
            .enter().append("text")
            .attr("class", "topic-label")
            .style("fill-opacity", function (d) { return d.parent === root ? 1 : 0; })
            .style("display", function (d) { return d.parent === root ? "inline" : "none"; })
            .text(function (d) { return d.data.name; });

        var node = g.selectAll("circle,text");

        topicSvg
            .style("background", color(-1, ""))
            .on("click", function () { zoom(root); });
        zoomTo([root.x, root.y, root.r * 2 + topicMargin]);
        hideNextChildren(root);

        function zoom(d) {
            var focus0 = focus; focus = d;
            // If the just selected is a speaker node, we dont want to zoom, but show a window
            if (selected.data.speakerId != null) return;
            var transition = topicSvg.transition()
                .duration(d3.event.altKey ? 7500 : 750)
                .tween("zoom", function (d) {
                    var i = d3.interpolateZoom(view, [focus.x, focus.y, focus.r * 2 + topicMargin]);
                    return function (t) { zoomTo(i(t)); };
                });
            transition.selectAll(".topic-label")
                .filter(function (d) { return d.parent === focus || this.style.display === "inline"; })
                .style("fill-opacity", function (d) { return d.parent === focus ? 1 : 0; })
                .on("start", function (d) { if (d.parent === focus) this.style.display = "inline"; })
                .on("end", function (d) { if (d.parent !== focus) this.style.display = "none"; });

            // Hide the next children bubbles, otherwise it gets messy
            hideNextChildren(d);
            // Show the current detail info of the clicked bubble
            $('.topic-map-info .header').html(d.data.name);
        }

        function zoomTo(v) {
            var k = diameter / v[2];
            view = v;
            node.attr("transform", function (d) {
                return "translate(" + (d.x - v[0]) * k + "," + (d.y - v[1]) * k + ")";
            });
            circle.attr("r", function (d) {
                return d.r * k;
            });
        }
    }

    function hideNextChildren(d) {
        // Hide the next depth details
        $("#topicMapContent circle").each(function () {
            var depth = $(this).data("d");
            if (depth == d.depth || depth - 1 == d.depth) $(this).show();
            else $(this).hide();
        })
    }

    // Init the topic map
    $(document).ready(async function () {
        await initTopicMapChart(await getTopicMapChartData("2017"));
    })

    // Handle the switching of the time frame year
    $('.topic-map-info .switch-year-btn').on('click', async function () {
        try {
            $("#topicMapContent .chart-loader").show();
            // Hihglight the correct button
            $('.topic-map-info .switch-year-btn').each(function () {
                $(this).removeClass('btn-warning');
                $(this).addClass('btn-dark');
            })
            $(this).removeClass('btn-dark');
            $(this).addClass('btn-warning');

            // Load the year and set the header
            var year = $(this).html();

            // Set the header message
            $('.topic-map-info .year').html(year);
            // Load the map
            await initTopicMapChart(await getTopicMapChartData(year));
            // The chart needs some after loading for some reason... wait a bit longer to hide the loader.
            setTimeout(function () { $("#topicMapContent .chart-loader").fadeOut(500); }, 3500);
        } catch (ex) {
            console.write("Error in ropic map loading " + ex);
            $("#topicMapContent .chart-loader").fadeOut(500);
        }
    })

    // We want to hide the window topic-speaker-window when clicking somwwhere
    $('#topicMapContent').on('click', function (e) {
        if (!e.target.classList.contains('speakernode'))
            $('.topic-speaker-window').hide(200);
    });

    // Handle the expanding of the info window
    $('.topic-map-info .expander').on('click', function () {
        var expanded = $(this).data('expanded');
        var id = $(this).data('id');

        if (expanded) {
            $(this).removeClass('fa-chevron-down');
            $(this).addClass('fa-chevron-up');
            $(`.topic-map-info .content-${id}`).hide(100);
        } else {
            $(this).removeClass('fa-chevron-up');
            $(this).addClass('fa-chevron-down');
            $(`.topic-map-info .content-${id}`).show(100);
        }

        $(this).data('expanded', !expanded);
    });
}