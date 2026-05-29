const UOM_ALIASES: Record<string, string> = {
  ea: 'each',
  each: 'each',
  pc: 'each',
  pcs: 'each',
  piece: 'each',
  pieces: 'each',
  unit: 'each',
  units: 'each',
  box: 'box',
  case: 'case',
  ft: 'ft',
  foot: 'ft',
  feet: 'ft',
  in: 'in',
  inch: 'in',
  inches: 'in',
  gal: 'gal',
  gallon: 'gal',
  gallons: 'gal',
  lb: 'lb',
  lbs: 'lb',
  pound: 'lb',
  pounds: 'lb',
  kg: 'kg',
  kilogram: 'kg',
  kilograms: 'kg',
  l: 'l',
  liter: 'l',
  liters: 'l',
  litre: 'l',
  litres: 'l',
  ml: 'ml',
  milliliter: 'ml',
  milliliters: 'ml',
}

export function normalizeUom(raw: string): string {
  const trimmed = raw.trim().toLowerCase()
  if (!trimmed) {
    return 'each'
  }
  return UOM_ALIASES[trimmed] ?? trimmed
}
