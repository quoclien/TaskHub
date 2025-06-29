import { describe, it, expect } from 'vitest';
import {
  assert,
  assertDefined,
  assertString,
  assertNumber,
  assertObject,
  assertArray,
  assertSchema,
  assertInstanceOf,
  assertEnumValue,
  assertUuid,
  assertEmail,
  assertInRange,
} from '../assert';
import { z } from 'zod';

class MyClass {}

enum MyEnum {
  Foo = 'foo',
  Bar = 'bar',
}

describe('assert utilities', () => {
  it('assert throws when condition is false', () => {
    expect(() => assert(false, 'fail')).toThrow('fail');
  });
  it('assert does not throw when condition is true', () => {
    expect(() => assert(true, 'fail')).not.toThrow();
  });
  it('assertDefined throws for null/undefined, returns value otherwise', () => {
    expect(() => assertDefined(null)).toThrow();
    expect(() => assertDefined(undefined)).toThrow();
    expect(assertDefined('ok')).toBe('ok');
  });
  it('assertString throws for non-string, returns string', () => {
    expect(() => assertString(123)).toThrow();
    expect(assertString('abc')).toBe('abc');
  });
  it('assertNumber throws for non-number, returns number', () => {
    expect(() => assertNumber('abc')).toThrow();
    expect(assertNumber(42)).toBe(42);
  });
  it('assertObject throws for non-object, returns object', () => {
    expect(() => assertObject('abc')).toThrow();
    expect(assertObject({ a: 1 })).toEqual({ a: 1 });
  });
  it('assertArray throws for non-array, returns array', () => {
    expect(() => assertArray('abc')).toThrow();
    expect(assertArray([1, 2])).toEqual([1, 2]);
  });
  it('assertSchema validates with zod', () => {
    const schema = z.object({ foo: z.string() });
    expect(assertSchema(schema, { foo: 'bar' })).toEqual({ foo: 'bar' });
    expect(() => assertSchema(schema, { foo: 1 })).toThrow();
  });
  it('assertInstanceOf throws for wrong type, returns value otherwise', () => {
    expect(() => assertInstanceOf({}, MyClass)).toThrow();
    const obj = new MyClass();
    expect(assertInstanceOf(obj, MyClass)).toBe(obj);
  });
  it('assertEnumValue throws for invalid, returns value otherwise', () => {
    expect(() => assertEnumValue(MyEnum, 'baz')).toThrow();
    expect(assertEnumValue(MyEnum, 'foo')).toBe('foo');
  });
  it('assertUuid throws for invalid, returns uuid otherwise', () => {
    expect(() => assertUuid('not-a-uuid')).toThrow();
    expect(assertUuid('550e8400-e29b-41d4-a716-446655440000')).toBe('550e8400-e29b-41d4-a716-446655440000');
  });
  it('assertEmail throws for invalid, returns email otherwise', () => {
    expect(() => assertEmail('not@email')).toThrow();
    expect(assertEmail('test@email.com')).toBe('test@email.com');
  });
  it('assertInRange throws for out of range, returns value otherwise', () => {
    expect(() => assertInRange(5, 10, 20)).toThrow();
    expect(assertInRange(15, 10, 20)).toBe(15);
  });
});
