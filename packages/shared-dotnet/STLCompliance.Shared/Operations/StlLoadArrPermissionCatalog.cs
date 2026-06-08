namespace STLCompliance.Shared.Operations;

public sealed record LoadArrPermissionCatalogItem(
    string PermissionKey,
    string Label,
    string? Description,
    string Scope,
    string Sensitivity,
    string Status = "active");

public static class StlLoadArrPermissionCatalog
{
    public static IReadOnlyList<LoadArrPermissionCatalogItem> All { get; } =
    [
        P("loadarr.dashboard.read", "Read Dashboard", "View the LoadArr operational dashboard.", "product"),

        P("loadarr.expected_receipts.read", "Read Expected Receipts", "View expected receipt records and details.", "product"),
        P("loadarr.expected_receipts.manage", "Manage Expected Receipts", "Create and adjust expected receipt coordination records.", "product", "sensitive"),

        P("loadarr.receiving.read", "Read Receiving", "View receiving sessions and receipt detail.", "product"),
        P("loadarr.receiving.create", "Create Receiving", "Start receiving sessions and guided receiving flows.", "product"),
        P("loadarr.receiving.confirm", "Confirm Receiving", "Complete receiving sessions and confirm receipt outcomes.", "product", "sensitive"),
        P("loadarr.receiving.exception.create", "Create Receiving Exception", "Capture a receiving exception that needs operational follow-up.", "product", "sensitive"),

        P("loadarr.dock_schedule.read", "Read Dock Schedule", "View dock schedule and warehouse task queues.", "product"),
        P("loadarr.dock_schedule.manage", "Manage Dock Schedule", "Create and update dock schedule tasks.", "product", "sensitive"),

        P("loadarr.putaway.read", "Read Putaway", "View putaway task status and completion detail.", "product"),
        P("loadarr.putaway.execute", "Execute Putaway", "Complete putaway tasks and confirm destination locations.", "product", "sensitive"),

        P("loadarr.inventory.read", "Read Inventory", "View inventory balances and stock state.", "product"),
        P("loadarr.inventory.adjust", "Adjust Inventory", "Post inventory adjustments from authorized operational workflows.", "product", "sensitive"),
        P("loadarr.inventory.hold", "Place Inventory Hold", "Place operational inventory holds.", "product", "sensitive"),
        P("loadarr.inventory.release", "Release Inventory Hold", "Release held inventory back to availability.", "product", "critical"),

        P("loadarr.transfers.read", "Read Transfers", "View transfer orders and transfer detail.", "product"),
        P("loadarr.transfers.create", "Create Transfer", "Create transfer requests and draft transfer orders.", "product"),
        P("loadarr.transfers.execute", "Execute Transfer", "Complete transfer execution between locations.", "product", "sensitive"),
        P("loadarr.transfers.cancel", "Cancel Transfer", "Cancel a transfer that should not proceed.", "product", "sensitive"),

        P("loadarr.reservations.read", "Read Reservations", "View reservation records and inventory commitments.", "product"),
        P("loadarr.reservations.create", "Create Reservation", "Create inventory reservations.", "product"),
        P("loadarr.reservations.manage", "Manage Reservations", "Update or reassign active reservations.", "product", "sensitive"),
        P("loadarr.reservations.release", "Release Reservations", "Release reserved inventory back to availability.", "product", "sensitive"),

        P("loadarr.picking.read", "Read Picking", "View pick tasks and pick queue detail.", "product"),
        P("loadarr.picking.execute", "Execute Picking", "Complete pick tasks and stage stock for loadout.", "product", "sensitive"),

        P("loadarr.staging.read", "Read Staging", "View staging assignments and staging detail.", "product"),
        P("loadarr.staging.manage", "Manage Staging", "Create and update staging assignments.", "product", "sensitive"),

        P("loadarr.shipping.read", "Read Shipping", "View shipping and loadout status.", "product"),
        P("loadarr.shipping.confirm", "Confirm Shipping", "Confirm staged stock as loaded or shipped.", "product", "sensitive"),

        P("loadarr.counts.read", "Read Cycle Counts", "View cycle count sessions and results.", "product"),
        P("loadarr.counts.create", "Create Cycle Count", "Create cycle count sessions.", "product"),
        P("loadarr.counts.execute", "Execute Cycle Count", "Capture counts and count lines.", "product", "sensitive"),
        P("loadarr.counts.approve", "Approve Cycle Count", "Approve variances before posting inventory adjustments.", "product", "sensitive"),

        P("loadarr.exceptions.read", "Read Exceptions", "View warehouse exceptions and exception queues.", "product"),
        P("loadarr.exceptions.create", "Create Exception", "Capture a warehouse exception from an operational workflow.", "product"),
        P("loadarr.exceptions.resolve", "Resolve Exception", "Resolve or close a warehouse exception operationally.", "product", "sensitive"),
        P("loadarr.exceptions.escalate_quality", "Escalate Quality Exception", "Escalate quality-bearing exceptions for formal AssurArr review.", "product", "critical"),

        P("loadarr.supply_coordination.read", "Read Supply Coordination", "View LoadArr supply coordination signals and coordination queues.", "product"),
        P("loadarr.supply_coordination.manage", "Manage Supply Coordination", "Coordinate PO receipts, vendor returns, backorders, and reorder signals.", "product", "sensitive"),

        P("loadarr.records.read", "Read Records", "View warehouse history, ledger, and adjustment records.", "record"),

        P("loadarr.setup.read", "Read Setup", "View LoadArr setup records and configuration.", "tenant"),
        P("loadarr.setup.manage", "Manage Setup", "Create and update LoadArr setup records and configuration.", "tenant", "sensitive"),

        P("loadarr.admin.read", "Read Admin", "View LoadArr admin surfaces and configuration summaries.", "tenant"),
        P("loadarr.admin.manage", "Manage Admin", "Update LoadArr-local admin settings.", "tenant", "critical"),
        P("loadarr.integrations.manage", "Manage Integrations", "Manage LoadArr integration health and retries.", "tenant", "critical"),
        P("loadarr.permissions.manage", "Manage Permissions", "Manage or sync LoadArr permission catalog entries.", "tenant", "critical"),
    ];

    private static LoadArrPermissionCatalogItem P(
        string permissionKey,
        string label,
        string description,
        string scope,
        string sensitivity = "standard",
        string status = "active") =>
        new(permissionKey, label, description, scope, sensitivity, status);
}
