var topicSearchIsLoading = false;
var selectedReportId = '';

// Handle the clikcing onto the date presets for the user
$('body').on('click', '.date-preset-analysis', function () {
    var from = $(this).data('from');
    var to = $(this).data('to');

    // Resolve 'today'
    if (to == 'today') to = new Date().toISOString().split("T")[0];

    $('#createAnalysisFromDate').val(from);
    $('#createAnalysisToDate').val(to);
})

// Inits and loads all the logic for the topic analysis to work.
async function initTopicAnalysis() {

    // Init the fractions
    for (var i = 0; i < allFractions.length; i++) {
        var fraction = allFractions[i];

        var item = document.createElement('div');
        item.classList.add('fraction-list-item');
        item.classList.add('col-6');
        item.classList.add('justify-content-between');
        item.classList.add('align-items-center');
        item.classList.add('flexed');
        item.innerHTML = `<label class="mb-0">${fraction.id}</label><input type="checkbox"/>`;

        $('.fraction-list').append(item);
    }

    // Init the parties
    for (var i = 0; i < allParties.length; i++) {
        var party = allParties[i];

        var item = document.createElement('div');
        item.classList.add('party-list-item');
        item.classList.add('col-6');
        item.classList.add('justify-content-between');
        item.classList.add('align-items-center');
        item.classList.add('flexed');
        item.innerHTML = `<label class="mb-0">${party.id}</label><input type="checkbox"/>`;

        $('.party-list').append(item);
    }
    speakerSearch('');

    // Init the named entity topic search
    searchTopic('{NULL}');

    // TESTING ====================
    // Send the api request.
    //var id = generateUUID();
    //var newReportView = await postNewTopicAnalysis({
    //    id,
    //    name: "Analysis Test",
    //    from: '2020-06-06',
    //    to: '2023-12-12',
    //    //speakerIds: [],
    //    speakerIds: ['5269E5BC-5911-46C3-848E-08DA10A1BBA9'],
    //    fractions: ['CDU/CSU'],
    //    parties: ['SPD'],
    //    topicLemmaValue: 'FDP'
    //});

    //// Add the new view
    //$('.topic-analysis-content').append(newReportView);
    //selectedReportId = id;
}

// Searches the topics
async function searchTopic(input) {
    $('.topic-list').html('');
    topicSearchIsLoading = true;
    $('#topicAnalysisTopicSearch').attr('disabled', true);
    $('.topic-list-container .loader').show();

    try {
        var nes = await getNamedEntitiesWithSearchString(input);
        for (var i = 0; i < nes.length; i++) {
            var ne = nes[i];

            var item = document.createElement('div');
            item.classList.add('topic-list-item');
            item.classList.add('justify-content-between');
            item.classList.add('align-items-center');
            item.classList.add('flexed');
            item.innerHTML = `<label class="mb-0"><i class="fas fa-globe mr-2"></i><span>${ne.element}</span></label><label class="mb-0">${ne.count}</label>`;

            $('.topic-list').append(item);
        }

    } catch (ex) {
        console.log(ex);
    }

    $('#topicAnalysisTopicSearch').attr('disabled', false);
    $('.topic-list-container .loader').fadeOut(250);
    topicSearchIsLoading = false;
}

// Searches the speakers for the topic analysis
function speakerSearch(input) {
    $('#topicAnalysisSpeakerSearchList').html('');
    input = input.toLowerCase();

    // Look for parties
    allSpeaker.forEach(function (speaker) {
        var org = speaker.party;
        if (org == '' || org == undefined) {
            org = speaker.fraction;
        }
        // If org still undefied
        if (org == undefined) org = 'Partei & Fraktionslos'

        var fullName = speaker.firstName + ' ' + speaker.lastName + ' (' + org + ')';
        var searchName = fullName.replace(/ /g, '').toLowerCase();
        if (searchName.includes(input) || input == '') {
            var item = document.createElement('div');
            item.setAttribute('data-id', speaker.id);
            item.classList.add('search-item');
            item.innerHTML = fullName;

            $('#topicAnalysisSpeakerSearchList').append(item);
        }
    })
}

