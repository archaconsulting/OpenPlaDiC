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
        error: function (jqXHR, textStatus, err) {
            if (onError)
            {
                onError(textStatus + ', ' + err);
            }
        }


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
        error: function (jqXHR, textStatus, err) { if (onError) { onError(textStatus + ', ' + err); } }



    }


    //jQuery.support.cors = true;
    $.ajax(settings);



}

function getQueryAsync(sqlQuery, parameters, token, onSuccess, onError) {
    const req = { SQLQuery: sqlQuery, Parameters: parameters };

    var settings = {
        async: true,
        method: "post",
        dataType: "json",
        url: '/API/GetQueryAsync', // Ajusta según la ruta en tu controlador
        contentType: "application/json; charset=utf-8",
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        data: JSON.stringify(req),
        success: function (response) { onSuccess(response); },
        error: function (jqXHR, textStatus, err) { onError(textStatus + ', ' + err); }
    };

    $.ajax(settings);
}

function execProcAsync(procName, parameters, token, onSuccess, onError) {
    const req = { ProcName: procName, Parameters: parameters };

    $.ajax({
        async: true,
        method: "post",
        dataType: "json",
        url: '/API/GetProcData', // Tu endpoint existente que ya funciona con SPs
        contentType: "application/json; charset=utf-8",
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        data: JSON.stringify(req),
        success: function (response) {
            // Manejo estandarizado de la respuesta del Framework
            onSuccess(typeof response === 'string' ? JSON.parse(response) : response);
        },
        error: function (jqXHR, textStatus, err) {
            if (onError) onError(textStatus, err);
        }
    });
}

// site.js - Capa de servicios dinámicos
const AppService = {
    // Inserción dinámica
    insert: async function (tableName, fields, callback) {
        return await this._call('POST', '/API/InsertData', { tableName, fields }, callback);
    },

    // Actualización dinámica
    update: async function (id, fields, callback) {
        return await this._call('POST', '/API/UpdateData', { id, fields }, callback);
    },

    // Eliminación dinámica
    delete: async function (id, remove = false, callback) {
        return await this._call('POST', '/API/DeleteData', { id, remove }, callback);
    },

    // Método privado interno de comunicación
    _call: async function (method, url, data, callback) {
        const result = await $.ajax({
            type: method,
            url: url,
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': window.AppUser.token }, // Token global
            data: JSON.stringify(data)
        });

        if (callback) callback(result);
        return result;
    }
};

// site.js - Puente para el motor de microservicios dinámicos
const APIService = {
    /**
     * Ejecuta una vista dinámica tipo API (Microservicio)
     * @param {string} viewName - Nombre de la vista dinámica API
     * @param {Array} fields - Arreglo de objetos GlobalItem {Name, Value}
     * @param {Function} callback - Función para manejar la respuesta
     */
    exec: async function (viewName, fields, callback) {
        const request = {
            View: viewName,
            Parameters: fields // Ya es un arreglo de objetos {Name, Value}
        };

        const result = await $.ajax({
            type: 'POST',
            url: '/API/ExecAPI',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': window.AppUser.token },
            data: JSON.stringify(request)
        });

        if (callback) callback(result);
        return result;
    }
};