import re

with open('Assets/Celestial-Cross/Scripts/Giulia_UI/Editor/UIBuilder_InventoryUI.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = re.sub(r'[^;\n]+\.onClick\.AddListener\([^;]+\);', '', text)
text = text.replace('InventoryKind', 'int')

text = re.sub(r'([a-zA-Z0-9_]+GO)\.target\.', r'\1.', text)
text = re.sub(r'([a-zA-Z0-9_]+RT)\.target\.', r'\1.', text)
text = re.sub(r'([a-zA-Z0-9_]+Text)\.target\.', r'\1.', text)
text = re.sub(r'([a-zA-Z0-9_]+TMP)\.target\.', r'\1.', text)
text = re.sub(r'([a-zA-Z0-9_]+Img)\.target\.', r'\1.', text)
text = re.sub(r'gameObject\.target\.', 'gameObject.', text)
text = re.sub(r'img\.target\.', 'img.', text)
text = re.sub(r'tmp\.target\.', 'tmp.', text)
text = re.sub(r'layout\.target\.', 'layout.', text)
text = re.sub(r'mask\.target\.', 'mask.', text)
text = re.sub(r'scroll\.target\.', 'scroll.', text)

text = re.sub(r'Enum\.GetValues\(typeof\(int\)\)\.+', 'new int[]{ 0, 1, 2, 3, 4, 5, 0 };', text)
text = text.replace('TabsBarHeight', '80f')

text = re.sub(r'if \(tabs\[i\] == null\) continue;', '', text)
text = re.sub(r'tabs\[[A-Za-z0-9_]+\]\.', '//', text)

text = text.replace('// Rename tab titles to match the new plan.', '')
text = re.sub(r'if \(tabs\.Length > [^\n]+', '', text)
text = text.replace('if (tabs == null) return;', '')

# Clean the tabCount loop
text = re.sub(r'// Reparent tabs into the bar[^{]*for \(int i = 0; i < tabCount; i\+\)[^{]*{[^}]*}', '', text, flags=re.DOTALL)
text = re.sub(r'\btabs\[([A-Za-z0-9_]+)\]', 'null', text)

with open('Assets/Celestial-Cross/Scripts/Giulia_UI/Editor/UIBuilder_InventoryUI.cs', 'w', encoding='utf-8') as f:
    f.write(text)
