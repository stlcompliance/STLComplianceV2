import type { InputHTMLAttributes } from 'react'

export type CheckboxFieldProps = Omit<InputHTMLAttributes<HTMLInputElement>, 'type' | 'id'> & {
  id: string
  label: string
  className?: string
}

/**
 * Checkbox with an explicit associated label (htmlFor + id).
 */
export function CheckboxField({
  id,
  label,
  className = 'flex items-center gap-2 text-sm text-slate-200',
  ...inputProps
}: CheckboxFieldProps) {
  return (
    <label htmlFor={id} className={className}>
      <input id={id} type="checkbox" {...inputProps} />
      {label}
    </label>
  )
}
