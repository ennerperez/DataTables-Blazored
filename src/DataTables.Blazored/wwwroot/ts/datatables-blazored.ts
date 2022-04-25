import $ from 'jquery';
import DataTables from 'datatables.net';
import Settings = DataTables.Settings;
import ColumnSettings = DataTables.ColumnSettings;

export class Table {
    
    private table: any = null;
    //private tableRef: any = null;
    static tableRef: any = null;

    static rowsSelected: any = [];
    
    private static assignCallback(callback: any){
        if (callback == null || callback.namespace == null || callback.function == null) {
            return undefined;
        }

        const namespace = window[callback.namespace];
        if (namespace === null || namespace === undefined) {
            return undefined;
        }

        const func = namespace[callback.function];
        if (typeof func === 'function') {
            return func;
        } else {
            return undefined;
        }
    }

    public create(id: string, options: Settings, ajax: any, data: any, dotNet: any): void {
        this.table = $(`#${id}`);

        if (options.columns != null) {
            for (let i = 0; i < options.columns.length; i++) {
                const col = options.columns[i];
                col.createdCell = Table.assignCallback(col.createdCell);
                col.render = Table.assignCallback(col.render);
            }
        }
        
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

    public destroy(): void {
        Table.tableRef.destroy();
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
