import $ from 'jquery';
import DataTables from 'datatables.net';
import 'datatables.net-bs4';
import 'datatables.net-responsive-bs4';
import 'datatables.net-scroller-bs4';
import Settings  = DataTables.Settings;
import Api = DataTables.Api;
//import DotNet from '@microsoft/dotnet-js-interop';

interface TableInstance {
    id: string;
    options: Settings;
}

export class BlazoredTable {
    
    private _tables: Array<TableInstance> = [];
    private _instances: Array<Api> = [];
    private _obj: any = null;

    //private dotNetHelper = window.DotNet;

    public create(id: string, options: Settings, assembly: string, method: string): void {
        this._obj = $(`#${id}`);
        options.ajax = (data: object, callback: ((data: any) => void), settings: DataTables.SettingsLegacy) => {
            let result = this.loadInfoFromServer(assembly, method, data);
            result.then((f: any) => {
                //console.log("f: ", f); /* RESULTADO DEL SERVIDOR */
                callback(f);
            }) 

        }

        console.log("options", options);
        this._obj.DataTable(options);
        
        this._instances.push(this._obj);
        this._tables.push({id: id, options: options});
    }

    private async loadInfoFromServer(assembly: string, method: string, data: object): Promise<any> {
        let result = await window.DotNet.invokeMethodAsync(assembly, method, data); /*https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-6.0 */
        return result;
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
        DataTables: DataTables<any>;
        DotNet: any;
    }
}

window.BlazoredTable = new BlazoredTable();
window.DataTables = DataTables;
