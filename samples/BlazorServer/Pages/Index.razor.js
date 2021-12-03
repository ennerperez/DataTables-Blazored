window.GetDataAsync = () => {
    DotNet.invokeMethodAsync('BlazorServer', 'GetDataAsync')
        .then(data => {
            console.log(data);
        });
};