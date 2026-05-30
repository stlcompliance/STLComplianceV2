const SEMANTIC_SEGMENT_MIN_LENGTH = 2
const SEMANTIC_SEGMENT_MAX_LENGTH = 64

const DEFAULT_STOP_WORDS = new Set([
  'a',
  'an',
  'and',
  'for',
  'in',
  'of',
  'on',
  'or',
  'the',
  'to',
  'with',
])

const KNOWN_ALIASES: Array<{ phrase: string; alias: string }> = [
  // Core document / compliance shorthand
  { phrase: 'driver qualification file', alias: 'dqf' },
  { phrase: 'driver qualification files', alias: 'dqf' },
  { phrase: 'safety data sheet', alias: 'sds' },
  { phrase: 'safety data sheets', alias: 'sds' },
  { phrase: 'material safety data sheet', alias: 'msds' },
  { phrase: 'material safety data sheets', alias: 'msds' },
  { phrase: 'standard operating procedure', alias: 'sop' },
  { phrase: 'standard operating procedures', alias: 'sop' },
  { phrase: 'job hazard analysis', alias: 'jha' },
  { phrase: 'job safety analysis', alias: 'jsa' },
  { phrase: 'safe work permit', alias: 'swp' },
  { phrase: 'safe work permits', alias: 'swp' },
  { phrase: 'certificate of insurance', alias: 'coi' },
  { phrase: 'proof of delivery', alias: 'pod' },
  { phrase: 'bill of lading', alias: 'bol' },
  { phrase: 'chain of custody', alias: 'coc' },
  { phrase: 'corrective action plan', alias: 'cap' },
  { phrase: 'preventive action plan', alias: 'pap' },
  { phrase: 'root cause analysis', alias: 'rca' },
  { phrase: 'standard work instruction', alias: 'swi' },
  { phrase: 'work instruction', alias: 'wi' },

  // Agencies / governing bodies
  { phrase: 'department of transportation', alias: 'dot' },
  { phrase: 'united states department of transportation', alias: 'usdot' },
  { phrase: 'federal motor carrier safety administration', alias: 'fmcsa' },
  { phrase: 'occupational safety and health administration', alias: 'osha' },
  { phrase: 'mine safety and health administration', alias: 'msha' },
  { phrase: 'environmental protection agency', alias: 'epa' },
  { phrase: 'pipeline and hazardous materials safety administration', alias: 'phmsa' },
  { phrase: 'national highway traffic safety administration', alias: 'nhtsa' },
  { phrase: 'federal aviation administration', alias: 'faa' },
  { phrase: 'federal railroad administration', alias: 'fra' },
  { phrase: 'federal transit administration', alias: 'fta' },
  { phrase: 'federal communications commission', alias: 'fcc' },
  { phrase: 'food and drug administration', alias: 'fda' },
  { phrase: 'equal employment opportunity commission', alias: 'eeoc' },
  { phrase: 'department of labor', alias: 'dol' },
  { phrase: 'national fire protection association', alias: 'nfpa' },
  { phrase: 'american national standards institute', alias: 'ansi' },
  { phrase: 'american society of mechanical engineers', alias: 'asme' },
  { phrase: 'international organization for standardization', alias: 'iso' },
  { phrase: 'international fire code', alias: 'ifc' },
  { phrase: 'national electrical code', alias: 'nec' },

  // FMCSA / transportation / fleet
  { phrase: 'commercial driver license', alias: 'cdl' },
  { phrase: 'commercial drivers license', alias: 'cdl' },
  { phrase: 'commercial driver’s license', alias: 'cdl' },
  { phrase: 'commercial motor vehicle', alias: 'cmv' },
  { phrase: 'commercial motor vehicles', alias: 'cmv' },
  { phrase: 'driver vehicle inspection report', alias: 'dvir' },
  { phrase: 'driver vehicle inspection reports', alias: 'dvir' },
  { phrase: 'daily vehicle inspection report', alias: 'dvir' },
  { phrase: 'electronic logging device', alias: 'eld' },
  { phrase: 'electronic logging devices', alias: 'eld' },
  { phrase: 'hours of service', alias: 'hos' },
  { phrase: 'record of duty status', alias: 'rods' },
  { phrase: 'records of duty status', alias: 'rods' },
  { phrase: 'motor vehicle record', alias: 'mvr' },
  { phrase: 'motor vehicle records', alias: 'mvr' },
  { phrase: 'pre employment screening program', alias: 'psp' },
  { phrase: 'commercial motor vehicle operator', alias: 'cmvo' },
  { phrase: 'medical examiner certificate', alias: 'mec' },
  { phrase: 'medical examiner’s certificate', alias: 'mec' },
  { phrase: 'medical examiners certificate', alias: 'mec' },
  { phrase: 'medical examiner registry', alias: 'nrcme' },
  { phrase: 'national registry of certified medical examiners', alias: 'nrcme' },
  { phrase: 'controlled substances and alcohol testing', alias: 'csat' },
  { phrase: 'drug and alcohol clearinghouse', alias: 'clearinghouse' },
  { phrase: 'commercial vehicle safety alliance', alias: 'cvsa' },
  { phrase: 'motor carrier safety assistance program', alias: 'mcsap' },
  { phrase: 'safety measurement system', alias: 'sms' },
  { phrase: 'compliance safety accountability', alias: 'csa' },
  { phrase: 'vehicle miles traveled', alias: 'vmt' },
  { phrase: 'international fuel tax agreement', alias: 'ifta' },
  { phrase: 'international registration plan', alias: 'irp' },
  { phrase: 'unified carrier registration', alias: 'ucr' },
  { phrase: 'motor carrier number', alias: 'mc' },
  { phrase: 'usdot number', alias: 'usdot' },
  { phrase: 'dot number', alias: 'usdot' },
  { phrase: 'boc three', alias: 'boc3' },
  { phrase: 'blanket of coverage', alias: 'boc3' },
  { phrase: 'process agent filing', alias: 'boc3' },
  { phrase: 'hazardous materials', alias: 'hazmat' },
  { phrase: 'hazardous material', alias: 'hazmat' },
  { phrase: 'hazardous materials endorsement', alias: 'hme' },
  { phrase: 'transportation worker identification credential', alias: 'twic' },

  // Maintenance / asset / inspection
  { phrase: 'preventive maintenance', alias: 'pm' },
  { phrase: 'preventative maintenance', alias: 'pm' },
  { phrase: 'predictive maintenance', alias: 'pdm' },
  { phrase: 'condition based maintenance', alias: 'cbm' },
  { phrase: 'corrective maintenance', alias: 'cm' },
  { phrase: 'computerized maintenance management system', alias: 'cmms' },
  { phrase: 'enterprise asset management', alias: 'eam' },
  { phrase: 'work order', alias: 'wo' },
  { phrase: 'work orders', alias: 'wo' },
  { phrase: 'repair order', alias: 'ro' },
  { phrase: 'repair orders', alias: 'ro' },
  { phrase: 'purchase order', alias: 'po' },
  { phrase: 'purchase orders', alias: 'po' },
  { phrase: 'request for quote', alias: 'rfq' },
  { phrase: 'request for proposal', alias: 'rfp' },
  { phrase: 'request for information', alias: 'rfi' },
  { phrase: 'original equipment manufacturer', alias: 'oem' },
  { phrase: 'vehicle identification number', alias: 'vin' },
  { phrase: 'equipment identification number', alias: 'ein' },
  { phrase: 'license plate number', alias: 'plate' },
  { phrase: 'unit number', alias: 'unit' },
  { phrase: 'asset number', alias: 'asset' },
  { phrase: 'fault code', alias: 'dtc' },
  { phrase: 'diagnostic trouble code', alias: 'dtc' },
  { phrase: 'suspect parameter number', alias: 'spn' },
  { phrase: 'failure mode identifier', alias: 'fmi' },
  { phrase: 'parameter identifier', alias: 'pid' },
  { phrase: 'component identifier', alias: 'cid' },
  { phrase: 'malfunction indicator lamp', alias: 'mil' },
  { phrase: 'check engine light', alias: 'cel' },
  { phrase: 'diesel particulate filter', alias: 'dpf' },
  { phrase: 'diesel exhaust fluid', alias: 'def' },
  { phrase: 'selective catalytic reduction', alias: 'scr' },
  { phrase: 'exhaust gas recirculation', alias: 'egr' },
  { phrase: 'aftertreatment system', alias: 'ats' },
  { phrase: 'anti lock braking system', alias: 'abs' },
  { phrase: 'tire pressure monitoring system', alias: 'tpms' },
  { phrase: 'power take off', alias: 'pto' },

  // OSHA / workplace safety
  { phrase: 'personal protective equipment', alias: 'ppe' },
  { phrase: 'lockout tagout', alias: 'loto' },
  { phrase: 'lock out tag out', alias: 'loto' },
  { phrase: 'control of hazardous energy', alias: 'loto' },
  { phrase: 'powered industrial truck', alias: 'pit' },
  { phrase: 'powered industrial trucks', alias: 'pit' },
  { phrase: 'forklift', alias: 'pit' },
  { phrase: 'forklifts', alias: 'pit' },
  { phrase: 'hazard communication', alias: 'hazcom' },
  { phrase: 'hazard communications', alias: 'hazcom' },
  { phrase: 'hazardous waste operations and emergency response', alias: 'hazwoper' },
  { phrase: 'process safety management', alias: 'psm' },
  { phrase: 'confined space', alias: 'cs' },
  { phrase: 'confined spaces', alias: 'cs' },
  { phrase: 'permit required confined space', alias: 'prcs' },
  { phrase: 'permit required confined spaces', alias: 'prcs' },
  { phrase: 'fall protection', alias: 'fp' },
  { phrase: 'fall prevention', alias: 'fp' },
  { phrase: 'respiratory protection', alias: 'rp' },
  { phrase: 'respiratory protection program', alias: 'rpp' },
  { phrase: 'bloodborne pathogens', alias: 'bbp' },
  { phrase: 'hearing conservation', alias: 'hc' },
  { phrase: 'hearing conservation program', alias: 'hcp' },
  { phrase: 'emergency action plan', alias: 'eap' },
  { phrase: 'fire prevention plan', alias: 'fpp' },
  { phrase: 'hot work', alias: 'hotwork' },
  { phrase: 'hot work permit', alias: 'hwp' },
  { phrase: 'walking working surfaces', alias: 'wws' },
  { phrase: 'machine guarding', alias: 'mg' },
  { phrase: 'personal fall arrest system', alias: 'pfas' },
  { phrase: 'safety harness', alias: 'harness' },
  { phrase: 'scaffold', alias: 'scaffold' },
  { phrase: 'scaffolding', alias: 'scaffold' },
  { phrase: 'aerial lift', alias: 'aeriallift' },
  { phrase: 'mobile elevated work platform', alias: 'mewp' },
  { phrase: 'powered air purifying respirator', alias: 'papr' },
  { phrase: 'self contained breathing apparatus', alias: 'scba' },
  { phrase: 'voluntary use respirator', alias: 'vur' },
  { phrase: 'exposure control plan', alias: 'ecp' },
  { phrase: 'permissible exposure limit', alias: 'pel' },
  { phrase: 'short term exposure limit', alias: 'stel' },
  { phrase: 'time weighted average', alias: 'twa' },
  { phrase: 'immediately dangerous to life or health', alias: 'idlh' },
  { phrase: 'lockout device', alias: 'loto' },
  { phrase: 'tagout device', alias: 'loto' },

  // Incident / injury / reporting
  { phrase: 'near miss', alias: 'nearmiss' },
  { phrase: 'near misses', alias: 'nearmiss' },
  { phrase: 'first aid case', alias: 'fac' },
  { phrase: 'medical treatment case', alias: 'mtc' },
  { phrase: 'lost time injury', alias: 'lti' },
  { phrase: 'lost time incident', alias: 'lti' },
  { phrase: 'recordable incident', alias: 'recordable' },
  { phrase: 'total recordable incident rate', alias: 'trir' },
  { phrase: 'days away restricted or transferred', alias: 'dart' },
  { phrase: 'lost time incident rate', alias: 'ltir' },
  { phrase: 'incident investigation', alias: 'ii' },
  { phrase: 'corrective action', alias: 'ca' },
  { phrase: 'preventive action', alias: 'pa' },
  { phrase: 'serious injury or fatality', alias: 'sif' },
  { phrase: 'serious injuries and fatalities', alias: 'sif' },
  { phrase: 'return to work', alias: 'rtw' },
  { phrase: 'restricted duty', alias: 'rd' },
  { phrase: 'light duty', alias: 'ld' },
  { phrase: 'workers compensation', alias: 'wc' },
  { phrase: 'workers comp', alias: 'wc' },

  // Environmental / EPA / waste
  { phrase: 'resource conservation and recovery act', alias: 'rcra' },
  { phrase: 'clean air act', alias: 'caa' },
  { phrase: 'clean water act', alias: 'cwa' },
  { phrase: 'comprehensive environmental response compensation and liability act', alias: 'cercla' },
  { phrase: 'superfund amendments and reauthorization act', alias: 'sara' },
  { phrase: 'emergency planning and community right to know act', alias: 'epcra' },
  { phrase: 'spill prevention control and countermeasure', alias: 'spcc' },
  { phrase: 'stormwater pollution prevention plan', alias: 'swppp' },
  { phrase: 'national pollutant discharge elimination system', alias: 'npdes' },
  { phrase: 'aboveground storage tank', alias: 'ast' },
  { phrase: 'above ground storage tank', alias: 'ast' },
  { phrase: 'underground storage tank', alias: 'ust' },
  { phrase: 'volatile organic compound', alias: 'voc' },
  { phrase: 'volatile organic compounds', alias: 'voc' },
  { phrase: 'hazardous waste', alias: 'hw' },
  { phrase: 'universal waste', alias: 'uw' },
  { phrase: 'used oil', alias: 'uo' },
  { phrase: 'wastewater', alias: 'ww' },
  { phrase: 'stormwater', alias: 'sw' },
  { phrase: 'air permit', alias: 'airpermit' },
  { phrase: 'tier two report', alias: 'tier2' },
  { phrase: 'toxic release inventory', alias: 'tri' },
  { phrase: 'threshold planning quantity', alias: 'tpq' },
  { phrase: 'reportable quantity', alias: 'rq' },

  // Hazmat / dangerous goods
  { phrase: 'dangerous goods', alias: 'dg' },
  { phrase: 'hazard class', alias: 'hazclass' },
  { phrase: 'hazard classes', alias: 'hazclass' },
  { phrase: 'packing group', alias: 'pg' },
  { phrase: 'identification number', alias: 'idno' },
  { phrase: 'un number', alias: 'un' },
  { phrase: 'na number', alias: 'na' },
  { phrase: 'shipping paper', alias: 'shipper' },
  { phrase: 'shipping papers', alias: 'shipper' },
  { phrase: 'emergency response guidebook', alias: 'erg' },
  { phrase: 'limited quantity', alias: 'ltdqty' },
  { phrase: 'flammable liquid', alias: 'flammableliquid' },
  { phrase: 'flammable liquids', alias: 'flammableliquid' },
  { phrase: 'flammable gas', alias: 'flammablegas' },
  { phrase: 'flammable gases', alias: 'flammablegas' },
  { phrase: 'combustible liquid', alias: 'combustibleliquid' },
  { phrase: 'combustible liquids', alias: 'combustibleliquid' },
  { phrase: 'corrosive material', alias: 'corrosive' },
  { phrase: 'corrosive materials', alias: 'corrosive' },
  { phrase: 'oxidizing material', alias: 'oxidizer' },
  { phrase: 'oxidizing materials', alias: 'oxidizer' },
  { phrase: 'poison inhalation hazard', alias: 'pih' },
  { phrase: 'marine pollutant', alias: 'mp' },

  // MSHA / mining
  { phrase: 'part forty six', alias: 'part46' },
  { phrase: 'part forty eight', alias: 'part48' },
  { phrase: 'new miner training', alias: 'nmt' },
  { phrase: 'newly hired experienced miner training', alias: 'nhemt' },
  { phrase: 'annual refresher training', alias: 'art' },
  { phrase: 'task training', alias: 'tasktraining' },
  { phrase: 'site specific hazard awareness training', alias: 'sshatraining' },
  { phrase: 'site specific hazard awareness', alias: 'ssha' },
  { phrase: 'competent person', alias: 'cp' },
  { phrase: 'experienced miner', alias: 'em' },
  { phrase: 'working place examination', alias: 'wpe' },
  { phrase: 'workplace examination', alias: 'wpe' },
  { phrase: 'metal nonmetal', alias: 'mnm' },
  { phrase: 'surface mine', alias: 'surface' },
  { phrase: 'underground mine', alias: 'underground' },

  // Training / qualification / HR-adjacent
  { phrase: 'learning management system', alias: 'lms' },
  { phrase: 'training matrix', alias: 'tm' },
  { phrase: 'training record', alias: 'tr' },
  { phrase: 'training records', alias: 'tr' },
  { phrase: 'certificate of completion', alias: 'coc' },
  { phrase: 'certification', alias: 'cert' },
  { phrase: 'certifications', alias: 'cert' },
  { phrase: 'qualification', alias: 'qual' },
  { phrase: 'qualifications', alias: 'qual' },
  { phrase: 'on the job training', alias: 'ojt' },
  { phrase: 'train the trainer', alias: 'ttt' },
  { phrase: 'subject matter expert', alias: 'sme' },
  { phrase: 'standardized work', alias: 'stdwork' },
  { phrase: 'skills assessment', alias: 'skills' },
  { phrase: 'competency assessment', alias: 'competency' },
  { phrase: 'remedial training', alias: 'remediation' },
  { phrase: 'retraining', alias: 'retraining' },
  { phrase: 'annual review', alias: 'annualreview' },
  { phrase: 'orientation', alias: 'orientation' },
  { phrase: 'onboarding', alias: 'onboarding' },

  // Organization / access / SaaS platform
  { phrase: 'single sign on', alias: 'sso' },
  { phrase: 'multi factor authentication', alias: 'mfa' },
  { phrase: 'two factor authentication', alias: '2fa' },
  { phrase: 'role based access control', alias: 'rbac' },
  { phrase: 'attribute based access control', alias: 'abac' },
  { phrase: 'personally identifiable information', alias: 'pii' },
  { phrase: 'service level agreement', alias: 'sla' },
  { phrase: 'service level objective', alias: 'slo' },
  { phrase: 'service level indicator', alias: 'sli' },
  { phrase: 'audit log', alias: 'auditlog' },
  { phrase: 'audit logs', alias: 'auditlog' },
  { phrase: 'application programming interface', alias: 'api' },
  { phrase: 'software as a service', alias: 'saas' },
  { phrase: 'system of record', alias: 'sor' },
  { phrase: 'source of truth', alias: 'sot' },
  { phrase: 'source of control', alias: 'soc' },
  { phrase: 'foreign key', alias: 'fk' },
  { phrase: 'primary key', alias: 'pk' },
  { phrase: 'service token', alias: 'st' },
  { phrase: 'tenant administrator', alias: 'tenantadmin' },
  { phrase: 'platform administrator', alias: 'platformadmin' },
  { phrase: 'product administrator', alias: 'productadmin' },

  // STL Compliance / Arr ecosystem
  { phrase: 'adaptive risk reduction', alias: 'arr' },
  { phrase: 'compliance core', alias: 'compliancecore' },
  { phrase: 'nexarr', alias: 'nexarr' },
  { phrase: 'staffarr', alias: 'staffarr' },
  { phrase: 'trainarr', alias: 'trainarr' },
  { phrase: 'maintainarr', alias: 'maintainarr' },
  { phrase: 'routarr', alias: 'routarr' },
  { phrase: 'supplyarr', alias: 'supplyarr' },
  { phrase: 'person id', alias: 'personid' },
  { phrase: 'tenant id', alias: 'tenantid' },
  { phrase: 'product entitlement', alias: 'entitlement' },
  { phrase: 'platform entitlement', alias: 'entitlement' },
  { phrase: 'cross product reference', alias: 'xref' },
  { phrase: 'cross product references', alias: 'xref' },
  { phrase: 'controlled vocabulary', alias: 'vocab' },
  { phrase: 'rule pack', alias: 'rulepack' },
  { phrase: 'rulepack', alias: 'rulepack' },
  { phrase: 'law citation', alias: 'citation' },
  { phrase: 'regulatory citation', alias: 'citation' },
]

