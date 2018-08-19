$(document).ready(function () {
    var pillName = $('#txtName');
    var formList = $('#formList');
    var imgContainer = $('#imgContainer');
    
    $('#btnWrite').click(function () {
        var prescriptionEntry = {};
        prescriptionEntry.docID = 'a';
        //prescriptionEntry.info = JSON.stringify({ "name": pillName.val(), "totalPills": parseInt(totalPills.val()) });
        prescriptionEntry.info = getInfo();
        prescriptionEntry.patientID = '14';
        prescriptionEntry.pillName = pillName.val();
        //datetime is added at server
        
        var options = {};
        options.url = 'api/prescriptions';
        options.type = 'POST';
        options.contentType = "application/json";
        options.accept = 'image/png';
        options.data = JSON.stringify(prescriptionEntry);
        options.success = function (imageResults) {
            console.log("success");
            //$('#imgContainer').append('<img src="' + imageResults + '" />');
            var config = 'data:image/png;base64,';
            //imgContainer.append('img src="' + config + imageResults + '"/>');
            imgContainer.attr("src", config + imageResults);
            
            
        };
        options.error = function (jqXHR) {
            console.log("error, " + jqXHR.responseText);
        };

        $.ajax(options);

    });

    $('#btnClear').click(function () {
        console.log("clear");
        $("#formTable").find("input").each(function () {
            $(this).val("");
            // working, but is it correct?
        });
        
        imgContainer.attr('src', "");

    });


});

function getInfo() {
    jsonObj = {};
    jsonObj.name = $('#txtName').val();
    jsonObj.totalPills =  parseInt($('#txtTotalPills').val());  // int
    jsonObj.each = parseInt($('#txtDose').val()); // int
    jsonObj.method = $('#txtMethod').val();
    jsonObj.frequency = $('#txtFrequency').val();
    jsonObj.days = parseInt($('#txtDays').val()); // int
    jsonObj.comments = $('#txtComments').val();

    return JSON.stringify(jsonObj);
}

