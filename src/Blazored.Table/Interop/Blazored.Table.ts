import focusTrap, { Options, FocusTrap } from 'focus-trap';

interface FocusTrapInstance {
    id: string;
    focusTrap: FocusTrap;
}

interface ModalInstance {
    id: string;
}

export class BlazoredTable {

}

declare global {
    interface Window { BlazoredTable: BlazoredTable; }
}

window.BlazoredTable = new BlazoredTable();