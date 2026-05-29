import type { PickerOption } from '@stl/shared-ui'

import type { DriverResponse, TripSummaryResponse, VehicleRefResponse } from '../api/types'

export function driverToPickerOption(driver: DriverResponse): PickerOption {
  return {
    value: driver.personId,
    label: driver.displayName,
  }
}

export function vehicleRefToPickerOption(ref: VehicleRefResponse): PickerOption {
  const label = ref.assetTag ? `${ref.displayLabel} (${ref.assetTag})` : ref.displayLabel
  return {
    value: ref.vehicleRefKey,
    label,
  }
}

export function tripToPickerOption(trip: TripSummaryResponse): PickerOption {
  const inactive = ['completed', 'cancelled'].includes(trip.dispatchStatus.toLowerCase())
  return {
    value: trip.tripId,
    label: `${trip.tripNumber} · ${trip.title}`,
    inactive,
  }
}

export function findDriverLabel(
  drivers: DriverResponse[],
  personId: string | null | undefined,
): string | undefined {
  if (!personId) {
    return undefined
  }
  return drivers.find((driver) => driver.personId === personId)?.displayName
}

export function findVehicleLabel(
  vehicleRefs: VehicleRefResponse[],
  vehicleRefKey: string | null | undefined,
): string | undefined {
  if (!vehicleRefKey) {
    return undefined
  }
  const ref = vehicleRefs.find((item) => item.vehicleRefKey === vehicleRefKey)
  if (!ref) {
    return undefined
  }
  return ref.assetTag ? `${ref.displayLabel} (${ref.assetTag})` : ref.displayLabel
}
