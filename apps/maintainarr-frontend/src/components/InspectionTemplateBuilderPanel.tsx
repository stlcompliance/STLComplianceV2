import type {
  AssetTypeResponse,
  InspectionTemplateDetailResponse,
  InspectionTemplateSummaryResponse,
} from '../api/types'

interface InspectionTemplateBuilderPanelProps {
  canManage: boolean
  templates: InspectionTemplateSummaryResponse[]
  selectedTemplate: InspectionTemplateDetailResponse | null
  assetTypes: AssetTypeResponse[]
  isLoading: boolean
  isDetailLoading: boolean
  templateKey: string
  templateName: string
  templateDescription: string
  categoryKey: string
  categoryName: string
  itemKey: string
  itemPrompt: string
  itemType: string
  selectedCategoryId: string
  selectedAssetTypeIds: string[]
  selectedTemplateId: string
  onTemplateKeyChange: (value: string) => void
  onTemplateNameChange: (value: string) => void
  onTemplateDescriptionChange: (value: string) => void
  onCategoryKeyChange: (value: string) => void
  onCategoryNameChange: (value: string) => void
  onItemKeyChange: (value: string) => void
  onItemPromptChange: (value: string) => void
  onItemTypeChange: (value: string) => void
  onSelectedCategoryIdChange: (value: string) => void
  onSelectedAssetTypeIdsChange: (value: string[]) => void
  onSelectedTemplateIdChange: (value: string) => void
  onCreateTemplate: () => void
  onCreateCategory: () => void
  onCreateItem: () => void
  onSaveAssetTypes: () => void
  onActivateTemplate: () => void
  isCreatingTemplate: boolean
  isSavingBuilder: boolean
}

