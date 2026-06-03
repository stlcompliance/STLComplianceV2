import { buildSemanticKey, GeneratedKeyField } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

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
  onCloneTemplate: () => void
  onImportTemplateJson: (json: string, templateKeyOverride: string) => Promise<void>
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
  onCloneTemplate,
  onImportTemplateJson,
  isCreatingTemplate,
  isSavingBuilder,
}: InspectionTemplateBuilderPanelProps) {
  const [showTemplateKeyPolicy, setShowTemplateKeyPolicy] = useState(false)
  const [showCategoryKeyPolicy, setShowCategoryKeyPolicy] = useState(false)
  const [showItemKeyPolicy, setShowItemKeyPolicy] = useState(false)
  const [importJson, setImportJson] = useState('')
  const [importTemplateKey, setImportTemplateKey] = useState('')
  const existingTemplateKeys = templates.map((template) => template.templateKey)
  const existingCategoryKeys = selectedTemplate?.categories.map((category) => category.categoryKey) ?? []
  const existingItemKeys = selectedTemplate?.checklistItems.map((item) => item.itemKey) ?? []
  const generatedTemplateKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'inspection',
        kind: 'template',
        title: templateName.trim(),
        existingKeys: existingTemplateKeys,
        maxLength: 128,
      }),
    [existingTemplateKeys, templateName],
  )
  const generatedCategoryKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'inspection',
        kind: 'category',
        title: categoryName.trim(),
        existingKeys: existingCategoryKeys,
        maxLength: 128,
      }),
    [categoryName, existingCategoryKeys],
  )
  const generatedItemKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'inspection',
        kind: 'item',
        title: itemPrompt.trim(),
        existingKeys: existingItemKeys,
        maxLength: 128,
      }),
    [existingItemKeys, itemPrompt],
  )

  const exportSelectedTemplateJson = () => {
    if (!selectedTemplate) {
      return
    }

    const blob = new Blob([JSON.stringify(selectedTemplate, null, 2)], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `${selectedTemplate.templateKey}-template.json`
    anchor.click()
    URL.revokeObjectURL(url)
  }

  useEffect(() => {
    onTemplateKeyChange(generatedTemplateKey)
  }, [generatedTemplateKey, onTemplateKeyChange])

  useEffect(() => {
    onCategoryKeyChange(generatedCategoryKey)
  }, [generatedCategoryKey, onCategoryKeyChange])

  useEffect(() => {
    onItemKeyChange(generatedItemKey)
  }, [generatedItemKey, onItemKeyChange])

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
          <div className="space-y-1 text-sm">
            <GeneratedKeyField
              sourceLabel={templateName.trim()}
              generatedKey={generatedTemplateKey}
              confirmedKey={templateKey}
              manualOverride=""
              onManualOverrideChange={() => {}}
              showAdvancedKey={showTemplateKeyPolicy}
              disabled={isCreatingTemplate}
              label="Template key"
            />
            {!showTemplateKeyPolicy ? (
              <button
                type="button"
                className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
                onClick={() => setShowTemplateKeyPolicy(true)}
                disabled={isCreatingTemplate}
              >
                Key policy
              </button>
            ) : null}
          </div>
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
              disabled={isCreatingTemplate || !templateKey.trim() || !templateName.trim()}
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
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h3 className="text-sm font-medium text-slate-300">
              Editing: {selectedTemplate.name}{' '}
              <span className="text-slate-500">({selectedTemplate.status}, v{selectedTemplate.version})</span>
            </h3>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                className="rounded bg-slate-700 px-3 py-1 text-xs text-white hover:bg-slate-600 disabled:opacity-50"
                disabled={isSavingBuilder}
                onClick={onCloneTemplate}
              >
                Clone template
              </button>
              <button
                type="button"
                className="rounded border border-slate-600 px-3 py-1 text-xs text-slate-100 hover:bg-slate-800 disabled:opacity-50"
                onClick={exportSelectedTemplateJson}
              >
                Export JSON
              </button>
            </div>
          </div>
        </div>

          <div className="grid gap-4 rounded-lg border border-slate-700 bg-slate-950/40 p-4 md:grid-cols-3">
            <div className="space-y-1 text-sm">
              <GeneratedKeyField
                sourceLabel={categoryName.trim()}
                generatedKey={generatedCategoryKey}
                confirmedKey={categoryKey}
                manualOverride=""
                onManualOverrideChange={() => {}}
                showAdvancedKey={showCategoryKeyPolicy}
                disabled={isSavingBuilder}
                label="Category key"
              />
              {!showCategoryKeyPolicy ? (
                <button
                  type="button"
                  className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
                  onClick={() => setShowCategoryKeyPolicy(true)}
                  disabled={isSavingBuilder}
                >
                  Key policy
                </button>
              ) : null}
            </div>
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
                disabled={isSavingBuilder || !categoryKey.trim() || !categoryName.trim()}
                onClick={onCreateCategory}
              >
                Add category
              </button>
            </div>
          </div>

          <div className="rounded-lg border border-slate-700 bg-slate-950/40 p-4">
            <h3 className="text-sm font-medium text-slate-300">Import template JSON</h3>
            <p className="mt-1 text-xs text-slate-500">
              Paste an exported template payload, optionally override the template key, and import it as a new draft.
            </p>
            <label htmlFor="inspectiontemplatebuilder-import-key" className="mt-3 block text-sm text-slate-300">
              Imported template key override
              <input
                id="inspectiontemplatebuilder-import-key"
                value={importTemplateKey}
                onChange={(event) => setImportTemplateKey(event.target.value)}
                placeholder="Leave blank to reuse the exported key"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
              />
            </label>
            <label htmlFor="inspectiontemplatebuilder-import-json" className="mt-3 block text-sm text-slate-300">
              Template JSON
              <textarea
                id="inspectiontemplatebuilder-import-json"
                value={importJson}
                onChange={(event) => setImportJson(event.target.value)}
                rows={8}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2 font-mono text-xs"
                placeholder="Paste exported template JSON here"
              />
            </label>
            <div className="mt-3">
              <button
                type="button"
                className="rounded bg-sky-700 px-4 py-2 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                disabled={isSavingBuilder || !importJson.trim()}
                onClick={async () => {
                  await onImportTemplateJson(importJson, importTemplateKey)
                  setImportJson('')
                  setImportTemplateKey('')
                }}
              >
                Import template
              </button>
            </div>
          </div>

          <div className="grid gap-4 rounded-lg border border-slate-700 bg-slate-950/40 p-4 md:grid-cols-2">
            <div className="space-y-1 text-sm">
              <GeneratedKeyField
                sourceLabel={itemPrompt.trim()}
                generatedKey={generatedItemKey}
                confirmedKey={itemKey}
                manualOverride=""
                onManualOverrideChange={() => {}}
                showAdvancedKey={showItemKeyPolicy}
                disabled={isSavingBuilder}
                label="Item key"
              />
              {!showItemKeyPolicy ? (
                <button
                  type="button"
                  className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
                  onClick={() => setShowItemKeyPolicy(true)}
                  disabled={isSavingBuilder}
                >
                  Key policy
                </button>
              ) : null}
            </div>
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
                disabled={isSavingBuilder || !itemKey.trim() || !itemPrompt.trim()}
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
