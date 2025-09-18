import { describe, it, expect } from 'vitest';
import {
  truncate,
  toTitleCase,
  toKebabCase,
  toCamelCase,
  toPascalCase,
  toSnakeCase,
  formatNumberWithCommas,
  formatPercent,
  formatFileSize,
  randomString,
  slugify,
  maskSensitive,
} from '../format';

describe('format utilities', () => {
  it('truncate returns original if short enough', () => {
    expect(truncate('abc', 5)).toBe('abc');
  });
  it('truncate truncates and adds ellipsis', () => {
    expect(truncate('abcdef', 3)).toBe('abc...');
  });
  it('toTitleCase works', () => {
    expect(toTitleCase('hello world')).toBe('Hello World');
  });
  it('toKebabCase works', () => {
    expect(toKebabCase('Hello World')).toBe('hello-world');
  });
  it('toCamelCase works', () => {
    expect(toCamelCase('hello-world')).toBe('helloWorld');
  });
  it('toPascalCase works', () => {
    expect(toPascalCase('hello-world')).toBe('HelloWorld');
  });
  it('toSnakeCase works', () => {
    expect(toSnakeCase('Hello World')).toBe('hello_world');
  });
  it('formatNumberWithCommas works', () => {
    expect(formatNumberWithCommas(1000000)).toBe('1,000,000');
  });
  it('formatPercent works', () => {
    expect(formatPercent(0.5)).toBe('50%');
    expect(formatPercent(0.1234, 2)).toBe('12.34%');
    expect(formatPercent(-0.5, 0, true)).toBe('-50%');
  });
  it('formatFileSize works', () => {
    expect(formatFileSize(1024)).toMatch(/1\s?KB/);
    expect(formatFileSize(1048576)).toMatch(/1\s?MB/);
  });
  it('randomString returns correct length', () => {
    expect(randomString(10)).toHaveLength(10);
  });
  it('slugify works', () => {
    expect(slugify('Hello World!')).toBe('hello-world');
  });
  it('maskSensitive masks all by default', () => {
    expect(maskSensitive('secret123')).toBe('*********');
  });
  it('maskSensitive masks except visible', () => {
    expect(maskSensitive('secret123', 3)).toBe('sec******');
  });
});
