const webpack = require('webpack');
const path = require('path');

//module.exports = env => {
module.exports = {
        entry: {
            'datatables-blazored.js' : ['./wwwroot/ts/datatables-blazored.ts','./wwwroot/css/datatables-blazored.css'],
            'datatables-blazored-bs4.js' : ['./wwwroot/ts/datatables-blazored-bs4.ts','./wwwroot/css/datatables-blazored-bs4.css'],
            'datatables-blazored-bs5.js' : ['./wwwroot/ts/datatables-blazored-bs5.ts', './wwwroot/css/datatables-blazored-bs5.css'],
            // 'datatables-blazored.css' :'./wwwroot/css/datatables-blazored.css',
            // 'datatables-blazored-bs4.css' :'./wwwroot/css/datatables-blazored-bs4.css',
            // 'datatables-blazored-bs5.css' :'./wwwroot/css/datatables-blazored-bs5.css',
        },
        //devtool: env && env.production ? 'none' : 'source-map',
        module: {
            rules: [
                {
                    test: /\.tsx?$/,
                    use: 'ts-loader',
                    exclude: /node_modules/
                },
                {
                    test: /\.css$/,
                    use: [
                        'style-loader',
                        'css-loader'
                    ],
                    exclude: /node_modules/
                },
                {
                    test: /\.scss$/,
                    use: [
                        'sass-loader'
                    ],
                    exclude: /node_modules/
                },
                {
                    test: /\.svg$/,
                    use: 'file-loader',
                    exclude: /node_modules/
                }
            ],
        },
        output: {
            filename: '[name]',
            path: path.resolve(__dirname, 'wwwroot'),
        },
    };
