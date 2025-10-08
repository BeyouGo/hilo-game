export function toKebabCase(value: string): string {
  return value
    .replace(/([a-z0-9])([A-Z])/g, '$1-$2') // insert - before capital letters
    .replace(/[\s_]+/g, '-')                // replace spaces/underscores with -
    .toLowerCase();
}

export function toCamelCase(value: string): string {
  return value.charAt(0).toLowerCase() + value.slice(1);
}

export function toPascalCase(value: string): string {
  if (!value) return '';

  return value
    // remove separators (- or _)
    .replace(/[-_]+(.)?/g, (_, c) => (c ? c.toUpperCase() : ''))
    // uppercase first letter
    .replace(/^(.)/, (_, c) => c.toUpperCase());
}
