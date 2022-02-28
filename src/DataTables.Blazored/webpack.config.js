const path = require('path');

module.exports = env => {

    return {
        entry: './Interop/DataTables.Blazored.ts',
        devtool: env && env.production ? 'none' : 'source-map',
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    use: 'ts-loader',
                    exclude: /node_modules/,
                },
            ],
        },
        resolve: {
            extensions: ['.tsx', '.ts', '.js'],
        },
        output: {
            filename: 'datatables-blazored.js',
            path: path.resolve(__dirname, 'wwwroot'),
        },
    };
};