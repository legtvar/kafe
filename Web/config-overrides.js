module.exports = function override(config, env) {
    config.module.rules[1].oneOf.splice(2, 0, {
        test: /\.less$/i,
        exclude: /\.module\.(less)$/,
        use: [
            { loader: 'style-loader' },
            { loader: 'css-loader' },
            {
                loader: 'less-loader',
                options: {
                    lessOptions: {
                        javascriptEnabled: true,
                    },
                },
            },
        ],
    });
    
    config.module.rules[2] = {
        test: /\.i18n\.html/,
        type: "asset/source"
    };

    // Do not treat warnings as errors
    config.plugins = config.plugins.map((plugin) => {
        if (plugin.constructor.name === 'ESLintWebpackPlugin') {
            plugin.options = {
                ...plugin.options,
                failOnError: false,
                failOnWarning: false,
            };
        }
        return plugin;
    });
    
    return config;
};
