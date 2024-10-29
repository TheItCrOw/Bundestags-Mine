// The object that holds the logic for the download center
var DownloadCenterHandler = (function () {
    DownloadCenterHandler.prototype.filterSpeakerResultList = [];
    DownloadCenterHandler.prototype.explicitSpeakers = false;
    DownloadCenterHandler.prototype.currentFilter = {};

    // Constructor
    function DownloadCenterHandler() { }

    // Inits the download center
    DownloadCenterHandler.prototype.init = async function (fractions, parties, speakers) {
        // We need to fill the filters dynamically
        // fractions
        for (var i = 0; i < fractions.length; i++) {
            var fraction = fractions[i];
            var html = `<div class="flexed align-items-center ml-1 mr-1">
                            <label class="mb-0">${fraction.id}</label>
                            <input type="checkbox" class="ml-2" data-value="${fraction.id}" data-type="fraction" checked/>
                        </div>`;
            $('#downloadCenterContent .filter .fractions').append(html);
        }
        // parties
        for (var i = 0; i < parties.length; i++) {
            var party = parties[i];
            var html = `<div class="flexed align-items-center ml-1 mr-1">
                            <label class="mb-0">${party.id}</label>
                            <input type="checkbox" class="ml-2" data-value="${party.id}" data-type="party" checked/>
                        </div>`;
            $('#downloadCenterContent .filter .parties').append(html);
        }
        // speaker
        for (var i = 0; i < speakers.length; i++) {
            var speaker = speakers[i];
            var fullname = speaker.firstName + " " + speaker.lastName + " (" + (speaker.fraction ?? speaker.party) + ")";
            var html = `<div class="pl-1 pr-1 item" data-id="${speaker.speakerId}">
                            <label class="mb-0 pointer-events-none">${fullname}</label>
                        </div>`;
            $('#downloadCenterContent .filter .speaker-list-div').append(html);
        }
    }

    // Applies the filter and fills the metadata of the download
    DownloadCenterHandler.prototype.applyFilter = async function (obj) {
        // build the obj
        var obj = {
            from: $('#downloadCenterContent .filter').find('input[data-type="from"]').val(),
            to: $('#downloadCenterContent .filter').find('input[data-type="to"]').val(),
            fractions: $('#downloadCenterContent .filter .filter-item[data-type="fractions"]').find('input[data-type="fraction"]')
                .map(function () { if ($(this).is(':checked')) return $(this).data('value'); }).toArray(),
            parties: $('#downloadCenterContent .filter .filter-item[data-type="parties"]').find('input[data-type="party"]')
                .map(function () { if ($(this).is(':checked')) return $(this).data('value'); }).toArray(),
            explicitSpeakers: this.explicitSpeakers ? this.filterSpeakerResultList : [],
            email: ''
        }
        // make the request
        $('#downloadCenterContent .filter-result-div').show(150);
        $('#downloadCenterContent .loader').fadeIn(150);
        $('#downloadCenterContent .apply-filter-btn').attr('disabled', true);
        var result = await postCalculateData(obj);
        if (result != undefined) {
            $('.filter-result-div .protocols').html(result.protocols);
            $('.filter-result-div .speeches').html(result.speeches);
            $('.filter-result-div .estimated-size').html(result.estimatedFileSizeInMB);
            $('.filter-result-div .estimated-time').html(result.estimatedMinutes);
            $('.filter-result-div .estimated-zip-size').html(result.estimatedZipFileSizeInMB);
            // Store the current filter. we need it for the download later.
            this.currentFilter = obj;
        }
        $('#downloadCenterContent .apply-filter-btn').attr('disabled', false);
        $('.filter-result-div .loader').fadeOut(150);
    }

    return DownloadCenterHandler;
}());

var downloadCenterHandler = new DownloadCenterHandler();

