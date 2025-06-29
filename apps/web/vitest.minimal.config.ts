/// <reference types="vitest" />
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'node', // Try with node environment instead of jsdom
    testTimeout: 5000,
    include: ['src/__tests__/minimal.test.ts'],
  },
});
