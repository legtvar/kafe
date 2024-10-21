import { Page, PageData } from "lume/core.ts";
import { Node as TocNode } from "lume_markdown_plugins/toc/mod.ts";

export const layout = "layouts/base.tsx";

interface DocPageData extends PageData {
  priority?: number;
  toc?: TocNode[];
}

export default ({ children, search }: PageData) => (
  <>
    <nav>
      <div class="menu-bar">
          <label for="sidebar-toggle">
            <img src="/img/vsc/layout-sidebar-left.svg" data-inline />
          </label>
      </div>
      <input type="checkbox" id="sidebar-toggle" />
      <label id="darkness" for="sidebar-toggle">
      </label>
      <div class="sidebar">
        <a class="logo" href="/">
          Helveg
        </a>
        <div id="search"></div>
        <ul>
          {(search.pages("doc", "priority=desc") as Page<DocPageData>[]).map(
            (p) => (
              <li>
                <a href={p.data.url.toString()}>{p.data.title}</a>
                {p.data.toc && p.data.toc.length > 0 && (
                  <ul>
                    {p.data.toc.map((n) => (
                      <li>
                        <a href={n.url}>{n.text}</a>
                      </li>
                    ))}
                  </ul>
                )}
              </li>
            )
          )}
        </ul>
      </div>
    </nav>
    <main>{children}</main>
  </>
);
