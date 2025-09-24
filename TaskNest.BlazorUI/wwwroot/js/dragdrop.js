export function initializeDragDrop(dotNetReference) {
    // Wait for Sortable to be available (loaded via CDN)
    if (typeof Sortable === 'undefined') {
        // Load Sortable.js if not already loaded
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js';
        script.onload = () => setupSortable(dotNetReference);
        document.head.appendChild(script);
    } else {
        setupSortable(dotNetReference);
    }
}

function setupSortable(dotNetReference) {
    const containers = document.querySelectorAll('.column-tasks');

    containers.forEach(container => {
        Sortable.create(container, {
            group: 'tasks',
            animation: 150,
            ghostClass: 'task-ghost',
            chosenClass: 'task-chosen',
            dragClass: 'task-drag',
            onEnd: function (evt) {
                const taskId = evt.item.getAttribute('data-task-id');
                const sourceColumnId = evt.from.getAttribute('data-column-id');
                const targetColumnId = evt.to.getAttribute('data-column-id');
                const newIndex = evt.newIndex;

                dotNetReference.invokeMethodAsync('TaskMoved', taskId, sourceColumnId, targetColumnId, newIndex);
            }
        });
    });
}
