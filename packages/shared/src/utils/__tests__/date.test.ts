import { describe, it, expect, vi, beforeAll, afterAll } from 'vitest';
import {
  formatDate,
  formatRelativeTime,
  isToday,
  isPast,
  addDays,
  startOfDay,
  endOfDay,
  diffInDays,
  formatDuration,
} from '../date';

// Test data builder for dates
class TestDateBuilder {
  private date: Date;

  constructor(year: number, month: number, day: number, hours = 0, minutes = 0, seconds = 0) {
    this.date = new Date(year, month - 1, day, hours, minutes, seconds);
  }

  toDate(): Date {
    return new Date(this.date);
  }

  addDays(days: number): TestDateBuilder {
    this.date.setDate(this.date.getDate() + days);
    return this;
  }
}

// Test suite organization follows: [Subject]_[Scenario]_[Expected]
describe('date utilities', () => {
  // Mock the current date for consistent testing
  const mockDate = new Date('2023-01-01T12:00:00Z');

  beforeAll(() => {
    // Mock the current date
    vi.useFakeTimers();
    vi.setSystemTime(mockDate);
  });

  afterAll(() => {
    // Restore the original Date object
    vi.useRealTimers();
  });

  describe('formatDate', () => {
    it('formatDate_withDefaultOptions_returnsFormattedDate', () => {
      // Arrange
      const date = new TestDateBuilder(2023, 1, 1, 12, 0, 0).toDate();

      // Act
      const result = formatDate(date);

      // Assert
      expect(result).toMatch(/^(Jan|1) (1|01),? 2023$/);
    });

    it('formatDate_withCustomOptions_returnsCustomFormattedDate', () => {
      // Arrange
      const date = new TestDateBuilder(2023, 1, 1, 12, 0, 0).toDate();
      const options = { year: 'numeric', month: 'long', day: 'numeric' } as const;

      // Act
      const result = formatDate(date, options);

      // Assert
      expect(result).toMatch(/^January (1|01),? 2023$/);
    });

    it('formatDate_withStringInput_returnsFormattedDate', () => {
      // Act
      const result = formatDate('2023-01-01T12:00:00Z');

      // Assert
      expect(result).toMatch(/^(Jan|1) (1|01),? 2023$/);
    });

    it('formatDate_withTimestamp_returnsFormattedDate', () => {
      // Arrange
      const timestamp = new TestDateBuilder(2023, 1, 1, 12, 0, 0).toDate().getTime();

      // Act
      const result = formatDate(timestamp);

      // Assert
      expect(result).toMatch(/^(Jan|1) (1|01),? 2023$/);
    });
  });

  describe('formatRelativeTime', () => {
    it('formatRelativeTime_withCurrentTime_returnsJustNow', () => {
      // Arrange
      const currentTime = new Date();

      // Act
      const result = formatRelativeTime(currentTime);

      // Assert
      expect(result).toBe('just now');
    });

    it('formatRelativeTime_withPastTime_returnsRelativeTimeString', () => {
      // Arrange
      const oneMinuteAgo = new Date(Date.now() - 60 * 1000);
      const twoHoursAgo = new Date(Date.now() - 2 * 60 * 60 * 1000);
      const threeDaysAgo = new Date(Date.now() - 3 * 24 * 60 * 60 * 1000);

      // Act & Assert
      expect(formatRelativeTime(oneMinuteAgo)).toBe('1 minute ago');
      expect(formatRelativeTime(twoHoursAgo)).toBe('2 hours ago');
      expect(formatRelativeTime(threeDaysAgo)).toBe('3 days ago');
    });
  });

  describe('isToday', () => {
    it('isToday_withCurrentDate_returnsTrue', () => {
      // Arrange
      const currentDate = new Date();

      // Act
      const result = isToday(currentDate);

      // Assert
      expect(result).toBe(true);
    });

    it('isToday_withDifferentDate_returnsFalse', () => {
      // Arrange
      const tomorrow = new TestDateBuilder(2023, 1, 2).toDate();

      // Act
      const result = isToday(tomorrow);

      // Assert
      expect(result).toBe(false);
    });
  });

  describe('isPast', () => {
    it('isPast_withPastDate_returnsTrue', () => {
      // Arrange
      const yesterday = new TestDateBuilder(2022, 12, 31).toDate();

      // Act
      const result = isPast(yesterday);

      // Assert
      expect(result).toBe(true);
    });

    it('isPast_withFutureDate_returnsFalse', () => {
      // Arrange
      const tomorrow = new TestDateBuilder(2023, 1, 2).toDate();

      // Act
      const result = isPast(tomorrow);

      // Assert
      expect(result).toBe(false);
    });
  });

  describe('addDays', () => {
    it('addDays_withPositiveDays_returnsDateWithDaysAdded', () => {
      // Arrange
      const date = new TestDateBuilder(2023, 1, 1).toDate();

      // Act
      const result = addDays(date, 5);

      // Assert
      expect(result.getDate()).toBe(6); // Jan 6, 2023
    });

    it('addDays_withNegativeDays_returnsDateWithDaysSubtracted', () => {
      // Arrange
      const date = new TestDateBuilder(2023, 1, 10).toDate();

      // Act
      const result = addDays(date, -5);

      // Assert
      expect(result.getDate()).toBe(5); // Jan 5, 2023
    });
  });

  describe('startOfDay', () => {
    it('startOfDay_withDate_returnsStartOfDay', () => {
      // Arrange
      const date = new TestDateBuilder(2023, 1, 1, 12, 34, 56).toDate();

      // Act
      const result = startOfDay(date);

      // Assert
      expect(result.getHours()).toBe(0);
      expect(result.getMinutes()).toBe(0);
      expect(result.getSeconds()).toBe(0);
      expect(result.getMilliseconds()).toBe(0);
    });
  });

  describe('endOfDay', () => {
    it('endOfDay_withDate_returnsEndOfDay', () => {
      // Arrange
      const date = new TestDateBuilder(2023, 1, 1, 12, 34, 56).toDate();

      // Act
      const result = endOfDay(date);

      // Assert
      expect(result.getHours()).toBe(23);
      expect(result.getMinutes()).toBe(59);
      expect(result.getSeconds()).toBe(59);
      expect(result.getMilliseconds()).toBe(999);
    });
  });

  describe('diffInDays', () => {
    it('diffInDays_withTwoDates_returnsCorrectDayDifference', () => {
      // Arrange
      const date1 = new TestDateBuilder(2023, 1, 1).toDate();
      const date2 = new TestDateBuilder(2023, 1, 10).toDate();

      // Act
      const result = diffInDays(date1, date2);

      // Assert
      expect(result).toBe(9);
    });

    it('diffInDays_withDateStrings_returnsCorrectDayDifference', () => {
      // Arrange
      const date1 = new Date('2023-01-01T00:00:00');
      const date2 = new Date('2023-01-11T00:00:00');

      // Act
      const result = diffInDays(date1, date2);

      // Assert
      expect(result).toBe(10);
    });
  });

  describe('formatDuration', () => {
    it('formatDuration_withVariousDurations_returnsFormattedString', () => {
      // Arrange & Act & Assert
      expect(formatDuration(1000)).toBe('1s');
      expect(formatDuration(60000)).toBe('1m');
      expect(formatDuration(3600000)).toBe('1h');
      expect(formatDuration(86400000)).toBe('1d');
      expect(formatDuration(93784000)).toBe('1d 2h 3m 4s');
    });

    it('should handle zero duration', () => {
      expect(formatDuration(0)).toBe('0s');
    });

    it('should handle negative duration', () => {
      expect(formatDuration(-1000)).toBe('0s');
    });
  });
});
