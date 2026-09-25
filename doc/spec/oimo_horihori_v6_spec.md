# OIMO HORIHORI v6.0.0 仕様書

## 1. 文書の目的

本書は、OIMO HORIHORI v6.0.0 の実装仕様をまとめたもの。

本仕様は、2026-09-25 時点の GitHub `main` / v5.1.1 を基準とする。

```text
Repository
mkunori/OimoHorihori

Baseline
v5.1.1
```

v6.0.0 は、新しい転生層を追加するアップデートではなく、
v4〜v5で追加された RETILL / REPLANT / ASCENT / ROOT を
「ひとつのゲームとして自然につながる形」に再整理する大型アップデートとする。

主なテーマ：

```text
進行階層の再整理
+
HORIHORI画面への操作集約
+
RETILL無制限化
+
REPLANT / ASCENT履歴の統合
+
いも図鑑の永久育成コンテンツ化
+
いも発見テンポの改善
```

---

# 2. v5.1.1 現状

主要値：

```text
REPLANT基準
6.000e+10

ASCENT基準
8.000e+10

RETILL上限
10回

畑Lv上限
100 + RETILL回数 × 10

RETILL基本倍率
×1.25

畑価格倍率
×1.005
```

現在は、

```text
REPLANT 6.000e+10
↓
ASCENT  8.000e+10
```

であり、ASCENTがREPLANTの約1.33倍先にしかない。

上位転生として距離が短すぎるため、
v6で進行階層を再設計する。

---

# 3. v6.0.0 実装範囲

v6.0.0では以下を実装する。

1. ASCENT条件の大幅引き上げ
2. REPLANT / ASCENT のHORIHORI画面統合
3. REPLANT / ASCENTを条件達成時のみ表示
4. NEXT GOALの常時表示
5. AUTO BUY / AUTO RETILL操作をHORIHORIへ移動
6. RETILL回数上限撤廃
7. REPLANT履歴 / ASCENT履歴を統計へ統合
8. MENU構成整理
9. ASCENT画面をROOT強化画面へ再定義
10. いも図鑑へ永久バフ「OIMO POWER」追加
11. いも発見判定を5分ごとへ変更
12. オフラインいも発見を「最大1個＋累積確率」方式へ変更
13. SaveVersion更新
14. v5 → v6 セーブ移行
15. 実績・統計の調整

---

# 4. v6のゲーム構造

```text
掘る！
↓
畑1〜8
↓
RETILL
↓
REPLANT
↓
種芋
↓
種芋強化
↓
REPLANT周回
↓
ASCENT
↓
ROOT
↓
ROOT強化
↓
ASCENT周回
```

別軸の永久収集育成：

```text
いも発見
↓
いも図鑑
↓
OIMO POWER購入
↓
永久バフ
```

---

# 5. ASCENT条件再設計

## 5.1 ASCENT基準

REPLANT基準：

```text
6.000e+10
```

基礎種芋獲得式：

```text
BaseSeedGain
=
floor(
    sqrt(
        RunProducedPotato / 6.000e+10
    )
)
```

種芋100個相当：

```text
6.000e+10 × 100²
=
6.000e+14
```

v6 ASCENT TARGET：

```text
6.000e+14
```

---

## 5.2 REPLANTとASCENTの距離

```text
REPLANT
6.000e+10

ASCENT
6.000e+14

距離
×10,000
```

ASCENTは、

```text
REPLANTを何度も行う
↓
種芋強化を育てる
↓
RETILLを深く進める
↓
ASCENT
```

という上位転生として扱う。

---

## 5.3 ASCENT条件は固定

ASCENT回数に関係なく、

```text
6.000e+14
```

で固定。

ROOT強化によりASCENT周回そのものが徐々に高速化する。

---

# 6. HORIHORI画面の役割

HORIHORIを、

```text
実際にゲームを進行させる場所
```

として統一する。

HORIHORIで行う：

```text
畑購入
RETILL
AUTO BUY ON/OFF
AUTO RETILL ON/OFF
REPLANT
ASCENT
```

MENUは、

```text
強化
収集
記録
プロフィール
設定
```

を扱う。

---

# 7. HORIHORI画面レイアウト

推奨：

```text
現在芋
芋/sec
TOTAL

NEXT GOAL

SAVE STATUS

[ +1 ] [ +10 ] [ MAX ]

AUTO
[ BUY ON ] [ RETILL ON ]
※解放済みだけ表示

RETILL READY
※対象がある場合のみ

畑1
畑2
畑3
畑4
畑5
畑6
畑7
畑8

[ REPLANT ]
※条件達成時のみ

[ ASCENT ]
※条件達成時のみ
```

