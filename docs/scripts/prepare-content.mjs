import {mkdir, readFile, writeFile} from 'node:fs/promises';

// Keep the maintainer guide readable on GitHub and in the site from one source.
const source = new URL('../publishing.md', import.meta.url);
const destination = new URL('../content/reference/publishing.md', import.meta.url);
await mkdir(new URL('../content/reference/', import.meta.url), {recursive: true});
const body = await readFile(source, 'utf8');
await writeFile(destination, `---\ntitle: Package publishing\nsidebar_position: 90\n---\n\n${body}`);
