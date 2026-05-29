import { GeneratedKeyField, slugifyKey } from '@stl/shared-ui'
import { useEffect, useState } from 'react'

import { keyCollisionWarning, resolveGeneratedKey } from './controlledFormHelpers'

type GeneratedKeyFieldGroupProps = {
  sourceLabel: string
  existingKeys: readonly string[]
  onKeyChange: (key: string) => void
  label?: string
  disabled?: boolean
}

export function GeneratedKeyFieldGroup({
  sourceLabel,
  existingKeys,
  onKeyChange,
  label = 'Key',
  disabled = false,
}: GeneratedKeyFieldGroupProps) {
  const [manualOverride, setManualOverride] = useState('')
  const [showAdvancedKey, setShowAdvancedKey] = useState(false)

  const generatedKey = slugifyKey(sourceLabel)
  const resolvedKey = resolveGeneratedKey(sourceLabel, manualOverride)
  const collisionWarning = keyCollisionWarning(resolvedKey, existingKeys)

  useEffect(() => {
    onKeyChange(resolvedKey)
  }, [resolvedKey, onKeyChange])

  useEffect(() => {
    if (!sourceLabel.trim()) {
      setManualOverride('')
      setShowAdvancedKey(false)
    }
  }, [sourceLabel])

  return (
    <div className="space-y-1">
      <GeneratedKeyField
        sourceLabel={sourceLabel}
        generatedKey={generatedKey}
        manualOverride={manualOverride}
        onManualOverrideChange={setManualOverride}
        showAdvancedKey={showAdvancedKey}
        collisionWarning={collisionWarning}
        disabled={disabled}
        label={label}
      />
      {!showAdvancedKey ? (
        <button
          type="button"
          className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
          onClick={() => setShowAdvancedKey(true)}
          disabled={disabled}
        >
          Customize key
        </button>
      ) : null}
    </div>
  )
}