スマートフォン縦画面優先。

---

# 8. REPLANT表示ルール

```text
Game.CanReplant == true
```

の場合のみHORIHORI下部に表示。

条件未達：

```text
非表示
```

進捗はNEXT GOALで案内。

---

# 9. ASCENT表示ルール

現在のHORIHORI上部 `ASCENT READY` バナーは廃止。

```text
Game.CanAscent == true
```

の場合のみHORIHORI下部に表示。

条件未達：

```text
非表示
```

---

# 10. REPLANT / ASCENT同時表示

ASCENT条件達成時はREPLANT条件も満たしているため、

```text
REPLANT
種芋 +XXX

ASCENT
ROOT +1
```

を両方表示する。

プレイヤーが選択する。

ASCENT実行時にREPLANTを自動実行しない。

---

# 11. NEXT GOAL

ASCENT後も含めて常時表示。

優先例：

```text
1. 畑8未購入
   → 畑8を購入する

2. REPLANT未経験
   → REPLANTまであと○芋

3. ASCENT未経験
   → ASCENTまであと○芋

4. ASCENT経験後
   → 次のASCENTまであと○芋
```

条件達成時：

```text
REPLANT可能
ASCENT可能
```

など短く表示してよい。

---

# 12. AUTO BUY / AUTO RETILL UI

## 12.1 HORIHORIへ移動

配置：

```text
[ +1 ] [ +10 ] [ MAX ]

AUTO
[ BUY ON ] [ RETILL ON ]
```

未解放能力は表示しない。

---

## 12.2 ROOT強化画面

未解放：

```text
AUTO BUY
ROOT 1で解放
```

解放済み：

```text
AUTO BUY
UNLOCKED
HORIHORI画面で設定できます
```

ON/OFF操作は置かない。

AUTO RETILLも同様。

---

# 13. RETILL上限撤廃

## 13.1 基本仕様

現在：

```text
最大10回
```

v6：

```text
上限なし
```

---

## 13.2 Lv上限

```text
MaxLevel
=
100 + RetillCount × 10
```

例：

| RETILL | Lv上限 |
|---:|---:|
| 0 | 100 |
| 10 | 200 |
| 25 | 350 |
| 50 | 600 |
| 100 | 1100 |

---

## 13.3 RETILL倍率

```text
RootAdjustedRetillBase
=
1.25
+ RootRetillLevel × 0.015

RetillMultiplier
=
RootAdjustedRetillBase ^ RetillCount
```

---

## 13.4 購入コスト

RETILL後もPurchaseCountを維持。

```text
Cost
=
BaseCost
× 1.005 ^ PurchaseCount
× 各種コスト軽減
```

無限RETILLでも、

```text
必要Lv増加
+
購入回数増加
+
コスト増加
```

により自然なソフトキャップがかかる。

---

## 13.5 REPLANT時

全RETILL進行はリセット。

```text
RETILL 0
Lv 0
PurchaseCount 0
```

---

# 14. RETILL実装変更

削除・変更：

```text
GameConstants.RetillMaxCount
Farm.IsRetillMax
RETILL MAX表示
RETILL n/10表示
RetillCount <= 10 のSave検証
AUTO RETILLの最大10回停止
```

表示：

```text
RETILL 17
```

のように回数のみ。

---

# 15. RETILL技術ガード

ゲーム上の上限は設けない。

ただし、

```text
double.IsFinite
```

等で、

- 生産倍率
- 生産量
- コスト

の非有限値を防御する。

---

# 16. MENU再構成

```text
MENU

種芋強化
ROOT強化
いも図鑑
実績
統計
プロフィール
ランキング
共有
設定
アカウント
```

削除：

```text
REPLANT履歴
ASCENT
```

---

# 17. ROOT強化画面

現在のASCENT画面を、

```text
ROOT強化
```

へ再構成する。

表示：

```text
現在ROOT
ROOT POWER

豊穣
肥沃
再耕
種の祝福

AUTO BUY
AUTO RETILL
```

ASCENT実行・ASCENT条件・ASCENT HISTORYは表示しない。

---

# 18. 統計画面

推奨セクション：

```text
芋
REPLANT
ASCENT
RETILL
畑
OIMO POWER
OFFLINE
PLAY
転生履歴
```

---

# 19. REPLANT / ASCENT履歴統合

MENUからREPLANT履歴を削除。

ROOT強化画面からASCENT HISTORYを削除。

統計画面最下部：

```text
転生履歴

[ REPLANT ] [ ASCENT ]
```

のタブ方式を推奨。

