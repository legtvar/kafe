import type { PageData } from "lume/core.ts";

export interface FrontPageData extends PageData {
  styles?: string[];
  math?: boolean;
}

export default ({ title, children }: FrontPageData) => {
  return (
    <html>
      <head>
        <meta charSet="utf-8" />
        <link rel="icon" sizes="32x32" href="/img/favicon-32.png" />
        <link rel="icon" href="/img/favicon.svg" type="img/svg+xml" />
        <link rel="apple-touch-icon" size={256} href="/img/favicon-256.png" />
        <meta name="viewport" content="width=device-width,initial-scale=1.0" />
        <meta name="theme-color" content="#ac162c" />
        <title>{title ? `${title} – Helveg` : "Helveg – An experimental visual alternative to an API reference"}</title>
        <link href="/styles/main.css" rel="stylesheet" />
      </head>

      <body>{children}</body>
    </html>
  );
};
