import lume from "lume/mod.ts";
import markdown from "lume/plugins/markdown.ts";
import jsx from "lume/plugins/jsx_preact.ts";
import sass from "lume/plugins/sass.ts";
import postcss from "lume/plugins/postcss.ts";
import imagick from "lume/plugins/imagick.ts";
import picture from "lume/plugins/picture.ts";
import metas from "lume/plugins/metas.ts";
import resolveUrls from "lume/plugins/resolve_urls.ts";
import sitemap from "lume/plugins/sitemap.ts";
import inline from "lume/plugins/inline.ts";
import toc from "lume_markdown_plugins/toc.ts";
import { Gravity } from "lume/deps/imagick.ts";
import { linkInsideHeader } from "lume_markdown_plugins/toc/anchors.ts";
import pagefind from "lume/plugins/pagefind.ts";

const site = lume({
    dest: "public/",
    includes: "_includes/",
    src: ".",
    location: new URL("https://helveg.net")
});

site.ignore("old", ".vscode")
    .ignore("README.md")
    .use(markdown())
    .use(toc({
        tabIndex: false,
        // anchor: false,
        anchor: linkInsideHeader({
            placement: "before"
        })
    }))
    .use(jsx())
    .use(sass())
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
    .use(imagick({
        extensions: [".png", ".jpg", ".jpeg", ".gif", ".webp"],
        name: "data-imagick",
        functions: {
            resizeCrop(img, size) {
                if (typeof (size) == "number") {
                    if (img.width < img.height) {
                        img.resize(size, 0);
                    } else {
                        img.resize(0, size);
                    }

                    img.crop(size, size, Gravity.Center);
                } else {
                    img.resize(size[0], size[1]);
                    img.crop(size[0], size[1], Gravity.Center);
                }
            }
        }
    }))
    .copy([".svg", ".png", ".webm", ".mp4"])
    .copy("video")
    .copy("fonts")
    .copy("samples")
    .remoteFile("schema/data.json", "https://gitlab.com/helveg/helveg/-/raw/main/schema/data.json")
    .remoteFile("schema/icon-set.json", "https://gitlab.com/helveg/helveg/-/raw/main/schema/icon-set.json")
    .copy("schema")
    .use(pagefind({
        indexing: {
            rootSelector: "main"
        },
        ui: {
            showSubResults: true
        }
    }));

export default site;
