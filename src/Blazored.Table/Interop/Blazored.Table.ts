import $ from 'jquery';
import DataTables from 'datatables.net';
import Settings = DataTables.Settings;
import Api = DataTables.Api;

interface TableInstance {
    id: string;
    options: Settings;
}

export class BlazoredTable {

    private _tables: Array<TableInstance> = [];
    private _instances: Array<Api> = [];

    public create(id:string, options:Settings):void {
        var fn = $(`#${id}`).dataTable();
        var table = fn.DataTable(options)
        // var fn = $.fn.DataTable(options);
        // var table = fn.table(`#${id}`);
        // let config: DataTables.Settings ={ 
        //     ...options,
        //     ...{
        //         dom: 'Bfrtip',
        //         buttons: [
        //             'copy', 'csv', 'excel', 'pdf', 'print'
        //         ]
        //     }
        // };
        // @ts-ignore
        this._instances.push(table);
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
