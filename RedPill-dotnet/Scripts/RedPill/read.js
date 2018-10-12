$(document).ready(function () {
    var pres = $('#ulPrescriptions');

    $('#btnPatient').click(function () {
        var urlbuilder = "../api/prescriptions" + query('id', $('#txtPatientID').val());
        $.ajax({
            type: 'GET',
            url: urlbuilder,
            dataType: 'json',
            success: function (data, textStatus, jqXHR) {
                console.log(textStatus);
                console.log(jqXHR.status);
                pres.empty();
                if (jqXHR.status == 200) {
                    // data [ index ] = val
                    $.each(data, function (index, val) {
                        var bullet = val.recordID + ')' + ' ' + val.pillName + ' ' + '[' + val.timeAdded + ']' + ' ' + val.info;
                        pres.append('<li>' + bullet + '</li>');
                    });
                }
                else if (jqXHR.status == 204) {
                    pres.append('<li>' + 'no content found matching search criteria' + '</li>');
                }
                
            }
        })          
    });

    $('#btnPillName').click(function () {
        var urlbuilder = "../api/prescriptions" + query('pillName', $('#txtPillName').val());
        $.ajax({
            type: 'GET',
            url: urlbuilder,
            dataType: 'json',
            success: function (data, textStatus, jqXHR) {
                console.log(textStatus);
                console.log(jqXHR.status);
                pres.empty();
                if (jqXHR.status == 200) {
                    // data [ index ] = val
                    $.each(data, function (index, val) {
                        var bullet = val.recordID + ')' + ' ' + val.pillName + ' ' + '[' + val.timeAdded + ']' + ' ' + val.info;
                        pres.append('<li>' + bullet + '</li>');
                    });
                }
                else if (jqXHR.status == 204) {
                    pres.append('<li>' + 'no content found matching search criteria' + '</li>');
                }
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

