// Author Kevin B�nisch
// In here we store whether we have already added a divider for a new period
var periodToAdded = {};
var showSentimentColors = true;
var runningFulltextAnalysisRequest = undefined;
var runningNLPSpeechStatisticsRequest = undefined;

// Takes in a list of protocols and builds the protocol tree
async function buildProtocolTree(protocols) {
    // Go through each protocol and put it into the tree
    for (var i = 0; i < protocols.length; i++) {
        var protocol = protocols[i];

        // Check if the protocol lies within a new period
        if (periodToAdded[protocol.legislaturePeriod] == undefined) {
            // Build a divider ui element
            var divider = document.createElement('div');
            divider.classList.add('item-divider');
            divider.innerHTML += `<label>${protocol.legislaturePeriod}. Legislaturperiode</label>`;

            // Add the element to the tree
            $('.protocol-tree').append(divider);
            periodToAdded[protocol.legislaturePeriod] = true;
        }

        // create the item wrapper
        var item = document.createElement('div');
        item.classList.add('tree-item');
        item.classList.add('protocol-item');
        item.setAttribute('data-expanded', false);
        // Set the tooltip popover stuff
        item.setAttribute('data-toggle', 'popover');
        item.setAttribute('data-id', protocol.id);
        item.setAttribute('data-param', protocol.legislaturePeriod + ',' + protocol.number);

        item.innerHTML += `<div class='flexed wrapper position-relative'>
                            <i class="fas fa-clipboard-list mr-2"></i>
                            <p class='w-100 mb-0'><b>${protocol.number}</b>. Sitzung vom ${parseToGermanDate(protocol.date)}</p>
                            <div class="flexed align-items-center">
                            <div class="poll-wrapper ${protocol.pollsAmount > 0 ? "flexed" : "display-none"}">
                            <i class="fas fa-poll icon"></i>
                            <div class="polls">${protocol.pollsAmount}</div></div>
                            <i class="arrow fas fa-angle-up"></i></div>
                            <div class="loader">TOP werden geladen...</div></div>`;

        $('.protocol-tree').append(item);
    }
}

// Loads and opens a new tab for a poll
async function openPoll(pollId) {
    // We want to open the details of this poll in the new tab.
    var url = await getBundestagUrlOfPoll(pollId);
    // This means we didnt find the poll. Probably because its a template.
    if (url == "") {
        showToast("Info", "Die Abstimmungs-Ergebnisse wurden leider nicht gefunden." +
            " Möglicherweise wurde die Abstimmung im Bundestag mit einem gänzlich anderen Namen hinterlegt. " +
            "Suchen Sie deshalb gegebenenfalls selbst auf der grade für Sie geöffneten Seite händisch nach der Abstimmung."
        );
        window.open("https://www.bundestag.de/parlament/plenum/abstimmung", '_blank');
        return;
    }
    window.open(url, '_blank').focus();
}

