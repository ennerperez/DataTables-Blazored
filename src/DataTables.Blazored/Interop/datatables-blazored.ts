import $ from 'jquery';
import DataTables from 'datatables.net';
import Settings = DataTables.Settings;

import "datatables.net-autofill";
import "datatables.net-buttons";
import "datatables.net-colreorder";
import "datatables.net-fixedcolumns";
import "datatables.net-fixedheader";
import "datatables.net-keytable";
import "datatables.net-responsive";
import "datatables.net-rowgroup";
import "datatables.net-rowreorder";
import "datatables.net-scroller";
import "datatables.net-searchbuilder";
import "datatables.net-searchpanes";
import "datatables.net-staterestore";
import "datatables.net-select";

export class Table {
    
    private table: any = null;
    private tableRef: any = null;

    public create(id: string, options: Settings, ajax: any, data: any, dotNet: any): void {
        this.table = $(`#${id}`);

        if (dotNet !== null) {
           options.ajax = (data: object, callback: ((data: any) => void), settings: DataTables.SettingsLegacy) => {
               let result = Table.loadInfoFromServer(data, dotNet);
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
        Table: Table;
        DataTables: DataTables<any>;
        DotNet: any;
    }
}

window.Table = new Table();
window.DataTables = DataTables;
