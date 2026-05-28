import { Library, GitBranch, Search, Play, Database, Terminal, Settings } from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const complianceCoreNavItems: ProductNavItem[] = [

  { label: 'Registry', to: '/registry', icon: Library as NavIcon },

  { label: 'Mappings', to: '/mappings', icon: GitBranch as NavIcon },

  { label: 'Findings', to: '/findings', icon: Search as NavIcon },

  { label: 'Evaluation', to: '/evaluation', icon: Play as NavIcon },

  { label: 'Fact sources', to: '/fact-sources', icon: Database as NavIcon },

  { label: 'Operator', to: '/operator', icon: Terminal as NavIcon },

  { label: 'Admin', to: '/admin', icon: Settings as NavIcon },

]


