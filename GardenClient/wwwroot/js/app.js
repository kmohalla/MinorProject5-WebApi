// app.js — the Garden Planner client.
// It calls GardenApi with fetch() and shows the results on the page.
// Every table can be read (Get all, Get by ID); Plants can also be added,
// edited and deleted.
'use strict';

// ---------------------------------------------------------------------------
// Settings: change these if your API address or controller names change.
// ---------------------------------------------------------------------------

// GardenApi's https address from its Properties/launchSettings.json, plus /api
const API_BASE = 'https://localhost:7212/api';

// One entry per table. "route" is the controller name (api/[controller]).
// "columns" lists the service model properties to show: [property, heading, format].
const TABLES = {
    plants: {
        route: 'Plants',
        singular: 'plant',
        editable: true,
        columns: [
            ['id', 'ID'],
            ['commonName', 'Common name'],
            ['family', 'Family'],
            ['type', 'Type'],
            ['color', 'Color'],
            ['sourceName', 'Source Name']
        ]
    },
    suppliers: {
        route: 'Suppliers',
        singular: 'supplier',
        columns: [
            ['id', 'ID'],
            ['supplier', 'Supplier'],
            ['location', 'Location'],
            ['plants', 'Plants', 'list']
        ]
    },
    tools: {
        route: 'Tools',
        singular: 'tool',
        columns: [
            ['id', 'ID'],
            ['toolName', 'Tool Name'],
            ['material', 'Material'],
            ['toolWeightMetricKg', 'Weight (kg)', 'number'],
            ['toolWeightImperialLbs', 'Weight (lb)', 'number'],
            ['toolLengthMetricCm', 'Length (cm)', 'number'],
            ['toolLengthImperialIn', 'Length (in)', 'number']
        ]
    },
    guides: {
        route: 'Guides',
        singular: 'guide',
        columns: [
            ['id', 'ID'],
            ['title', 'Title'],
            ['author', 'Author'],
            ['plantingSeason', 'Planting Season'],
            ['difficulty', 'Difficulty']
        ]
    },
    gardenBeds: {
        route: 'GardenBeds',
        singular: 'garden bed',
        columns: [
            ['id', 'ID'],
            ['bedName', 'Bed Name'],
            ['bedState', 'Bed State'],
            ['datePlanted', 'Date planted', 'date'],
            ['guideTitle', 'Guide Title'],
            ['plantName', 'Plant Name'],
            ['toolName', 'Tool Name']
        ]
    }
};

// ---------------------------------------------------------------------------
// Talking to the API
// ---------------------------------------------------------------------------

// Sends one request and always returns { ok, status, data, message }.
// It never throws, so every caller can handle errors the same way.
async function callApi(method, path, body) {
    const options = { method: method, headers: { 'Accept': 'application/json, text/plain' } };
    if (body !== undefined) {
        options.headers['Content-Type'] = 'application/json';
        options.body = JSON.stringify(body);
    }

    let response;
    try {
        response = await fetch(API_BASE + path, options);
    } catch (error) {
        // fetch only throws when no answer came back at all:
        // the API is not running, the address is wrong, or CORS blocked it.
        return {
            ok: false,
            status: 0,
            data: null,
            message: 'Could not reach the API at ' + API_BASE + '. Make sure GardenApi is running. ' +
                     'If it is, this is usually a CORS problem: press F12 and check the Console.'
        };
    }

    // Read the body as text first, then try JSON. The controllers return
    // JSON objects, JSON strings (Ok("...")) and plain-text error messages.
    const text = await response.text();
    let data = null;
    if (text) {
        try { data = JSON.parse(text); } catch (e) { data = text; }
    }

    return { ok: response.ok, status: response.status, data: data, message: describeBody(data) };
}

// Turns whatever the API sent back into a readable sentence.
function describeBody(data) {
    if (data === null || data === undefined) return '';
    if (typeof data === 'string') return data;
    if (typeof data === 'boolean' || typeof data === 'number') return '';
    if (data.errors) {
        // ASP.NET Core validation errors: { errors: { CommonName: ["..."] } }
        return Object.values(data.errors).flat().join(' ');
    }
    if (data.detail) return data.detail;
    if (data.title) return data.title;
    return '';
}

