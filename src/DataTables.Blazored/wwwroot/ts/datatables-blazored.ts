import $ from 'jquery';
import DataTables from 'datatables.net';
import Settings = DataTables.Settings;
import ColumnSettings = DataTables.ColumnSettings;

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
    //private tableRef: any = null;
    static tableRef: any = null;

    static rowsSelected: any = [];

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

        /*if (options.columns !== null) {
            let cs: ColumnSettings = {
                defaultContent: ""
            }

            options.columns?.push(cs)
        } */

        Table.tableRef = this.table.DataTable(options);

        Table.tableRef.on("click", "tbody tr", (f: Event) => {
            const data: any = Table.tableRef.row(f.currentTarget).data();

            if (f.currentTarget !== null) {
                Table.tableRef.$('tr.selected').removeClass('selected');
                (<Element>f.currentTarget).classList.add('selected');
            }

            Table.selectRow(data, dotNet);
        });
    }

    public reload(): void {
        Table.tableRef.ajax.reload();
    }

    private static async loadInfoFromServer(data: object, dotNet: any): Promise<any> {
        return await dotNet.invokeMethodAsync('OnLoadAsync', data);
    }

    private static async selectRow(data: object, dotNet: any): Promise<any> {
        return await dotNet.invokeMethodAsync('OnRowSelectedAsync', data);
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
