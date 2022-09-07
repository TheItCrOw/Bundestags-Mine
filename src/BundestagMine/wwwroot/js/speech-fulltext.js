// Author Kevin B�nisch
// In here we store whether we have already added a divider for a new period
var periodToAdded = {};
var showSentimentColors = true;
var isSearching = false;
var pendingRequest = undefined;

// Takes in a list of protocols and builds the protocol tree
async function buildProtocolTree(protocols, agendaItems) {
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
        // Delete the time of the date
        var content = `Protokoll vom ${parseToGermanDate(protocol.date)}`;
        if (protocol.pollsAmount > 0) content += ` mit <b>${protocol.pollsAmount}</b> Abstimmungen.`
        item.setAttribute('data-content', content);
        item.setAttribute('data-html', true);
        item.setAttribute('data-trigger', 'hover');
        item.setAttribute('data-placement', 'top');
        item.setAttribute('data-id', protocol.id);
        item.setAttribute('data-param', protocol.legislaturePeriod + ',' + protocol.number);

        item.innerHTML += `<div class='flexed wrapper position-relative'>
                            <i class="fas fa-clipboard-list mr-2"></i>
                            <p class='w-100 mb-0'>${protocol.title}</p>
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

    var agendaItems = await getAgendaItemsOfProtocol(protocolId);
    var polls = await getPollsOfProtocol(period, protocol);
    agendaItems = agendaItems.reverse();

    // First agendaitems.
    for (var i = 0; i < agendaItems.length; i++) {
        var agendaItem = agendaItems[i];
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

        var name = 'Tagesordnungspunkt ' + agendaItem.agendaItemNumber;
        aItem.innerHTML += `<div class='flexed wrapper position-relative'>
                            <i class="fas fa-exclamation mr-2"></i>
                            <p class='w-100 mb-0'>${agendaItem.title}</p>
                            <i class="arrow fas fa-angle-up"></i>
                            <div class="loader">Reden werden geladen...</div></div>`;
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

// Handles the preparing of a speech for the fulltext analysis
async function insertSpeechIntoFulltextAnalysis(speechId) {
    // Enable the loading screen
    $('.fulltext-analysis-div').find('.loader').fadeIn(500);
    $('.empty-message').hide();

    var result = await getNLPSpeechById(speechId);
    if (!result) return;
    var speech = result.speech;
    var agendaItem = result.agendaItem;
    var topics = result.topics;

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
    $('.fulltext-analysis-div').find('.since').html(speaker ? parseToGermanDate(speaker.historySince): '');
    $('.fulltext-analysis-div').find('.job').html(speaker ? speaker.profession : '');
    $('.fulltext-analysis-div').find('.title').html(speaker ? speaker.academicTitle : '');

    // Set the breadcrumbps
    $('.breadcrumbs').find('.period').html('Legislaturperiode ' + speech.legislaturePeriod);
    $('.breadcrumbs').find('.protocol').html('Sitzung ' + speech.protocolNumber);
    $('.breadcrumbs').find('.agenda').html(agendaItem.title);
    $('.breadcrumbs').find('.agenda').data('text', agendaItem.description);

    // Set the topic of the speech
    if (topics != undefined && topics.length >= 3) {
        $('.fulltext-analysis-div').find('.topic-header .topic-1').html(topics[0]?.value);
        $('.fulltext-analysis-div').find('.topic-header .topic-1').attr('data-content',
            'Thema mit ' + topics[0].count + ' Erwähnungen');

        $('.fulltext-analysis-div').find('.topic-header .topic-2').html(topics[1]?.value);
        $('.fulltext-analysis-div').find('.topic-header .topic-2').attr('data-content',
            'Thema mit ' + topics[1].count + ' Erwähnungen');

        $('.fulltext-analysis-div').find('.topic-header .topic-3').html(topics[2]?.value);
        $('.fulltext-analysis-div').find('.topic-header .topic-3').attr('data-content',
            'Thema mit ' + topics[2].count + ' Erwähnungen');
    }

    // Visualize the nlp speech in the content
    var html = await buildHtmlOfFulltextAnalysis(speech);
    // Add the text to the ui
    $('.analysis-content').html("Test");
    $('.analysis-content').html(html);

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
    $('.fulltext-analysis-div').find('.loader').fadeOut(500);
}


// Takes in a nlp speech and parses it into the html we add to the UI
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
            // Does the named entity maybe start her
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
                sentenceAndTokenEnd = sentenceAndTokenEnd;
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
                    spanEnd += `</i><i class="far fa-question-circle sentiment-question" data-expanded="true" data-content="${tooltip}" data-toggle="popover" data-trigger="hover" data-placement="top" data-html="true"></i>`;
                } else {
                    spanEnd += '</i>'
                }
                alreadyPlacedQuestion = !alreadyPlacedQuestion;
            }
            else if (sentenceAndTokenStart != undefined) {
                sentenceAndTokenStart = sentenceAndTokenStart;
                var mode = 'neu-sentence';
                if (sentenceAndTokenStart.sentimentSingleScore > 0) {
                    mode = 'pos-sentence';
                } else if (sentenceAndTokenStart.sentimentSingleScore < 0) {
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
        console.log("Error in fulltext analysis: " + ex);
        return "";
    }
}

// Handles the opening and closing of the side tree
$('body').on('click', '.close-open-btn', function () {
    var expanded = $(this).data('expanded');

    if (expanded) {
        $('.protocol-tree-content').find('.header').hide();
        $('.protocol-tree-content').find('.protocol-tree').hide();
        $('.close-open-btn').find('i').removeClass('fa-chevron-left');
        $('.close-open-btn').find('i').addClass('fa-chevron-right');
    } else {
        $('.protocol-tree-content').find('.header').show();
        $('.protocol-tree-content').find('.protocol-tree').show();
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
