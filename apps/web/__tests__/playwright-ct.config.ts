import { defineConfig, devices } from '@playwright/experimental-ct-react';
import path from 'path';

/**
 * See https://playwright.dev/docs/test-components for more information
 */
export default defineConfig({
  testDir: './',  // Look for test files in the current directory
  testMatch: '**/*.spec.tsx',  // Only match component test files
  timeout: 10 * 1000,
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    trace: 'on-first-retry',
    ctPort: 3100,
    ctViteConfig: {
      resolve: {
        alias: {
          '@': path.resolve(__dirname, '../src'),
          '@taskhub/shared': path.resolve(__dirname, '../../../packages/shared'),
        },
      },
    },
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
});