export function InspectionTemplateBuilderPanel({
  canManage,
  templates,
  selectedTemplate,
  assetTypes,
  isLoading,
  isDetailLoading,
  templateKey,
  templateName,
  templateDescription,
  categoryKey,
  categoryName,
  itemKey,
  itemPrompt,
  itemType,
  selectedCategoryId,
  selectedAssetTypeIds,
  selectedTemplateId,
  onTemplateKeyChange,
  onTemplateNameChange,
  onTemplateDescriptionChange,
  onCategoryKeyChange,
  onCategoryNameChange,
  onItemKeyChange,
  onItemPromptChange,
  onItemTypeChange,
  onSelectedCategoryIdChange,
  onSelectedAssetTypeIdsChange,
  onSelectedTemplateIdChange,
  onCreateTemplate,
  onCreateCategory,
  onCreateItem,
  onSaveAssetTypes,
  onActivateTemplate,
  isCreatingTemplate,
  isSavingBuilder,
}: InspectionTemplateBuilderPanelProps) {
  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading inspection templates…</p>
  }

  const toggleAssetType = (assetTypeId: string) => {
    if (selectedAssetTypeIds.includes(assetTypeId)) {
      onSelectedAssetTypeIdsChange(selectedAssetTypeIds.filter((id) => id !== assetTypeId))
      return
    }
    onSelectedAssetTypeIdsChange([...selectedAssetTypeIds, assetTypeId])
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <h2 className="text-lg font-semibold text-white">Inspection template builder</h2>
      <p className="mt-1 text-sm text-slate-400">
        Define checklist categories, items, and asset type applicability for field inspections.
      </p>

      {canManage ? (
        <div className="mt-6 grid gap-4 rounded-lg border border-slate-700 bg-slate-950/40 p-4 md:grid-cols-3">
          <label className="block text-sm" htmlFor="inspectiontemplatebuilder-template-key">
          <span className="text-slate-300">Template key</span>
          <input id="inspectiontemplatebuilder-template-key"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
              value={templateKey}
              onChange={(e) => onTemplateKeyChange(e.target.value)}
            />
          </label>
          <label className="block text-sm" htmlFor="inspectiontemplatebuilder-name">
          <span className="text-slate-300">Name</span>
          <input id="inspectiontemplatebuilder-name"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
              value={templateName}
              onChange={(e) => onTemplateNameChange(e.target.value)}
            />
          </label>
          <label className="block text-sm md:col-span-1" htmlFor="inspectiontemplatebuilder-description">
          <span className="text-slate-300">Description</span>
          <input id="inspectiontemplatebuilder-description"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
              value={templateDescription}
              onChange={(e) => onTemplateDescriptionChange(e.target.value)}
            />
          </label>
          <div className="md:col-span-3">
            <button
              type="button"
              className="rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
              disabled={isCreatingTemplate}
              onClick={onCreateTemplate}
            >
              {isCreatingTemplate ? 'Creating…' : 'Create template'}
            </button>
          </div>
        </div>
      ) : null}

      <div className="mt-6">
        <h3 className="text-sm font-medium text-slate-300">Templates</h3>
        {templates.length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No inspection templates yet.</p>
        ) : (
          <ul className="mt-2 divide-y divide-slate-800 rounded-lg border border-slate-800">
            {templates.map((template) => (
              <li key={template.inspectionTemplateId}>
                <button
                  type="button"
                  className={`flex w-full items-center justify-between px-4 py-3 text-left text-sm hover:bg-slate-800/60 ${
                    selectedTemplateId === template.inspectionTemplateId ? 'bg-slate-800/80' : ''
                  }`}
                  onClick={() => onSelectedTemplateIdChange(template.inspectionTemplateId)}
                >
                  <span>
                    <span className="font-medium text-white">{template.name}</span>
                    <span className="ml-2 text-slate-500">({template.templateKey})</span>
                  </span>
                  <span className="flex gap-3 text-xs text-slate-400">
                    <span>v{template.version}</span>
                    <span className="capitalize">{template.status}</span>
                    <span>{template.checklistItemCount} items</span>
                  </span>
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>

      {selectedTemplateId && isDetailLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading template detail…</p>
      ) : null}

      {selectedTemplate && canManage ? (
        <div className="mt-6 space-y-6 border-t border-slate-800 pt-6">
          <div>
            <h3 className="text-sm font-medium text-slate-300">
              Editing: {selectedTemplate.name}{' '}
              <span className="text-slate-500">({selectedTemplate.status}, v{selectedTemplate.version})</span>
            </h3>
          </div>

          <div className="grid gap-4 rounded-lg border border-slate-700 bg-slate-950/40 p-4 md:grid-cols-3">
            <label className="block text-sm" htmlFor="inspectiontemplatebuilder-category-key">
          <span className="text-slate-300">Category key</span>
          <input id="inspectiontemplatebuilder-category-key"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
                value={categoryKey}
                onChange={(e) => onCategoryKeyChange(e.target.value)}
              />
            </label>
            <label className="block text-sm" htmlFor="inspectiontemplatebuilder-category-name">
          <span className="text-slate-300">Category name</span>
          <input id="inspectiontemplatebuilder-category-name"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
                value={categoryName}
                onChange={(e) => onCategoryNameChange(e.target.value)}
              />
            </label>
            <div className="flex items-end">
              <button
                type="button"
                className="rounded bg-slate-700 px-4 py-2 text-sm hover:bg-slate-600 disabled:opacity-50"
                disabled={isSavingBuilder}
                onClick={onCreateCategory}
              >
                Add category
              </button>
            </div>
          </div>

          <div className="grid gap-4 rounded-lg border border-slate-700 bg-slate-950/40 p-4 md:grid-cols-2">
            <label className="block text-sm" htmlFor="inspectiontemplatebuilder-item-key">
          <span className="text-slate-300">Item key</span>
          <input id="inspectiontemplatebuilder-item-key"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
                value={itemKey}
                onChange={(e) => onItemKeyChange(e.target.value)}
              />
            </label>
            <label className="block text-sm" htmlFor="inspectiontemplatebuilder-item-type">
          <span className="text-slate-300">Item type</span>
          <select id="inspectiontemplatebuilder-item-type"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
                value={itemType}
                onChange={(e) => onItemTypeChange(e.target.value)}
              >
                <option value="pass_fail">Pass / fail</option>
                <option value="numeric">Numeric</option>
                <option value="text">Text</option>
              </select>
            </label>
            <label className="block text-sm md:col-span-2" htmlFor="inspectiontemplatebuilder-prompt">
          <span className="text-slate-300">Prompt</span>
          <input id="inspectiontemplatebuilder-prompt"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
                value={itemPrompt}
                onChange={(e) => onItemPromptChange(e.target.value)}
              />
            </label>
            <label className="block text-sm" htmlFor="inspectiontemplatebuilder-category-optional">
          <span className="text-slate-300">Category (optional)</span>
          <select id="inspectiontemplatebuilder-category-optional"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
                value={selectedCategoryId}
                onChange={(e) => onSelectedCategoryIdChange(e.target.value)}
              >
                <option value="">Uncategorized</option>
                {selectedTemplate.categories.map((category) => (
                  <option key={category.categoryId} value={category.categoryId}>
                    {category.name}
                  </option>
                ))}
              </select>
            </label>
            <div className="flex items-end">
              <button
                type="button"
                className="rounded bg-slate-700 px-4 py-2 text-sm hover:bg-slate-600 disabled:opacity-50"
                disabled={isSavingBuilder}
                onClick={onCreateItem}
              >
                Add checklist item
              </button>
            </div>
          </div>

          <div>
            <h4 className="text-sm font-medium text-slate-300">Linked asset types</h4>
            <div className="mt-2 flex flex-wrap gap-2">
              {assetTypes.map((type) => (
                <label
                  key={type.assetTypeId}
                  className="flex cursor-pointer items-center gap-2 rounded border border-slate-700 px-3 py-2 text-sm"
                >
                  <input id="inspectiontemplatebuilder"
                    type="checkbox"
                    checked={selectedAssetTypeIds.includes(type.assetTypeId)}
                    onChange={() => toggleAssetType(type.assetTypeId)}
                  />
                  <span>
                    {type.name} <span className="text-slate-500">({type.typeKey})</span>
                  </span>
                </label>
              ))}
            </div>
            <button
              type="button"
              className="mt-3 rounded bg-slate-700 px-4 py-2 text-sm hover:bg-slate-600 disabled:opacity-50"
              disabled={isSavingBuilder}
              onClick={onSaveAssetTypes}
            >
              Save asset type links
            </button>
          </div>

          {selectedTemplate.status === 'draft' ? (
            <button
              type="button"
              className="rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
              disabled={isSavingBuilder || selectedTemplate.checklistItems.length === 0}
              onClick={onActivateTemplate}
            >
              Activate template
            </button>
          ) : null}
        </div>
      ) : null}

      {selectedTemplate ? (
        <div className="mt-6 grid gap-6 md:grid-cols-2">
          <div>
            <h4 className="text-sm font-medium text-slate-300">Categories</h4>
            {selectedTemplate.categories.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500">No categories.</p>
            ) : (
              <ul className="mt-2 space-y-1 text-sm text-slate-200">
                {selectedTemplate.categories.map((category) => (
                  <li key={category.categoryId}>
                    {category.name} <span className="text-slate-500">({category.categoryKey})</span>
                  </li>
                ))}
              </ul>
            )}
          </div>
          <div>
            <h4 className="text-sm font-medium text-slate-300">Checklist items</h4>
            {selectedTemplate.checklistItems.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500">No checklist items.</p>
            ) : (
              <ul className="mt-2 space-y-2 text-sm">
                {selectedTemplate.checklistItems.map((item) => (
                  <li key={item.checklistItemId} className="rounded border border-slate-800 px-3 py-2">
                    <p className="text-slate-200">{item.prompt}</p>
                    <p className="mt-1 text-xs text-slate-500">
                      {item.itemKey} · {item.itemType}
                      {item.categoryKey ? ` · ${item.categoryKey}` : ''}
                      {item.isRequired ? ' · required' : ''}
                    </p>
                  </li>
                ))}
              </ul>
            )}
          </div>
          <div className="md:col-span-2">
            <h4 className="text-sm font-medium text-slate-300">Applicable asset types</h4>
            {selectedTemplate.linkedAssetTypes.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500">Not linked to any asset types.</p>
            ) : (
              <ul className="mt-2 flex flex-wrap gap-2 text-sm text-slate-200">
                {selectedTemplate.linkedAssetTypes.map((link) => (
                  <li key={link.assetTypeId} className="rounded border border-slate-700 px-2 py-1">
                    {link.typeName} ({link.classKey})
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}
    </section>
  )
}