// Handle the clicking onto a tree item
$('body').on('click', '.tree-item', async function () {
    // Check the expanded status
    var expanded = $(this).data('expanded');
    var itemType = '';

    // Check what item we are clicking onto
    if ($(this).hasClass('read-more-item')) return;
    if ($(this).hasClass('poll-item')) {
        $(this).find('.loader').show();
        await openPoll($(this).data('id'));
        $(this).find('.loader').hide();
        return;
    }
    if ($(this).hasClass('agenda-item')) itemType = 'agenda';
    if ($(this).hasClass('protocol-item')) itemType = 'protocol';
    if ($(this).hasClass('speech-item')) itemType = 'speech';

    // Remove any one time markers
    $(this).removeClass('tree-item-marked');

    // If it is currently NOT expanded, we want to expand it!
    var foundEnd = false;
    if (!expanded) {
        // Expand with protocol
        if (itemType == 'protocol') {
            // Make sure the angedaitems are loaded first
            await loadAgendaItemsToProtocol($(this));
            $(this).nextAll('.tree-item').each(function () {
                if ($(this).hasClass('agenda-item') || $(this).hasClass('poll-item')) {
                    $(this).show(100);
                } else if ($(this).hasClass('protocol-item')) {
                    return false;
                }
            });
        }
        // Expand with agenda
        else if (itemType == 'agenda') {
            // Make sure the speeches are loaded first
            await loadSpeechesToAgendaItem($(this));
            // And then show them
            $(this).nextAll('.tree-item').each(function () {
                if ($(this).hasClass('speech-item')) {
                    $(this).show(100);
                } else if ($(this).hasClass('agenda-item')) {
                    return false;
                }
            });
        }
        // If we click onto a speech here, we cant expand anything, but we want to dehighlight all other speeches
        else if (itemType == 'speech') {
            // Get the first previous agendaitem, then dehighlight all speeches of that
            $('.protocol-tree').children('.speech-item').each(function () {
                $(this).removeClass('expanded-item');
            });
        }

        // Store the new status und do some UI stuff
        $(this).addClass('expanded-item');
        if (itemType != 'speech') {
            $(this).data('expanded', true);
            $(this).find('.arrow').removeClass('fa-angle-up').addClass('fa-angle-down');
        }
    }
    // If it is currenlty expanded, then expand it back
    else {
        // Expand back with protocol
        if (itemType == 'protocol') {
            $(this).nextAll('.tree-item').each(function () {
                if ($(this).hasClass('agenda-item') || $(this).hasClass('speech-item') || $(this).hasClass('poll-item')) {
                    if (foundEnd) return false;

                    $(this).hide(100);
                    $(this).data('expanded', false);
                    $(this).removeClass('expanded-item');
                    $(this).find('.arrow').removeClass('fa-angle-down').addClass('fa-angle-up');
                } else {
                    foundEnd = true;
                }
            });
        }
        // Expand back with agenda
        else if (itemType == 'agenda') {
            $(this).nextAll('.tree-item').each(function () {
                if ($(this).hasClass('speech-item')) {
                    if (foundEnd) return false;

                    $(this).hide(100);
                    $(this).data('expanded', false);
                    $(this).removeClass('expanded-item');
                    $(this).find('.arrow').removeClass('fa-angle-down').addClass('fa-angle-up');
                } else {
                    foundEnd = true;
                }
            });
        }

        // Store the new status und do some UI stuff
        $(this).data('expanded', false);
        $(this).removeClass('expanded-item');
        $(this).find('.arrow').removeClass('fa-angle-down').addClass('fa-angle-up');
    }
})

