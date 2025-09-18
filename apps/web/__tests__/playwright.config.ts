import path from 'path';

import { defineConfig, devices } from '@playwright/test';

const PORT = process.env.PORT || 3000;
const baseURL = `http://localhost:${PORT}`;

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: '.', // Look for tests in the current directory
  fullyParallel: true, // Run tests in parallel
  forbidOnly: !!process.env.CI, // Fail the build on CI if you accidentally left test.only in the source code
  retries: process.env.CI ? 2 : 0, // Retry on CI only
  workers: process.env.CI ? 1 : undefined, // Opt out of parallel tests on CI
  reporter: 'html', // Use HTML reporter for better test output visualization
  use: {
    baseURL,
    trace: 'on-first-retry', // Collect trace when retrying the failed test
    screenshot: 'only-on-failure', // Take screenshots only on test failures
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
  webServer: {
    command: 'pnpm dev', // Command to start the Next.js dev server
    url: baseURL,
    reuseExistingServer: !process.env.CI, // Reuse the server in watch mode
    env: {
      NODE_ENV: 'test',
    },
  },
});
