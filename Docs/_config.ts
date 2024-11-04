import lume from "lume/mod.ts";
import markdown from "lume/plugins/markdown.ts";
import jsx from "lume/plugins/jsx_preact.ts";
import sass from "lume/plugins/sass.ts";
import postcss from "lume/plugins/postcss.ts";
import transformImages from "lume/plugins/transform_images.ts";
import picture from "lume/plugins/picture.ts";
import metas from "lume/plugins/metas.ts";
import resolveUrls from "lume/plugins/resolve_urls.ts";
import sitemap from "lume/plugins/sitemap.ts";
import inline from "lume/plugins/inline.ts";
import markdownToc from "lume_markdown_plugins/toc.ts";
import markdownTitle from "lume_markdown_plugins/title.ts";
import { Node as TocNode } from "lume_markdown_plugins/toc/mod.ts";
import { linkInsideHeader } from "lume_markdown_plugins/toc/anchors.ts";
import pagefind from "lume/plugins/pagefind.ts";
import { default as markdownItAlerts } from "npm:markdown-it-github-alerts";
import markdownDl from "npm:markdown-it-deflist";
import codeHighlight from "./_plugins/shiki.ts";
import basePath from "lume/plugins/base_path.ts";
import { SassString } from "lume/deps/sass.ts";

const site = lume({
    dest: "public/",
    src: ".",
    location: new URL("https://kafe.fi.muni.cz/docs")
});

site.ignore(".vscode", "public", "_plugins", "Docs.Dockerfile")
    .ignore("README.md")
    .use(basePath())
    .use(markdown({
        plugins: [[markdownItAlerts, {
            titles: {
                "tip": "",
                "note": "",
                "important": "",
                "warning": "",
                "caution": ""
            },
            icons: {
                "tip": " ",
                "note": " ",
                "important": " ",
                "warning": " ",
                "caution": " "
            },
            classPrefix: "alert"
        }],
        [markdownDl, {}]]
    }))
    .use(markdownToc({
        tabIndex: false,
        // anchor: false,
        anchor: linkInsideHeader({
            placement: "before"
        })
    }))
    .use(markdownTitle())
    .use(jsx())
    .use(sass({
        options: {
            functions: {
                'base-path()': _ => {
                    const basepath = site.options.location.hostname === "localhost"
                        ? ""
                        : site.options.location.pathname;
                    return new SassString(basepath);
                }
            }
        }
    }))
    .use(postcss())
    .use(metas())
    .use(resolveUrls())
    .use(sitemap({
        query: "indexable=true"
    }))
    .use(inline({
        attribute: "data-inline"
    }))
    .use(picture())
    .use(transformImages({
        extensions: [".png", ".jpg", ".jpeg", ".gif", ".webp"],
        functions: {
            async resizeCrop(img, size) {
                const metadata = await img.metadata();
                let width = -1;
                let height = -1;

                if (typeof (size) == "number") {
                    width = size;
                    height = size;
                }
                else {
                    width = size[0];
                    height = size[1];
                }

                if (metadata.width! < metadata.height!) {
                    img.resize(width, null);
                } else {
                    img.resize(null, height);
                }

                img.extract({
                    width: width,
                    height: height,
                    left: Math.floor((metadata.width! - width) / 2),
                    top: Math.floor((metadata.height! - height) / 2)
                });
            }

        }
    }))
    .use(await codeHighlight())
    .copy([".svg", ".png", ".webm", ".mp4", ".woff2"])
    .copy("fonts")
    .use(pagefind({
        indexing: {
            rootSelector: "main"
        },
        ui: {
            showSubResults: true
        }
    }))
    .preprocess("*", (pages) => {
        for (const page of pages) {
            if (page.data.collection && typeof (page.data.collection) == "string") {
                const entries = pages.filter(p => p.data.tags.includes(page.data.collection));
                page.data.entries = entries
                    .sort((a, b) => a.data.date < b.data.date ? +1
                        : a.data.date > b.data.date ? -1
                            : (a.data.title ?? "").localeCompare(b.data.title ?? ""))
                    .map<TocNode>(p => {
                        return {
                            level: p.data?.toc?.level ?? 0 + 1,
                            text: p.data.title ?? "",
                            url: p.data.url,
                            children: [],
                            slug: ""
                        }
                    });
            }
        }
    });

export default site;