// Adds the agendaitems to a given protocol in the search tree
async function loadAgendaItemsToProtocol($protocol) {
    if ($protocol.data('loaded') == true) return;

    $protocol.find('.loader').fadeIn(250);
    var protocolId = $protocol.data('id');
    var param = $protocol.data('param').split(',');
    var period = param[0];
    var protocol = param[1];

    var result = await getAgendaItemsOfProtocol(protocolId);
    console.log(result);
    var polls = await getPollsOfProtocol(period, protocol);
    agendaItems = result.agendaItems.reverse();

    // Builds the html for an agenda item
    function buildAgendaItemHtml(agendaItem) {
        var number = agendaItem.order;

        var aItem = document.createElement('div');
        aItem.classList.add('tree-item');
        aItem.classList.add('agenda-item');
        aItem.classList.add('display-none');
        aItem.setAttribute('data-expanded', false);
        // We need some parameters to fetch the speeches upon click
        aItem.setAttribute('data-param', period + ',' + protocol + ',' + number);
        // Store whether we have already loaded the speeches of this agendaitem.
        aItem.setAttribute('data-loaded', false);
        aItem.setAttribute('data-id', agendaItem.id);
        aItem.setAttribute('data-description', agendaItem.description);

        // If we have a fake agendaitem, change the icon
        var icon = 'fas fa-exclamation';
        if (number == -1) icon = 'fas fa-exclamation-circle';
        aItem.innerHTML += `<div class='flexed wrapper position-relative'>
                            <i class="${icon} mr-2"></i>
                            <p class='w-100 mb-0'>${agendaItem.title}</p>
                            <i class="arrow fas fa-angle-up"></i>
                            <div class="loader">Reden werden geladen...</div></div>`;
        return aItem;
    }

    // Check if there are unassigned speeches to an agenda item in this protocol
    if (result.unassingableSpeechesCount > 0) {
        var fakeItem = {
            order: -1,
            id: "00000000-0000-0000-0000-000000000000",
            description: "Die Reden werden aus den XML-Protokollen des Bundestags entzogen. In den Protokollen sind jedoch nicht "
                + "die vollständigen Tagesordnungspunkte hinterlegt, wie man sie auf der Website des Bundestags findet. Deshalb "
                + "muss die Bundestags-Mine die TOP von der Website akquerieren, um diese dann auf die Reden der XML-Protokolle zu matchen. "
                + "Leider gibt es Unterschiede zwischen den XML-Protokollen und der Website, weshalb dieses Matching nicht immer korrekt "
                + "stattfinden kann. Dies kann zu falsch eingeordneten Reden in den TOPs oder sogar dem nicht Anzeigen von diesen führen. "
                + "In dieser Auflistung werden die Reden angezeigt, bei denen das der Fall ist, damit diese trotzdem zur Verfügung stehen. "
                + "Es handelt sich hierbei also um Reden, die von der Bundestags-Mine einfach nicht richtig zugeordnet werden können.",
            title: "Nicht zuweisbare Reden"
        }
        var aItem = buildAgendaItemHtml(fakeItem);
        $protocol.after(aItem);
    }

    // First agendaitems.
    for (var i = 0; i < agendaItems.length; i++) {
        var agendaItem = agendaItems[i];
        var aItem = buildAgendaItemHtml(agendaItem);
        // Add the agenda to the protocol
        $protocol.after(aItem);
    }

    // Now the polls
    for (var i = 0; i < polls.length; i++) {
        var poll = polls[i];

        var pItem = document.createElement('div');
        pItem.classList.add('tree-item');
        pItem.classList.add('poll-item');
        // Store whether we have already loaded the speeches of this agendaitem.
        pItem.setAttribute('data-id', poll.id);

        pItem.innerHTML += `<div class='flexed wrapper position-relative' data-trigger="hover" data-toggle="popover"
                            data-content="Mehr zur Abstimmung erfahren" data-placement="top">
                            <i class="fas fa-poll mr-2"></i>
                            <p class='w-100 mb-0'>${poll.title}</p>
                            <i class="arrow fas fa-mouse"></i>
                            <div class="loader">Abstimmung wird geladen...</div></div>`;
        // Add the agenda to the protocol
        $protocol.after(pItem);
    }

    $('[data-toggle="popover"]').popover();
    $protocol.data('loaded', true);
    $protocol.find('.loader').fadeOut(250);
}

// Adds speeches to a given agenda item and adds it to the tree.
async function loadSpeechesToAgendaItem($agenda) {
    if ($agenda.data('loaded') == true) return;

    $agenda.find('.loader').fadeIn(250);
    var param = $agenda.data('param').split(',');
    var period = param[0];
    var protocol = param[1];
    var number = param[2];

    // Now do the speeches
    var speeches = await getSpeechesOfAgendaItem(period, protocol, number);

    for (var i = 0; i < speeches.length; i++) {
        var speech = speeches[i];
        // Fetch the name of the speaker by the speaker id
        var speaker = allSpeaker.find(s => s.speakerId == speech.speakerId);
        var fullname = 'Unbekannt';
        var fractionOrParty = "Parteilos";

        if (speaker != undefined) {
            fullname = speaker.firstName + ' ' + speaker.lastName;
            if (speaker.fraction != undefined) fractionOrParty = speaker.fraction;
            if (speaker.party != undefined) fractionOrParty = speaker.party;
        }
        // Add the speakername to the speaker obj
        speech.speakerName = fullname;

        // Build the speech obj to the tree as a UI element.
        var item = document.createElement('div');
        item.classList.add('tree-item');
        item.classList.add('speech-item');
        // TODO: This must be the speech id someday
        item.setAttribute('data-id', speech.id);

        // Add the summary popover here
        item.setAttribute('data-trigger', 'hover');
        item.setAttribute('data-toggle', 'popover');
        item.setAttribute('data-placement', 'top');
        item.setAttribute('data-html', 'true');
        item.setAttribute('data-content', buildHtmlForSpeechSummaryPopover(speech));
        item.innerHTML += `<div class='flexed wrapper'>
                            <i class="fas fa-comments mr-2"></i>
                            <p class='w-100 mb-0'>Rede von ${speech.speakerName} (${fractionOrParty})</p>
                            </div>`;

        // Add the objects right under the agendaItem
        $agenda.after(item);
    }

    // Lets put a "read more" item at the end of the speech for the agendaitem.
    var item = document.createElement('div');
    item.setAttribute('data-text', $agenda.data('description'));
    item.classList.add('read-more-item');
    item.classList.add('read-more-agenda');
    item.classList.add('tree-item');
    item.classList.add('speech-item');
    item.classList.add('bg-warning');
    item.innerHTML += 'Mehr zum Tagesordnungspunkt <i class="ml-2 fas fa-long-arrow-alt-right"></i>';
    $agenda.after(item);

    $agenda.data('loaded', true);
    $agenda.find('.loader').fadeOut(250);
    // Activate potential popovers
    $('[data-toggle="popover"]').popover();
}

