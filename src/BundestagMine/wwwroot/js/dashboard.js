// Author: Kevin

// Builds a new dashboard
async function buildNewDashboard(dashboardId, name, fetchId, type, from, to) {
    try {
        // FIrst copy the template
        var $template = $('#dashboardTemplate').clone();
        $('#dashboardsSection').append($template);

        // Set the dasboard id
        $template.attr('id', dashboardId);
        $template.addClass('new-dashboard');
        // Set the header text
        $template.find('.header-name').html(name);
        $template.find('.header-time').html(parseToGermanDate(from) + ' - ' + parseToGermanDate(to));
        // Add the token chart
        addTokenChartToDashboard($template, fetchId, type, from, to);
        // Add the POS chart
        addPOSChartToDashboard($template, fetchId, type, from, to);
        // Add the sentiment radar char
        addSentimentRadarChartToDashboard($template, fetchId, type, from, to);
        // Add the named entity chart
        addNamedEntityChartToDashboard($template, fetchId, type, from, to);

        // Add the speaker chart online to overall and fractions/parties
        if (type == 'Gesamt' || type == 'Fraktion' || type == 'Party') {
            $template.find('.speaker-chart-div').show();
            addSpeakerChartToDashboard($template, fetchId, type, from, to);
        }

    } catch (ex) {

    }
}

// Adds a speaker chart to a dashboard
async function addSpeakerChartToDashboard($dashboard, name, type, from, to) {
    // Get parameters
    var dashboardId = $dashboard.attr('id');

    // Set the header
    $dashboard.find('.speaker-chart').attr('id', dashboardId + '-speakerChart');
    $dashboard.find('.speaker-chart').next('.chart-loader').fadeIn(250);
    $dashboard.find('.speaker-chart-card .minimum').html(0);
    // Build and fill the chart.
    var speakerChart = buildEmptySpeakerChart(dashboardId + '-speakerChart');
    var speakerChartData = await getChartData('Speaker', name, type, from, to);
    updateSpeakerChart(speakerChart, speakerChartData);
    $dashboard.find('.speaker-chart').next('.chart-loader').fadeOut(250);
}

// Adds a topic chart to a dashboard
async function addTopicChartToDashboard($dashboard, name, type, from, to) {
    // Get parameters
    var dashboardId = $dashboard.attr('id');

    // Set the header
    $dashboard.find('.topic-chart').attr('id', dashboardId + '-topicChart');
    $dashboard.find('.topic-chart').next('.chart-loader').fadeIn(250);
    $dashboard.find('.topic-chart-card .minimum').html(0);
    // Build and fill the chart.
    var topicChart = buildEmptyTopicChart(dashboardId + '-topicChart');
    var topicChartData = await getChartData('Topic', name, type, from, to);
    // Set the zoom out btn
    $dashboard.find('.zoom-out-btn').click(function () {
        topicChart.resetZoom();
    });
    updateTopicChart(topicChart, topicChartData);
    $dashboard.find('.topic-chart').next('.chart-loader').fadeOut(250);
}

// Adds a named entity chart to a dashboard
async function addNamedEntityChartToDashboard($dashboard, name, type, from, to) {
    // Get parameters
    var dashboardId = $dashboard.attr('id');

    // Set the header
    $dashboard.find('.named-entity-chart').attr('id', dashboardId + '-namedEntityChart');
    $dashboard.find('.named-entity-chart').next('.chart-loader').fadeIn(250);
    $dashboard.find('.named-entity-chart-card .minimum').html(0);
    // Build and fill the chart.
    var entityChart = buildEmptyNamedEntityChart(dashboardId + '-namedEntityChart');
    var entityChartData = await getChartData('Named Entity', name, type, from, to);
    updateNamedEntityChart(entityChart, entityChartData);
    $dashboard.find('.named-entity-chart').next('.chart-loader').fadeOut(250);
}

// Adds a sentiment radar chart to a dashboard
async function addSentimentRadarChartToDashboard($dashboard, name, type, from, to) {
    // Get parameters
    var dashboardId = $dashboard.attr('id');

    // Set the header
    $dashboard.find('.sentiment-radar-chart').attr('id', dashboardId + '-sentimentRadarChart');
    $dashboard.find('.sentiment-radar-chart').next('.chart-loader').fadeIn(250);
    $dashboard.find('.sentiment-radar-chart-card .minimum').html(0);
    // Build and fill the chart.
    var sentimentChart = buildEmptySentimentRadarChart(dashboardId + '-sentimentRadarChart');
    var sentimentChartData = await getChartData('Sentiment', name, type, from, to);
    updateSentimentRadarChart(sentimentChart, sentimentChartData);
    $dashboard.find('.sentiment-radar-chart').next('.chart-loader').fadeOut(250);
}

