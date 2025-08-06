import { test, expect } from '@playwright/test';
import { faker } from '@faker-js/faker';

test.describe('Authentication Flows', () => {
  // Test user credentials
  const testUser = {
    email: `testuser_${Date.now()}@example.com`,
    password: 'Test@1234!',
    firstName: 'Test',
    lastName: 'User'
  };

  test.beforeEach(async ({ page }) => {
    // Navigate to the home page before each test
    await page.goto('/');
  });

  test('should allow user to sign up with valid credentials', async ({ page }) => {
    // Navigate to sign up page
    await page.getByRole('link', { name: /sign up/i }).click();
    await expect(page).toHaveURL('/sign-up');

    // Fill in the sign-up form
    await page.getByLabel(/first name/i).fill(testUser.firstName);
    await page.getByLabel(/last name/i).fill(testUser.lastName);
    await page.getByLabel(/email/i).fill(testUser.email);
    await page.getByLabel(/^password$/i).fill(testUser.password);
    await page.getByLabel(/confirm password/i).fill(testUser.password);

    // Submit the form
    await page.getByRole('button', { name: /create account/i }).click();

    // Verify successful sign up and redirection
    await expect(page).toHaveURL('/boards');
    await expect(page.getByRole('heading', { name: /your boards/i })).toBeVisible();
  });

  test('should show validation errors for invalid sign-up data', async ({ page }) => {
    // Navigate to sign up page
    await page.getByRole('link', { name: /sign up/i }).click();

    // Try to submit an empty form
    await page.getByRole('button', { name: /create account/i }).click();

    // Verify validation errors
    await expect(page.getByText(/first name is required/i)).toBeVisible();
    await expect(page.getByText(/last name is required/i)).toBeVisible();
    await expect(page.getByText(/email is required/i)).toBeVisible();
    await expect(page.getByText(/password is required/i)).toBeVisible();
  });

  test('should allow user to sign in with valid credentials', async ({ page }) => {
    // Navigate to sign in page
    await page.getByRole('link', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/sign-in');

    // Fill in the sign-in form
    await page.getByLabel(/email or username/i).fill(testUser.email);
    await page.getByLabel(/password/i).fill(testUser.password);

    // Submit the form
    await page.getByRole('button', { name: /sign in/i }).click();

    // Verify successful sign in and redirection
    await expect(page).toHaveURL('/boards');
    await expect(page.getByRole('heading', { name: /your boards/i })).toBeVisible();
  });

  test('should show error for invalid login credentials', async ({ page }) => {
    // Navigate to sign in page
    await page.getByRole('link', { name: /sign in/i }).click();

    // Fill in with invalid credentials
    await page.getByLabel(/email or username/i).fill('nonexistent@example.com');
    await page.getByLabel(/password/i).fill('wrongpassword');

    // Submit the form
    await page.getByRole('button', { name: /sign in/i }).click();

    // Verify error message
    await expect(page.getByText(/invalid login credentials/i)).toBeVisible();
  });

  test('should allow user to toggle password visibility', async ({ page }) => {
    // Test in sign up page (similar test can be added for sign in)
    await page.getByRole('link', { name: /sign up/i }).click();

    // Initially password should be hidden
    const passwordInput = page.getByLabel(/^password$/i);
    await expect(passwordInput).toHaveAttribute('type', 'password');

    // Toggle password visibility
    await page.getByRole('button', { name: /toggle password visibility/i }).first().click();
    
    // Password should be visible
    await expect(passwordInput).toHaveAttribute('type', 'text');
  });

  test('should redirect authenticated users away from auth pages', async ({ page }) => {
    // First sign in
    await page.getByRole('link', { name: /sign in/i }).click();
    await page.getByLabel(/email or username/i).fill(testUser.email);
    await page.getByLabel(/password/i).fill(testUser.password);
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/boards');

    // Try to access sign in page again
    await page.goto('/sign-in');
    
    // Should be redirected to boards
    await expect(page).toHaveURL('/boards');
  });
});