// Handles the searching start by the button
$('body').on('click', '.add-topic-to-list-btn', function () {
    searchTopic($('#topicAnalysisTopicSearch').val());
})

// Handles the searching start by enter
$('body').on('keypress', '#topicAnalysisTopicSearch', function (e) {
    if (e.which == 13) {
        searchTopic($(this).val());
    }
})

// Handle the searching of the searchbar. Always open the search result list here.
$('body').on('input', '#topicAnalysisSpeakerSearch', function () {
    $('#topicAnalysisSpeakerSearchList').show();
    speakerSearch($(this).val());
})

// Handle the focusing of the searchbar
$('body').on('click', '#topicAnalysisSpeakerSearch', function () {
    $('#topicAnalysisSpeakerSearchList').show();
})

// We only want to close the search list if we click somewhere else but the input
// and the actual list!
$(document).on('click', function (event) {
    if (event.target.classList.contains('exception-click')) return;
    $('#topicAnalysisSpeakerSearchList').hide();
})

// Handle the clicking onto a search item
$('body').on('click', '#analysisConfiguratorModal .search-item', function () {
    var displayName = $(this).html();
    var id = $(this).data('id');
    var type = $(this).data('type');

    $('#topicAnalysisSpeakerSearch').val(displayName);
    $('#topicAnalysisSpeakerSearch').data("id", id);
    $('#topicAnalysisSpeakerSearch').data("type", type);

    // Add the speaker to the list, but check if that speaker isnt alread added.
    var alreadyAdded = false;
    $('.speakers-list .speaker-list-item').each(function () {
        if ($(this).data('id') == id) alreadyAdded = true;
    })
    if (alreadyAdded) return;

    var item = document.createElement('div');
    item.setAttribute('data-id', id);
    item.classList.add('speaker-list-item');
    item.classList.add('flexed');
    item.classList.add('justify-content-between');
    item.classList.add('align-items-center');

    item.innerHTML = `<label class="mb-0">${displayName}</label><a><i class="fas fa-trash-alt delete"></i></a>`;
    $('.speakers-list').append(item);
})

// Deletes a speaker from the added list
$('body').on('click', '.speakers-list .delete', function () {
    $(this).closest('.speaker-list-item').remove();
})

// Handles the selecting of a topic in the topic configurator.
$('body').on('click', '#analysisConfiguratorModal .topic-list .topic-list-item', function () {
    $('#analysisConfiguratorModal .selected-topic').val($(this).find('span').first().html());
})

// Handles the switching of the pages
$('body').on('click', '#analysisConfiguratorModal .switch-page', function () {
    var pageTo = $(this).data('to');

    // If were going to second page, check if the first page is filled in correctly.
    if (pageTo == 2) {
        var error = '';

        var name = $('#analysisConfiguratorModal').find('.name').val();
        if (name == '') error = 'Geben Sie der Analyse einen Namen.';

        var from = $('#analysisConfiguratorModal').find('#createAnalysisFromDate').val();
        var to = $('#analysisConfiguratorModal').find('#createAnalysisToDate').val();
        if (from == '' || to == '') error = 'Geben Sie einen Zeitraum an.';

        if (error != '') {
            $(this).closest('.modal-body').find('.error-msg').html(error);
            return;
        }

        $(this).closest('.modal-body').find('.error-msg').html('');
    }

    $(this).closest('.modal-body').hide(100);
    $('#analysisConfiguratorModal').find(`.page-${pageTo}`).show(100);
})

