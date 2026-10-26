import {themes as prismThemes} from 'prism-react-renderer';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'ProTest',
  tagline: 'Headless Avalonia testing for xUnit 3 and 4',
  favicon: 'img/protest.svg',
  url: 'https://wieslawsoltes.github.io',
  baseUrl: '/ProTest/',
  trailingSlash: true,
  organizationName: 'wieslawsoltes',
  projectName: 'ProTest',
  onBrokenLinks: 'throw',
  onBrokenAnchors: 'throw',
  markdown: {hooks: {onBrokenMarkdownLinks: 'throw'}},
  i18n: {defaultLocale: 'en', locales: ['en']},
  presets: [
    ['classic', {
      docs: {
        path: 'content',
        routeBasePath: '/',
        sidebarPath: './sidebars.js',
        editUrl: ({docPath}) => docPath === 'reference/publishing.md'
          ? 'https://github.com/wieslawsoltes/ProTest/edit/main/docs/publishing.md'
          : `https://github.com/wieslawsoltes/ProTest/edit/main/docs/content/${docPath}`,
        showLastUpdateTime: false,
      },
      blog: false,
      theme: {customCss: './src/css/custom.css'},
      sitemap: {changefreq: 'weekly', priority: 0.5},
    }],
  ],
  themes: [
    ['@easyops-cn/docusaurus-search-local', {
      hashed: 'filename',
      language: 'en',
      docsDir: 'content',
      docsRouteBasePath: '/',
      indexBlog: false,
      indexPages: false,
      highlightSearchTermsOnTargetPage: true,
      searchResultLimits: 8,
      searchBarShortcutHint: true,
    }],
  ],
  themeConfig: {
    metadata: [
      {name: 'description', content: 'Test Avalonia applications with xUnit 3 and 4. Learn headless setup, input simulation, gestures, UI tree assertions, automation, and recording with ProTest.'},
      {name: 'theme-color', content: '#155e75'},
    ],
    colorMode: {defaultMode: 'light', respectPrefersColorScheme: true},
    navbar: {
      title: 'ProTest',
      logo: {alt: '', src: 'img/protest.svg', width: 30, height: 30},
      items: [
        {type: 'docSidebar', sidebarId: 'docs', position: 'left', label: 'Documentation'},
        {href: 'https://www.nuget.org/packages?q=ProTest.Avalonia', label: 'NuGet', position: 'right'},
        {href: 'https://github.com/wieslawsoltes/ProTest', label: 'GitHub', position: 'right'},
      ],
    },
    docs: {sidebar: {hideable: true, autoCollapseCategories: false}},
    footer: {
      style: 'dark',
      links: [
        {title: 'ProTest', items: [
          {label: 'Documentation', to: '/'},
          {label: 'Package publishing', to: '/reference/publishing/'},
        ]},
        {title: 'Project', items: [
          {label: 'Source code', href: 'https://github.com/wieslawsoltes/ProTest'},
          {label: 'Issues', href: 'https://github.com/wieslawsoltes/ProTest/issues'},
          {label: 'Wiesław Šoltés', href: 'https://github.com/wieslawsoltes'},
        ]},
        {title: 'Upstream', items: [
          {label: 'Avalonia documentation', href: 'https://docs.avaloniaui.net'},
          {label: 'xUnit documentation', href: 'https://xunit.net'},
          {label: 'PanAndZoom', href: 'https://github.com/wieslawsoltes/PanAndZoom'},
        ]},
      ],
      copyright: 'ProTest · Maintained by Wiesław Šoltés. Built on Avalonia and PanAndZoom. Source attribution and licenses are included in the documentation.',
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'powershell', 'bash', 'xml-doc', 'diff', 'yaml'],
    },
    tableOfContents: {minHeadingLevel: 2, maxHeadingLevel: 3},
  },
};

export default config;
