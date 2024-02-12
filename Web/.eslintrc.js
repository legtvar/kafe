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
        'react/display-name': 'off',
        'no-case-declarations': 'off',
        'no-prototype-builtins': 'off',
        '@typescript-eslint/no-explicit-any': 'off',
        '@typescript-eslint/no-this-alias': 'off',
        '@typescript-eslint/no-unused-vars': 'off',
        '@typescript-eslint/ban-types': 'warn',
        'prefer-const': 'warn',
    },
    settings: {
        react: {
            version: 'detect',
        },
    },
};
