type BrowserSpeechRecognition = {
  lang: string
  interimResults: boolean
  maxAlternatives: number
  onresult: ((event: { results: ArrayLike<ArrayLike<{ transcript?: string }>> }) => void) | null
  onerror: (() => void) | null
  onnomatch: (() => void) | null
  start: () => void
  stop: () => void
}

type SpeechRecognitionConstructor = new () => BrowserSpeechRecognition

function getSpeechRecognitionConstructor(): SpeechRecognitionConstructor | null {
  const globalWindow = window as Window & {
    SpeechRecognition?: SpeechRecognitionConstructor
    webkitSpeechRecognition?: SpeechRecognitionConstructor
  }
  return globalWindow.SpeechRecognition ?? globalWindow.webkitSpeechRecognition ?? null
}

export function isSpeechSynthesisSupported(): boolean {
  return typeof window !== 'undefined' && 'speechSynthesis' in window
}

export function isSpeechRecognitionSupported(): boolean {
  return typeof window !== 'undefined' && getSpeechRecognitionConstructor() !== null
}

export function speakPrompt(text: string): void {
  if (!isSpeechSynthesisSupported()) {
    return
  }

  window.speechSynthesis.cancel()
  const utterance = new SpeechSynthesisUtterance(text)
  utterance.rate = 0.95
  window.speechSynthesis.speak(utterance)
}

export function parsePassFailTranscript(transcript: string): string | null {
  const normalized = transcript.trim().toLowerCase()
  if (!normalized) {
    return null
  }

  if (/\b(pass|passed|ok|okay|good|yes)\b/.test(normalized)) {
    return 'pass'
  }

  if (/\b(fail|failed|no|bad|not ok|not okay)\b/.test(normalized)) {
    return 'fail'
  }

  if (/\b(n\.?\s*a\.?|not applicable|na)\b/.test(normalized)) {
    return 'na'
  }

  return null
}

function normalizeOptionText(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/\s+/g, ' ')
}

export function parseControlledOptionsTranscript(
  transcript: string,
  controlledOptions: string[],
  allowMultiple: boolean,
): string[] | null {
  const normalizedTranscript = normalizeOptionText(transcript)
  if (!normalizedTranscript || controlledOptions.length === 0) {
    return null
  }

  const optionMap = new Map(
    controlledOptions.map((option) => [normalizeOptionText(option), option] as const),
  )

  if (!allowMultiple) {
    const exact = optionMap.get(normalizedTranscript)
    return exact ? [exact] : null
  }

  const exact = optionMap.get(normalizedTranscript)
  if (exact) {
    return [exact]
  }

  const parts = normalizedTranscript
    .split(/,|\band\b/)
    .map((part) => part.trim())
    .filter(Boolean)

  if (parts.length === 0) {
    return null
  }

  const selected: string[] = []
  for (const part of parts) {
    const exact = optionMap.get(part)
    if (!exact) {
      return null
    }
    if (!selected.some((value) => normalizeOptionText(value) === normalizeOptionText(exact))) {
      selected.push(exact)
    }
  }

  return selected.length > 0 ? selected : null
}

export function listenForTranscript(timeoutMs = 8000): Promise<string> {
  const Recognition = getSpeechRecognitionConstructor()
  if (!Recognition) {
    return Promise.reject(new Error('Speech recognition is not supported in this browser.'))
  }

  return new Promise((resolve, reject) => {
    const recognition = new Recognition()
    recognition.lang = 'en-US'
    recognition.interimResults = false
    recognition.maxAlternatives = 1

    const timeout = window.setTimeout(() => {
      recognition.stop()
      reject(new Error('Voice capture timed out.'))
    }, timeoutMs)

    recognition.onresult = (event: { results: ArrayLike<ArrayLike<{ transcript?: string }>> }) => {
      window.clearTimeout(timeout)
      const transcript = event.results[0]?.[0]?.transcript?.trim() ?? ''
      resolve(transcript)
    }

    recognition.onerror = () => {
      window.clearTimeout(timeout)
      reject(new Error('Voice capture failed.'))
    }

    recognition.onnomatch = () => {
      window.clearTimeout(timeout)
      reject(new Error('Voice answer was not recognized.'))
    }

    recognition.start()
  })
}
