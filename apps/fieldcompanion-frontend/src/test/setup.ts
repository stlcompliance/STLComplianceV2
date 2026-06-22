import { createElement } from 'react'
import '@testing-library/jest-dom/vitest'
import { vi } from 'vitest'

vi.mock('@stl/shared-ui', () => ({
  ApiErrorCallout: ({
    title,
    message,
    retryLabel,
    onRetry,
    footer,
    className,
    testId,
  }: {
    title?: string
    message: string
    retryLabel?: string
    onRetry?: () => void
    footer?: unknown
    className?: string
    testId?: string
  }) =>
    createElement(
      'div',
      { className, role: 'alert', 'data-testid': testId },
      title ? createElement('p', { className: 'font-medium' }, title) : null,
      createElement('p', { className: 'mt-1' }, message),
      retryLabel && onRetry
        ? createElement(
            'div',
            { className: 'mt-2 flex flex-wrap items-center gap-2' },
            createElement('button', { type: 'button', onClick: onRetry }, retryLabel),
            footer ?? null,
          )
        : footer ?? null,
    ),
  PageHeader: ({
    title,
    subtitle,
  }: {
    title: string
    subtitle?: string
  }) =>
    createElement(
      'header',
      null,
      createElement('h1', null, title),
      subtitle ? createElement('p', null, subtitle) : null,
    ),
  ProductWorkspaceFrame: ({
    children,
    productName,
  }: {
    children?: unknown
    productName?: string
  }) =>
    createElement(
      'div',
      { 'data-testid': 'mock-product-workspace-frame' },
      productName ? createElement('h1', null, productName) : null,
      children,
    ),
  AdvancedReferenceField: ({
    label,
    value,
    onChange,
    testId,
    placeholder,
  }: {
    label?: string
    value: string
    onChange: (value: string) => void
    testId?: string
    placeholder?: string
  }) =>
    createElement(
      'label',
      null,
      label,
      createElement('input', {
        'aria-label': label ?? placeholder ?? 'Advanced reference field',
        'data-testid': testId,
        value,
        onChange: (event: Event) => onChange((event.target as HTMLInputElement).value),
      }),
    ),
  ControlledSelect: ({
    label,
    value,
    options,
    onChange,
    emptyLabel,
    testId,
  }: {
    label?: string
    value: string
    options: Array<{ value: string; label: string }>
    onChange: (value: string) => void
    emptyLabel?: string
    testId?: string
  }) =>
    createElement(
      'label',
      null,
      label,
      createElement(
        'select',
        {
          'aria-label': label ?? emptyLabel ?? 'Controlled select',
          'data-testid': testId,
          value,
          onChange: (event: Event) => onChange((event.target as HTMLSelectElement).value),
        },
        createElement('option', { value: '' }, emptyLabel ?? 'Select…'),
        ...options.map((option) => createElement('option', { key: option.value, value: option.value }, option.label)),
      ),
    ),
  StaticSearchPicker: ({
    label,
    value,
    options,
    onChange,
    placeholder,
    testId,
  }: {
    label?: string
    value: string
    options: Array<{ value: string; label: string }>
    onChange: (value: string) => void
    placeholder?: string
    testId?: string
  }) =>
    createElement(
      'div',
      { 'data-testid': testId },
      createElement(
        'label',
        null,
        label,
        createElement('input', {
          'aria-label': label ?? placeholder ?? 'Static search picker',
          value,
          onChange: (event: Event) => onChange((event.target as HTMLInputElement).value),
        }),
      ),
      createElement(
        'ul',
        null,
        ...options.map((option) =>
          createElement(
            'li',
            { key: option.value },
            createElement('button', { type: 'button', onClick: () => onChange(option.value) }, option.label),
          ),
        ),
      ),
    ),
  ReferenceProviderClient: class ReferenceProviderClient {
    constructor(_options: unknown) {}
  },
  ReferencePicker: ({
    value,
    onChange,
    placeholder,
    label,
  }: {
    value: { displayLabelSnapshot?: string } | null
    onChange: (value: { displayLabelSnapshot?: string } | null) => void
    placeholder?: string
    label?: string
  }) =>
    createElement('input', {
      'aria-label': label ?? placeholder ?? 'Reference picker',
      value: value?.displayLabelSnapshot ?? '',
      onChange: (event: Event) =>
        onChange((event.target as HTMLInputElement).value ? { displayLabelSnapshot: (event.target as HTMLInputElement).value } : null),
    }),
  getErrorMessage: (error: unknown, fallback: string) => {
    if (error instanceof Error && error.message.trim()) {
      return error.message
    }
    if (typeof error === 'string' && error.trim()) {
      return error
    }
    return fallback
  },
  normalizeProductKey: (value: string) => value.trim().toLowerCase().replace(/[^a-z0-9]+/g, ''),
  buildProductLaunchUrlMap: () => ({}),
  resolveProductLaunchUrl: (productKey: string, suiteHomeUrl: string, productLaunchUrls: Record<string, string> = {}) => {
    const key = productKey.trim().toLowerCase().replace(/[^a-z0-9]+/g, '')
    if (key === 'nexarr') {
      return suiteHomeUrl || '/'
    }
    if (productLaunchUrls[key]) {
      return productLaunchUrls[key]
    }
    return suiteHomeUrl || '/'
  },
  resolveSuiteHomeUrl: (value?: string | null) => value ?? '/',
  resolveProductLaunchCallbackPath: () => '/launch',
  saveThemePreferenceFromSession: () => undefined,
  resolveProductWorkspaceBootstrapError: () => null,
}))
