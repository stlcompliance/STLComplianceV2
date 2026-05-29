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