type BuildSemanticKeyInput = {
  domain: string
  kind: string
  title: string
  aliases?: readonly string[]
  existingKeys?: readonly string[]
  maxLength?: number
}

function normalizePhrase(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, ' ')
    .replace(/\s+/g, ' ')
}

function sanitizeSegment(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]/g, '')
    .slice(0, SEMANTIC_SEGMENT_MAX_LENGTH)
}

function makeUniqueSemanticKey(
  candidate: string,
  existingKeys: readonly string[],
  maxLength?: number,
): string {
  const normalizedExisting = new Set(existingKeys.map((key) => key.trim().toLowerCase()).filter(Boolean))
  let normalizedCandidate = candidate.trim().toLowerCase()
  if (typeof maxLength === 'number' && maxLength > 0 && normalizedCandidate.length > maxLength) {
    normalizedCandidate = normalizedCandidate.slice(0, maxLength).replace(/\.+$/g, '')
  }

  if (!normalizedCandidate || !normalizedExisting.has(normalizedCandidate)) {
    return normalizedCandidate
  }

  let suffix = 2
  while (true) {
    const suffixToken = `.${suffix}`
    let base = normalizedCandidate
    if (typeof maxLength === 'number' && maxLength > 0 && base.length + suffixToken.length > maxLength) {
      const maxBaseLength = maxLength - suffixToken.length
      if (maxBaseLength < SEMANTIC_SEGMENT_MIN_LENGTH) {
        return ''
      }
      base = base.slice(0, maxBaseLength).replace(/\.+$/g, '')
    }
    const candidateWithSuffix = `${base}${suffixToken}`
    if (!normalizedExisting.has(candidateWithSuffix)) {
      return candidateWithSuffix
    }
    suffix += 1
  }
}