// Handle the clicking onto the read more item
$('body').on('click', '.read-more-agenda', async function () {
    // Show it
    $('.agenda-item-inspector').show(100);

    // Fill the text.
    $('.agenda-item-inspector .content').html($(this).data('text'));

    // Move the inspector
    var left = $(this).offset().left;
    var top = $(this).offset().top + $(this).height();
    $('.agenda-item-inspector').offset({ top: top, left: left });
})

// Handle the clicking onto a speech in the tree view
$('body').on('click', '.speech-item', async function () {
    if ($(this).hasClass('read-more-item')) return;
    var speechId = $(this).data('id');
    insertSpeechIntoFulltextAnalysis(speechId);
})

// Open the speech when opned from different views. We change that in the future maybe...
$('body').on('click', '.open-speech-btn', async function () {
    // Hide potential modals
    $('#speakerInspectorModal').modal('hide');
    $('.nav-item-switcher[data-id="speechContent"]').trigger('click');
    var speechId = $(this).data('id');
    insertSpeechIntoFulltextAnalysis(speechId);
})

// Handles the opening of a protocol/agenda in the protocol tree from a different view.
$('body').on('click', '.open-agenda-item-btn', async function () {
    // Hide potential modals.
    $('#speakerInspectorModal').modal('hide');
    $('.nav-item-switcher[data-id="speechContent"]').trigger('click');

    var $protocol = undefined;
    var $container = $('.protocol-tree');
    var protocolParam = $(this).data('protocolparam');
    var agendaId = $(this).data('agendaid');

    // If we open the agendaitem by legislature and protocl number
    if (protocolParam == undefined || protocolParam == '') return;

    // Open the protocol
    $protocol = $container.find(`.tree-item[data-param="${protocolParam}"]`);
    // If the protocol is alread expanded, dont expand it.
    if ($protocol.data('expanded') == false) {
        $protocol.trigger('click');
    }

    // scroll into vision, but only after the animation
    setTimeout(function () {
        // If we have an agendaitem to highlight, then highlight it.
        var agendaItem = $container.find(`.agenda-item[data-id="${agendaId}"]`);
        if (agendaItem != undefined) {
            agendaItem.addClass('tree-item-marked');
        }
        // Scroll into view.
        $container.animate({
            scrollTop: $protocol.offset().top - $container.offset().top + $container.scrollTop()
        })
    }, 1000);
})

