# LedgArr 07 - Fixed Assets, Project Costing, and Budgets

LedgArr owns financial fixed asset records, depreciation, project/job costing, and budgets. MaintainArr owns physical assets and maintenance execution.

Fixed asset entities:

- FixedAssetFinancialRecord
- AssetCapitalizationEvent
- AssetDepreciationBook
- AssetDepreciationSchedule
- AssetDepreciationRun
- AssetDisposal
- AssetImpairment
- AssetRevaluation

Project/job costing entities:

- FinancialProject
- FinancialProjectTask
- JobCostCode
- ProjectBudget and ProjectBudgetLine
- ProjectActualCost
- ProjectCommittedCost
- ProjectCostAllocation
- ProjectBillingStatus

Budgeting entities:

- Budget
- BudgetVersion
- BudgetLine
- BudgetApproval
- BudgetActualSnapshot
- BudgetVarianceSnapshot

Typical flows:

- MaintainArr work order costs, vendor services, labor costing, and asset capitalization candidates flow to LedgArr as financial packets.
- LedgArr classifies costs as expense, capitalization, warranty recovery, project cost, or asset clearing.
- LedgArr creates FixedAssetFinancialRecord references to MaintainArr asset IDs when capitalization is approved.
- Depreciation runs post scheduled depreciation to GL.
- FinancialProject and JobCostCode accumulate actual, committed, and billable costs from operational packets.
- Budgets are approved by FinancialLegalEntity, fiscal period, account, department, site, cost center, project, or other dimensions.
- SupplyArr, MaintainArr, RoutArr, and LoadArr may call LedgArr budget checks before approving spend.

Budget results should be `allowed`, `warning`, `blocked`, or `approval_required`.
