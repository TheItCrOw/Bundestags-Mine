// author Kevin Bönisch

// Searches thorugh parties, speaker, fractions and shows them
function search(input) {
    input = input.replace(/ /g, '').toLowerCase();
    var result = [];

    // We want a divider
    result.push({
        id: 'Gesamt',
        displayName: 'Gesamt',
        type: 'divider'
    });
    // Add the total option
    result.push({
        id: 'Gesamt',
        displayName: 'Gesamt',
        type: 'Gesamt'
    });

    // We want a divider
    result.push({
        id: 'Fraktion',
        displayName: 'Fraktionen',
        type: 'divider'
    });
    // Look for fractions
    allFractions.forEach(function (fraction) {
        var displayName = fraction.id + ' (' + 'Fraktion' + ')'
        var search = displayName.replace(/ /g, '').toLowerCase();
        if (search.includes(input) || input == '') {
            result.push(
                {
                    id: fraction.id,
                    displayName: displayName,
                    type: 'Fraktion'
                });
        }
    })

    // We want a divider
    result.push({
        id: 'Partei',
        displayName: 'Parteien',
        type: 'divider'
    });
    // Look for parties
    allParties.forEach(function (party) {
        var displayName = party.id + ' (' + 'Partei' + ')'
        var search = displayName.replace(/ /g, '').toLowerCase();
        if (search.includes(input) || input == '') {
            result.push(
                {
                    id: party.id,
                    displayName: displayName,
                    type: 'Party'
                });
        }
    })

    // We want a divider
    result.push({
        id: 'Redner',
        displayName: 'Redner(innen)',
        type: 'divider'
    });
    // Look for speaker
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
            result.push(
                {
                    id: speaker.speakerId,
                    displayName: fullName,
                    type: 'Redner'
                });
        }
    })

    // delete the old list
    $('#searchBarResultListDiv').remove();

    var list = document.createElement("div");
    list.id = 'searchBarResultListDiv';
    // Now add all results to the list.
    result.forEach(function (obj) {
        var item = document.createElement("div");

        if (obj.type == 'divider') {
            item.classList.add('divide-item');
        } else {
            item.classList.add('search-item');
        }
        item.setAttribute("data-id", obj.id);
        item.setAttribute("data-type", obj.type);

        // Set the value
        item.appendChild(document.createTextNode(obj.displayName));
        list.appendChild(item);
    })
    $('#dashboardCreateSearchList').get(0).appendChild(list);
}

// Handle the searching of the searchbar. Always open the search result list here.
$('body').on('input', '#dashboardCreateSearch', function () {
    $('#dashboardCreateSearchList').show();
    search($(this).val());
})

// Handle the focusing of the searchbar
$('body').on('click', '#dashboardCreateSearch', function () {
    $('#dashboardCreateSearchList').show();
})

// Handle the clicking onto a search item
$('body').on('click', '.search-item', function () {
    var displayName = $(this).html();
    var id = $(this).data('id');
    var type = $(this).data('type');

    $('#dashboardCreateSearch').val(displayName);
    $('#dashboardCreateSearch').data("id", id);
    $('#dashboardCreateSearch').data("type", type);
})

// Handle the clikcing onto the date presets for the user
$('body').on('click', '.date-preset', function () {
    var from = $(this).data('from');
    var to = $(this).data('to');

    // Resolve 'today'
    if (to == 'today') to = new Date().toISOString().split("T")[0];

    $('#createDashboardFromDate').val(from);
    $('#createDashboardToDate').val(to);
})

// Handles the initiation of a dashboard creation
$('body').on('click', '.create-dashboard-btn', async function () {
    var name = $('#dashboardCreateSearch').val();
    var fetchId = $('#dashboardCreateSearch').data('id');
    var type = $('#dashboardCreateSearch').data('type');
    var from = $('#createDashboardFromDate').val();
    var to = $('#createDashboardToDate').val();

    // Check for correctness
    if (from == "" || to == "") {
        $('#addDashboardModal').find('.error-message').html('Das Start- und Enddatum muss gesetzt werden.');
        $('#addDashboardModal').find('.error-message').show();
        return;
    } else if ($('#dashboardCreateSearch').val() == "") {
        $('#addDashboardModal').find('.error-message').html('Bitte wählen Sie eine Entität aus.');
        $('#addDashboardModal').find('.error-message').show();
        return;
    }
    addNewDashboard(name, fetchId, type, from, to);

    $('#addDashboardModal').modal('hide');
})

$(document).ready(function () {
    // Set the max dates to today with js
    $('#createDashboardFromDate').get(0).max = new Date().toISOString().split("T")[0];
})