// Handles the preparing of a speech for the fulltext analysis
async function insertSpeechIntoFulltextAnalysis(speechId) {
    // Enable the loading screen
    $('.fulltext-analysis-div ').find('.fulltext-loader').fadeIn(500);
    $('.empty-message').hide();

    var result = await getNLPSpeechById(speechId);
    if (!result) return;
    var speech = result.speech;
    var agendaItem = result.agendaItem;

    var imgSrc = await getSpeakerPortrait(speech.speakerId);
    var speaker = await getSpeakerById(speech.speakerId);
    // Set the speaker image
    $('.fulltext-analysis-div').find('.portrait').attr('src', imgSrc);
    $('.fulltext-analysis-div').find('.portrait').attr('data-id', speaker.speakerId);
    // Set the date
    var protocol = allProtocols.find(p => p.legislaturePeriod == speech.legislaturePeriod && p.number == speech.protocolNumber);
    $('.fulltext-analysis-div').find('.name-badge').html(parseToGermanDate(protocol.date));

    // Set the speaker infos
    $('.fulltext-analysis-div').find('.name').html(speaker ? speaker.firstName + ' ' + speaker.lastName : '');
    $('.fulltext-analysis-div').find('.gender').html(speaker ? speaker.gender : '');
    $('.fulltext-analysis-div').find('.bday').html(speaker ? parseToGermanDate(speaker.birthDate) : '');
    $('.fulltext-analysis-div').find('.religion').html(speaker ? speaker.religion : '');
    $('.fulltext-analysis-div').find('.family').html(speaker ? speaker.maritalStatus : '');
    $('.fulltext-analysis-div').find('.fraction').html(speaker ? speaker.fraction : '');
    $('.fulltext-analysis-div').find('.party').html(speaker ? speaker.party : '');
    $('.fulltext-analysis-div').find('.since').html(speaker ? parseToGermanDate(speaker.historySince) : '');
    $('.fulltext-analysis-div').find('.job').html(speaker ? speaker.profession : '');
    $('.fulltext-analysis-div').find('.title').html(speaker ? speaker.academicTitle : '');

    // Set the breadcrumbps
    $('.breadcrumbs').find('.period').html('Legislaturperiode ' + speech.legislaturePeriod);
    $('.breadcrumbs').find('.protocol').html('Sitzung ' + speech.protocolNumber);
    $('.breadcrumbs').find('.agenda').html(agendaItem?.title);
    $('.breadcrumbs').find('.agenda').data('text', agendaItem?.description);

    // Visualize the nlp speech in the content
    var html = await buildHtmlOfSpeech(speech);
    // Add the text to the ui
    $('.analysis-content').html(html);
    // Alread add the english translation to the english tab
    var englishSpeech = speech.englishTranslationOfSpeech;
    if (englishSpeech == null || englishSpeech == '') englishSpeech = 'Übersetzung nicht vorhanden. Diese sollte demnächst durch die Pipeline generiert werden.';
    $('.english-speech-view .english-speech').html(englishSpeech);
    $('.english-speech-view .score').html(speech.englishTranslationScore);

    // Activate popovers again.
    $('[data-toggle="popover"]').popover();

    // Show the anaylis board
    $('.empty-message').hide();
    $('.analysis-wrapper').fadeIn(1000);

    // Check if the user wants the sentiments colors or not
    if (!showSentimentColors) {
        $('.sentence').addClass('bg-transparent');
    }

    // Disable the loading screen
    $('.fulltext-analysis-div').find('.fulltext-loader').fadeOut(500);

    // Start the analysis in the background
    startFulltextAnalysis(speech);
    // Start the statistics in the background
    startNLPSpeechStatistics(speech);
}

// Fetches and sets the statistics view in the fulltext analysis tab
async function startNLPSpeechStatistics(speech) {

    function finish() {
        // Activate popovers again.
        $('[data-toggle="popover"]').popover();
        $('.fulltext-analysis-div .statistics-loader').fadeOut(150);
    }

    // abort any running requests
    if (runningNLPSpeechStatisticsRequest != undefined) {
        runningNLPSpeechStatisticsRequest.abort();
        finish();
    }

    // show loader
    $('.fulltext-analysis-div .statistics-loader').fadeIn(150);

    runningNLPSpeechStatisticsRequest = $.ajax({
        url: "/api/DashboardController/GetNLPSpeechStatisticsView/" + speech.id,
        type: "GET",
        dataType: "json",
        accepts: {
            text: "application/json"
        },
        success: async function (result) {
            var statisticsView = result.result;
            $('.fulltext-analysis-div .speech-statistics-view .content').html(statisticsView);
            finish();
        }
    });
}

