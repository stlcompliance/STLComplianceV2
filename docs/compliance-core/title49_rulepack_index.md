# Title 49 rulepack index

| Rule pack | Level | Program | Citations | Facts | Products | Entities |
| --- | --- | --- | ---: | ---: | --- | --- |
| `title49.motorcarrier.registration_authority` | operational | `fmcsa_fmcsr` | 11 | 3 | StaffArr, RoutArr, ComplianceCore | carrier, authority_record, registration |
| `title49.motorcarrier.insurance_financial_responsibility` | operational | `fmcsa_fmcsr` | 56 | 2 | StaffArr, RoutArr, SupplyArr, ComplianceCore | carrier, insurance_policy, shipment |
| `title49.driver.drug_alcohol_program` | operational | `dot_part40` | 282 | 5 | StaffArr, TrainArr, RoutArr, ComplianceCore | driver, test_result, clearinghouse_query, program |
| `title49.driver.cdl_clp_endorsements` | operational | `fmcsa_fmcsr` | 49 | 3 | StaffArr, TrainArr, RoutArr, ComplianceCore | driver, license, endorsement |
| `title49.driver.entry_level_driver_training` | operational | `fmcsa_fmcsr` | 45 | 3 | TrainArr, StaffArr, RoutArr, ComplianceCore | driver, training_assignment, training_provider |
| `title49.driver.medical_qualification` | operational | `fmcsa_fmcsr` | 24 | 3 | StaffArr, TrainArr, RoutArr, ComplianceCore | driver, medical_certificate, examiner |
| `title49.driver.qualification_file` | operational | `fmcsa_fmcsr` | 30 | 8 | StaffArr, TrainArr, RoutArr, ComplianceCore | driver, qualification_file, mvr |
| `title49.driver.hours_of_service` | operational | `fmcsa_fmcsr` | 12 | 3 | RoutArr, StaffArr, ComplianceCore | driver, trip, duty_status, hos_log |
| `title49.driver.eld_records` | operational | `fmcsa_fmcsr` | 12 | 3 | RoutArr, StaffArr, ComplianceCore | driver, eld_device, eld_record |
| `title49.driver.accident_post_accident_actions` | operational | `fmcsa_fmcsr` | 2 | 3 | StaffArr, RoutArr, ComplianceCore | incident, driver, accident_register, test_decision |
| `title49.motorcarrier.applicability` | operational | `fmcsa_fmcsr` | 86 | 3 | StaffArr, RoutArr, MaintainArr, ComplianceCore | carrier, driver, vehicle, dispatch |
| `title49.vehicle.cargo_securement` | operational | `fmcsa_fmcsr` | 20 | 2 | MaintainArr, RoutArr, SupplyArr, ComplianceCore | vehicle, load, securement_device |
| `title49.vehicle.parts_accessories_condition` | operational | `fmcsa_fmcsr` | 81 | 2 | MaintainArr, RoutArr, ComplianceCore | vehicle, asset, defect |
| `title49.vehicle.dvir` | operational | `fmcsa_fmcsr` | 2 | 2 | MaintainArr, RoutArr, StaffArr, ComplianceCore | vehicle, dvir, defect, repair |
| `title49.vehicle.annual_inspection` | operational | `fmcsa_fmcsr` | 5 | 3 | MaintainArr, RoutArr, ComplianceCore | vehicle, inspection, inspector |
| `title49.vehicle.roadside_inspection_correction` | operational | `fmcsa_fmcsr` | 1 | 2 | MaintainArr, RoutArr, ComplianceCore | vehicle, roadside_inspection, defect |
| `title49.vehicle.out_of_service_readiness` | operational | `fmcsa_fmcsr` | 1 | 2 | MaintainArr, RoutArr, StaffArr, ComplianceCore | vehicle, driver, dispatch, out_of_service_order |
| `title49.vehicle.inspection_repair_maintenance` | operational | `fmcsa_fmcsr` | 7 | 3 | MaintainArr, RoutArr, ComplianceCore | vehicle, asset, work_order, inspection |
| `title49.hazmat.applicability` | operational | `phmsa_hmr` | 20 | 2 | SupplyArr, RoutArr, TrainArr, ComplianceCore | material, shipment, hazmat_function |
| `title49.hazmat.incident_reporting` | operational | `phmsa_hmr` | 7 | 2 | StaffArr, SupplyArr, RoutArr, ComplianceCore | incident, shipment, hazmat_release |
| `title49.hazmat.hazardous_materials_table` | operational | `phmsa_hmr` | 3 | 2 | SupplyArr, RoutArr, ComplianceCore | material, shipment, shipping_description |
| `title49.hazmat.shipping_papers` | operational | `phmsa_hmr` | 12 | 2 | SupplyArr, RoutArr, ComplianceCore | shipment, shipping_paper, emergency_response_info |
| `title49.hazmat.marking` | operational | `phmsa_hmr` | 28 | 2 | SupplyArr, RoutArr, ComplianceCore | package, shipment, marking |
| `title49.hazmat.labeling` | operational | `phmsa_hmr` | 34 | 2 | SupplyArr, RoutArr, ComplianceCore | package, shipment, label |
| `title49.hazmat.placarding` | operational | `phmsa_hmr` | 39 | 2 | SupplyArr, RoutArr, ComplianceCore | vehicle, shipment, placard |
| `title49.hazmat.training` | operational | `phmsa_hmr` | 5 | 2 | TrainArr, StaffArr, SupplyArr, RoutArr, ComplianceCore | worker, training_record, hazmat_employee |
| `title49.hazmat.security_plan` | operational | `phmsa_hmr` | 6 | 2 | SupplyArr, RoutArr, StaffArr, ComplianceCore | shipment, route, security_plan |
| `title49.hazmat.classification` | operational | `phmsa_hmr` | 114 | 2 | SupplyArr, TrainArr, RoutArr, ComplianceCore | material, sds, classification |
| `title49.hazmat.packaging` | operational | `phmsa_hmr` | 136 | 3 | SupplyArr, MaintainArr, RoutArr, ComplianceCore | material, package, container, asset |
| `title49.hazmat.loading_unloading_segregation` | operational | `phmsa_hmr` | 66 | 3 | SupplyArr, RoutArr, MaintainArr, ComplianceCore | shipment, vehicle, load, route |
| `title49.hazmat.registration` | operational | `phmsa_hmr` | 7 | 2 | SupplyArr, RoutArr, StaffArr, ComplianceCore | carrier, shipper, registration |
| `title49.hazmat.special_permits_exceptions` | operational | `phmsa_hmr` | 20 | 2 | SupplyArr, RoutArr, ComplianceCore | special_permit, approval, shipment |
| `title49.hazmat.loading_unloading_segregation_reference` | reference | `phmsa_hmr` | 414 | 0 | SupplyArr, RoutArr, ComplianceCore | shipment, mode, package |
| `title49.pipeline.safety_reference` | reference | `phmsa_pipeline` | 749 | 0 | ComplianceCore | pipeline_operator, pipeline_program |
| `title49.rail.safety_reference` | reference | `fra_rail` | 2374 | 0 | ComplianceCore | railroad, rail_equipment, rail_operation |
| `title49.transit.safety_reference` | reference | `fta_transit` | 451 | 0 | ComplianceCore | transit_agency, transit_program |
| `title49.nhtsa.vehicle_standards_reference` | reference | `nhtsa_vehicle` | 1244 | 0 | MaintainArr, ComplianceCore | vehicle, vehicle_standard, manufacturer |
| `title49.tsa.transportation_security_reference` | reference | `tsa_security` | 447 | 0 | StaffArr, TrainArr, RoutArr, ComplianceCore | worker, credential, transportation_security |
| `title49.stb.surface_transportation_reference` | reference | `stb_surface` | 874 | 0 | ComplianceCore | carrier, rate_case, surface_transportation |
| `title49.passenger_carrier.operations` | reference | `fmcsa_passenger` | 4 | 0 | RoutArr, StaffArr, ComplianceCore | passenger_trip, carrier, lease |
| `title49.household_goods.consumer_protection` | reference | `fmcsa_household_goods` | 58 | 0 | SupplyArr, RoutArr, ComplianceCore | household_goods_shipment, carrier, consumer_disclosure |
| `title49.motorcarrier.safety_fitness_proceedings_reference` | reference | `fmcsa_fmcsr` | 247 | 0 | StaffArr, RoutArr, ComplianceCore | state_compliance, safety_fitness, enforcement_proceeding |
| `title49.intermodal.equipment_provider` | reference | `fmcsa_intermodal` | 9 | 0 | MaintainArr, RoutArr, ComplianceCore | intermodal_equipment, provider, inspection |
| `title49.transportation.citation_metadata` | metadata | `dot_title49_metadata` | 2692 | 0 | ComplianceCore | citation, regulatory_hierarchy |
