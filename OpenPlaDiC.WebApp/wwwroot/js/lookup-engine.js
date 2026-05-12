
let currentLookupField = "";

function openLookup(entityName, fieldName) {
    currentLookupField = fieldName;
    $('#lookupTitle').text('Buscador: ' + entityName);
    $('#lookupResults').html('');
    $('#lookupSearch').val('');
    
    // Guardamos la entidad actual para la búsqueda
    $('#lookupModal').data('entity', entityName);
    new bootstrap.Modal(document.getElementById('lookupModal')).show();
}

function executeLookup() {
    const term = $('#lookupSearch').val();
    const entity = $('#lookupModal').data('entity');

    $.get(`/Search/Lookup?entity=${entity}&term=${term}`, function(data) {
        let html = '';
        data.forEach(item => {
            html += `
                <button type="button" class="list-group-item list-group-item-action" 
                        onclick="selectLookup('${item.id}', '${item.text}')">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">${item.text}</h6>
                        <small class="text-primary">${item.folio}</small>
                    </div>
                </button>`;
        });
        $('#lookupResults').html(html || '<p class="p-3">No hay resultados.</p>');
    });
}

function selectLookup(id, text) {
    $(`#val-${currentLookupField}`).val(id);
    $(`#text-${currentLookupField}`).val(text);
    bootstrap.Modal.getInstance(document.getElementById('lookupModal')).hide();
}
