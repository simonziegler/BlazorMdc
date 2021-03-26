import { MDCRipple } from '@material/ripple';

export function init(elem): void {
    elem._ripple = MDCRipple.attachTo(elem);
}
