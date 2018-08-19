$(document).ready(function () {
    var pillName = $('#txtName');
    var totalPills = $('#txtTotalPills');
    var textStatus = $('#txtStatus');
    var imgContainer = $('#imgContainer');

    $('#btnWrite').click(function () {
        var prescriptionEntry = {};
        prescriptionEntry.docID = 'a';
        prescriptionEntry.info = JSON.stringify({ "name": pillName.val(), "totalPills": parseInt(totalPills.val()) });
        prescriptionEntry.patientID = '12';
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
        options.error = function (err) {
            console.log("error");
        };

        $.ajax(options);

    });

    $('#btnClear').click(function () {
        console.log("clear");
        pillName.val("");
        totalPills.val("");
        imgContainer.attr('src', "");

    });
});