// Performs the fulltext analysis of the speech by fetching the annotations and building the html
async function startFulltextAnalysis(speech) {

    function finish() {
        // Once were done, show the legend instead.
        // Hide the legend
        $('.fulltext-analysis-div .legend').show();
        // show progress bar
        $('.fulltext-analysis-div .analysis-loading-div').hide();
        // Activate popovers again.
        $('[data-toggle="popover"]').popover();
    }

    // Stop any running requests
    if (runningFulltextAnalysisRequest != undefined) {
        runningFulltextAnalysisRequest.abort();
        finish();
    }

    // Hide the legend
    $('.fulltext-analysis-div .legend').hide();
    // show progress bar
    $('.fulltext-analysis-div .analysis-loading-div').show();

    // Set the progress bar to 0
    $('.fulltext-analysis-div .analysis-loading-div .progress-bar').get(0).style.width = '30%';

    // Load the annotations for the fulltext analysis. We do this here because we need to store the request to abort it maybe.
    runningFulltextAnalysisRequest = $.ajax({
        url: "/api/DashboardController/GetNLPAnnotationsOfSpeech/" + speech.id,
        type: "GET",
        dataType: "json",
        accepts: {
            text: "application/json"
        },
        success: async function (result) {
            var annotations = result.result;
            speech.tokens = annotations.tokens;
            speech.namedEntities = annotations.namedEntities;
            speech.sentiments = annotations.sentiments;
            $('.fulltext-analysis-div .analysis-loading-div .progress-bar').get(0).style.width = '75%';

            var html = await buildHtmlOfFulltextAnalysis(speech);
            $('.fulltext-analysis-div .analysis-loading-div .progress-bar').get(0).style.width = '90%';
            // Add the text to the ui
            $('.analysis-content').html(html);
            $('.fulltext-analysis-div .analysis-loading-div .progress-bar').get(0).style.width = '100%';

            finish();
        }
    });
}

// Takes in a nlp speech and returns only the text with its comments - no tokens, sentiments etc.
async function buildHtmlOfSpeech(speech) {
    console.log(speech);

    var html = "";

    for (var i = 0; i < speech.segments.length; i++) {
        var curSegment = speech.segments[i];
        html += curSegment.text + "<br/>";

        for (var s = 0; s < curSegment.shouts.length; s++) {
            var curShout = curSegment.shouts[s];
            var shoutHtml = await buildShoutHtmlFromShout(curShout);
            html += shoutHtml;
        }
    }

    return html;
}

