import { InspectionTemplateBuilderPanel } from '../../components/InspectionTemplateBuilderPanel'
import { useLocation } from 'react-router-dom'
import { InspectionTemplateProfile } from './MaintenanceDetailProfiles'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function InspectionTemplatesSection({ state }: Props) {
  const s = state
  const location = useLocation()
  if (location.pathname.startsWith('/inspection-templates/details')) {
    return <InspectionTemplateProfile state={s} />
  }

  return (
    <div className="mb-8">
      <InspectionTemplateBuilderPanel
        canManage={s.canManage}
        templates={s.templatesQuery.data ?? []}
        selectedTemplate={s.templateDetailQuery.data ?? null}
        assetTypes={s.typesQuery.data ?? []}
        isLoading={s.templatesQuery.isLoading}
        isDetailLoading={s.templateDetailQuery.isLoading}
        templateKey={s.templateKey}
        templateName={s.templateName}
        templateDescription={s.templateDescription}
        categoryKey={s.categoryKey}
        categoryName={s.categoryName}
        itemKey={s.itemKey}
        itemPrompt={s.itemPrompt}
        itemType={s.itemType}
        itemControlledOptionsText={s.itemControlledOptionsText}
        itemMeterReadingMin={s.itemMeterReadingMin}
        itemMeterReadingMax={s.itemMeterReadingMax}
        itemUnitOfMeasure={s.itemUnitOfMeasure}
        selectedCategoryId={s.selectedCategoryId}
        selectedAssetTypeIds={s.selectedAssetTypeIds}
        selectedTemplateId={s.selectedTemplateId}
        onTemplateKeyChange={s.setTemplateKey}
        onTemplateNameChange={s.setTemplateName}
        onTemplateDescriptionChange={s.setTemplateDescription}
        onCategoryKeyChange={s.setCategoryKey}
        onCategoryNameChange={s.setCategoryName}
        onItemKeyChange={s.setItemKey}
        onItemPromptChange={s.setItemPrompt}
        onItemTypeChange={s.setItemType}
        onItemControlledOptionsTextChange={s.setItemControlledOptionsText}
        onItemMeterReadingMinChange={s.setItemMeterReadingMin}
        onItemMeterReadingMaxChange={s.setItemMeterReadingMax}
        onItemUnitOfMeasureChange={s.setItemUnitOfMeasure}
        onSelectedCategoryIdChange={s.setSelectedCategoryId}
        onSelectedAssetTypeIdsChange={s.setSelectedAssetTypeIds}
        onSelectedTemplateIdChange={s.setSelectedTemplateId}
        onCreateTemplate={() => s.createTemplateMutation.mutate()}
        onCreateCategory={() => s.createCategoryMutation.mutate()}
        onCreateItem={() => s.createItemMutation.mutate()}
        onSaveAssetTypes={() => s.saveAssetTypesMutation.mutate()}
        onActivateTemplate={() => s.activateTemplateMutation.mutate()}
        onCloneTemplate={() => s.cloneTemplateMutation.mutate()}
        isCreatingTemplate={s.createTemplateMutation.isPending}
        isSavingBuilder={
          s.createCategoryMutation.isPending ||
          s.createItemMutation.isPending ||
          s.saveAssetTypesMutation.isPending ||
          s.activateTemplateMutation.isPending ||
          s.cloneTemplateMutation.isPending ||
          s.importTemplateMutation.isPending
        }
      />
    </div>
  )
}
