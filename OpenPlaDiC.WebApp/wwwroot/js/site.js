// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// =============================================================================
// 1. FORMATEADORES NUMÉRICOS Y MONEDA
// =============================================================================
function toDecimal(value, digits) {
    if (value === null || value === undefined || isNaN(value)) value = 0;
    return parseFloat(value).toFixed((digits != null ? digits : 2)).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();
}

function toMoney(value, digits) {
    if (value === null || value === undefined || isNaN(value)) value = 0;
    return '$' + parseFloat(value).toFixed((digits != null ? digits : 2)).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();
}

// =============================================================================
// 2. CAPA CORE DE COMUNICACIÓN CON SPs Y QUERIES (CON VALIDADORES DE LOADING)
// =============================================================================
function getProcData(procName, parameters, token, onSuccess, onError, showSplash) {
    const req = { procName: procName, parameters: parameters };      
    
    // ⚡ INTEGRACIÓN: Si showSplash es verdadero, disparamos el loader premium homologado
    const ejecutarSplash = (showSplash === null || showSplash === true);
    if (ejecutarSplash && typeof OpenPlaDiC !== 'undefined') {
        OpenPlaDiC.UI.showLoading('Procesando Datos...', 'Ejecutando procedimiento en el Kernel.');
    }

    var settings = {
        async: true, // Cambiado a true (Mejor práctica para no congelar el DOM del navegador)
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
        global: ejecutarSplash,
        data: JSON.stringify(req),
        success: function (response) { 
            if (ejecutarSplash && typeof OpenPlaDiC !== 'undefined') OpenPlaDiC.UI.close();
            onSuccess(response); 
        },
        error: function (jqXHR, textStatus, err) {
            if (ejecutarSplash && typeof OpenPlaDiC !== 'undefined') OpenPlaDiC.UI.close();
            if (onError) {
                onError(textStatus + ', ' + err);
            } else if (typeof OpenPlaDiC !== 'undefined') {
                OpenPlaDiC.UI.showError('Error de Kernel', textStatus + ': ' + err);
            }
        }
    };

    $.ajax(settings);
}

function getProcDataAsync(procName, parameters, token, onSuccess, onError) {
    const req = { procName: procName, parameters: parameters };      
    
    // ⚡ CORREGIDO: Declaración explícita para evitar fugas de scope global
    let hideShowsplash = 1; 

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
        global: false, 
        data: JSON.stringify(req),
        success: function (response) { onSuccess(response); },
        error: function (jqXHR, textStatus, err) { 
            if (onError) { onError(textStatus + ', ' + err); } 
        }
    };

    $.ajax(settings);
}

function getQueryAsync(sqlQuery, parameters, token, onSuccess, onError) {
    const req = { SQLQuery: sqlQuery, Parameters: parameters };

    var settings = {
        async: true,
        method: "post",
        dataType: "json",
        url: '/API/GetQueryAsync', 
        contentType: "application/json; charset=utf-8",
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        data: JSON.stringify(req),
        success: function (response) { onSuccess(response); },
        error: function (jqXHR, textStatus, err) { if (onError) onError(textStatus + ', ' + err); }
    };

    $.ajax(settings);
}

function execProcAsync(procName, parameters, token, onSuccess, onError) {
    const req = { ProcName: procName, Parameters: parameters };

    $.ajax({
        async: true,
        method: "post",
        dataType: "json",
        url: '/API/GetProcData', 
        contentType: "application/json; charset=utf-8",
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        data: JSON.stringify(req),
        success: function (response) {
            onSuccess(typeof response === 'string' ? JSON.parse(response) : response);
        },
        error: function (jqXHR, textStatus, err) {
            if (onError) onError(textStatus, err);
        }
    });
}

// =============================================================================
// 3. APPSERVICE - CAPA DE CRUD DINÁMICO IMPULSADA POR METADATA
// =============================================================================
const AppService = {
    insert: async function (tableName, fields, callback) {
        return await this._call('POST', '/API/InsertData', { tableName, fields }, callback);
    },

    update: async function (id, fields, callback) {
        return await this._call('POST', '/API/UpdateData', { id, fields }, callback);
    },

    delete: async function (id, remove = false, callback) {
        return await this._call('POST', '/API/DeleteData', { id, remove }, callback);
    },

    _call: async function (method, url, data, callback) {
        // Mostramos loader discreto para el CRUD Dinámico
        if (typeof OpenPlaDiC !== 'undefined') OpenPlaDiC.UI.showLoading('Procesando datos...', 'Escribiendo en el Kernel de Base de Datos.');

        try {
            const result = await $.ajax({
                type: method,
                url: url,
                contentType: 'application/json',
                headers: { 'RequestVerificationToken': window.AppUser?.token }, 
                data: JSON.stringify(data)
            });

            if (typeof OpenPlaDiC !== 'undefined') OpenPlaDiC.UI.close();
            if (callback) callback(result);
            return result;
        } catch (error) {
            if (typeof OpenPlaDiC !== 'undefined') {
                OpenPlaDiC.UI.close();
                OpenPlaDiC.UI.showError('Fallo en Operación', 'No se pudieron consolidar los cambios dinámicos.');
            }
            throw error;
        }
    }
};

// =============================================================================
// 4. APISERVICE - PUENTE DE MICROSERVICIOS Y COMANDOS RAZOR
// =============================================================================
const APIService = {
    exec: async function (viewName, fields, callback) {
        const request = {
            View: viewName,
            Parameters: fields 
        };

        if (typeof OpenPlaDiC !== 'undefined') OpenPlaDiC.UI.showLoading('Ejecutando API...', 'Procesando microservicio dinámico.');

        try {
            const result = await $.ajax({
                type: 'POST',
                url: '/API/ExecAPI',
                contentType: 'application/json',
                headers: { 'RequestVerificationToken': window.AppUser?.token },
                data: JSON.stringify(request)
            });

            if (typeof OpenPlaDiC !== 'undefined') OpenPlaDiC.UI.close();
            if (callback) callback(result);
            return result;
        } catch (error) {
            if (typeof OpenPlaDiC !== 'undefined') {
                OpenPlaDiC.UI.close();
                OpenPlaDiC.UI.showError('API Exception', 'El microservicio dinámico falló al ejecutarse.');
            }
            throw error;
        }
    }
};

// =============================================================================
// 5. NAMESPACE OPENPLADIC.UI - NOTIFICACIONES Y LOCKS HOMOLOGADOS
// =============================================================================
var OpenPlaDiC = OpenPlaDiC || {};
OpenPlaDiC.UI = {
    showToast: function (message, icon = 'success') {
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
        });
        return Toast.fire({
            icon: icon,
            title: message
        });
    },

    showLoading: function (title = 'Procesando...', text = 'Por favor, espera un momento.') {
        Swal.fire({
            title: title,
            text: text,
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    },

    close: function () {
        Swal.close();
    },

    showError: function (title, message) {
        Swal.fire({
            icon: 'error',
            title: title,
            text: message,
            confirmButtonColor: '#3085d6'
        });
    }
};