// Reads a property whatever its capitalisation (id, ID, Id ...).
function field(record, name) {
    if (record == null) return undefined;
    if (name in record) return record[name];
    const lower = name.toLowerCase();
    const key = Object.keys(record).find(k => k.toLowerCase() === lower);
    return key === undefined ? undefined : record[key];
}

// ---------------------------------------------------------------------------
// Showing results
// ---------------------------------------------------------------------------

function panelFor(tableName) {
    return document.querySelector('.panel[data-table="' + tableName + '"]');
}

// kind is "success", "error" or "info"
function showMessage(element, kind, text) {
    element.className = 'message ' + (text ? 'message-' + kind : '');
    element.textContent = text || '';
}

function panelMessage(tableName, kind, text) {
    showMessage(panelFor(tableName).querySelector('.message'), kind, text);
}

function formatValue(value, format) {
    if (value === null || value === undefined || value === '') return '—';
    if (format === 'list') return Array.isArray(value) && value.length ? value.join(', ') : '—';
    if (format === 'number') return Number(value).toLocaleString(undefined, { maximumFractionDigits: 2 });
    if (format === 'date') {
        // "2026-05-10T00:00:00" -> 5/10/2026, without time-zone shifts
        const parts = String(value).substring(0, 10).split('-');
        if (parts.length === 3) {
            return new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2])).toLocaleDateString();
        }
        return String(value);
    }
    return String(value);
}

// Draws an HTML table for a list of records.
function renderTable(tableName, records) {
    const config = TABLES[tableName];
    const result = panelFor(tableName).querySelector('.result');
    result.textContent = '';
    if (!records.length) return;

    const wrapper = document.createElement('div');
    wrapper.className = 'table-wrap';
    const table = document.createElement('table');

    const headRow = table.createTHead().insertRow();
    config.columns.forEach(function (column) {
        const th = document.createElement('th');
        th.scope = 'col';
        th.textContent = column[1];
        if (column[2] === 'number' || column[0] === 'id') th.className = 'num';
        headRow.appendChild(th);
    });
    if (config.editable) {
        const th = document.createElement('th');
        th.scope = 'col';
        th.textContent = 'Actions';
        headRow.appendChild(th);
    }

    const body = table.createTBody();
    records.forEach(function (record) {
        const row = body.insertRow();
        config.columns.forEach(function (column) {
            const cell = row.insertCell();
            cell.textContent = formatValue(field(record, column[0]), column[2]);
            if (column[2] === 'number' || column[0] === 'id') cell.className = 'num';
        });
        if (config.editable) {
            const id = field(record, 'id');
            const cell = row.insertCell();
            cell.className = 'actions';
            cell.appendChild(makeButton('Edit', 'btn btn-small btn-secondary', 'edit', id));
            cell.appendChild(makeButton('Delete', 'btn btn-small btn-danger', 'delete', id));
        }
    });

    wrapper.appendChild(table);
    result.appendChild(wrapper);
}

function makeButton(label, className, action, id) {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = className;
    button.textContent = label;
    button.dataset.action = action;
    button.dataset.id = id;
    return button;
}

// ---------------------------------------------------------------------------
// Reading: Get all and Get by ID (every table)
// ---------------------------------------------------------------------------

async function getAll(tableName) {
    const config = TABLES[tableName];
    panelMessage(tableName, 'info', 'Loading…');
    const r = await callApi('GET', '/' + config.route);

    if (r.status === 404) {
        // The controllers answer 404 when a table has no rows
        renderTable(tableName, []);
        panelMessage(tableName, 'info', 'No ' + config.singular + ' records found.');
        return;
    }
    if (!r.ok) {
        renderTable(tableName, []);
        panelMessage(tableName, 'error', errorText(r));
        return;
    }

    const records = Array.isArray(r.data) ? r.data : [];
    renderTable(tableName, records);
    panelMessage(tableName, 'success',
        'Loaded ' + records.length + ' ' + config.singular + (records.length === 1 ? '' : 's') + '.');
}