既存の直近10件データをそのまま利用する。

---

# 20. いも発見システム変更

## 20.1 オンライン判定間隔

現在の短い判定間隔を廃止。

v6では、

```text
5分ごと
```

に発見判定する。

```text
OimoDiscoveryIntervalSeconds
= 300
```

---

## 20.2 1回あたり発見確率

従来の「約24時間で1個程度」という発見ペースを大きく崩さないため、

```text
0.35%
```

を初期値とする。

```text
OimoDiscoveryChance
= 0.0035
```

5分ごとに0.35%判定すると、
期待値はおおむね1日1個程度。

実プレイを見てv6.xで調整可能。

---

## 20.3 オンライン発見

5分経過するたびに1回判定。

成功：

```text
20種類からランダムで1種類獲得
```

重複可。

オンライン中は複数回成功すれば複数個獲得可能。

従来どおり小通知を表示。

---

# 21. オフラインいも発見

## 21.1 基本方針

オフライン中は、

```text
最大1個だけ
```

獲得可能とする。

5分ごとに個別に芋を抽選して複数個付与する方式は廃止。

---

## 21.2 オフライン判定回数

実際の放置時間を5分単位へ変換。

```text
trialCount
=
floor(
    cappedOfflineSeconds / 300
)
```

図鑑用オフライン上限は従来どおり、

```text
24時間
```

を維持。

最大：

```text
24h × 60 / 5
= 288回
```

---

## 21.3 累積発見確率

1回の基礎確率：

```text
p = 0.0035
```

N回相当の累積確率：

```text
P
=
1 - (1 - p)^N
```

とする。

これは、

```text
5分ごとの判定をN回行ったとき
1回以上成功する確率
```

と同等。

ただし獲得数は最大1個。

---

## 21.4 例

30分放置：

```text
N = 6
P ≒ 2.08%
```

1時間：

```text
N = 12
P ≒ 4.12%
```

8時間：

```text
N = 96
P ≒ 28.6%
```

24時間：

```text
N = 288
P ≒ 63.5%
```

※概算。

---

## 21.5 オフライン復帰時処理

```text
N計算
↓
累積確率P計算
↓
1回だけ乱数判定
↓
成功ならランダムな芋1個
↓
失敗なら0個
```

最大獲得数：

```text
1
```

---

## 21.6 端数時間

5分未満の端数は次回へ持ち越せるよう、

```text
OimoDiscoveryElapsedSeconds
```

を利用してよい。

オンライン・オフラインで同じ経過時間カウンタを安全に共有する。

---

# 22. オフライン復帰表示

成功時：

```text
いも図鑑

NEW!
オイモザウルス
```

または重複：

```text
いも図鑑

ダンシャクイモ
×1
```

失敗時：

```text
図鑑項目を表示しない
```

従来どおり。

---

# 23. いも図鑑永久育成「OIMO POWER」

## 23.1 解放条件

```text
ASCENT 1回以上
```

でOIMO POWER機能を解放。

---

## 23.2 OIMO POWER

発見済みの芋へ大量の現在芋を消費し、
一度きりの永久バフを取得する。

維持：

```text
REPLANT
ASCENT
```

の両方をまたぐ。

---

## 23.3 購入条件

```text
ASCENT経験済み
対象芋発見済み
OIMO POWER未解放
必要な現在芋を所持
```

---

# 24. OIMO POWER一覧

## 通常15種

| # | 芋 | コスト | 永久効果 |
|---:|---|---:|---|
| 1 | ダンシャクイモ | `1.000e+10` | 全生産 ×1.05 |
| 2 | メークインヌ | `2.000e+10` | 全畑生産 ×1.08 |
| 3 | キタアカリィ | `4.000e+10` | 種芋獲得 ×1.05 |
| 4 | トヨシロウ | `8.000e+10` | 畑価格 ×0.99 |
| 5 | インカノメザメ | `1.500e+11` | RETILL最終生産 ×1.08 |
| 6 | ベニアズマァ | `2.500e+11` | 全生産 ×1.05 |
| 7 | ベニハルカナ | `4.000e+11` | 全畑生産 ×1.08 |
| 8 | シルクスイートォ | `7.000e+11` | 種芋獲得 ×1.05 |
| 9 | アンノウイモン | `1.000e+12` | 畑価格 ×0.99 |
| 10 | ムラサキマサリーヌ | `2.000e+12` | RETILL最終生産 ×1.08 |
| 11 | コガネセンガンヌ | `4.000e+12` | 全生産 ×1.05 |
| 12 | ナルットキントキ | `8.000e+12` | 全畑生産 ×1.08 |
| 13 | サトイモォ | `1.500e+13` | 種芋獲得 ×1.05 |
| 14 | ヤマノイーモ | `3.000e+13` | 畑価格 ×0.99 |
| 15 | ナガイモーン | `6.000e+13` | RETILL最終生産 ×1.08 |

