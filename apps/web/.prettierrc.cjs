/** @type {import('prettier').Config} */
module.exports = {
  // Use single quotes instead of double quotes
  singleQuote: true,

  // Use 2 spaces for indentation
  tabWidth: 2,

  // Add a trailing comma wherever possible (for cleaner Git diffs)
  trailingComma: 'es5',

  // Print semicolons at the end of statements
  semi: true,

  // Use single quotes in JSX
  jsxSingleQuote: true,

  // Line width before wrapping
  printWidth: 100,

  // Use spaces instead of tabs
  useTabs: false,

  // Put the `>` of a multi-line JSX element at the end of the last line instead of being alone on the next line
  bracketSameLine: false,

  // Include parentheses around a sole arrow function parameter
  arrowParens: 'always',

  // Format embedded code if Prettier can identify it
  embeddedLanguageFormatting: 'auto',

  // Use the default HTML handling
  htmlWhitespaceSensitivity: 'css',

  // Line endings (Unix-style)
  endOfLine: 'lf',

  // Don't insert @format pragma
  insertPragma: false,

  // Don't wrap markdown text as-is
  proseWrap: 'preserve',

  // Don't require a special @format marker for formatting
  requirePragma: false,

  // Don't add spaces inside brackets
  bracketSpacing: true,

  // Put each JSX attribute on a new line when the component doesn't fit on one line
  singleAttributePerLine: true,
};
