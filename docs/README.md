# ProTest documentation site

The site uses Docusaurus, following the guide and reference organization of
[Avalonia's documentation](https://github.com/AvaloniaUI/avalonia-docs).
All published content lives under `content/`. The package publishing guide is
generated from `publishing.md` before each build so it has one editable source.

## Local development

Install Node.js 24, then run these commands from this directory:

```sh
npm ci
npm start
```

To validate the production site, including internal links and heading anchors:

```sh
npm run build
npm run serve
```

The production preview is available at `http://localhost:3000/ProTest/`.
Search uses a local index generated during the production build; it does not
require a hosted search account. Use the production preview to test search.

The dependency lock includes targeted overrides for patched versions of
`tinypool`, `serialize-javascript`, `postcss-selector-parser`, and SockJS's
`uuid` dependency while Docusaurus still requests older versions. Recheck
these overrides with `npm audit` and a complete production build when updating
Docusaurus.

## Editing

- Add Markdown pages under `content/` with a title and sidebar position.
- Add `_category_.json` files to group related guides and reference pages.
- Use relative Markdown links between source pages so the build can validate them.
- Keep upstream attribution and license notices when adapting imported content.
- Edit `publishing.md` directly; its copy under `content/reference/` is generated.

The **Documentation** workflow builds pull requests and pushes to `main`.
Successful builds on `main` deploy through the `github-pages` environment to
[the documentation site](https://wieslawsoltes.github.io/ProTest/).
GitHub Pages must use **GitHub Actions** as its build source. Deployment uses
GitHub's short-lived workflow identity and needs no publishing secret.
