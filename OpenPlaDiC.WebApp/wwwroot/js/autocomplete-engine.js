
let debounceTimer;

function quickSearch(event, entity, fieldName) {
    const term = event.target.value;
    const suggestionsDiv = $(`#suggestions-${fieldName}`);

    // Si presiona flecha abajo o Enter, podemos manejar el foco (opcional)
    if (term.length < 3) {
        suggestionsDiv.hide();
        return;
    }

    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        $.get(`/Search/Lookup?entity=${entity}&term=${term}`, function(data) {
            if (data.length > 0) {
                let html = '';
                data.slice(0, 5).forEach(item => { // Limitamos a 5 sugerencias
                    html += `
                        <button type="button" class="list-group-item list-group-item-action py-1" 
                                onclick="selectLookup('${item.id}', '${item.text}', '${fieldName}')">
                            <small><strong>${item.folio}</strong> - ${item.text}</small>
                        </button>`;
                });
                suggestionsDiv.html(html).show();
            } else {
                suggestionsDiv.hide();
            }
        });
    }, 300); // Espera 300ms para no saturar el servidor
}

// Sobrecarga de selectLookup para manejar el cierre de sugerencias
function selectLookup(id, text, fieldName) {
    $(`#val-${fieldName}`).val(id);
    $(`#text-${fieldName}`).val(text);
    $(`#suggestions-${fieldName}`).hide();
    
    // Si el modal de búsqueda estaba abierto, lo cerramos
    const modalEl = document.getElementById('lookupModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
}
