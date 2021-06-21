// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function showModal() {
    var cookie = localStorage.getItem('cookie');
    if (!cookie) {
        $('#exampleModal').modal("show");
    }
}

showModal();

function accept() {
    localStorage.setItem('cookie', true);
    $('#exampleModal').modal("hide");
}


var icon = L.divIcon({
    className: 'custom-div-icon',
    html: "<div style='background-color:#c30b82;' class='marker-pin'></div><i class='material-icons'>weekend</i>",
    iconSize: [30, 42],
    iconAnchor: [15, 42]
});