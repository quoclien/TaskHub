/**
 * Format a date to a human-readable string
 * @param date Date, timestamp, or date string to format
 * @param options Intl.DateTimeFormatOptions
 * @param locale Locale string (default: 'en-US')
 * @returns Formatted date string
 */
export function formatDate(
  date: Date | number | string | null | undefined,
  options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  },
  locale = 'en-US',
): string {
  // Handle null or undefined dates
  if (!date) {
    return 'N/A';
  }

  let dateObj: Date;

  if (typeof date === 'string') {
    // Handle ISO strings and ensure consistent parsing
    if (date.includes('T')) {
      // For ISO strings, parse as UTC to avoid timezone issues
      dateObj = new Date(date);
    } else {
      // For date-only strings, ensure consistent parsing
      dateObj = new Date(`${date}T00:00:00`);
    }
  } else if (typeof date === 'number') {
    dateObj = new Date(date);
  } else {
    dateObj = date;
  }

  // Handle invalid dates
  if (isNaN(dateObj.getTime())) {
    return 'Invalid Date';
  }

  return new Intl.DateTimeFormat(locale, options).format(dateObj);
}

/**
 * Format a date to a relative time string (e.g., "2 days ago")
 * @param date Date or timestamp to format
 * @returns Relative time string
 */
export function formatRelativeTime(date: Date | number | string): string {
  const dateObj = typeof date === 'string' || typeof date === 'number' ? new Date(date) : date;

  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - dateObj.getTime()) / 1000);

  const intervals = {
    year: 31536000,
    month: 2592000,
    week: 604800,
    day: 86400,
    hour: 3600,
    minute: 60,
    second: 1,
  } as const;

  for (const [unit, seconds] of Object.entries(intervals)) {
    const interval = Math.floor(diffInSeconds / seconds);
    if (interval >= 1) {
      return interval === 1 ? `1 ${unit} ago` : `${interval} ${unit}s ago`;
    }
  }

  return 'just now';
}

/**
 * Check if a date is today
 * @param date Date to check
 * @returns True if the date is today
 */
export function isToday(date: Date | string | number): boolean {
  const inputDate = new Date(date);
  const today = new Date();

  return (
    inputDate.getDate() === today.getDate() &&
    inputDate.getMonth() === today.getMonth() &&
    inputDate.getFullYear() === today.getFullYear()
  );
}

/**
 * Check if a date is in the past
 * @param date Date to check
 * @returns True if the date is in the past
 */
export function isPast(date: Date): boolean {
  return date < new Date();
}

/**
 * Add days to a date
 */
export function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

/**
 * Get the start of the day (00:00:00)
 */
export function startOfDay(date: Date): Date {
  const result = new Date(date);
  result.setHours(0, 0, 0, 0);
  return result;
}

/**
 * Get the end of the day (23:59:59.999)
 */
export function endOfDay(date: Date): Date {
  const result = new Date(date);
  result.setHours(23, 59, 59, 999);
  return result;
}

/**
 * Get the difference in days between two dates
 */
export function diffInDays(date1: Date, date2: Date): number {
  const diffTime = Math.abs(date2.getTime() - date1.getTime());
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
}

/**
 * Format a duration in milliseconds to a human-readable string
 * @param ms Duration in milliseconds
 * @param includeMs Whether to include milliseconds in the output
 * @example formatDuration(3661000) // "1h 1m 1s"
 * @example formatDuration(1500, true) // "1s 500ms"
 */
export function formatDuration(ms: number, includeMs = false): string {
  if (ms < 0) return '0s';

  const seconds = Math.floor((ms / 1000) % 60);
  const minutes = Math.floor((ms / (1000 * 60)) % 60);
  const hours = Math.floor((ms / (1000 * 60 * 60)) % 24);
  const days = Math.floor(ms / (1000 * 60 * 60 * 24));
  const milliseconds = Math.floor(ms % 1000);

  const parts = [];

  if (days > 0) parts.push(`${days}d`);
  if (hours > 0) parts.push(`${hours}h`);
  if (minutes > 0) parts.push(`${minutes}m`);
  if (seconds > 0 || parts.length === 0) parts.push(`${seconds}s`);
  if (includeMs && milliseconds > 0) parts.push(`${milliseconds}ms`);

  return parts.join(' ');
}
