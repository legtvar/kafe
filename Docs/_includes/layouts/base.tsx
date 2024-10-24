export interface FrontPageData extends Lume.Data {
  styles?: string[];
  math?: boolean;
}

export default ({ title, children }: FrontPageData) => {
  return (
    <html>
      <head>
        <meta charSet="utf-8" />
        <link rel="icon" href="/img/favicon.svg" type="img/svg+xml" />
        <meta name="viewport" content="width=device-width,initial-scale=1.0" />
        <meta name="theme-color" content="#ac162c" />
        <title>{title ? `${title} – KAFE` : "KAFE – Kontrol and Archival of Fantastic Entertainment"}</title>
        <link href="/styles/main.css" rel="stylesheet" />
      </head>

      <body>{children}</body>
    </html>
  );
};
