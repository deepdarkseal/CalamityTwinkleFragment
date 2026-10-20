// 校验 DSH 预设补丁的 YAML 结构与关键字段；仅用于安装前自检。
const fs = require('fs');
const YAML = require('C:/Users/whitebitingseal/.dsh/profiles/desktop/node_modules/yaml');

const file = process.argv[2];
const raw = fs.readFileSync(file, 'utf8');
// 去掉 Loader 自定义 tag，仅做结构校验
const src = raw.replace(/!!js /g, '');
const doc = YAML.parse(src);

function fail(msg) {
  console.error('FAIL:', msg);
  process.exit(1);
}

if (!Array.isArray(doc)) fail('顶层不是数组');
const entry = doc.find((e) => e && Array.isArray(e.insert) && e.insert.some((r) => r.id === 'preset-twinkle-fragment'));
if (!entry) fail('找不到 preset-twinkle-fragment 的 insert 行');
const row = entry.insert.find((r) => r.id === 'preset-twinkle-fragment');
if (row.name !== '@deepseek-ai/dsh-agent-preset') fail('name 不是 @deepseek-ai/dsh-agent-preset');
const cfg = row.config || {};
if (cfg.id !== 'twinkle-fragment') fail('config.id 不是 twinkle-fragment');
if (typeof cfg.name !== 'string' || cfg.name.length === 0) fail('config.name 缺失');
if (!Array.isArray(cfg.plugins) || cfg.plugins.length < 10) fail('plugins 列表异常');
const persona = cfg.plugins.find((p) => p.id === 'persona');
if (!persona) fail('缺少 persona 行');
const prefix = persona.config && persona.config.prefix;
if (typeof prefix !== 'string') fail('persona.prefix 缺失');
for (const key of ['简体中文', 'docs/lore/00-canon.md', '吉光片羽', 'Calamitytwinklefragment']) {
  if (!prefix.includes(key)) fail('persona 缺少关键词: ' + key);
}
for (const p of cfg.plugins) {
  if (!p.name || typeof p.name !== 'string') fail('插件行缺少 name');
}
console.log('OK: rows=' + cfg.plugins.length + ', persona=' + prefix.length + ' chars, file=' + file);
