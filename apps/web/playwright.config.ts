import { defineConfig, devices } from '@playwright/test';

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
// require('dotenv').config();

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './__tests__',
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: 'html',
  /* Global expect timeout */
  expect: {
    timeout: 10000,
  },
  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    baseURL: 'http://127.0.0.1:3000',

    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    trace: 'on-first-retry',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
      testMatch: ['**/*.spec.ts'],
    },
    {
      name: 'chromium-critical',
      use: { 
        ...devices['Desktop Chrome'],
        // Increase timeout for critical path tests
        actionTimeout: 15000,
      },
      testMatch: ['**/critical-path.spec.ts'],
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
      testMatch: ['**/*.spec.ts'],
      testIgnore: ['**/critical-path.spec.ts'], // Skip critical path on Firefox for speed
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
      testMatch: ['**/auth.spec.ts', '**/accessibility-performance.spec.ts'], // Essential tests only
    },
  ],

  /* Run your local dev server before starting the tests */
  webServer: [
    {
      command: 'pnpm dev',
      url: 'http://127.0.0.1:3000',
      reuseExistingServer: !process.env.CI,
      timeout: 120 * 1000,
      env: {
        NEXT_PUBLIC_DISABLE_MOCKS: 'true',
        NEXT_PUBLIC_API_BASE_URL: 'http://127.0.0.1:5205/api',
      },
    },
    {
      command: 'dotnet run --project ../api/TaskHub.Api.csproj',
      url: 'http://127.0.0.1:5205',
      reuseExistingServer: !process.env.CI,
      timeout: 120 * 1000,
      cwd: '../api',
    },
  ],
});