// Handle the start of the data download
$('body').on('click', '#downloadCenterContent .generate-data-btn', async function () {
    // Check if the mail is valid
    var mail = $('#downloadCenterContent .filter-result-div .mail-input').val();
    if (!validateEmail(mail)) {
        $('#downloadCenterContent .filter-result-div .error-message').html('Die Mail-Adresse hat ein falsches Format.');
        $('#downloadCenterContent .filter-result-div .error-message').show();
        return;
    }
    // dont show the error message anymore
    $('#downloadCenterContent .filter-result-div .error-message').hide();
    // Set the mail
    downloadCenterHandler.currentFilter.email = mail;
    var result = await postGenerateDatasetByFilter(downloadCenterHandler.currentFilter);

    if (result.status = "200") {
        showToast('Daten-Anfrage', result.message);
        // We want the download button to be a checkmark
        $(this).hide();
        $(this).next('button').show(150);
    } else {
        showToast('Fehler', 'Es gab einen Fehler beim Starten der Kalkulation. Probieren Sie es noch einmal oder melden Sie den Fehler.');
    }
})

// Handle the graying out of the speaker list when chosen 'all speakers'
$('body').on('input', '#downloadCenterContent .filter .choose-btn', function () {
    var type = $(this).data('type');
    if (type == 'all') {
        $('#downloadCenterContent .filter .speaker-search-input').prop('disabled', true);
        downloadCenterHandler.explicitSpeakers = false;
    } else {
        $('#downloadCenterContent .filter .speaker-search-input').prop('disabled', false);
        downloadCenterHandler.explicitSpeakers = true;
    }
})

// Handles the expanding of the menu items
$('body').on('click', '#downloadCenterContent .filter .expander', function () {
    var expanded = $(this).data('expanded');
    var type = $(this).data('type');

    var expandable = $('#downloadCenterContent .filter').find(`.expandable[data-type="${type}"]`);
    if (expanded) {
        expandable.hide(150);
        $(this).find('i').removeClass('fa-chevron-circle-down');
        $(this).find('i').addClass('fa-chevron-circle-up');
        $(this).closest('.filter-item').removeClass('selected-filter-item');
    } else {
        expandable.show(150);
        $(this).find('i').removeClass('fa-chevron-circle-up');
        $(this).find('i').addClass('fa-chevron-circle-down');
        $(this).closest('.filter-item').addClass('selected-filter-item');
    }

    $(this).data('expanded', !expanded);
})

// Handles the searching of the speakers in the vorschläge box
$('body').on('input', '#downloadCenterContent .filter .speaker-search-input', function () {
    $('#downloadCenterContent .filter .speaker-list-div').show();
    var search = $(this).val().toLowerCase().replace(' ', '');

    $('#downloadCenterContent .filter .speaker-list-div .item').each(function () {
        if ($(this).html().toLowerCase().replace(' ', '').includes(search)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    })
})

// Open the speaker list when clicked
$('body').on('click', '#downloadCenterContent .filter .speaker-search-input', function () {
    $('#downloadCenterContent .filter .speaker-list-div').show();
})

// Handle the selecting of a speaker
$('body').on('click', '#downloadCenterContent .filter .speaker-list-div .item', function () {
    var speakerId = $(this).data('id');
    // Check if the speaker is already added
    if (downloadCenterHandler.filterSpeakerResultList.includes(speakerId)) return;

    downloadCenterHandler.filterSpeakerResultList.push(speakerId.toString());
    var fullname = $(this).html();
    var html = `<div class="flexed justify-content-between align-items-center result pl-2 pr-2" data-id="${speakerId}">
                    <label class="mb-0">${fullname}</label>
                    <button class="btn p-0"><i class="small-font fas fa-trash-alt"></i></button>
                </div>`;
    $('#downloadCenterContent .filter .speaker-list-result').append(html);
})

// Handle the deleting of a speaker
$('body').on('click', '#downloadCenterContent .filter .speaker-list-result .result button', function () {
    var speakerId = $(this).closest('.result').data('id');
    $(this).closest('.result').remove();
    // This is the 'list remove' function
    downloadCenterHandler.filterSpeakerResultList = downloadCenterHandler.filterSpeakerResultList.filter(item => item !== speakerId);
})

// Handle the applying of the filter
$('body').on('click', '#downloadCenterContent .apply-filter-btn', function () {
    // Show the right button
    $('#downloadCenterContent .generate-data-btn').show(150);
    $('#downloadCenterContent .generate-data-btn').next('button').hide();

    // apply the filter
    downloadCenterHandler.applyFilter();
})