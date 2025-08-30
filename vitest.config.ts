import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
  plugins: [react(), tsconfigPaths()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./vitest.setup.ts'],
    // Exclude Playwright E2E tests - they run separately
    exclude: [
      '**/node_modules/**',
      '**/dist/**',
      '**/.next/**',
      '**/__tests__/**/*.spec.ts', // Playwright E2E tests
      '**/*.e2e.test.*',
      '**/playwright.config.*',
    ],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      reportsDirectory: './coverage',
      exclude: [
        '**/node_modules/**',
        '**/dist/**',
        '**/coverage/**',
        '**/.next/**',
        '**/*.d.ts',
        '**/.{idea,git,cache,output,temp}/**',
        '**/{karma,rollup,webpack,vite,vitest,jest,ava,babel,nyc,cypress,tsup,build,eslint,prettier}*.*',
        '**/__tests__/**/*.spec.ts', // Exclude Playwright files from coverage
      ],
      thresholds: {
        lines: 70, // Adjusted to realistic initial target
        functions: 70,
        branches: 60,
        statements: 70,
      },
    },
  },
});
