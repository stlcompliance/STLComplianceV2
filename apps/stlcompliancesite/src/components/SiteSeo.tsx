import { useEffect } from 'react'

import {

  applyPageSeo,

  buildOrganizationJsonLd,

  removeJsonLdScript,

  upsertJsonLd,

  type PageSeoInput,

} from '../lib/seo'



type SiteSeoProps = PageSeoInput & {

  includeOrganizationJsonLd?: boolean

}



export function SiteSeo({

  title,

  description,

  path,

  ogType,

  noIndex,

  ogImagePath,

  includeOrganizationJsonLd = false,

}: SiteSeoProps) {

  useEffect(() => {

    applyPageSeo({

      title,

      description,

      path,

      ogType,

      noIndex,

      ogImagePath,

    })



    if (includeOrganizationJsonLd) {

      upsertJsonLd('stl-organization-jsonld', buildOrganizationJsonLd())

    } else {

      removeJsonLdScript('stl-organization-jsonld')

    }

  }, [title, description, path, ogType, noIndex, ogImagePath, includeOrganizationJsonLd])



  return null

}


