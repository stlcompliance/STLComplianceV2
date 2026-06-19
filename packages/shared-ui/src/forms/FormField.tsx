import type { ReactElement, ReactNode } from 'react'
import { cloneElement, useId } from 'react'

export type FormFieldProps = {
  label: string
  /** Stable id for the control; auto-generated when omitted. */
  id?: string
  hint?: ReactNode
  className?: string
  labelClassName?: string
  children: ReactElement<{ id?: string }>
}

/**
 * Accessible block field: visible label with matching htmlFor/id on the control.
 */
export function FormField({
  label,
  id: idProp,
  hint,
  className = 'block text-sm',
  labelClassName = 'text-slate-300',
  children,
}: FormFieldProps) {
  const generatedId = useId()
  const fieldId = idProp ?? generatedId.replace(/:/g, '')

  return (
    <div className={className}>
      <label htmlFor={fieldId} className={labelClassName}>
        {label}
      </label>
      <div className="mt-1">{cloneElement(children, { id: fieldId })}</div>
      {hint ? <p className="mt-1 text-xs text-[var(--color-text-muted)]">{hint}</p> : null}
    </div>
  )
}
