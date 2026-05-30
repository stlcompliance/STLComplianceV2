import { GeneratedKeyField, slugifyKey } from '@stl/shared-ui'
import { useEffect, useState } from 'react'

import { keyCollisionWarning, resolveGeneratedKey } from './controlledFormHelpers'

type GeneratedKeyFieldGroupProps = {
  sourceLabel: string
  existingKeys: readonly string[]
  onKeyChange: (key: string) => void
  domain?: string
  kind?: string
  aliases?: readonly string[]
  maxLength?: number
  label?: string
  disabled?: boolean
}

export function GeneratedKeyFieldGroup({
  sourceLabel,
  existingKeys,
  onKeyChange,
  domain,
  kind,
  aliases,
  maxLength,
  label = 'Key',
  disabled = false,
}: GeneratedKeyFieldGroupProps) {
  const [showPolicyHint, setShowPolicyHint] = useState(false)

  const generatedKey =
    resolveGeneratedKey(sourceLabel, { domain, kind, aliases, maxLength, existingKeys }) ||
    slugifyKey(sourceLabel)
  const resolvedKey = generatedKey
  const collisionWarning = keyCollisionWarning(resolvedKey, existingKeys)

  useEffect(() => {
    onKeyChange(resolvedKey)
  }, [resolvedKey, onKeyChange])

  useEffect(() => {
    if (!sourceLabel.trim()) {
      setShowPolicyHint(false)
    }
  }, [sourceLabel])

  return (
    <div className="space-y-1">
      <GeneratedKeyField
        sourceLabel={sourceLabel}
        generatedKey={generatedKey}
        manualOverride=""
        onManualOverrideChange={() => {}}
        showAdvancedKey={showPolicyHint}
        collisionWarning={collisionWarning}
        disabled={disabled}
        label={label}
      />
      {!showPolicyHint ? (
        <button
          type="button"
          className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
          onClick={() => setShowPolicyHint(true)}
          disabled={disabled}
        >
          Key policy
        </button>
      ) : null}
    </div>
  )
}
