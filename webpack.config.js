// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

var path = require("path");
const CopyPlugin = require('copy-webpack-plugin');

module.exports = {
    mode: "development",
    entry: "./src/App/App.fs.js",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js",
    },
    devServer: {
        publicPath: "/",
        contentBase: "./public",
        port: 8081,
    },
    module: {
        rules: [
            {
                test: /\.(sa|c)ss$/,
                use: ["style-loader", "css-loader"]
              }
             ]
    },
    plugins: [
        new CopyPlugin({
            patterns: [
                {
                    from: "README.md",
                    to: "README.md"
                },
            ],
        }),
    ],
}