## ネタ5種

| # | 芋 | コスト | 永久効果 |
|---:|---|---:|---|
| 16 | 黄金いも | `1.000e+14` | 全生産 ×1.25 |
| 17 | 透明いも | `1.500e+14` | 畑価格 ×0.90 |
| 18 | 逆に土 | `2.500e+14` | RETILL基礎倍率 +0.01 |
| 19 | 芋ではない何か | `4.000e+14` | 種芋獲得 ×1.25 |
| 20 | オイモザウルス | `6.000e+14` | 全生産 ×2.00 |

---

# 25. OIMO POWER計算

## 全生産

ROOT豊穣と乗算。

## 全畑生産

畑1〜8の生産のみへ乗算。

## 種芋

```text
FinalSeedGain
=
floor(
    BaseSeedGain
    × RootSeedBlessingMultiplier
    × OimoSeedMultiplier
)
```

## 畑価格

```text
FinalCost
=
ceil(
    RawCost
    × SeedFieldCostMultiplier
    × RootFertilityMultiplier
    × OimoFieldCostMultiplier
)
```

## RETILL基礎倍率

逆に土購入時：

```text
RetillBase
=
1.25
+ RootRetillLevel × 0.015
+ 0.01
```

---

# 26. OIMO POWER UI

発見済み・未購入：

```text
オイモザウルス
×3

OIMO POWER
全生産 ×2.00

必要
6.000e+14 芋

[ 解放 ]
```

購入済み：

```text
OIMO POWER
ACTIVE

全生産 ×2.00
```

---

# 27. OIMO POWER購入確認

```text
オイモザウルスの
OIMO POWERを解放しますか？

消費
6.000e+14 芋

永久効果
全生産 ×2.00

REPLANT / ASCENT後も維持されます。

[ CANCEL ]
[ 解放 ]
```

---

# 28. OIMO POWER統計

統計へ追加候補：

```text
解放数 / 20
累計消費芋
全生産倍率
種芋倍率
畑価格倍率
```

---

# 29. SaveVersion

```text
v5
SaveVersion = 5

v6
SaveVersion = 6
```

---

# 30. v5 → v6 移行

維持：

```text
現在芋
総生産
周回生産
畑
PurchaseCount
RETILL
種芋
種芋強化
REPLANT
ROOT
ROOT強化
ASCENT
AUTO解放
図鑑発見
実績
履歴
アカウント
ランキング
```

新規：

```text
OIMO POWER
全20種未解放
```

既存ASCENT回数・ROOTは巻き戻さない。

---

# 31. RETILL移行

v5でRETILL 10だった畑もそのまま読み込み、

```text
RETILL 11
```

へ進行可能にする。

---

# 32. ASCENT基準変更時

既存v5で獲得済みの、

```text
ASCENT回数
ROOT
ROOT強化
```

は維持。

v6更新後の次回ASCENTから、

```text
6.000e+14
```

を適用。

---

# 33. OIMO POWER保存

推奨：

```text
List<string> UnlockedOimoPowerIds
```

species ID：

```text
oimo_01
...
oimo_20
```

を保存。

GameState内部ではHashSet化してよい。

---

# 34. いも発見セーブ互換

既存：

```text
OimoDiscoveryElapsedSeconds
OimoDiscoveryCounts
```

は維持。

新しい5分判定へそのまま利用する。

v5以前の端数秒も破棄せず、
v6ロード後の次の5分判定へ持ち越す。

---

# 35. サーバーセーブ

SaveVersion 6へ対応。

OIMO POWER購入は永久変更なので、
購入成功後は即時保存対象。

既存Revision競合制御を維持。

---

# 36. セーブ検証

RETILL：

```text
RetillCount >= 0
```

のみ。

Lv：

```text
0 <= Level <= 100 + RetillCount × 10
```

OIMO POWER：

```text
存在するspecies IDのみ
重複IDなし
```

---

# 37. 実績調整

既存RETILL10回実績は維持。

OIMO POWER候補：

```text
芋に力を
OIMO POWER初解放

育つ図鑑
5 / 20

芋の加護
10 / 20

OIMO POWER MASTER
20 / 20
```

報酬なし。

---

# 38. v6で見送るもの

```text
AUTO REPLANT
AUTO ASCENT
OIMO POWER多段Lv
OIMO重複による追加効果
OIMO POWER振り直し
新しい芋品種
畑9以降
第3転生
ROOT上限拡張
ASCENT条件の回数依存
```

