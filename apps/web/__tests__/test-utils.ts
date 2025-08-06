import { test as base, expect } from '@playwright/test';
import { faker } from '@faker-js/faker';

// Test user factory
export const createTestUser = () => ({
  email: `test_${Date.now()}_${faker.internet.email()}`,
  password: `P@ssw0rd_${Date.now()}`,
  firstName: faker.person.firstName(),
  lastName: faker.person.lastName(),
});

// Extend base test with auth fixtures
export const test = base.extend<{
  authenticatedPage: any;
  testUser: any;
}>({
  testUser: [async ({}, use) => {
    // Create a new test user for each test
    const user = createTestUser();
    await use(user);
    // Cleanup can be added here if needed
  }, { scope: 'test' }],

  authenticatedPage: async ({ page, testUser }, use) => {
    // Sign up the test user
    await page.goto('/sign-up');
    
    // Fill in the sign-up form
    await page.getByLabel(/first name/i).fill(testUser.firstName);
    await page.getByLabel(/last name/i).fill(testUser.lastName);
    await page.getByLabel(/email/i).fill(testUser.email);
    await page.getByLabel(/^password$/i).fill(testUser.password);
    await page.getByLabel(/confirm password/i).fill(testUser.password);

    // Submit the form
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Wait for navigation to complete
    await expect(page).toHaveURL('/boards');
    
    // Use the authenticated page in the test
    await use(page);
    
    // Logout after the test
    await page.goto('/api/auth/signout');
    await page.getByRole('button', { name: /sign out/i }).click();
  },
});

export { expect } from '@playwright/test';
