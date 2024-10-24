import { Node as TocNode } from "lume_markdown_plugins/toc/mod.ts";
import { formatDate } from "../../_plugins/utils.ts";

export const layout = "layouts/base.tsx";

interface DocPageData extends Lume.Data {
  priority?: number;
  toc?: TocNode[];
  entries?: TocNode[];
  showDate: boolean;
}

export default function ({ children, search, date, showDate }: DocPageData) {
  return (
    <>
      <nav>
        <div class="menu-bar">
          <label for="sidebar-toggle">
            <img src="/img/vsc/layout-sidebar-left.svg" data-inline />
          </label>
        </div>
        <input type="checkbox" id="sidebar-toggle" />
        <label id="darkness" for="sidebar-toggle"></label>
        <div class="sidebar">
          <a class="logo" href="/">
            KAFE
          </a>
          <div id="search"></div>
          <ul>
            {search.pages<DocPageData>("doc", "priority=desc").map((p) => (
              <li>
                <a href={p.url.toString()}>{p.title}</a>
                {((p.toc && p.toc.length > 0) ||
                  (p.entries && p.entries.length > 0)) && (
                  <ul>
                    {(p.toc?.length ?? 0 > 0 ? p.toc : p.entries)!.map((n) => (
                      <li>
                        <a href={n.url}>{n.text}</a>
                      </li>
                    ))}
                  </ul>
                )}
              </li>
            ))}
          </ul>
        </div>
      </nav>
      <main>
        {showDate && date && <span>{formatDate(date)}</span>}
        {children}
      </main>
    </>
  );
}
