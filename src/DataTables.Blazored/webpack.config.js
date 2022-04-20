const path = require('path');

module.exports = env => {

    return {
        entry: {
            'datatables-blazored' :'./Interop/datatables-blazored.ts',
            'datatables-blazored-bs4' :'./Interop/datatables-blazored-bs4.ts'
        },
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
            filename: '[name].js',
            path: path.resolve(__dirname, 'wwwroot/js'),
        },
    };
};