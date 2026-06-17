import { Fragment, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import type { KbArticle } from '../content/docs'
import { resolveArticleLink } from '../content/docs'

type MarkdownArticleProps = {
  article: KbArticle
}

type ListState = {
  type: 'ul' | 'ol'
  items: string[]
}

type ImageLine = {
  alt: string
  caption: string
  src: string
}

function parseImageLine(text: string): ImageLine | null {
  const image = text.match(/^!\[([^\]]*)]\(([^)\s]+)(?:\s+"([^"]+)")?\)$/)
  if (!image) {
    return null
  }

  const alt = image[1].trim()
  const caption = image[3]?.trim() || alt
  return {
    alt,
    caption,
    src: image[2],
  }
}

function renderImage(image: ImageLine, index: number): ReactNode {
  return (
    <figure className="article-screenshot" key={`image-${index}`}>
      <img alt={image.alt} loading="lazy" src={image.src} />
      {image.caption ? <figcaption>{image.caption}</figcaption> : null}
    </figure>
  )
}

function renderInline(text: string, article: KbArticle): ReactNode[] {
  const nodes: ReactNode[] = []
  const linkPattern = /\[([^\]]+)]\(([^)]+)\)/g
  let lastIndex = 0
  let match = linkPattern.exec(text)

  while (match) {
    if (match.index > lastIndex) {
      nodes.push(...renderBold(text.slice(lastIndex, match.index)))
    }

    const label = match[1]
    const href = match[2]
    const resolvedLink = resolveArticleLink(article.relativePath, href)

    if (resolvedLink) {
      const isExternal = /^(https?:|mailto:|tel:)/i.test(resolvedLink.href)
      const displayLabel = resolvedLink.title ?? label
      nodes.push(
        isExternal ? (
          <a key={`${match.index}-${href}`} href={resolvedLink.href} rel="noopener noreferrer" target="_blank">
            {displayLabel}
          </a>
        ) : (
          <Link key={`${match.index}-${href}`} to={resolvedLink.href}>
            {displayLabel}
          </Link>
        ),
      )
    } else {
      nodes.push(<Fragment key={`${match.index}-${href}`}>{label}</Fragment>)
    }

    lastIndex = match.index + match[0].length
    match = linkPattern.exec(text)
  }

  if (lastIndex < text.length) {
    nodes.push(...renderBold(text.slice(lastIndex)))
  }

  return nodes
}

function renderBold(text: string): ReactNode[] {
  return text.split(/(\*\*[^*]+\*\*)/g).map((part, index) => {
    const bold = part.match(/^\*\*([^*]+)\*\*$/)?.[1]
    if (bold) {
      return <strong key={`${part}-${index}`}>{bold}</strong>
    }

    return <Fragment key={`${part}-${index}`}>{part}</Fragment>
  })
}

function formatListItemText(text: string, article: KbArticle): string {
  if (article.sectionId === 'reference') {
    return text
  }

  if (!/^[a-z]/.test(text)) {
    return text
  }

  if (/^`|\[[^\]]+]\(/.test(text)) {
    return text
  }

  return `${text.charAt(0).toUpperCase()}${text.slice(1)}`
}

function flushList(elements: ReactNode[], list: ListState | null, article: KbArticle): ListState | null {
  if (!list) {
    return null
  }

  const children = list.items.map((item, index) => (
    <li key={`${list.type}-${index}`}>{renderInline(formatListItemText(item, article), article)}</li>
  ))
  elements.push(
    list.type === 'ol' ? (
      <ol key={`list-${elements.length}`}>{children}</ol>
    ) : (
      <ul key={`list-${elements.length}`}>{children}</ul>
    ),
  )
  return null
}

export function MarkdownArticle({ article }: MarkdownArticleProps) {
  const elements: ReactNode[] = []
  const lines = article.body.split(/\r?\n/)
  let list: ListState | null = null
  let paragraph: string[] = []
  let codeBlock: string[] | null = null

  const flushParagraph = () => {
    if (paragraph.length === 0) {
      return
    }

    elements.push(<p key={`p-${elements.length}`}>{renderInline(paragraph.join(' '), article)}</p>)
    paragraph = []
  }

  for (const line of lines) {
    const trimmed = line.trim()

    if (trimmed.startsWith('```')) {
      if (codeBlock) {
        elements.push(<pre key={`code-${elements.length}`}>{codeBlock.join('\n')}</pre>)
        codeBlock = null
      } else {
        flushParagraph()
        list = flushList(elements, list, article)
        codeBlock = []
      }
      continue
    }

    if (codeBlock) {
      codeBlock.push(line)
      continue
    }

    if (trimmed.length === 0) {
      flushParagraph()
      list = flushList(elements, list, article)
      continue
    }

    const image = parseImageLine(trimmed)
    if (image) {
      flushParagraph()
      list = flushList(elements, list, article)
      elements.push(renderImage(image, elements.length))
      continue
    }

    const heading = trimmed.match(/^(#{2,4})\s+(.+)$/)
    if (heading) {
      flushParagraph()
      list = flushList(elements, list, article)
      const level = heading[1].length
      const label = heading[2]
      if (level === 2) {
        elements.push(<h2 key={`h-${elements.length}`}>{label}</h2>)
      } else {
        elements.push(<h3 key={`h-${elements.length}`}>{label}</h3>)
      }
      continue
    }

    const unordered = trimmed.match(/^-\s+(.+)$/)
    if (unordered) {
      flushParagraph()
      if (list?.type !== 'ul') {
        list = flushList(elements, list, article)
        list = { type: 'ul', items: [] }
      }
      list.items.push(unordered[1])
      continue
    }

    const ordered = trimmed.match(/^\d+\.\s+(.+)$/)
    if (ordered) {
      flushParagraph()
      if (list?.type !== 'ol') {
        list = flushList(elements, list, article)
        list = { type: 'ol', items: [] }
      }
      list.items.push(ordered[1])
      continue
    }

    list = flushList(elements, list, article)
    paragraph.push(trimmed)
  }

  flushParagraph()
  flushList(elements, list, article)

  return <div className="markdown-article">{elements}</div>
}
