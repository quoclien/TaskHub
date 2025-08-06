import { test, expect, type Page } from '@playwright/test';
import { faker } from '@faker-js/faker';

// Define test user interface for better type safety
interface TestUser {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

test.describe('Authentication Flows', () => {
  // Test user credentials with explicit type
  const testUser: TestUser = {
    email: `testuser_${Date.now()}@example.com`,
    password: 'Test@1234!',
    firstName: 'Test',
    lastName: 'User'
  };

  // Helper function to navigate to sign up page
  const goToSignUp = async (page: Page): Promise<void> => {
    await page.getByRole('link', { name: /sign up/i }).click();
    await expect(page).toHaveURL('/sign-up');
  };

  // Helper function to navigate to sign in page
  const goToSignIn = async (page: Page): Promise<void> => {
    await page.getByRole('link', { name: /sign in/i }).click();
    await expect(page).toHaveURL('/sign-in');
  };

  test.beforeEach(async ({ page }) => {
    // Navigate to the home page before each test
    await page.goto('/');
  });

  test('should allow user to sign up with valid credentials', async ({ page }) => {
    try {
      // Navigate to sign up page using helper
      await goToSignUp(page);

      // Fill in the sign-up form with test data
      const formData = [
        { selector: 'first name', value: testUser.firstName },
        { selector: 'last name', value: testUser.lastName },
        { selector: 'email', value: testUser.email },
        { selector: 'password', value: testUser.password, exact: true },
        { selector: 'confirm password', value: testUser.password },
      ];

      for (const { selector, value, exact } of formData) {
        const label = exact ? new RegExp(`^${selector}$`, 'i') : new RegExp(selector, 'i');
        await page.getByLabel(label).fill(value);
      }

      // Submit the form and wait for navigation
      const navigationPromise = page.waitForURL('**/boards');
      await page.getByRole('button', { name: /create account/i }).click();
      await navigationPromise;

      // Verify successful sign up and redirection
      await expect(page).toHaveURL('/boards');
      await expect(page.getByRole('heading', { name: /your boards/i })).toBeVisible({
        timeout: 5000,
      });
    } catch (error) {
      console.error('Sign up test failed:', error);
      throw error;
    }
  });

  test('should show validation errors for invalid sign-up data', async ({ page }) => {
    try {
      // Navigate to sign up page using helper
      await goToSignUp(page);

      // Try to submit an empty form
      await page.getByRole('button', { name: /create account/i }).click();

      // Verify all required field validation errors are shown
      const validationErrors = [
        /first name is required/i,
        /last name is required/i,
        /email is required/i,
        /password is required/i,
      ];

      for (const errorPattern of validationErrors) {
        await expect(page.getByText(errorPattern)).toBeVisible({
          timeout: 2000,
        });
      }
    } catch (error) {
      console.error('Validation error test failed:', error);
      throw error;
    }
  });

  test('should allow user to sign in with valid credentials', async ({ page }) => {
    try {
      // Navigate to sign in page using helper
      await goToSignIn(page);

      // Fill in the sign-in form with test data
      await page.getByLabel(/email or username/i).fill(testUser.email);
      await page.getByLabel(/password/i).fill(testUser.password);

      // Submit the form and wait for navigation
      const navigationPromise = page.waitForURL('**/boards');
      await page.getByRole('button', { name: /sign in/i }).click();
      await navigationPromise;

      // Verify successful sign in and redirection
      await expect(page).toHaveURL('/boards');
      await expect(page.getByRole('heading', { name: /your boards/i })).toBeVisible({
        timeout: 5000,
      });
    } catch (error) {
      console.error('Sign in test failed:', error);
      throw error;
    }
  });

  test('should show error for invalid login credentials', async ({ page }) => {
    try {
      // Navigate to sign in page using helper
      await goToSignIn(page);

      // Fill in with invalid credentials
      const invalidCredentials = {
        email: 'nonexistent@example.com',
        password: 'wrongpassword',
      };

      await page.getByLabel(/email or username/i).fill(invalidCredentials.email);
      await page.getByLabel(/password/i).fill(invalidCredentials.password);

      // Submit the form
      await page.getByRole('button', { name: /sign in/i }).click();

      // Verify error message is shown
      await expect(
        page.getByText(/invalid login credentials/i),
        'Error message should be visible for invalid credentials'
      ).toBeVisible({
        timeout: 3000,
      });
    } catch (error) {
      console.error('Invalid credentials test failed:', error);
      throw error;
    }
  });

  test('should allow user to toggle password visibility', async ({ page }) => {
    try {
      // Test in sign up page (similar test can be added for sign in)
      await goToSignUp(page);

      // Initially password should be hidden
      const passwordInput = page.getByLabel(/^password$/i);
      await expect(
        passwordInput,
        'Password input should be hidden by default'
      ).toHaveAttribute('type', 'password');

      // Toggle password visibility
      const toggleButton = page
        .getByRole('button', { name: /toggle password visibility/i })
        .first();
      
      await toggleButton.click();
      
      // Password should be visible after toggle
      await expect(
        passwordInput,
        'Password input should be visible after toggle'
      ).toHaveAttribute('type', 'text');

      // Toggle back to hidden
      await toggleButton.click();
      
      // Password should be hidden again
      await expect(
        passwordInput,
        'Password input should be hidden after second toggle'
      ).toHaveAttribute('type', 'password');
    } catch (error) {
      console.error('Password visibility toggle test failed:', error);
      throw error;
    }
  });

  test('should redirect authenticated users away from auth pages', async ({ page }) => {
    try {
      // First sign in
      await goToSignIn(page);
      
      // Fill in and submit the sign-in form
      await page.getByLabel(/email or username/i).fill(testUser.email);
      await page.getByLabel(/password/i).fill(testUser.password);
      
      const navigationPromise = page.waitForURL('**/boards');
      await page.getByRole('button', { name: /sign in/i }).click();
      await navigationPromise;
      
      await expect(page).toHaveURL('/boards');

      // Try to access sign in page again
      await page.goto('/sign-in');
      
      // Should be redirected back to boards
      await expect(
        page,
        'Should be redirected to /boards when trying to access /sign-in while authenticated'
      ).toHaveURL('/boards');

      // Try to access sign up page as well
      await page.goto('/sign-up');
      
      // Should be redirected back to boards
      await expect(
        page,
        'Should be redirected to /boards when trying to access /sign-up while authenticated'
      ).toHaveURL('/boards');
    } catch (error) {
      console.error('Auth redirection test failed:', error);
      throw error;
    }
  });
});
