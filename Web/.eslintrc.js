module.exports = {
    env: {
        browser: true,
        node: true,
    },
    parser: '@typescript-eslint/parser',
    parserOptions: {
        ecmaFeatures: {
            jsx: true,
        },
        ecmaVersion: 'latest',
        project: 'tsconfig.json',
        sourceType: 'module',
    },
    plugins: [
        '@typescript-eslint',
        'eslint-plugin-flowtype',
        'eslint-plugin-import',
        'eslint-plugin-jsdoc',
        'eslint-plugin-jsx-a11y',
        'eslint-plugin-prefer-arrow',
        'eslint-plugin-react-hooks',
        'react',
    ],
    extends: ['eslint:recommended', 'plugin:react/recommended', 'plugin:@typescript-eslint/recommended'],
    root: true,
    rules: {
        'react/react-in-jsx-scope': 'off',
        'react/jsx-uses-react': 'off',
        'react/no-unescaped-entities': 'off',
        'no-case-declarations': 'off',

        // Maybe switch to warn?
        '@typescript-eslint/no-explicit-any': 'off',
        '@typescript-eslint/no-this-alias': 'off',
        '@typescript-eslint/ban-types': 'warn',
        '@typescript-eslint/no-unused-vars': 'warn',
        'prefer-const': 'warn',
    },
    settings: {
        react: {
            version: 'detect',
        },
    },
};