export function compactSemanticSlug(title: string, stopWords: ReadonlySet<string> = DEFAULT_STOP_WORDS): string {
  const normalized = normalizePhrase(title)
  const tokens = normalized.split(' ').filter(Boolean)
  const filtered = tokens.filter((token) => !stopWords.has(token))
  const source = filtered.length > 0 ? filtered : tokens
  const compact = source.join('')
  return compact.slice(0, SEMANTIC_SEGMENT_MAX_LENGTH)
}

export function chooseSemanticAlias(title: string, aliases: readonly string[] = []): string | null {
  for (const alias of aliases) {
    const normalizedAlias = sanitizeSegment(alias)
    if (normalizedAlias.length >= SEMANTIC_SEGMENT_MIN_LENGTH) {
      return normalizedAlias
    }
  }

  const normalizedTitle = normalizePhrase(title)
  for (const known of KNOWN_ALIASES) {
    if (normalizedTitle.includes(known.phrase)) {
      return known.alias
    }
  }

  return null
}

export function buildSemanticKey({
  domain,
  kind,
  title,
  aliases = [],
  existingKeys = [],
  maxLength,
}: BuildSemanticKeyInput): string {
  const normalizedDomain = sanitizeSegment(domain)
  const normalizedKind = sanitizeSegment(kind)
  if (!normalizedDomain || !normalizedKind) {
    return ''
  }

  const alias = chooseSemanticAlias(title, aliases)
  const slug = alias ?? compactSemanticSlug(title)
  if (slug.length < SEMANTIC_SEGMENT_MIN_LENGTH) {
    return ''
  }

  const prefix = `${normalizedDomain}.${normalizedKind}.`
  let normalizedSlug = slug
  if (typeof maxLength === 'number' && maxLength > 0) {
    const maxSlugLength = maxLength - prefix.length
    if (maxSlugLength < SEMANTIC_SEGMENT_MIN_LENGTH) {
      return ''
    }
    normalizedSlug = normalizedSlug.slice(0, maxSlugLength)
  }

  const candidate = `${prefix}${normalizedSlug}`
  return makeUniqueSemanticKey(candidate, existingKeys, maxLength)
}
