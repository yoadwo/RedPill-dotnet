$(document).ready(function () {
    var pres = $('#ulPrescriptions');

    $('#btnPatient').click(function () {
        var urlbuilder = "api/prescriptions" + query('id', $('#txtPatientID').val());
        $.ajax({
            type: 'GET',
            url: urlbuilder,
            dataType: 'json',
            success: function (data) {
                pres.empty();
                // data [ index ] = val
                $.each(data, function (index, val) {
                    var bullet = val.recordID + ')' + ' ' + val.pillName + ' ' + '[' + val.timeAdded + ']' + ' ' + val.info;
                    pres.append('<li>' + bullet + '</li>');
                });
            }
        })          
    });

    $('#btnPillName').click(function () {
        var urlbuilder = "api/prescriptions" + query('pillName', $('#txtPillName').val());
        $.ajax({
            type: 'GET',
            url: urlbuilder,
            dataType: 'json',
            success: function (data) {
                pres.empty();
                // data [ index ] = val
                $.each(data, function (index, val) {
                    var bullet = val.recordID + ')' + ' ' + val.pillName + ' ' + '[' + val.timeAdded + ']' + ' ' +  val.info;
                    pres.append('<li>' + bullet + '</li>');
                });
            }
        })
    });

    $('#btnClear').click(function () {
        console.log("clear form");
        // sluggish but working
        $(':input').not(':button, :submit, :reset, :hidden, :checkbox, :radio').val('');

        pres.empty();
    });
});

function query(key, value) {
    return '?' + key + '=' + value;
}

