import { MDCSelect } from '@material/select';

export function init(elem) {
    elem._select = MDCSelect.attachTo(elem);
}

export function setDisabled(elem, value) {
    elem._select.disabled = value;
}

export function listItemClick(elem, elemText) {
    elem.innerText = elemText;
    elem.click();
}

function tryScrollToYear(id, attempt: number) {
    setTimeout(() => {
        var element = document.getElementById(id);
        if (element) {
            element.scrollIntoView({ behavior: 'auto', block: 'nearest', inline: 'nearest' });
        } else {
            if (attempt < 10) {
                tryScrollToYear(id, attempt + 1);
            }
        }
    }, 16);
}

export function scrollToYear(id) {
    // we allow up to 10 attempts every 16ms, because Virtualize may have not yet rendered the year we want to scroll to.
    tryScrollToYear(id, 0);
}
