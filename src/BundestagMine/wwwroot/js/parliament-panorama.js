var PanoramaHandler = (function () {

    var alreadyOpenedCategoryCards = [];
    var lineChartsToCategoryContainer = {};

    // Constructor
    function PanoramaHandler() { }

    // Inits the handler. That is the up most paper in the dropdown
    PanoramaHandler.prototype.init = async function () {
        // We load the 'start page' only when the site is visited.
        var categoriesPanorama = await getCategoriesPanoramaView();
        $('#categoriesPanoramaContent').html(categoriesPanorama);
    }

    // Handles the clicking onto a category card
    PanoramaHandler.prototype.handleCategoryCardClicked = async function ($card) {
        if ($card.hasClass('selected-category-card')) return;

        // Move the card to the top
        $('.categories-div').prepend($card.parent());

        // Disable all other selected cards
        $('.parliament-panorama-container .category-card').each(function () {
            $(this).parent().removeClass('col-md-12');
            $(this).parent().addClass('col-md-4');
            $(this).removeClass('selected-category-card');
            $(this).find('.expanded-content').hide();
        });

        $card.parent().removeClass('col-md-4');
        $card.parent().addClass('col-md-12');
        $card.addClass('selected-category-card');
        $card.find('.expanded-content').show(150);

        // We want to render a line chart once when the categfory is clicked for the first time.  
        var cardId = $card.data('id');

        var chartData = await getCategoryLineChartData(cardId);
        var chart = await panoramaHandler.buildCategoryLineChart(chartData, $card.find('.line-chart'));
        // Store all the instancens of charts for future work.
        lineChartsToCategoryContainer[$card.closest('.category-container').data('id')] = chart;

        // Store that we handled this card at least once
        alreadyOpenedCategoryCards.push(cardId);
    }

    // Handles the clicking onto a category itself instead of the card
    PanoramaHandler.prototype.handleCategoryClicked = async function ($category) {
        // We need to switch the line-chart of this category container here.
        var categoryId = $category.data('id');
        var $container = $category.closest('.category-container');
        var containerId = $container.data('id');

        // Before drawing the new chart, delete the old one
        var oldChart = lineChartsToCategoryContainer[containerId];
        oldChart.destroy();

        var chartData = await getCategoryLineChartData(categoryId);
        var chart = await panoramaHandler.buildCategoryLineChart(chartData, $container.find('.line-chart'));

        // Store all the instancens of charts for future work.
        lineChartsToCategoryContainer[containerId] = chart;
    }

    // Builds the line chart for a category
    PanoramaHandler.prototype.buildCategoryLineChart = async function (chartData, $canvas) {
        // Sample data for the line chart
        var data = {
            labels: chartData.map(d => d.label),
            datasets: [{
                label: "Reden, die in diese Kategorie fallen",
                borderColor: "gold",
                data: chartData.map(d => d.value),
                fill: true
            }]
        };

        // Configuration options for the chart
        var options = {
            responsive: true,
            maintainAspectRatio: false,
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
                },
                y: {
                    grid: {
                        display: false
                    }
                }
            }
        };

        // Get the context of the canvas element we want to select
        var ctx = $canvas.get(0).getContext('2d');

        // Create the line chart
        var myLineChart = new Chart(ctx, {
            type: 'line',
            data: data,
            options: options
        });

        return myLineChart;
    }

    return PanoramaHandler;
}());

// Use this item for all panorama logic
var panoramaHandler = new PanoramaHandler();
panoramaHandler.init();

// Handles the clicking onto a category card
$('body').on('click', '.parliament-panorama-container .category-card', async function () {
    panoramaHandler.handleCategoryCardClicked($(this));
})

// Handles the clicking onto a category itself
$('body').on('click', '.parliament-panorama-container .clickable-category', async function () {
    panoramaHandler.handleCategoryClicked($(this));
})