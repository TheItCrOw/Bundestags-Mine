var GlobalSearchHandler = (function () {

    // We need a way to store running reqeusts to potentially abort them.
    GlobalSearchHandler.prototype.searchingSpeakersRequest = undefined;
    GlobalSearchHandler.prototype.searchingSpeechesRequest = undefined;
    GlobalSearchHandler.prototype.searchingAgendaItemsRequest = undefined;
    GlobalSearchHandler.prototype.searchingPollsRequest = undefined;

    // Constructor
    function GlobalSearchHandler() { }

    // Starts a global search for speeches, fetches the returned html view and puts it into UI
    async function globalSearchSpeeches(obj) {
        // Set all the other filters to false.
        obj.searchSpeakers = false;
        obj.searchAgendaItems = false;
        obj.searchPolls = false;

        // Do the request
        searchingSpeechesRequest = postNewGlobalSearch(obj,
            // On success
            function (response) {
                console.log(response);
                $('.global-search').find('.results').find('div[data-id="speeches"]').html(response.result);
                searchingSpeechesRequest = undefined;
            },
            // On error
            function (response) {
                searchingSpeechesRequest = undefined;
            });
    }

    // Public function: Start a new global search
    GlobalSearchHandler.prototype.startNewGlobalSearch = async function () {
        var searchString = $('.global-search-input').val();
        if (searchString == '') return;

        // Build the request model
        var obj = {
            searchString,
            searchSpeeches: $('.global-search-filter').find('input[data-id="speeches"]').is(':checked'),
            searchSpeakers: $('.global-search-filter').find('input[data-id="speakers"]').is(':checked'),
            searchAgendaItems: $('.global-search-filter').find('input[data-id="agendaItems"]').is(':checked'),
            searchPolls: $('.global-search-filter').find('input[data-id="polls"]').is(':checked'),
            from: $('.global-search-filter').find('input[data-id="from"]').val(),
            to: $('.global-search-filter').find('input[data-id="to"]').val(),
            offset: 0
        }

        // Foreach included filter, make a request
        if (obj.searchSpeeches) {
            globalSearchSpeeches(obj);
        }
    }

    return GlobalSearchHandler;
}());

// Use this item to call all globalsearch logic
var globalSearchHandler = new GlobalSearchHandler();

// Handles the start of the search with the search button
$('body').on('click', '.global-search-start-btn', function () { globalSearchHandler.startNewGlobalSearch(); })

// Handles the start of the with the enter button
$('body').on('keypress', '.global-search-input', function (e) {
    if (e.which == 13) {
        globalSearchHandler.startNewGlobalSearch();
    }
})