async function getById(tableName, id) {
    const config = TABLES[tableName];
    if (!Number.isInteger(id) || id < 1) {
        panelMessage(tableName, 'error', 'Enter a whole number of 1 or more.');
        return;
    }
    panelMessage(tableName, 'info', 'Loading…');
    const r = await callApi('GET', '/' + config.route + '/' + id);

    if (r.status === 404) {
        renderTable(tableName, []);
        panelMessage(tableName, 'error', 'There is no ' + config.singular + ' with ID ' + id + '.');
        return;
    }
    if (!r.ok) {
        renderTable(tableName, []);
        panelMessage(tableName, 'error', errorText(r));
        return;
    }

    renderTable(tableName, [r.data]);
    panelMessage(tableName, 'success', 'Showing ' + config.singular + ' ' + id + '.');
}

function errorText(r) {
    if (r.status === 0) return r.message;
    return 'Error ' + r.status + (r.message ? ': ' + r.message : '.');
}

// ---------------------------------------------------------------------------
// Plants: insert, update, delete
// ---------------------------------------------------------------------------

const plantForm = () => document.getElementById('plantForm');
const plantFormMessage = () => document.getElementById('plantFormMessage');

// Fills the Supplier dropdown from GET api/Suppliers
async function loadSupplierOptions() {
    const select = plantForm().elements.sourceName;
    const current = select.value;
    const r = await callApi('GET', '/' + TABLES.suppliers.route);
    const suppliers = r.ok && Array.isArray(r.data) ? r.data : [];

    select.textContent = '';
    select.appendChild(new Option('(none)', ''));
    suppliers
        .map(s => field(s, 'supplier'))
        .filter(name => name)
        .sort((a, b) => a.localeCompare(b))
        .forEach(name => select.appendChild(new Option(name, name)));

    select.value = current;
    if (select.value !== current) select.value = '';
    if (!r.ok && r.status !== 404) {
        showMessage(plantFormMessage(), 'error', 'Could not load suppliers. ' + errorText(r));
    }
}

function resetPlantForm() {
    const form = plantForm();
    form.reset();
    form.elements.id.value = '0';
    document.getElementById('plantFormTitle').textContent = 'Add a plant';
    document.getElementById('plantSaveButton').textContent = 'Add plant';
}

async function startEditPlant(id) {
    const r = await callApi('GET', '/' + TABLES.plants.route + '/' + id);
    if (!r.ok) {
        showMessage(plantFormMessage(), 'error', r.status === 404 ? 'That plant no longer exists.' : errorText(r));
        return;
    }
    await loadSupplierOptions();

    const form = plantForm();
    const plant = r.data;
    form.elements.id.value = field(plant, 'id');
    form.elements.commonName.value = field(plant, 'commonName') || '';
    form.elements.family.value = field(plant, 'family') || '';
    form.elements.type.value = field(plant, 'type') || '';
    form.elements.color.value = field(plant, 'color') || '';
    const supplier = field(plant, 'sourceName') || '';
    if (supplier && ![...form.elements.sourceName.options].some(o => o.value === supplier)) {
        form.elements.sourceName.appendChild(new Option(supplier, supplier));
    }
    form.elements.sourceName.value = supplier;

    document.getElementById('plantFormTitle').textContent = 'Edit plant ' + id;
    document.getElementById('plantSaveButton').textContent = 'Save changes';
    showMessage(plantFormMessage(), 'info', 'Editing plant ' + id + '. Change the fields and select Save changes.');
    form.scrollIntoView({ behavior: 'smooth', block: 'start' });
    form.elements.commonName.focus({ preventScroll: true });
}

async function savePlant(event) {
    event.preventDefault();
    const form = plantForm();

    // Check the required boxes in the browser first, for a quick message
    const missing = ['commonName', 'family', 'type', 'color']
        .filter(name => !form.elements[name].value.trim())
        .map(name => form.elements[name].closest('label').firstChild.textContent.trim());
    if (missing.length) {
        showMessage(plantFormMessage(), 'error', 'Please fill in: ' + missing.join(', ') + '.');
        return;
    }

    const id = Number(form.elements.id.value) || 0;
    const plant = {
        id: id,
        commonName: form.elements.commonName.value.trim(),
        family: form.elements.family.value.trim(),
        type: form.elements.type.value.trim(),
        color: form.elements.color.value.trim(),
        sourceName: form.elements.sourceName.value || null
    };

    const isNew = id === 0;
    showMessage(plantFormMessage(), 'info', isNew ? 'Adding…' : 'Saving…');
    const r = isNew
        ? await callApi('POST', '/' + TABLES.plants.route, plant)               // insert
        : await callApi('PUT', '/' + TABLES.plants.route + '/' + id, plant);   // update

    if (!r.ok) {
        // 400 = a business rule or validation message from the API
        showMessage(plantFormMessage(), 'error', errorText(r));
        return;
    }

    showMessage(plantFormMessage(), 'success',
        r.message || (isNew ? 'Plant added.' : 'Plant ' + id + ' updated.'));
    resetPlantForm();
    await getAll('plants');
}

