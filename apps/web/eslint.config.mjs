import { dirname } from "path";
import { fileURLToPath } from "url";
import { FlatCompat } from "@eslint/eslintrc";
import js from '@eslint/js';
import typescriptEslint from 'typescript-eslint';
import reactRecommended from 'eslint-plugin-react/configs/recommended.js';
import reactHooks from 'eslint-plugin-react-hooks';
import jsxA11y from 'eslint-plugin-jsx-a11y';
import nextPlugin from '@next/eslint-plugin-next';
import importPlugin from 'eslint-plugin-import';
import prettierConfig from 'eslint-config-prettier';
import prettierPlugin from 'eslint-plugin-prettier';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const compat = new FlatCompat({
  baseDirectory: __dirname,
  recommendedConfig: js.configs.recommended,
});

const eslintConfig = [
  // Global ignores
  {
    ignores: ['node_modules/', '.next/'],
  },

  // Base JavaScript configuration
  js.configs.recommended,

  // TypeScript configurations
  ...typescriptEslint.config(
    {
      files: ['**/*.ts', '**/*.tsx'],
      extends: [
        ...typescriptEslint.configs.strictTypeChecked,
        ...typescriptEslint.configs.stylisticTypeChecked,
      ],
      languageOptions: {
        parserOptions: {
          project: true,
          tsconfigRootDir: __dirname,
        },
      },
    }
  ),
  
  // Prettier integration (must be after other configs to override them)
  prettierConfig,
  {
    plugins: {
      'prettier': prettierPlugin,
    },
    rules: {
      ...prettierPlugin.configs.recommended.rules,
      'prettier/prettier': 'error',
    },
  },
  
  // React and Next.js specific configurations
  {
    files: ['**/*.{ts,tsx}'],
    plugins: {
      'react': reactRecommended,
      'react-hooks': reactHooks,
      'jsx-a11y': jsxA11y,
      '@next/next': nextPlugin,
      'import': importPlugin,
    },
    languageOptions: {
      parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module',
        ecmaFeatures: {
          jsx: true,
        },
        project: './tsconfig.json',
      },
    },
    rules: {
      // TypeScript rules
      '@typescript-eslint/explicit-function-return-type': ['warn', { allowExpressions: true }],
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      '@typescript-eslint/consistent-type-imports': 'error',
      '@typescript-eslint/no-explicit-any': 'warn',
      
      // React rules
      'react/react-in-jsx-scope': 'off', // Not needed with Next.js
      'react/prop-types': 'off', // Not needed with TypeScript
      'react/display-name': 'off',
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': 'warn',
      
      // JSX A11y rules
      'jsx-a11y/alt-text': 'warn',
      'jsx-a11y/aria-props': 'warn',
      'jsx-a11y/aria-role': ['warn', { ignoreNonDOM: true }],
      'jsx-a11y/role-supports-aria-props': 'warn',
      
      // Import rules
      'import/order': [
        'error',
        {
          groups: ['builtin', 'external', 'internal', 'parent', 'sibling', 'index'],
          'newlines-between': 'always',
          alphabetize: { order: 'asc', caseInsensitive: true },
        },
      ],
      'import/no-duplicates': 'error',
      'import/no-unresolved': 'error',
      'import/named': 'error',
      
      // General code quality
      'no-console': ['warn', { allow: ['warn', 'error'] }],
      'no-debugger': 'warn',
      'no-var': 'error',
      'prefer-const': 'error',
      'prefer-arrow-callback': 'warn',
      'object-shorthand': 'warn',
      'no-param-reassign': 'error',
      'eqeqeq': ['error', 'always', { null: 'ignore' }],
      'no-nested-ternary': 'warn',
      'no-unneeded-ternary': 'warn',
    },
    settings: {
      react: {
        version: 'detect',
      },
      'import/resolver': {
        typescript: {},
        node: {
          extensions: ['.js', '.jsx', '.ts', '.tsx'],
        },
      },
    },
  },
  // Next.js specific rules
  {
    files: ['**/*.tsx'],
    rules: {
      '@next/next/no-html-link-for-pages': 'off', // Handled by Next.js
      '@next/next/no-img-element': 'off', // We'll use next/image
    },
  },
];

export default eslintConfig;
