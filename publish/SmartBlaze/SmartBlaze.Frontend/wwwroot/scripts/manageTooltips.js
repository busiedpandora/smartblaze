let tooltipList = [];

window.initializeTooltips = function () {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
    tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
}

window.deactivateTooltips = function () {
    tooltipList.forEach(tooltipInstance => tooltipInstance.dispose());
    tooltipList = [];
}