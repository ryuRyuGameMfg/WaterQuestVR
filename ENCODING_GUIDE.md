# 文字エンコーディングと改行コードの設定ガイド

このプロジェクトは、WindowsとmacOSの両方で問題なく動作するように設定されています。

## 設定内容

### 1. ファイルエンコーディング
- **すべてのテキストファイル**: UTF-8（BOMなし）
- **C#ファイル**: UTF-8（BOMなし）

### 2. 改行コード
- **すべてのテキストファイル**: LF（Unix形式）
- WindowsでもmacOSでもLF形式で統一されています

### 3. 設定ファイル

#### `.gitattributes`
Gitで改行コードを自動的にLFに統一します。
- Windowsでチェックアウトしても、コミット時にLFに変換されます
- macOS/LinuxでもLFのまま維持されます

#### `.editorconfig`
エディタの設定を統一します。
- Visual Studio Code、Rider、Visual Studioなどが自動的に認識します
- 改行コード、インデント、文字エンコーディングを自動設定します

## エディタの設定

### Visual Studio Code
`.editorconfig`ファイルが自動的に認識されます。追加の設定は不要です。

### Visual Studio（Windows）
1. **ツール** → **オプション** → **テキストエディター** → **詳細設定**
2. **エンコード**: UTF-8（BOMなし）を選択
3. **改行コード**: LF（Unix）を選択

### JetBrains Rider
`.editorconfig`ファイルが自動的に認識されます。追加の設定は不要です。

### Unity Editor
Unity Editorは自動的にUTF-8を認識します。特別な設定は不要です。

## トラブルシューティング

### 文字化けが発生した場合
1. ファイルをUTF-8（BOMなし）で保存し直してください
2. 改行コードがLFになっているか確認してください
3. `.editorconfig`が正しく認識されているか確認してください

### Gitで改行コードの警告が出る場合
`.gitattributes`ファイルが正しく設定されていれば、自動的に処理されます。
手動で修正する場合は：
```bash
# すべてのファイルをLFに変換
git add --renormalize .
```

### Windowsで開発する場合
Git for Windowsを使用している場合、`.gitattributes`の設定により自動的にLFに変換されます。
手動で設定する場合：
```bash
git config core.autocrlf input
```

## 確認方法

### ファイルのエンコーディングを確認（macOS/Linux）
```bash
file Assets/Scripts/YourFile.cs
```

### 改行コードを確認（macOS/Linux）
```bash
file Assets/Scripts/YourFile.cs | grep -E "(CRLF|CR|LF)"
```

### すべてのC#ファイルを確認
```bash
find Assets/Scripts -name "*.cs" -exec file {} \;
```

## 注意事項

- **Obiフォルダ内のファイル**: サードパーティライブラリのため、変更しないでください
- **プロジェクトのScriptsフォルダ**: すべてUTF-8（BOMなし）、LF改行コードで統一されています
- **Unityのメタファイル**: 自動生成されるため、手動で変更しないでください

