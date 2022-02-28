import $ from 'jquery';
import DataTables from 'datatables.net';
import 'datatables.net-bs4';
import 'datatables.net-responsive-bs4';
import 'datatables.net-scroller-bs4';
import Settings = DataTables.Settings;

//import DotNet from '@microsoft/dotnet-js-interop';

export class BlazoredTable {
    
    private table: any = null;
    private tableRef: any = null;

    public create(id: string, options: Settings, ajax: any, data: any, dotNet: any): void {
        this.table = $(`#${id}`);

        if (dotNet !== null) {
           options.ajax = (data: object, callback: ((data: any) => void), settings: DataTables.SettingsLegacy) => {
               let result = BlazoredTable.loadInfoFromServer(data, dotNet);
               result.then(f => callback(f));
           }
        }
        else if (ajax != null) {
           ajax.data = function (s: any) {
               return JSON.stringify(s);
           };
           options.ajax = ajax;
        }
        else if (data != null) {
           options.data = data;
        }

        this.tableRef = this.table.DataTable(options);
    }

    public reload(): void {
        this.tableRef.ajax.reload();
    }

    private static async loadInfoFromServer(data: object, dotNet: any): Promise<any> {
        return await dotNet.invokeMethodAsync('OnLoadAsync', data);
    }
}

declare global {
    interface Window { 
        BlazoredTable: BlazoredTable;
        DataTables: DataTables<any>;
        DotNet: any;
    }
}

window.BlazoredTable = new BlazoredTable();
window.DataTables = DataTables;