// Takes in a nlp speech and parses it into the html we add to the UI
// Idk what I did here, this looks disgusting...
async function buildHtmlOfFulltextAnalysis(speech) {
    try {
        // These are the NE, Tokens etc.
        var html = '';
        html = speech.text;
        var addedText = 0;
        // Traverse the segments so we know where shouts happened
        var curSegment = '';
        var i = 0;
        var lastSegmentEnd = -1;
        var segmentsEndToShouts = {};
        while (curSegment != undefined) {
            // Get the current segment
            var curSegment = speech.segments[i];
            if (curSegment == undefined) break;

            var end = lastSegmentEnd + curSegment.text.length;
            segmentsEndToShouts[end] = curSegment.shouts;
            i++;
            lastSegmentEnd = end;
        }
        var alreadyPlacedQuestion = true;
        var lastSentimentScore = 0;
        // Create a span around each token so we can control it.
        for (var i = 0; i < speech.tokens.length; i++) {
            var token = speech.tokens[i];

            // Add the span around the token and a popover
            var popoverContent = token.value == null ? "Keine Information" : token.value;
            var spanBegin = `<span class="token" data-expanded="true" data-toggle="popover" data-trigger="hover" data-placement="top"
                    data-content="${popoverContent}">`;
            var spanEnd = '</span>';

            // Is this token a namedentity? Then add the entity class.
            var tokenAndEnitySame = speech.namedEntities.find(s => s.begin == token.begin && (s.end == token.end || s.end == token.end + 1));
            // Does the named entity maybe start here
            var tokenAndEntityStart = speech.namedEntities.find(s => s.begin == token.begin);
            // Or does a entity end here?
            var tokenAndEntityEnd = speech.namedEntities.find(s => s.end == token.end || s.end == token.end + 1);

            // Handle the diff entity types
            if (tokenAndEnitySame != undefined) {
                spanBegin = spanBegin.replace('class="token"', `class="token entity ${tokenAndEnitySame.value}"`);
            } else if (tokenAndEntityStart != undefined) {
                spanBegin = spanBegin.insert_at(0, `<b class="entity ${tokenAndEntityStart.value}">`);
            } else if (tokenAndEntityEnd != undefined) {
                spanEnd += '</b>'
            }

            // Is this token the start of a sentence of maybe the end?
            var sentenceAndTokenStart = speech.sentiments.find(s => s.begin == token.begin || s.begin == token.begin - 1);
            var sentenceAndTokenEnd = speech.sentiments.find(s => s.end == token.end || s.end == token.end + 1);
            if (sentenceAndTokenEnd != undefined) {
                lastSentimentScore = sentenceAndTokenEnd.sentimentSingleScore;
                var mode = '<b class=\'text-primary\'>Neutral</b>';
                if (sentenceAndTokenEnd.sentimentSingleScore > 0) {
                    mode = '<b class=\'text-success\'>Positiv</b>';
                } else if (sentenceAndTokenEnd.sentimentSingleScore < 0) {
                    mode = '<b class=\'text-danger\'>Negativ</b>';
                }
                // We have problems with the irregularity of which the sentiments sentences end. They sometimes end one character
                // earlier or later... So we counter that by checking the begin - 1 and end + 1, but this leads to double the
                // info buttons... We we hack them: EVery second info button, we ingore hehe
                if (!alreadyPlacedQuestion) {
                    var tooltip = `Sentiment Wert: ${sentenceAndTokenEnd.sentimentSingleScore}<br/>Satz ist ${mode} gestimmt.`;
                    spanEnd += `</i><i class="far fa-question-circle sentiment-question" data-expanded="true" data-content="${tooltip}" data-toggle="popover" data-trigger="hover" data-placement="bottom" data-html="true"></i>`;
                } else {
                    spanEnd += '</i>'
                }
                alreadyPlacedQuestion = !alreadyPlacedQuestion;
            }
            else if (sentenceAndTokenStart != undefined) {
                var score = sentenceAndTokenStart.sentimentSingleScore;
                // If we got a . or ! or ? or whatever, we cannot take the sss of that. We need to get the sss of the next sentence.
                // Hacky as fuck, I know, but there are so many special cases...
                if (sentenceAndTokenStart.end - sentenceAndTokenStart.begin < 2) score = speech.sentiments.find(s => s.begin == sentenceAndTokenStart.begin + 1)?.sentimentSingleScore;
                var mode = 'neu-sentence';
                if (score > 0) {
                    mode = 'pos-sentence';
                } else if (score < 0) {
                    mode = 'neg-sentence';
                }
                spanBegin = spanBegin.insert_at(0, `<i class="sentence ${mode}">`);
            }

            // Insert the span around the token
            html = html.insert_at(token.begin + addedText, spanBegin);
            addedText += spanBegin.length;
            html = html.insert_at(token.end + addedText, spanEnd);
            addedText += spanEnd.length;
            // If the current token end is the end of a segment, handle it
            if (token.end in segmentsEndToShouts) {
                // Add a simple line break foreach segment
                var segmentBreak = '<br/>';
                html = html.insert_at(token.end + addedText, segmentBreak);
                addedText += segmentBreak.length;

                // If this segment has shouts, add them
                var shouts = segmentsEndToShouts[token.end];
                if (shouts == undefined) continue;
                var keys = Object.keys(shouts);

                for (var k = 0; k < keys.length; k++) {
                    var shout = shouts[keys[k]];
                    var shoutHtml = await buildShoutHtmlFromShout(shout);
                    html = html.insert_at(token.end + addedText, shoutHtml);
                    addedText += shoutHtml.length;
                }

                //var hr = '<hr class="mt-1 mb-1"/>';
                //html = html.insert_at(token.end + addedText, hr);
                //addedText += hr.length;
            }
        }

        return html;
    } catch (ex) {
        console.log("Error in fulltext analysis: ");
        console.log(ex);
        return "";
    }
}