// Adds a pos chart to a dashboard
async function addPOSChartToDashboard($dashboard, name, type, from, to) {
    // Get parameters
    var minimum = 20;
    var dashboardId = $dashboard.attr('id');

    // Set the header
    $dashboard.find('.pos-chart').attr('id', dashboardId + '-posChart');
    $dashboard.find('.pos-chart').next('.chart-loader').fadeIn(250);
    $dashboard.find('.pos-chart-card .minimum').html(minimum);

    // Build and fill the chart.
    var posChart = buildEmptyPOSChart(dashboardId + '-posChart');
    var posChartData = await getChartData('POS', name, type, from, to);
    updatePOSChart(posChart, posChartData);
    $dashboard.find('.pos-chart').next('.chart-loader').fadeOut(250);
}

// Adds a token chart to a dashboard
async function addTokenChartToDashboard($dashboard, name, type, from, to) {
    // Get parameters
    var dashboardId = $dashboard.attr('id');

    // Set the header
    $dashboard.find('.token-chart').attr('id', dashboardId + '-tokenChart');
    $dashboard.find('.token-chart').next('.chart-loader').fadeIn(250);
    $dashboard.find('.token-chart-card .minimum').html(20);
    // Build and fill the chart.
    var tokenChart = buildEmptyTokenChart(dashboardId + '-tokenChart');
    var tokenChartData = await getChartData('Token', name, type, from, to);
    updateTokenChart(tokenChart, tokenChartData);
    $dashboard.find('.token-chart').next('.chart-loader').fadeOut(250);
}

// Fetches the data to the given chart from the REST Api
async function getChartData(chartType, fetchId, type, from, to) {
    // Token fetching
    if (chartType == 'Token') {
        var tokens = [];
        if (type == 'Gesamt') {
            tokens = await getTokens(20, from, to);
        } else if (type == 'Redner') {
            tokens = await getTokensOfSpeaker(20, fetchId, from, to);
        } else if (type == 'Fraktion') {
            tokens = await getTokensOfFraction(20, fetchId, from, to);
        } else if (type == 'Party') {
            tokens = await getTokensOfParty(20, fetchId, from, to);
        }
        return await buildTokenDataForChart(tokens);
    }

    // POS fetching
    else if (chartType == 'POS') {
        var pos = [];
        if (type == 'Gesamt') {
            pos = await getPOS(50000, from, to);
        } else if (type == 'Redner') {
            pos = await getPOSOfSpeaker(10, fetchId, from, to);
        } else if (type == 'Fraktion') {
            pos = await getPOSOfFraction(10000, fetchId, from, to);
        } else if (type == 'Party') {
            pos = await getPOSOfParty(10000, fetchId, from, to);
        }
        return await buildPOSDataForChart(pos);
    }

    // Sentiments fetching
    else if (chartType == 'Sentiment') {
        var sentiments = [];
        if (type == 'Gesamt') {
            sentiments = await getSentiment(from, to);
        } else if (type == 'Redner') {
            sentiments = await getSentimentOfSpeaker(fetchId, from, to);
        } else if (type == 'Fraktion') {
            sentiments = await getSentimentOfFraction(fetchId, from, to);
        } else if (type == 'Party') {
            sentiments = await getSentimentOfParty(fetchId, from, to);
        }
        return await buildSentimentDataForChart(sentiments);
    }

    // NE fetching
    else if (chartType == 'Named Entity') {
        var namedEntities = [];
        if (type == 'Gesamt') {
            namedEntities = await getNamedEntity(20, from, to);
        } else if (type == 'Redner') {
            namedEntities = await getNamedEntityOfSpeaker(20, fetchId, from, to);
        } else if (type == 'Fraktion') {
            namedEntities = await getNamedEntityOfFraction(20, fetchId, from, to);
        } else if (type == 'Party') {
            namedEntities = await getNamedEntityOfParty(20, fetchId, from, to);
        }
        return await buildNamedEntityDataForChart(namedEntities);
    }

    // Topic fetching
    else if (chartType == 'Topic') {
        return await buildNamedEntityDataForTopicMap();
    }

    // speaker fetching
    else if (chartType == 'Speaker') {
        // TODO: Adjust this once we have speech count
        if (type == 'Fraktion') {
            return buildSpeakerDataForChart(await getSpeakers(20, from, to, fetchId));
        } else if (type == 'Party') {
            return buildSpeakerDataForChart(await getSpeakers(20, from, to, undefined, fetchId));
        } else if (type == 'Gesamt') {
            return buildSpeakerDataForChart(await getSpeakers(20, from, to));
        }
        return buildSpeakerDataForChart(allSpeaker.slice(0, 20));
    }
}