async function deletePlant(id, button) {
    const row = button.closest('tr');
    const name = row ? row.cells[1].textContent : 'this plant';
    if (!window.confirm('Delete plant "' + name + '" (ID ' + id + ')?')) return;

    const r = await callApi('DELETE', '/' + TABLES.plants.route + '/' + id);
    if (r.status === 404) {
        await getAll('plants');
        panelMessage('plants', 'error', 'Plant ' + id + ' was already gone.');
        return;
    }
    if (!r.ok) {
        // 403 = a business rule blocked the delete (for example, a bed uses it)
        panelMessage('plants', 'error', errorText(r));
        return;
    }
    if (r.data === false) {
        panelMessage('plants', 'error', 'The API reported that plant ' + id + ' was not deleted.');
        return;
    }

    if (Number(plantForm().elements.id.value) === id) resetPlantForm();
    await getAll('plants');
    panelMessage('plants', 'success', 'Deleted plant ' + id + ' ("' + name + '").');
}

// ---------------------------------------------------------------------------
// Tabs and theme
// ---------------------------------------------------------------------------

function showTab(tableName) {
    document.querySelectorAll('.tab').forEach(function (tab) {
        const selected = tab.dataset.tab === tableName;
        tab.setAttribute('aria-selected', String(selected));
        tab.tabIndex = selected ? 0 : -1;
    });
    document.querySelectorAll('.panel').forEach(function (panel) {
        panel.hidden = panel.dataset.table !== tableName;
    });
    try { localStorage.setItem('garden-tab', tableName); } catch (e) { }
    if (tableName === 'plants') loadSupplierOptions();
}

function updateThemeButton() {
    const button = document.getElementById('themeToggle');
    const dark = window.gardenTheme.get() === 'dark';
    button.textContent = dark ? 'Light mode' : 'Dark mode';
    button.setAttribute('aria-pressed', String(dark));
}

// ---------------------------------------------------------------------------
// Wire everything up once the page has loaded
// ---------------------------------------------------------------------------

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('apiAddress').textContent = API_BASE;

    // Theme toggle
    updateThemeButton();
    document.getElementById('themeToggle').addEventListener('click', function () {
        window.gardenTheme.set(window.gardenTheme.get() === 'dark' ? 'light' : 'dark');
        updateThemeButton();
    });

    // Tabs
    document.querySelectorAll('.tab').forEach(function (tab) {
        tab.addEventListener('click', function () { showTab(tab.dataset.tab); });
    });
    let firstTab = 'plants';
    try { firstTab = localStorage.getItem('garden-tab') || 'plants'; } catch (e) { }
    showTab(TABLES[firstTab] ? firstTab : 'plants');

    // Get all / Get by ID / Edit / Delete buttons in every panel
    document.querySelectorAll('.panel').forEach(function (panel) {
        const tableName = panel.dataset.table;

        panel.querySelector('[data-action="get-all"]').addEventListener('click', function () {
            getAll(tableName);
        });

        panel.querySelector('form.by-id').addEventListener('submit', function (event) {
            event.preventDefault();
            getById(tableName, Number(event.target.elements.id.value));
        });

        panel.querySelector('.result').addEventListener('click', function (event) {
            const button = event.target.closest('button[data-action]');
            if (!button) return;
            const id = Number(button.dataset.id);
            if (button.dataset.action === 'edit') startEditPlant(id);
            if (button.dataset.action === 'delete') deletePlant(id, button);
        });
    });

    // Plant form
    plantForm().addEventListener('submit', savePlant);
    document.getElementById('plantClearButton').addEventListener('click', function () {
        resetPlantForm();
        showMessage(plantFormMessage(), 'info', '');
    });
});
