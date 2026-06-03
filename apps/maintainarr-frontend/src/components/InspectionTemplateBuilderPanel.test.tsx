import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { InspectionTemplateBuilderPanel } from './InspectionTemplateBuilderPanel'

describe('InspectionTemplateBuilderPanel', () => {
  afterEach(() => {
    cleanup()
  })

  const baseProps = {
    canManage: true,
    templates: [
      {
        inspectionTemplateId: '11111111-1111-1111-1111-111111111111',
        templateKey: 'daily-walkaround',
        name: 'Daily Walkaround',
        description: 'Pre-shift check',
        version: 2,
        status: 'draft',
        categoryCount: 1,
        checklistItemCount: 3,
        linkedAssetTypeCount: 1,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-05-27T12:00:00Z',
      },
    ],
    selectedTemplate: {
      inspectionTemplateId: '11111111-1111-1111-1111-111111111111',
      templateKey: 'daily-walkaround',
      name: 'Daily Walkaround',
      description: 'Pre-shift check',
      version: 2,
      status: 'draft',
      categories: [
        {
          categoryId: '22222222-2222-2222-2222-222222222222',
          categoryKey: 'safety',
          name: 'Safety',
          sortOrder: 10,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        },
      ],
      checklistItems: [
        {
          checklistItemId: '33333333-3333-3333-3333-333333333333',
          categoryId: '22222222-2222-2222-2222-222222222222',
          categoryKey: 'safety',
          itemKey: 'horn-works',
          prompt: 'Horn operates correctly',
          itemType: 'pass_fail',
          isRequired: true,
          sortOrder: 10,
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        },
      ],
      linkedAssetTypes: [
        {
          assetTypeId: '44444444-4444-4444-4444-444444444444',
          typeKey: 'forklift',
          typeName: 'Forklift',
          classKey: 'vehicles',
          className: 'Vehicles',
        },
      ],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-05-27T12:00:00Z',
    },
    assetTypes: [
      {
        assetTypeId: '44444444-4444-4444-4444-444444444444',
        assetClassId: '55555555-5555-5555-5555-555555555555',
        classKey: 'vehicles',
        className: 'Vehicles',
        typeKey: 'forklift',
        name: 'Forklift',
        description: '',
        status: 'active',
        createdAt: '2026-01-01T00:00:00Z',
      },
    ],
    isLoading: false,
    isDetailLoading: false,
    templateKey: '',
    templateName: '',
    templateDescription: '',
    categoryKey: '',
    categoryName: '',
    itemKey: '',
    itemPrompt: '',
    itemType: 'pass_fail',
    selectedCategoryId: '',
    selectedAssetTypeIds: ['44444444-4444-4444-4444-444444444444'],
    selectedTemplateId: '11111111-1111-1111-1111-111111111111',
    onTemplateKeyChange: vi.fn(),
    onTemplateNameChange: vi.fn(),
    onTemplateDescriptionChange: vi.fn(),
    onCategoryKeyChange: vi.fn(),
    onCategoryNameChange: vi.fn(),
    onItemKeyChange: vi.fn(),
    onItemPromptChange: vi.fn(),
    onItemTypeChange: vi.fn(),
    onSelectedCategoryIdChange: vi.fn(),
    onSelectedAssetTypeIdsChange: vi.fn(),
    onSelectedTemplateIdChange: vi.fn(),
    onCreateTemplate: vi.fn(),
    onCreateCategory: vi.fn(),
    onCreateItem: vi.fn(),
    onSaveAssetTypes: vi.fn(),
    onActivateTemplate: vi.fn(),
    onCloneTemplate: vi.fn(),
    onImportTemplateJson: vi.fn(async () => {}),
    isCreatingTemplate: false,
    isSavingBuilder: false,
  }

  it('renders template list and checklist detail', () => {
    render(<InspectionTemplateBuilderPanel {...baseProps} />)

    expect(screen.getByText('Inspection template builder')).toBeInTheDocument()
    expect(screen.getByText('Daily Walkaround')).toBeInTheDocument()
    expect(screen.getByText('Horn operates correctly')).toBeInTheDocument()
    expect(screen.getByText('Forklift (vehicles)')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Clone template' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Export JSON' })).toBeInTheDocument()
    expect(screen.getByLabelText('Template JSON')).toBeInTheDocument()
  })

  it('downloads the selected template JSON export', () => {
    const createObjectUrl = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:template-json')
    const revokeObjectUrl = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    const click = vi.fn()
    const originalCreateElement = document.createElement.bind(document)
    const createElementSpy = vi.spyOn(document, 'createElement').mockImplementation(((tagName: string) => {
      const element = originalCreateElement(tagName)
      if (tagName.toLowerCase() === 'a') {
        return { ...element, click } as unknown as HTMLAnchorElement
      }
      return element
    }) as typeof document.createElement)

    render(<InspectionTemplateBuilderPanel {...baseProps} />)
    fireEvent.click(screen.getByRole('button', { name: 'Export JSON' }))

    expect(createObjectUrl).toHaveBeenCalledTimes(1)
    expect(click).toHaveBeenCalledTimes(1)
    expect(revokeObjectUrl).toHaveBeenCalledWith('blob:template-json')

    createObjectUrl.mockRestore()
    revokeObjectUrl.mockRestore()
    createElementSpy.mockRestore()
  })

  it('shows empty state when no templates exist', () => {
    render(
      <InspectionTemplateBuilderPanel
        {...baseProps}
        templates={[]}
        selectedTemplate={null}
        selectedTemplateId=""
        canManage={false}
      />,
    )

    expect(screen.getByText('No inspection templates yet.')).toBeInTheDocument()
  })

  it('submits imported JSON with key override', async () => {
    const onImportTemplateJson = vi.fn(async () => {})
    render(
      <InspectionTemplateBuilderPanel
        {...baseProps}
        onImportTemplateJson={onImportTemplateJson}
      />,
    )

    fireEvent.change(screen.getByLabelText('Imported template key override'), {
      target: { value: 'imported-walkaround' },
    })
    fireEvent.change(screen.getByLabelText('Template JSON'), {
      target: { value: JSON.stringify(baseProps.selectedTemplate, null, 2) },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Import template' }))

    await vi.waitFor(() => {
      expect(onImportTemplateJson).toHaveBeenCalledWith(
        JSON.stringify(baseProps.selectedTemplate, null, 2),
        'imported-walkaround',
      )
    })
  })
})
