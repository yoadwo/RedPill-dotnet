$(document).ready(function () {
    var pres = $('#ulPrescriptions');

    $('#btnGet').click(function () {
        $.ajax({
            type: 'GET',
            url: "api/prescriptions",
            dataType: 'json',
            success: function (data) {
                pres.empty();
                // data [ index ] = val
                $.each(data, function (index, val) {
                    var bullet = val.recordID + ' ' + val.pillName;
                    pres.append('<li>' + bullet + '</li>');
                });
            }
        })          
    });

    $('#btnClear').click(function () {
        pres.empty();
    });
});