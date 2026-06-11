export interface StaffArrWorkerPanelConfig {
  workerKey: string
  panelTestId: string
  saveTestId: string
  heading: string
  description: string
  supportsStaleness: boolean
}

export const STAFFARR_SCHEDULED_WORKER_PANELS: StaffArrWorkerPanelConfig[] = [
  {
    workerKey: 'certification-expiration',
    panelTestId: 'certification-expiration-settings-panel',
    saveTestId: 'certification-expiration-save',
    heading: 'Certification expiration worker',
    description:
      'Automatically expire active person certifications past their expiry date on a schedule.',
    supportsStaleness: false,
  },
  {
    workerKey: 'readiness-rollup',
    panelTestId: 'readiness-rollup-settings-panel',
    saveTestId: 'readiness-rollup-save',
    heading: 'Readiness rollup worker',
    description:
      'Refresh materialized team and site readiness rollups used by supervisor readiness views.',
    supportsStaleness: true,
  },
  {
    workerKey: 'permission-projection',
    panelTestId: 'permission-projection-settings-panel',
    saveTestId: 'permission-projection-save',
    heading: 'Permission projection worker',
    description:
      'Materialize effective permission keys per person for faster permission reads and history views.',
    supportsStaleness: true,
  },
  {
    workerKey: 'personnel-history-rollup',
    panelTestId: 'personnel-history-rollup-settings-panel',
    saveTestId: 'personnel-history-rollup-save',
    heading: 'Personnel history rollup worker',
    description:
      'Rebuild per-person workforce history summaries and timeline events from StaffArr source records.',
    supportsStaleness: true,
  },
]
