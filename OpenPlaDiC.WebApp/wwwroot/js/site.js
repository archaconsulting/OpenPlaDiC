// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function toDecimal(value, digits) {

    return parseFloat(value, 10).toFixed((digits != null ? digits : 2)).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString()
}

function toMoney(value, digits) {

    return '$' + parseFloat(value, 10).toFixed((digits != null ? digits : 2)).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString()
}

function getProcData(procName, parameters, token, onSuccess, onError, showSplash) {

    const req = { procName: procName, parameters: parameters };      

    var settings = {
        async: false,
        method: "post",
        dataType: "json",
        url: '/API/GetProcData',
        contentType: "application/json; charset=utf-8",
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'RequestVerificationToken': token
        },
        traditional: true,
        global: (showSplash === null ? true : showSplash),
        data: JSON.stringify(req),
        success: function (response) { onSuccess(response); },
        error: function (jqXHR, textStatus, err) { onError(textStatus + ', ' + err); }


    }


    //jQuery.support.cors = true;
    $.ajax(settings);

}

function getProcDataAsync(procName, parameters, token, onSuccess, onError) {
    const req = { procName: procName, parameters: parameters };      

    hideShowsplash = 1;
    var settings = {
        async: true,
        method: "post",
        dataType: "json",
        url: '/API/GetProcData',
        contentType: "application/json; charset=utf-8",
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'RequestVerificationToken': token
        },
        traditional: true,
        global: false, // Para no ejecutar jQuery(document).ajaxStart
        data: JSON.stringify(req),
        success: function (response) { onSuccess(response); },
        error: function (jqXHR, textStatus, err) { onError(textStatus + ', ' + err); }



    }


    //jQuery.support.cors = true;
    $.ajax(settings);



}
