module.exports = {
  // Lint and format TypeScript and JavaScript files
  '**/*.{ts,tsx,js,jsx}': ['eslint --fix', 'prettier --write'],
  // Format other files
  '**/*.{json,md,mdx,yml,yaml,css,scss}': ['prettier --write'],
};
