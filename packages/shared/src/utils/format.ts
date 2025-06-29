/**
 * Truncate a string to a specified length and add an ellipsis if it's too long
 * @param str String to truncate
 * @param maxLength Maximum length before truncation
 * @param ellipsis String to append if truncated (default: '...')
 * @returns Truncated string
 */
export function truncate(
  str: string,
  maxLength: number,
  ellipsis = '...'
): string {
  if (str.length <= maxLength) return str;
  return str.slice(0, maxLength) + ellipsis;
}

/**
 * Convert a string to title case
 * @example "hello world" -> "Hello World"
 */
export function toTitleCase(str: string): string {
  return str.replace(
    /\w\S*/g,
    (txt) => txt.charAt(0).toUpperCase() + txt.slice(1).toLowerCase()
  );
}

/**
 * Convert a string to kebab-case
 * @example "Hello World" -> "hello-world"
 */
export function toKebabCase(str: string): string {
  return str
    .replace(/([a-z])([A-Z])/g, '$1-$2')
    .replace(/[\s_]+/g, '-')
    .toLowerCase();
}

/**
 * Convert a string to camelCase
 * @example "hello-world" -> "helloWorld"
 */
export function toCamelCase(str: string): string {
  return str
    .replace(/(?:^\w|[A-Z]|\b\w)/g, (word, index) => {
      return index === 0 ? word.toLowerCase() : word.toUpperCase();
    })
    .replace(/[\s-]+/g, '');
}

/**
 * Convert a string to PascalCase
 * @example "hello-world" -> "HelloWorld"
 */
export function toPascalCase(str: string): string {
  return str
    .replace(/(?:^\w|[A-Z]|\b\w)/g, (word) => word.toUpperCase())
    .replace(/[\s-]+/g, '');
}

/**
 * Convert a string to snake_case
 * @example "Hello World" -> "hello_world"
 */
export function toSnakeCase(str: string): string {
  return str
    .replace(/([a-z])([A-Z])/g, '$1_$2')
    .replace(/[\s-]+/g, '_')
    .toLowerCase();
}

/**
 * Format a number with commas as thousands separators
 * @example 1000000 -> "1,000,000"
 */
export function formatNumberWithCommas(num: number): string {
  return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}

/**
 * Format a number as a percentage
 * @example 0.5 -> "50%"
 */
export function formatPercent(
  value: number,
  decimals = 0,
  includeSign = true
): string {
  const percent = value * 100;
  const formatted = percent.toFixed(decimals);
  return includeSign ? `${formatted}%` : formatted;
}

/**
 * Format a file size in bytes to a human-readable string
 * @example 1024 -> "1 KB"
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

/**
 * Generate a random string of specified length
 * @param length Length of the random string to generate
 * @param chars Characters to use for generation (default: alphanumeric)
 */
export function randomString(
  length: number,
  chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'
): string {
  let result = '';
  const charsLength = chars.length;
  
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * charsLength));
  }
  
  return result;
}

/**
 * Slugify a string for use in URLs
 * @example "Hello World!" -> "hello-world"
 */
export function slugify(str: string): string {
  return str
    .toLowerCase()
    .trim()
    .replace(/[^\w\s-]/g, '') // Remove non-word chars
    .replace(/[\s_-]+/g, '-') // Replace spaces and underscores with hyphens
    .replace(/^-+|-+$/g, ''); // Remove leading/trailing hyphens
}

/**
 * Mask sensitive information like passwords or API keys
 * @example maskSensitive("secret123") -> "*******"
 * @example maskSensitive("secret123", 3) -> "sec..."
 */
export function maskSensitive(
  str: string,
  visibleChars = 0,
  maskChar = '*'
): string {
  if (!str) return '';
  if (visibleChars <= 0) return maskChar.repeat(str.length);
  
  const visible = str.slice(0, visibleChars);
  const masked = maskChar.repeat(Math.max(0, str.length - visibleChars));
  
  return `${visible}${masked}`;
}
