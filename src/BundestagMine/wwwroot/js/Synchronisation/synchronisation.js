// Handles the synch jobs
$(".import-btn").on("click", function () {
    var type = $(this).data("id");
    console.log("Start import");
    $.ajax({
        url: "/api/SynchronisationController/StartImport/" + type,
        type: "POST",
        dataType: "json",
        success: function (response) {
            if (response.status == "200") {
                startPolling(type);
            }
            console.log(response);
        }
    });
})

// Polls the status of a given entity
async function startPolling(type) {
    $.ajax({
        url: "/api/SynchronisationController/GetEntityCount/" + type,
        type: "GET",
        dataType: "json",
        success: function (response) {
            if (response.status == "200") {
                $(`#${type}Div`).find('p').html(response.count);
            }
            console.log(response);
        }
    });

    setTimeout(startPolling, 4000, type);
}
console.log("Loaded js");
