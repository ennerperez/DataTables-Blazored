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

    public create(id: string, options: Settings): void {
        this._obj = $(`#${id}`);
        options.ajax = (data: object, callback: ((data: any) => void), settings: DataTables.SettingsLegacy) => {
            let result = this.loadInfoFromServer(data);
            result.then((f: any) => {
                console.log("f: ", f); /* RESULTADO DEL SERVIDOR */
                console.log("data:", data);
                console.log("settings:", settings);
                callback({
                    draw: 1,
                    recordsTotal: 160,
                    recordsFiltered: 160,
                    data: [
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                        { id: "1", firstName: "Alex", lastName: "Acosta" },
                        { id: "2", firstName: "Mario", lastName: "Munoz" },
                    ]
                });
            }) 

        }

        console.log("options", options);
        this._obj.DataTable(options);
        
        this._instances.push(this._obj);
        this._tables.push({id: id, options: options});
    }

    private async loadInfoFromServer(data: object): Promise<any> {
        let result = await window.DotNet.invokeMethodAsync('Blazored.Table', 'ReturnArrayAsync', data); /*https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-6.0 */
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
