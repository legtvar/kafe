import { Node as TocNode } from "lume_markdown_plugins/toc/mod.ts";
import { formatDate } from "../../_plugins/utils.ts";

export const layout = "layouts/default.tsx";

interface ListPageData extends Lume.Data {
  priority?: number;
  toc?: TocNode[];
  entries?: TocNode[];
  showDate: boolean;
  collection: string;
  sortBy?: string;
}

export default function ({
  children,
  search,
  collection,
  sortBy,
}: ListPageData) {
  return (
    <>
      {children}
      <ul class="monospace no-list">
        {search.pages(collection, sortBy ?? "date=desc title=asc").map((p) => (
          <li>
            <a href={p.url} class="no-decoration">
              {formatDate(p.date)} {p.title}
            </a>
          </li>
        ))}
      </ul>
    </>
  );
}
