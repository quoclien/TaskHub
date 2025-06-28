// External dependencies
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

/**
 * Combines multiple class names and merges Tailwind CSS classes
 * 
 * @param {...ClassValue[]} inputs - Class names or class name objects to be combined
 * @returns {string} A single string of combined and deduplicated class names
 * 
 * @example
 * // Returns 'p-4 bg-blue-500 hover:bg-blue-600'
 * cn('p-4', 'bg-blue-500', 'hover:bg-blue-600')
 * 
 * @example
 * // Returns 'p-4 bg-red-500 text-white'
 * cn('p-4', { 'bg-red-500': true, 'text-white': true }, ['bg-blue-500', 'text-black'])
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