// Builds the html for a shout from a shout
async function buildShoutHtmlFromShout(shout) {
    var shoutImage = 'img/Unbekannt.jpg';
    var shoutName = 'Unbekannt';
    var shoutClass = '';

    if (shout.speakerId != undefined) {
        shoutImage = await getSpeakerPortrait(shout.speakerId);
        shoutName = shout.firstName + " " + shout.lastName;
        shoutClass = 'open-speaker-inspector';
    }

    var shoutHtml = `<div class="shout">
                                <span class="m-0 p-0 ${shoutClass}" data-id="${shout.speakerId}">
                                <img class="shout-img" src=\"${shoutImage}\" onerror="$(this).attr('src', 'img/Unbekannt.jpg')"/>
                                <i class="ml-2 mr-1 fas fa-comment-dots"></i>
                                ${shoutName}:
                                </span>
                                <span class="text-center">
                                "${shout.text}"
                                </span>
                                </div>`;
    return shoutHtml;
}

// Takes in a speech and builds the summary html which we show in a popover
function buildHtmlForSpeechSummaryPopover(speech) {
    // We got a template in the html which we fill with the data we need
    var $template = $('.summary-popover-template').clone();

    var content = speech.abstractSummary;
    if (content == null || content == '')
        content = "Keine Zusammenfassung vorhanden. Dies liegt entweder daran, dass die Rede zu kurz ist, "
            + "um eine Zusammenfassung zu generieren, oder die Pipeline noch kalkuliert.";

    $template.find('.summary-abstract-content').html(content);
    return $template.html();
}

// Handles the opening and closing of the side tree
$('body').on('click', '.close-open-btn', function () {
    var expanded = $(this).data('expanded');

    if (expanded) {
        $('.protocol-tree-content').find('.header').hide();
        $('.protocol-tree-content').find('.protocol-tree').hide();
        $('.protocol-tree-content').find('.protocol-tree-info').hide();
        $('.close-open-btn').find('i').removeClass('fa-chevron-left');
        $('.close-open-btn').find('i').addClass('fa-chevron-right');
    } else {
        $('.protocol-tree-content').find('.header').show();
        $('.protocol-tree-content').find('.protocol-tree').show();
        $('.protocol-tree-content').find('.protocol-tree-info').show();
        $('.close-open-btn').find('i').removeClass('fa-chevron-right');
        $('.close-open-btn').find('i').addClass('fa-chevron-left');
    }

    $(this).data('expanded', !expanded);
})

// We want a hover effect over the sentiment info which highlights the according sentence
$('body').on('mouseover', '.sentiment-question', function () {
    $(this).prevAll('.sentence').first().addClass('hovered-sentence');
});

$('body').on('mouseout', '.sentiment-question', function () {
    $(this).prevAll('.sentence').first().removeClass('hovered-sentence');
});

// Handles the activating and deactivating of the sentiment colors 
$('body').on('click', '.small-options-menu .sentiment-color-cb', function () {
    if ($(this).prop('checked') == false) {
        $('.sentence').addClass('bg-transparent');
    } else {
        $('.sentence').removeClass('bg-transparent');
    }
    showSentimentColors = !showSentimentColors;
})

// Handles the switching of the tabs in the fulltext speech view
$('body').on('click', '.fulltext-analysis-div .analysis-menu-header button', function () {
    var targetTab = $(this).data('tab');
    $('.fulltext-analysis-div .analysis-menu-header button').each(function () {
        $(this).removeClass('selected-btn');
    });
    $(this).addClass('selected-btn');

    // Show the right view
    $('.fulltext-analysis-div .tab-view').each(function () {
        if ($(this).hasClass(targetTab)) $(this).fadeIn(150);
        else $(this).fadeOut(150);
    });
})

// Handles the switching of the cards in the statitics view of the nlp speech
$('body').on('click', '.fulltext-analysis-div .speech-statistics-view .expander-btn', function () {
    var targetTab = $(this).data('tab');
    var $card = $(`.fulltext-analysis-div .speech-statistics-view .${targetTab}`);
    var expanded = $card.data('expanded');
    if (expanded) {
        $card.fadeOut(0);
        // Rotate the expander arrow
        $(this).get(0).style.transform = 'rotate(0deg)';
    }
    else {
        $card.fadeIn(0);
        // Rotate the expander arrow
        $(this).get(0).style.transform = 'rotate(180deg)';
    }
    $card.data('expanded', !expanded);
})