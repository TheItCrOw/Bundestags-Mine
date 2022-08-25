var GlobalSearchHandler = (function () {
    // Constructor
    function GlobalSearchHandler() {
    }

    // Private function
    function privateFun(prefix) {
        // call like this: return privateFun.call(this, ">>");
    }

    // Public function
    GlobalSearchHandler.prototype.startNewGlobalSearch = async function () {
        console.log('Starting a new global search');
        var searchString = $('.global-search-input').val();
        if (searchString == '') return;

        // Build the request model
        var obj = {
            searchString,
            includeSpeeches: $('.global-search-filter').find('input[data-id="speeches"]').is(':checked'),
            includeSpeakers: $('.global-search-filter').find('input[data-id="speakers"]').is(':checked'),
            includeAgendaItems: $('.global-search-filter').find('input[data-id="agendaItems"]').is(':checked'),
            includePolls: $('.global-search-filter').find('input[data-id="polls"]').is(':checked'),
            from: $('.global-search-filter').find('input[data-id="from"]').val(),
            to: $('.global-search-filter').find('input[data-id="to"]').val()
        }

        // Do the request
        const result = await $.ajax({
            url: "/api/DashboardController/GlobalSearch/",
            type: "POST",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: "application/json",
            accepts: {
                text: "application/json"
            },
        });

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
