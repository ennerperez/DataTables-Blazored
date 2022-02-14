import $
    from 'jquery';
import DataTables
    from 'datatables.net';
import 'datatables.net-bs4';
import 'datatables.net-responsive-bs4';
import 'datatables.net-scroller-bs4';
import Settings = DataTables.Settings;

//import DotNet from '@microsoft/dotnet-js-interop';

interface TableInstance {
    id: string;
    options: Settings;
}

export class BlazoredTable {
    
    // private _tables: Array<TableInstance> = [];
    // private _instances: Array<Api> = [];
    private _obj: any = null;

    public create(id: string, options: Settings, assembly: string, method: string, ajax:any, data: any): void {
        this._obj = $(`#${id}`);
        if (method != null && method != '') {
            options.ajax = (data: object, callback: ((data: any) => void), settings: DataTables.SettingsLegacy) => {
                let result = BlazoredTable.loadInfoFromServer(assembly, method, data);
                result.then((f: any) => { callback(f);});
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
        this._obj.DataTable(options);
        // this._instances.push(this._obj);
        // this._tables.push({id: id, options: options});
    }

    private static async loadInfoFromServer(assembly: string, method: string, data: object): Promise<any> {
         /*https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-6.0 */
        return await window.DotNet.invokeMethodAsync(assembly, method, data);
    }

    // public destroy(id:string):void{
    //     // var index = this._tables.findIndex(x => x.id == id);
    //     // var table =  this._instances[index];
    //     // table.destroy();
    //     // this._tables.splice(index, 1);
    // }

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