// Handles the start creation of the analysis.
$('body').on('click', '#analysisConfiguratorModal .create', async function () {
    var topic = $('#analysisConfiguratorModal .selected-topic').val();
    // Disable the button while loading
    var preHtml = $(this).html();
    $(this).html('Lädt...');
    $(this).attr('disabled', true);

    var error = '';
    if (topic == '') {
        error = 'Geben Sie ein Thema an.';
        return;
    }

    // When we're here, we can start the creation of the analysis.
    var name = $('#analysisConfiguratorModal').find('.name').val();
    var from = $('#analysisConfiguratorModal').find('#createAnalysisFromDate').val();
    var to = $('#analysisConfiguratorModal').find('#createAnalysisToDate').val();

    // Gather selected speakers, fractions and parties.
    var speakerIds = [];
    $('#analysisConfiguratorModal').find('.speakers-list .speaker-list-item').each(function () {
        speakerIds.push($(this).data('id'));
    })

    var fractions = [];
    $('#analysisConfiguratorModal').find('.fraction-list .fraction-list-item').each(function () {
        var checked = $(this).find("input").prop('checked');
        if (checked) {
            fractions.push($(this).find('label').first().html());
        }
    })

    var parties = [];
    $('#analysisConfiguratorModal').find('.party-list .party-list-item').each(function () {
        var checked = $(this).find("input").prop('checked');
        if (checked) {
            parties.push($(this).find('label').first().html());
        }
    })

    var id = generateUUID();
    // Send the api request.
    var newReportView = await postNewTopicAnalysis({
        id,
        name,
        from,
        to,
        speakerIds,
        fractions: fractions.map(f => replaceUmlaute(f)),
        parties: parties.map(f => replaceUmlaute(f)),
        topicLemmaValue: replaceUmlaute(topic)
    });

    // Add the new view
    $('.topic-analysis-content').append(newReportView);

    // Add the ribbon button
    $('.topic-analysis-ribbon').append(
        `<a class="btn switch-analysis-btn w-100 rounded-0 text-dark" data-id="${id}"><i class="fas fa-chart-pie"></i> ${name}</a>`);
    selectedReportId = id;
    switchAnalysis(selectedReportId);

    // Enable the button while loading
    $(this).html(preHtml);
    $(this).attr('disabled', false);

    // Close the modal
    $('#analysisConfiguratorModal').modal('hide');
})

// Switches between analysis
async function switchAnalysis(reportId) {
    // Show the correct analysis
    $('.topic-analysis-content .analysis-report').each(function () {
        var id = $(this).data('id');
        if (id == reportId) {
            $(this).show(250);
        } else {
            $(this).hide();
        }
    })

    // highlight the button
    $('.topic-analysis-ribbon .switch-analysis-btn').each(function () {
        var id = $(this).data('id');
        if (id == reportId) {
            $(this).addClass('selected-analysis-btn');
        } else {
            $(this).removeClass('selected-analysis-btn');
        }
    })
}

// Switch the analysis here
$('body').on('click', '.topic-analysis-ribbon .switch-analysis-btn', async function () {
    selectedReportId = $(this).data('id');
    switchAnalysis(selectedReportId);
})

// Switches the pages in an anlysis report
async function switchPage(reportId, toPageNumber) {
    var curReport = $(`.analysis-report[data-id="${reportId}"]`);

    // Dont siwtch when there are no pages left or right.
    if (toPageNumber < 0 || toPageNumber > curReport.find('.page-container').last().data('num')) return;

    curReport.find('.pages .page-container').each(function () {
        var pageNum = $(this).data('num');

        // Reset the page first
        $(this).removeClass('next-page');
        $(this).removeClass('before-page');
        $(this).removeClass('hidden-page');
        $(this).removeClass('adjacent-page');
        $(this).removeClass('current-page');

        // This will be the new next page.
        if (pageNum - 1 == toPageNumber) {
            $(this).addClass('next-page');
            $(this).addClass('adjacent-page');
        }
        // This will be the new before page
        else if (pageNum + 1 == toPageNumber) {
            $(this).addClass('before-page');
            $(this).addClass('adjacent-page');
        }
        // This will be the new current selected page.
        else if (pageNum == toPageNumber) {
            $(this).addClass('current-page');
            $(this).hide();
            $(this).fadeIn(250);
        }
        else {
            $(this).addClass('hidden-page');
        }
    })
}