---

# 39. 実装優先順位

## Phase 1

```text
ASCENT
8.000e+10
→
6.000e+14
```

## Phase 2

RETILL無制限化。

## Phase 3

HORIHORIへ、

```text
AUTO
REPLANT
ASCENT
```

を整理。

## Phase 4

MENU / ROOT / 統計 / 履歴を整理。

## Phase 5

いも発見を、

```text
5分ごと
0.35%
```

へ変更。

オフライン：

```text
最大1個
累積確率
```

へ変更。

## Phase 6

OIMO POWER実装。

## Phase 7

SaveVersion 6 / サーバー移行。

## Phase 8

バランス・モバイル確認。

---

# 40. v6.0.0 完了条件

```text
ASCENT基準 6.000e+14

REPLANT条件未達
HORIHORI非表示

ASCENT条件未達
HORIHORI非表示

REPLANT / ASCENT実行がHORIHORI

AUTO操作がHORIHORI

NEXT GOAL常時表示

RETILL上限なし

RETILL >10 セーブ可能

REPLANT / ASCENT履歴が統計へ統合

MENUからREPLANT履歴削除

ASCENTメニューをROOT強化へ変更

いも発見オンライン5分ごと

いも発見確率0.35% / 5分

オフライン発見最大1個

オフライン確率
1 - (1 - p)^N

オフライン図鑑判定上限24時間

OIMO POWER ASCENT1回後解放

20種類すべて永久バフ

OIMO POWERはREPLANT / ASCENTで維持

SaveVersion 6

v5セーブ移行可能

サーバーセーブ正常

スマホ操作可能

NaN / Infinity等の重大不具合なし
```

---

# 41. 数値まとめ

```text
REPLANT BASE
6.000e+10

ASCENT TARGET
6.000e+14

ASCENT / REPLANT DISTANCE
×10,000

RETILL MAX
なし

INITIAL LEVEL CAP
100

RETILL CAP BONUS
+10

RETILL BASE
×1.25

FARM COST SCALE
×1.005

OIMO DISCOVERY INTERVAL
300 sec

OIMO DISCOVERY CHANCE
0.0035
= 0.35%

OIMO OFFLINE LIMIT
24h

OIMO OFFLINE MAX REWARD
1

OIMO OFFLINE CHANCE
1 - (1 - 0.0035)^N

N
floor(cappedSeconds / 300)

OIMO POWER
20種類
永久
ASCENT 1回後解放
```

---

# 42. v6の画面思想

## HORIHORI

```text
操作する場所
```

- 畑購入
- RETILL
- AUTO
- REPLANT
- ASCENT

## MENU

```text
育成・収集・記録
```

- 種芋強化
- ROOT強化
- いも図鑑
- 実績
- 統計
- プロフィール
- ランキング
- 設定

---

# 43. v6の狙い

新しい転生階層を増やすのではなく、

```text
RETILL
REPLANT
ASCENT
ROOT
AUTO
いも図鑑
```

を一本の長期ゲームループとしてつなげる。

理想：

```text
序盤
畑を増やす

↓
RETILL

↓
REPLANT
種芋強化

↓
さらに深いRETILLとREPLANT

↓
ASCENT

↓
ROOT / AUTO

↓
OIMO POWER

↓
以前より速くASCENT

↓
ASCENT周回
```

v6.0.0を、
OIMO HORIHORIの長期プレイ基盤を完成させるバージョンとする。

---

# 44. 実装チャットへの引き継ぎメモ

実装開始時は最新 `main` を確認すること。

v5.1.1時点では、

```text
ReplantBaseProduction = 6.000e10
AscentTargetProduction = 8.000e10
RetillMaxCount = 10
```

v6では、

```text
AscentTargetProduction = 6.000e14
RetillMaxCount = 廃止

OimoDiscoveryIntervalSeconds = 300
OimoDiscoveryChance = 0.0035
```

へ変更。

オフライン図鑑処理は、
従来の複数回抽選・複数獲得ではなく、

```text
trialCount算出
↓
累積確率算出
↓
1回抽選
↓
最大1個
```

とする。

推奨責務：

```text
HorihoriView
ゲーム操作

RootUpgradesView
ROOT強化 / AUTO解放

StatisticsView
統計 / 転生履歴

OimoBookView
図鑑 / OIMO POWER

GameState
ゲームルール
```

判定はUIへ直書きせず、

```text
CanReplant
CanAscent
CanRetill
CanUnlockOimoPower
```

などGameState側へ集約する。
