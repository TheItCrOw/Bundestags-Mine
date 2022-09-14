// The object that holds the logic for the download center
var DownloadCenterHandler = (function () {
    DownloadCenterHandler.prototype.filterSpeakerResultList = [];

    // Constructor
    function DownloadCenterHandler() { }

    // Starts a global search for shouts, fetches the returned html view and puts it into UI
    DownloadCenterHandler.prototype.init = async function (fractions, parties, speakers) {
        // We need to fill the filters dynamically
        // fractions
        for (var i = 0; i < fractions.length; i++) {
            var fraction = fractions[i];
            var html = `<div class="flexed align-items-center ml-1 mr-1">
                            <label class="mb-0">${fraction.id}</label>
                            <input type="checkbox" class="ml-2" checked/>
                        </div>`;
            $('#downloadCenterContent .filter .fractions').append(html);
        }
        // parties
        for (var i = 0; i < parties.length; i++) {
            var party = parties[i];
            var html = `<div class="flexed align-items-center ml-1 mr-1">
                            <label class="mb-0">${party.id}</label>
                            <input type="checkbox" class="ml-2" checked/>
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

    return DownloadCenterHandler;
}());

var downloadCenterHandler = new DownloadCenterHandler();

// Handle the graying out of the speaker list when chosen 'all speakers'
$('body').on('input', '#downloadCenterContent .filter .choose-btn', function () {
    var type = $(this).data('type');
    if (type == 'all') {
        $('#downloadCenterContent .filter .speaker-search-input').prop('disabled', true);
    } else {
        $('#downloadCenterContent .filter .speaker-search-input').prop('disabled', false);
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
        } else{
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

    downloadCenterHandler.filterSpeakerResultList.push(speakerId);
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