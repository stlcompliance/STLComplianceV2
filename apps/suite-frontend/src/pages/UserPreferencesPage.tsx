import { useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import {
  PreferenceField,
  PreferenceResetButton,
  PreferenceSection,
  PreferenceSelect,
  PreferenceToggle,
} from '../components/preferences/PreferenceControls'
import {
  nexarrPreferenceOptions,
  useCurrentProductPreferences,
  useSuitePreferences,
  type NexarrPreferences,
  type SuitePreferences,
} from '../preferences/preferences'
import { PageHeader } from '@stl/shared-ui/PageHeader'
import { getSuiteProductCatalogEntry, normalizeProductKey } from '@stl/shared-ui/productCatalog'

const suiteThemeOptions = [
  { value: 'system', label: 'System' },
  { value: 'light', label: 'Light' },
  { value: 'dark', label: 'Dark' },
] as const

const suiteDensityOptions = [
  { value: 'comfortable', label: 'Comfortable' },
  { value: 'compact', label: 'Compact' },
] as const

const suiteDateFormatOptions = [
  { value: 'MM/DD/YYYY', label: 'MM/DD/YYYY' },
  { value: 'DD/MM/YYYY', label: 'DD/MM/YYYY' },
  { value: 'YYYY-MM-DD', label: 'YYYY-MM-DD' },
] as const

const suiteTimeFormatOptions = [
  { value: '12h', label: '12-hour' },
  { value: '24h', label: '24-hour' },
] as const

const suiteNumberFormatOptions = [
  { value: 'system', label: 'System default' },
  { value: 'en-US', label: 'English (United States)' },
  { value: 'de-DE', label: 'German (Germany)' },
] as const

const suiteVerbosityOptions = [
  { value: 'concise', label: 'Concise' },
  { value: 'normal', label: 'Normal' },
  { value: 'detailed', label: 'Detailed' },
] as const

const suiteTimeZoneOptions = [
  { value: 'system', label: 'System default' },
  { value: 'America/Chicago', label: 'America/Chicago' },
  { value: 'America/New_York', label: 'America/New_York' },
  { value: 'America/Denver', label: 'America/Denver' },
  { value: 'America/Los_Angeles', label: 'America/Los_Angeles' },
  { value: 'UTC', label: 'UTC' },
] as const

const currentProductLandingOptions = nexarrPreferenceOptions.defaultLandingProducts

function renderSuitePreferenceSummary(preferences: SuitePreferences): string {
  return [
    `Theme: ${preferences.theme}`,
    `Density: ${preferences.density}`,
    `Date format: ${preferences.dateFormat}`,
    `Time format: ${preferences.timeFormat}`,
  ].join(' | ')
}

function renderProductPreferenceSummary(preferences: NexarrPreferences): string {
  return [
    `Landing: ${preferences.defaultLandingProduct}`,
    `Launcher order: ${preferences.launcherOrder}`,
    `Home view: ${preferences.defaultHomeView}`,
  ].join(' | ')
}

export function UserPreferencesPage() {
  const { me } = useAuth()
  const { productKey: routeProductKey } = useParams<{ productKey?: string }>()
  const currentProductKey = normalizeProductKey(routeProductKey ?? 'nexarr')
  const product = useMemo(
    () => getSuiteProductCatalogEntry(currentProductKey) ?? getSuiteProductCatalogEntry('nexarr'),
    [currentProductKey],
  )
  const productDisplayName = product?.displayName ?? 'NexArr'
  const activeProductKey = product?.productKey ?? 'nexarr'
  const suitePreferences = useSuitePreferences({
    tenantId: me?.tenantId,
    personId: me?.userId,
    initialTheme: me?.themePreference,
  })
  const currentProductPreferences = useCurrentProductPreferences({
    tenantId: me?.tenantId,
    personId: me?.userId,
    productKey: activeProductKey,
  })
  const [statusMessage, setStatusMessage] = useState<string | null>(null)

  useEffect(() => {
    function handleBeforeUnload(event: BeforeUnloadEvent) {
      if (!suitePreferences.isDirty && !currentProductPreferences.isDirty) {
        return
      }
      event.preventDefault()
      event.returnValue = ''
    }

    window.addEventListener('beforeunload', handleBeforeUnload)
    return () => window.removeEventListener('beforeunload', handleBeforeUnload)
  }, [currentProductPreferences.isDirty, suitePreferences.isDirty])

  const saveSuitePreferences = async () => {
    setStatusMessage(null)
    try {
      await suitePreferences.save()
      setStatusMessage('Suite preferences saved.')
    } catch (error) {
      setStatusMessage(
        error instanceof Error ? error.message : 'Failed to save suite preferences.',
      )
    }
  }

  const saveCurrentProductPreferences = async () => {
    setStatusMessage(null)
    try {
      await currentProductPreferences.save()
      setStatusMessage(`${productDisplayName} preferences saved.`)
    } catch (error) {
      setStatusMessage(
        error instanceof Error ? error.message : `Failed to save ${productDisplayName} preferences.`,
      )
    }
  }

  const resetSuitePreferences = () => {
    suitePreferences.reset()
    setStatusMessage('Suite preferences reset to defaults.')
  }

  const resetCurrentProductPreferences = () => {
    currentProductPreferences.reset()
    setStatusMessage(`${productDisplayName} preferences reset to defaults.`)
  }

  if (suitePreferences.isLoading || currentProductPreferences.isLoading) {
    return (
      <div className="flex min-h-[40vh] items-center justify-center rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-6 py-12 text-sm text-[var(--color-text-muted)]">
        Loading preferences...
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <PageHeader
        title="Preferences"
        subtitle={`Personal preferences for STL Compliance and ${productDisplayName}.`}
        eyebrow={productDisplayName}
      />

      {statusMessage ? (
        <div
          role="status"
          aria-live="polite"
          className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-4 py-3 text-sm text-[var(--color-text-secondary)]"
        >
          {statusMessage}
        </div>
      ) : null}

      <PreferenceSection
        title="Suite Preferences"
        subtitle="These preferences follow you across STL Compliance products."
        actions={
          <>
            <PreferenceResetButton onClick={resetSuitePreferences}>Reset to defaults</PreferenceResetButton>
            <button
              type="button"
              onClick={() => void saveSuitePreferences()}
              disabled={suitePreferences.isSaving || !suitePreferences.isDirty}
              className="inline-flex min-h-10 items-center rounded-lg bg-[var(--color-accent)] px-3 text-sm font-medium text-white transition hover:bg-[var(--color-accent-hover)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {suitePreferences.isSaving ? 'Saving...' : 'Save suite preferences'}
            </button>
          </>
        }
      >
        <PreferenceField
          label="Theme"
          description="Choose light mode, dark mode, or follow your operating system."
        >
          <PreferenceSelect
            aria-label="Theme"
            value={suitePreferences.preferences.theme}
            onChange={(value) => suitePreferences.setPreference('theme', value as SuitePreferences['theme'])}
            options={suiteThemeOptions}
          />
        </PreferenceField>

        <PreferenceField label="Density" description="Choose comfortable or compact spacing.">
          <PreferenceSelect
            aria-label="Density"
            value={suitePreferences.preferences.density}
            onChange={(value) =>
              suitePreferences.setPreference('density', value as SuitePreferences['density'])
            }
            options={suiteDensityOptions}
          />
        </PreferenceField>

        <PreferenceField label="Reduced motion" description="Reduce animations and motion effects.">
          <PreferenceToggle
            checked={suitePreferences.preferences.reducedMotion}
            onChange={(checked) => suitePreferences.setPreference('reducedMotion', checked)}
            label="Reduce motion across the suite"
          />
        </PreferenceField>

        <PreferenceField label="High contrast" description="Increase contrast for readability.">
          <PreferenceToggle
            checked={suitePreferences.preferences.highContrast}
            onChange={(checked) => suitePreferences.setPreference('highContrast', checked)}
            label="Use high contrast colors"
          />
        </PreferenceField>

        <PreferenceField label="Time zone" description="Choose the time zone used for dates and times.">
          <PreferenceSelect
            aria-label="Time zone"
            value={suitePreferences.preferences.timeZone}
            onChange={(value) => suitePreferences.setPreference('timeZone', value)}
            options={suiteTimeZoneOptions}
          />
        </PreferenceField>

        <div className="grid gap-4 sm:grid-cols-2">
          <PreferenceField label="Date format" description="Choose how dates are displayed.">
            <PreferenceSelect
              aria-label="Date format"
              value={suitePreferences.preferences.dateFormat}
              onChange={(value) =>
                suitePreferences.setPreference('dateFormat', value as SuitePreferences['dateFormat'])
              }
              options={suiteDateFormatOptions}
            />
          </PreferenceField>

          <PreferenceField label="Time format" description="Choose 12-hour or 24-hour time.">
            <PreferenceSelect
              aria-label="Time format"
              value={suitePreferences.preferences.timeFormat}
              onChange={(value) =>
                suitePreferences.setPreference('timeFormat', value as SuitePreferences['timeFormat'])
              }
              options={suiteTimeFormatOptions}
            />
          </PreferenceField>
        </div>

        <PreferenceField label="Number format" description="Choose the default number formatting style.">
          <PreferenceSelect
            aria-label="Number format"
            value={suitePreferences.preferences.numberFormat}
            onChange={(value) =>
              suitePreferences.setPreference('numberFormat', value as SuitePreferences['numberFormat'])
            }
            options={suiteNumberFormatOptions}
          />
        </PreferenceField>

        <PreferenceField
          label="Assistant detail level"
          description="Choose how detailed assistant responses should be."
        >
          <PreferenceSelect
            aria-label="Assistant detail level"
            value={suitePreferences.preferences.assistantDefaultVerbosity}
            onChange={(value) =>
              suitePreferences.setPreference(
                'assistantDefaultVerbosity',
                value as SuitePreferences['assistantDefaultVerbosity'],
              )
            }
            options={suiteVerbosityOptions}
          />
        </PreferenceField>

        <PreferenceField
          label="Show assumptions"
          description="Show assumptions when the assistant proposes actions or explanations."
        >
          <PreferenceToggle
            checked={suitePreferences.preferences.assistantShowAssumptions}
            onChange={(checked) =>
              suitePreferences.setPreference('assistantShowAssumptions', checked)
            }
            label="Show assumptions in assistant responses"
          />
        </PreferenceField>

        <p className="text-sm text-[var(--color-text-muted)]">
          {renderSuitePreferenceSummary(suitePreferences.preferences)}
        </p>
        {suitePreferences.error ? (
          <p className="text-sm text-rose-300" role="alert">
            {suitePreferences.error}
          </p>
        ) : null}
      </PreferenceSection>

      <PreferenceSection
        title={`${productDisplayName} Preferences`}
        subtitle={`These preferences apply only in ${productDisplayName}.`}
        actions={
          <>
            <PreferenceResetButton onClick={resetCurrentProductPreferences}>
              Reset to defaults
            </PreferenceResetButton>
            <button
              type="button"
              onClick={() => void saveCurrentProductPreferences()}
              disabled={currentProductPreferences.isSaving || !currentProductPreferences.isDirty}
              className="inline-flex min-h-10 items-center rounded-lg bg-[var(--color-accent)] px-3 text-sm font-medium text-white transition hover:bg-[var(--color-accent-hover)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {currentProductPreferences.isSaving ? 'Saving...' : `Save ${productDisplayName} preferences`}
            </button>
          </>
        }
      >
        <PreferenceField
          label="Default landing page"
          description={`Choose where ${productDisplayName} opens for you first.`}
        >
          <PreferenceSelect
            aria-label="Default landing page"
            value={currentProductPreferences.preferences.defaultLandingProduct}
            onChange={(value) =>
              currentProductPreferences.setPreference(
                'defaultLandingProduct',
                value,
              )
            }
            options={currentProductLandingOptions}
          />
        </PreferenceField>

        <div className="grid gap-4 sm:grid-cols-2">
          <PreferenceField label="Launcher order" description="Choose how products are ordered in the launcher.">
            <PreferenceSelect
              aria-label="Launcher order"
              value={currentProductPreferences.preferences.launcherOrder}
              onChange={(value) =>
                currentProductPreferences.setPreference('launcherOrder', value as NexarrPreferences['launcherOrder'])
              }
              options={nexarrPreferenceOptions.launcherOrders}
            />
          </PreferenceField>

          <PreferenceField label="Home view" description="Choose the default home experience.">
            <PreferenceSelect
              aria-label="Home view"
              value={currentProductPreferences.preferences.defaultHomeView}
              onChange={(value) =>
                currentProductPreferences.setPreference(
                  'defaultHomeView',
                  value as NexarrPreferences['defaultHomeView'],
                )
              }
              options={nexarrPreferenceOptions.homeViews}
            />
          </PreferenceField>
        </div>

        <PreferenceField
          label="Product access alerts"
          description="Receive alerts when entitled products become available or change status."
        >
          <PreferenceToggle
            checked={currentProductPreferences.preferences.productAccessAlerts}
            onChange={(checked) =>
              currentProductPreferences.setPreference('productAccessAlerts', checked)
            }
            label="Enable product access alerts"
          />
        </PreferenceField>

        <PreferenceField
          label="Assistant launch behavior"
          description={`Choose whether ${productDisplayName} remembers the last assistant state.`}
        >
          <PreferenceSelect
            aria-label="Assistant launch behavior"
            value={currentProductPreferences.preferences.assistantLaunchBehavior}
            onChange={(value) =>
              currentProductPreferences.setPreference(
                'assistantLaunchBehavior',
                value as NexarrPreferences['assistantLaunchBehavior'],
              )
            }
            options={nexarrPreferenceOptions.assistantLaunchBehaviors}
          />
        </PreferenceField>

        <p className="text-sm text-[var(--color-text-muted)]">
          {renderProductPreferenceSummary(currentProductPreferences.preferences)}
        </p>
        {currentProductPreferences.error ? (
          <p className="text-sm text-rose-300" role="alert">
            {currentProductPreferences.error}
          </p>
        ) : null}
      </PreferenceSection>
    </div>
  )
}
