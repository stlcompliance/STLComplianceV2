import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { RoutesSection } from './RoutesSection'

type Props = { state: RoutArrWorkspaceState }

export function RoutePlannerSection({ state }: Props) {
  return <RoutesSection state={state} />
}