// Switch the page here
$('body').on('click', '.topic-analysis-content .adjacent-page', async function () {
    switchPage(selectedReportId, $(this).data('num'));
})

// Switch the page with arrows for smaller devices
$('body').on('click', '.topic-analysis-content .arrow', async function () {
    var curNum = $(this).closest('.page-container').data('num');
    if ($(this).hasClass('go-next')) curNum += 1;
    else curNum -= 1;
    switchPage(selectedReportId, curNum);
})

// Builds a page by fetching it rendered from the controller
async function buildPage(obj) {
    try {
        // show the loader
        $(`.analysis-report[data-id="${obj.reportId}"] .page-container[data-num="${obj.pageNumber}"] .page-loader`)
            .fadeIn(500);
        // Hide any info divs.
        $(`.analysis-report[data-id="${obj.reportId}"] .page-container[data-num="${obj.pageNumber}"] .page-info-div`)
            .hide();
        // Save the obj for when we have to reload the page
        $(`.analysis-report[data-id="${obj.reportId}"] .page-container[data-num="${obj.pageNumber}"]`)
            .attr('data-obj', JSON.stringify(obj));

        // This HAS TO be a POST. Why? No clue. Does it makes sense? No. Just leave it as POST.
        const result = await $.ajax({
            url: "/api/DashboardController/BuildReportPage/",
            type: "POST",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: "application/json",
            accepts: {
                text: "application/json"
            },
        });

        if (result.status != '200') {
            // Something went wrong.
            $(`.analysis-report[data-id="${obj.reportId}"] .page-container[data-num="${obj.pageNumber}"] .page-info-div`)
                .show();
        } else {
            // Add the view.
            $(`.analysis-report[data-id="${obj.reportId}"] .page-container[data-num="${obj.pageNumber}"] .page-content`)
                .html(result.result);
        }

        // disable the loader
        $(`.analysis-report[data-id="${obj.reportId}"] .page-container[data-num="${obj.pageNumber}"] .page-loader`)
            .fadeOut(500);
    } catch (error) {
        console.error(error);
    }
}

// Builds the sentiment with ne chart for each page
async function buildSentimentChartForPage(pageId, data) {

    data = JSON.parse(data);
    var chart = new Chart($(`#${pageId}-sentimentChart`), {
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
        labels: ["Positiv", "Neutral", "Negativ"],
        datasets: [{
            data: [data?.find(d => d.value == 'pos')?.count, data?.find(d => d.value == 'neu')?.count, data?.find(d => d.value == 'neg')?.count],
            backgroundColor: ['rgba(37, 180, 11, 0.5)', 'rgba(19, 91, 143, 0.6)', 'rgba(196, 24, 24, 0.6)'],
            borderColor: 'rgba(0,0,0,1)',
            borderWidth: '0.4',
            hoverBackgroundColor: ['#69946a', '#607da1', '#e85a5a'],
            hoverBorderColor: "rgba(234, 236, 244, 1)",
        }],
    }
    chart.update();
}

// Builds the topic compare to other topics chart for each page
async function buildTopicComparedToOtherTopicsChart(pageId, data) {
    data = JSON.parse(data);
    var chart = new Chart($(`#${pageId}-topicComparedToOtherTopicsChart`), {
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
        labels: data.map(a => a.element),
        datasets: [{
            data: data.map(a => a.count),
            backgroundColor: ['gold'].concat(data.map(d => 'lightblue')),
            borderColor: 'gray',
            borderWidth: '1'
        }],
    }
    chart.update();
}

// Open more of the poll
$('body').on('click', '.open-poll', async function () {
    var pre = $(this).html();
    $(this).html('Lädt...');

    try {
        await openPoll($(this).data('id'));
    } catch (ex) {
        console.log('Error opening poll: ' + ex);
    }

    $(this).html(pre);
})