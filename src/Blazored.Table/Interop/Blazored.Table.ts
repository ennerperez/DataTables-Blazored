import $ from 'jquery';
import DataTables from 'datatables.net';
import 'datatables.net-bs4';
import 'datatables.net-responsive-bs4';
import 'datatables.net-scroller-bs4';
import Settings  = DataTables.Settings;
import Api = DataTables.Api;

interface TableInstance {
    id: string;
    options: Settings;
}

export class BlazoredTable {

    private _tables: Array<TableInstance> = [];
    private _instances: Array<Api> = [];
    private _obj: any = null;

    public create(id: string, options: Settings): void {
        this._obj = $(`#${id}`);
        this._obj.DataTable(options);
        
        this._instances.push(this._obj);
        this._tables.push({id: id, options: options});
    }

    public destroy(id:string):void{
        var index = this._tables.findIndex(x => x.id == id);
        var table =  this._instances[index];
        table.destroy();
        this._tables.splice(index, 1);
    }

}

declare global {
    interface Window { 
        BlazoredTable: BlazoredTable;
        DataTables: DataTables<any>
    }
}

window.BlazoredTable = new BlazoredTable();
window.DataTables = DataTables;
