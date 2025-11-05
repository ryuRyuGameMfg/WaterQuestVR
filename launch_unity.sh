#!/bin/bash
# Unity起動スクリプト（環境変数を設定してからUnity Hubを起動）

# 短いパスの一時ディレクトリを設定
export TMPDIR="$HOME/tmp"

# launchdにも環境変数を設定（システム全体に適用）
launchctl setenv TMPDIR "$HOME/tmp"

echo "環境変数 TMPDIR を設定しました: $TMPDIR"
echo ""
echo "Unity Hubを起動します..."
echo "注意: 既にUnity Hubが起動している場合は、一度終了してから"
echo "このスクリプトを実行してください。"
echo ""

# Unity Hubを起動
open -a "Unity Hub"

echo ""
echo "Unity Hubを起動しました。"
echo "プロジェクトを開く際は、このスクリプトから起動したUnity Hubを使用してください。"
