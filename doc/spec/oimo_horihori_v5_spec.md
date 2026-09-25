# OIMO HORIHORI v5.0.0 仕様書（v4.2.1準拠・修正版）

## 1. 文書の目的

本書は、OIMO HORIHORI v5.0.0 の実装仕様をまとめたもの。

本仕様は、2026-09-21 時点の現行リポジトリ `main` / v4.2.1 を基準とする。

基準実装：

```text
Repository:
mkunori/OimoHorihori

Baseline:
v4.2.1
```

v5では、v4までの

```text
芋
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
```

という成長層のさらに上に、新しい上位転生システム

```text
ASCENT
```

を追加する。

ASCENTでは、REPLANTによって積み上げた種芋・種芋強化などをリセットする代わりに、
最上位の永続通貨 `ROOT` を獲得し、非常に強力な永久強化を選択できる。

v5.0.0では、ASCENTそのものとROOT強化、自動購入・自動RETILL、ASCENT記録/UIを完成させることを主目的とする。

---

# 2. 現行v4.2.1の基準値

v5は、以下の現行値を前提として設計する。

## 2.1 畑

| 畑 | 初期コスト | 1Lvあたり基本生産 |
|---|---:|---:|
| 畑1 | `1.000e+1` | `1.000e-1 /sec` |
| 畑2 | `1.500e+2` | `1.000e+0 /sec` |
| 畑3 | `3.000e+3` | `1.000e+1 /sec` |
| 畑4 | `4.500e+4` | `1.000e+2 /sec` |
| 畑5 | `6.000e+5` | `1.000e+3 /sec` |
| 畑6 | `1.300e+7` | `1.000e+4 /sec` |
| 畑7 | `2.000e+8` | `1.000e+5 /sec` |
| 畑8 | `1.100e+10` | `1.000e+6 /sec` |

畑レベル価格倍率：

```text
×1.005
```

---

## 2.2 RETILL

現行仕様：

```text
初期Lv上限
100

RETILL 1回
Lv → 0
Lv上限 +10
生産倍率 ×1.25

最大RETILL回数
10

最大Lv上限
200
```

畑ごとの基本RETILL倍率：

```text
1.25 ^ RetillCount
```

---

## 2.3 REPLANT

現行REPLANT基準：

```text
9.000e+10
```

種芋獲得式：

```text
BaseSeedGain
= floor(
    sqrt(
        RunProducedPotato / 9.000e+10
    )
)
```

代表値：

| 種芋 | 必要周回生産 |
|---:|---:|
| 1 | `9.000e+10` |
| 2 | `3.600e+11` |
| 5 | `2.250e+12` |
| 10 | `9.000e+12` |
| 50 | `2.250e+14` |
| 100 | `9.000e+14` |

---

## 2.4 現行種芋強化

v4.2.1では以下の5種類。

```text
1. 生産力強化
2. 掘り出し強化
3. 放置強化
4. RETILL効率
5. 畑コスト軽減
```

主要値：

```text
生産力強化
+2% / Lv

掘り出し強化
+0.5 芋/sec / Lv

放置強化
+1時間 / Lv
最大24時間

RETILL効率
1 RETILL × 1Lv ごとに +0.5%

畑コスト軽減
-0.25% / Lv
最大Lv.40
最大 -10%
```

RETILL効率の現行計算：

```text
SeedRetillEfficiencyMultiplier
= 1
+ RetillCount
× RetillEfficiencyUpgradeLevel
× 0.005
```

---

# 3. v5の基本コンセプト

v5ではゲーム全体を以下の3層へ整理する。

```text
第1層
畑育成
↓
RETILL

第2層
REPLANT
↓
種芋
↓
種芋強化

第3層
ASCENT
↓
ROOT
↓
強力な永久強化
```

ASCENTでは第1層・第2層の進行を大きくリセットする。

一方、

```text
ROOT
ROOT強化
ASCENT回数
実績
図鑑
長期統計
```

などは維持する。

ROOT強化によってASCENT到達速度が徐々に上がり、
最終的にはASCENT周回そのものを長期コンテンツとする。

---

# 4. v5.0.0 実装範囲

v5.0.0では以下を実装する。

1. ASCENTシステム
2. ASCENT条件
3. ROOT通貨
4. ROOT強化4種類
5. AUTO BUY解放
6. AUTO RETILL解放
7. ASCENT専用画面
8. ASCENT READY表示
9. ASCENT確認
10. ASCENT結果カード
11. ASCENT履歴
12. ASCENT統計
13. ASCENT関連実績・称号
14. セーブデータ拡張
15. v4 → v5 セーブ移行

以下はv5.0.0では実装しない。

```text
AUTO REPLANT
AUTO ASCENT
ROOT振り直し
ASCENTランキング
ASCENTタイムランキング
ASCENTチャレンジ
ASCENT100回以降の新能力
さらに上位の転生
```

---

# 5. ASCENT

## 5.1 概要

ASCENTはREPLANTより上位の転生システム。

現在周回生産が非常に大きくなったときに実行できる。

ASCENT実行時：

```text
ROOT +1
```

ROOTはASCENTしても失われない。

---

## 5.2 ASCENT条件

ASCENT条件：

```text
現在周回生産 >= 9.000e+14
```

とする。

これは現行REPLANT式で、

```text
floor(
    sqrt(
        9.000e+14 / 9.000e+10
    )
)

= floor(sqrt(10000))
= 100
```

となるため、

```text
REPLANTした場合に種芋100個を得られる地点
```

に相当する。

したがってASCENT READY時には、

```text
REPLANTして種芋100個以上を得る
```

か、

```text
ASCENTしてROOT 1個を得る
```

かを選択できる。

---

## 5.3 ASCENT条件は固定

ASCENT条件は何回実行しても増加しない。

```text
ASCENT 0 → 1
9.000e+14

ASCENT 1 → 2
9.000e+14

ASCENT 49 → 50
9.000e+14

ASCENT 99 → 100
9.000e+14
```

ROOT強化によってASCENT周回が徐々に高速化することを狙う。

---

# 6. ASCENTでリセットするもの

ASCENT実行時、現在のREPLANT層までを大きく初期化する。

## 6.1 現在周回

- 現在芋
- 現在周回生産
- HasStarted
- RunStartedAt
- 畑1〜8の現在Lv
- 畑1〜8のPurchaseCount
- 畑1〜8のRETILL回数

各畑は、

```text
Lv 0
PurchaseCount 0
RETILL 0
Lv上限 100
RETILL倍率 ×1.000
```

へ戻る。

---

## 6.2 REPLANT層

以下もリセットする。

- 現在保有種芋
- 生産力強化Lv
- 掘り出し強化Lv
- 放置強化Lv
- RETILL効率強化Lv
- 畑コスト軽減Lv
- 今回ASCENT中のREPLANT回数

つまり種芋強化5系統はすべてLv.0へ戻る。

---

# 7. ASCENTで維持するもの

以下はASCENT後も維持する。

- ROOT
- ROOT累計獲得数
- ROOT強化
- AUTO BUY解放状態
- AUTO RETILL解放状態
- ASCENT累計回数
- lifetime total production
- lifetime REPLANT count
- lifetime seed potato earned
- lifetime seed potato spent
- 実績
- 称号
- 図鑑
- プロフィール
- アカウント
- ランキング関連長期記録
- REPLANT履歴
- ASCENT履歴
- 各種長期統計
- 設定

重要：

```text
生涯REPLANT回数
```

はリセットしない。

一方、

```text
今回ASCENT中のREPLANT回数
```

は0へ戻す。

---

# 8. ROOT

## 8.1 基本仕様

ASCENT 1回につき、

```text
ROOT +1
```

ROOTは最上位永続通貨。

ASCENTしても失われない。

---

## 8.2 ROOT完全解放

v5では、

```text
ASCENT 100回
```

でROOT強化をすべて完成できるようにする。

総必要ROOT：

```text
100
```

---

## 8.3 ROOT配分

| 強化 | 最大Lv / 解放 | 必要ROOT |
|---|---:|---:|
| 豊穣 | Lv.30 | 30 |
| 肥沃 | Lv.20 | 20 |
| 再耕 | Lv.24 | 24 |
| 種の祝福 | Lv.24 | 24 |
| AUTO BUY | 1回 | 1 |
| AUTO RETILL | 1回 | 1 |
| **合計** |  | **100** |

すべて、

```text
1Lv = ROOT 1
```

または、

```text
能力解放 = ROOT 1
```

とする。

---

# 9. ROOT強化：豊穣

## 9.1 効果

全最終生産量を乗算する。

```text
1Lvごとに ×1.25
```

最大Lv：

```text
30
```

---

## 9.2 計算

```text
RootAbundanceMultiplier
= 1.25 ^ RootAbundanceLevel
```

最大：

```text
1.25 ^ 30
≈ 807.8
```

最終的に、

```text
全生産 約×807.8
```

---

## 9.3 適用位置

ROOT豊穣は最終的な全生産倍率として適用する。

概念：

```text
BaseProduction
+
全畑生産
↓
種芋 生産力強化
↓
ROOT 豊穣
```

例：

```text
FinalProductionPerSecond
=
ProductionBeforeRoot
× RootAbundanceMultiplier
```

---

# 10. ROOT強化：肥沃

## 10.1 効果

全畑購入コストを軽減。

```text
-2.5% / Lv
```

最大Lv：

```text
20
```

最大：

```text
-50%
```

---

## 10.2 計算

```text
RootFertilityMultiplier
= 1.0 - 0.025 × RootFertilityLevel
```

最大時：

```text
0.50
```

---

## 10.3 種芋「畑コスト軽減」との関係

現行種芋強化：

```text
SeedFieldCostMultiplier
= 1.0 - min(
    FieldCostReductionUpgradeLevel × 0.0025,
    0.10
)
```

ROOT肥沃と乗算する。

```text
RawCost
= BaseCost
× 1.005 ^ PurchaseCount

FinalCost
= ceil(
    RawCost
    × SeedFieldCostMultiplier
    × RootFertilityMultiplier
)
```

最大時の理論軽減：

```text
種芋強化MAX ×0.90
ROOT肥沃MAX ×0.50

合成
×0.45
```

つまり通常価格の45%。

---

# 11. ROOT強化：再耕

## 11.1 効果

RETILLそのものの基礎倍率を強化する。

現行：

```text
RETILL基礎倍率
1.25
```

ROOT再耕：

```text
+0.015 / Lv
```

最大Lv：

```text
24
```

---

## 11.2 計算

```text
RootAdjustedRetillBase
=
1.25
+ 0.015 × RootRetillLevel
```

最大：

```text
1.25 + 0.015 × 24
= 1.61
```

各畑：

```text
BaseRetillMultiplier
=
RootAdjustedRetillBase ^ RetillCount
```

---

## 11.3 種芋「RETILL効率」との関係

現行種芋強化のRETILL効率は別倍率として維持する。

```text
SeedRetillEfficiencyMultiplier
=
1
+ RetillCount
× RetillEfficiencyUpgradeLevel
× 0.005
```

畑生産：

```text
FarmProduction
=
ProductionPerLevel
× Level
× BaseRetillMultiplier
× SeedRetillEfficiencyMultiplier
```

ROOT再耕は、

```text
1.25 ^ RetillCount
```

部分そのものを強化する。

種芋RETILL効率とは役割が異なる。

---

## 11.4 最大時

通常RETILL 10回：

```text
1.25 ^ 10
≈ 9.313
```

ROOT再耕MAX：

```text
1.61 ^ 10
≈ 117
```

さらに種芋RETILL効率強化が別途乗る。

---

# 12. ROOT強化：種の祝福

## 12.1 効果

REPLANT時の種芋獲得数を乗算強化。

```text
×1.12 / Lv
```

最大Lv：

```text
24
```

---

## 12.2 基礎種芋

```text
BaseSeedGain
=
floor(
    sqrt(
        RunProducedPotato / 9.000e+10
    )
)
```

---

## 12.3 ROOT倍率

```text
RootSeedBlessingMultiplier
=
1.12 ^ RootSeedBlessingLevel
```

最終種芋：

```text
FinalSeedGain
=
floor(
    BaseSeedGain
    × RootSeedBlessingMultiplier
)
```

---

## 12.4 最大時

```text
1.12 ^ 24
≈ 15.18
```

ASCENT条件 `9.000e+14` でREPLANTした場合、

```text
BaseSeedGain
100
```

ROOT種の祝福MAXなら、

```text
floor(100 × 15.18)
≈ 1518
```

となる。

ASCENT後に消失した種芋強化を高速で再構築する能力として位置付ける。

---

# 13. ROOT能力：AUTO BUY

## 13.1 解放

必要：

```text
ROOT 1
```

一度解放すれば永久。

ASCENT後も失われない。

---

## 13.2 UI

```text
AUTO BUY
[ OFF / ON ]
```

ON/OFF状態を保存する。

---

## 13.3 v5.0.0の購入ルール

複雑な設定は入れない。

基本：

```text
購入可能な解放済み最上位畑を優先
```

例：

```text
畑8 購入可能
→ 畑8 +1

畑8 不可
畑7 購入可能
→ 畑7 +1
```

Lv上限の畑は購入対象から除外。

---

## 13.4 購入単位

v5.0.0では、

```text
1回のAUTO BUY処理につき +1Lv
```

を基本とする。

AUTO BUY処理間隔は、
既存ゲームループへ過剰な負荷をかけない値とする。

必要であれば一定秒数ごとに処理する。

---

## 13.5 将来へ回す設定

v5.0.0では以下を実装しない。

```text
畑別ON/OFF
優先順位変更
MAX購入
+10購入
資金キープ
次畑購入用貯金
バランス購入
```

---

# 14. ROOT能力：AUTO RETILL

## 14.1 解放

必要：

```text
ROOT 1
```

永久解放。

---

## 14.2 UI

```text
AUTO RETILL
[ OFF / ON ]
```

状態を保存する。

---

## 14.3 動作

ONの場合：

```text
畑が現在Lv上限へ到達
↓
RETILL可能
↓
自動RETILL
```

最大RETILL：

```text
10回
```

最大到達後はRETILLしない。

```text
RETILL 10
Lv上限 200
```

でLv.200まで育成して終了。

---

# 15. AUTO BUY + AUTO RETILL

両方ONの場合：

```text
AUTO BUY
↓
Lv上限
↓
AUTO RETILL
↓
Lv.0
↓
AUTO BUY再開
↓
...
```

という畑育成ループが自動化される。

ただし、

```text
REPLANT
ASCENT
ROOT配分
```

は手動判断として残す。

---

# 16. AUTO REPLANT / AUTO ASCENT

v5.0.0では実装しない。

AUTO BUY / AUTO RETILLだけを自動化し、

```text
いつREPLANTするか
いつASCENTするか
```

はプレイヤー判断として維持する。

ASCENT周回が十分高速化した後の将来バージョンで検討する。

---

# 17. ASCENT専用画面

MENUへ、

```text
ASCENT
```

を追加する。

例：

```text
ASCENT

CURRENT RUN
6.381e+14

TARGET
9.000e+14

ASCENT COUNT
12

ROOT
3

ROOT POWER
37 / 100

あと
2.619e+14 芋
```

条件達成後：

```text
ASCENT READY

REPLANT IF NOW
SEED POTATO +100 以上

[ ASCENT ]
```

ROOT「種の祝福」がある場合、
REPLANT IF NOWにはROOT倍率適用後の実獲得値を表示してよい。

---

# 18. ROOT強化画面

例：

```text
ROOT POWER
37 / 100

ROOT
4

豊穣
Lv.12 / 30
現在 ×14.55
次   ×18.19
[ ROOT 1 ]

肥沃
Lv.5 / 20
現在 -12.5%
次   -15.0%
[ ROOT 1 ]

再耕
Lv.8 / 24
RETILL基礎倍率
現在 ×1.37
次   ×1.385
[ ROOT 1 ]

種の祝福
Lv.10 / 24
現在 ×3.106
次   ×3.479
[ ROOT 1 ]

AUTO BUY
UNLOCKED

AUTO RETILL
LOCKED
[ ROOT 1 ]
```

最大Lv：

```text
MAX
```

と表示。

---

# 19. ROOT POWER

進捗：

```text
ROOT POWER
37 / 100
```

計算：

```text
RootAbundanceLevel
+ RootFertilityLevel
+ RootRetillLevel
+ RootSeedBlessingLevel
+ (AutoBuyUnlocked ? 1 : 0)
+ (AutoRetillUnlocked ? 1 : 0)
```

最大：

```text
100
```

---

# 20. ASCENT READY

現在周回生産が、

```text
9.000e+14
```

へ到達したら表示。

候補：

```text
HORIHORI
MENU
ASCENT画面
```

強制モーダルにはしない。

---

# 21. ASCENT確認

ASCENTは大規模リセットのため必ず確認。

例：

```text
ASCENTしますか？

以下がリセットされます。

・現在芋
・畑1〜8
・RETILL
・現在保有種芋
・生産力強化
・掘り出し強化
・放置強化
・RETILL効率
・畑コスト軽減
・今回ASCENT中のREPLANT進行

獲得
ROOT +1

ROOT・ROOT強化・実績・図鑑・
長期記録は維持されます。

[ CANCEL ]
[ ASCENT ]
```

---

# 22. ASCENT結果カード

例：

```text
ASCENT COMPLETE

ASCENT #12

ROOT
+1

ASCENT TIME
3d 14:22:08

REPLANT
38

FINAL RUN
9.842e+14
```

最低限：

- ASCENT番号
- 実行日時
- 前回ASCENTからの経過時間
- 今回ASCENT中のREPLANT回数
- ASCENT直前の現在周回生産

を保存する。

---

# 23. ASCENT履歴

直近10件。

例：

```text
ASCENT HISTORY

#12
2026/10/20 21:34
03d 14:22:08
REPLANT 38
FINAL 9.842e+14
```

11件目以降は最古を削除。

REPLANT履歴とは別管理。

---

# 24. ASCENT統計

統計へ追加：

```text
ASCENT累計回数
現在ROOT
累計獲得ROOT
使用済みROOT
ROOT POWER
最速ASCENT
今回ASCENT開始からの経過時間
今回ASCENT中のREPLANT回数
```

最速ASCENTは将来のASCENT周回コンテンツ向けに保存。

---

# 25. ASCENT関連実績

報酬なし。

候補：

## さらなる高みへ

```text
初ASCENT
```

## 根を張る

```text
初ROOT強化
```

## 自動農場

```text
AUTO BUY解放
```

## 永久機関

```text
AUTO BUY
+
AUTO RETILL
```

## 十度目の上昇

```text
ASCENT 10回
```

## ROOT MASTER

```text
ROOT POWER 100 / 100
```

---

# 26. ASCENT関連称号

候補：

```text
さらなる高みへ
根を張る者
自動農場主
ASCENDER
ROOT MASTER
```

ゲーム効果なし。

---

# 27. ASCENT 100回以降

ASCENT 100回で、

```text
ROOT POWER 100 / 100
```

が可能。

101回目以降もASCENT可能。

ROOTも引き続き、

```text
+1
```

してよい。

v5.0.0では余剰ROOTの用途は設けない。

将来：

```text
ASCENT 250
ASCENT 1000
ASCENTランキング
最速ASCENT
AUTO REPLANT
追加ROOT能力
ASCENTチャレンジ
```

などへ拡張する。

---

# 28. ROOT MAX時の理論値

```text
豊穣
全生産 約×807.8

肥沃
畑コスト ×0.50

再耕
RETILL基礎倍率
×1.25 → ×1.61

種の祝福
種芋獲得 約×15.18

AUTO BUY
永久解放

AUTO RETILL
永久解放
```

さらに現行種芋強化もASCENTごとに再構築可能。

---

# 29. 生産計算の推奨順序

現行v4.2.1との整合を保つため、
概念上は以下の順序を推奨する。

各畑：

```text
FarmBase
=
ProductionPerLevel
× Level
```

RETILL：

```text
RootAdjustedRetillBase
=
1.25
+ 0.015 × RootRetillLevel

RetillMultiplier
=
RootAdjustedRetillBase ^ RetillCount
```

種芋RETILL効率：

```text
SeedRetillEfficiencyMultiplier
=
1
+ RetillCount
× RetillEfficiencyUpgradeLevel
× 0.005
```

畑最終：

```text
FarmFinal
=
FarmBase
× RetillMultiplier
× SeedRetillEfficiencyMultiplier
```

全体：

```text
ProductionBeforeSeedPower
=
BaseProduction
+ Σ FarmFinal
```

種芋生産力：

```text
SeedProductionMultiplier
=
1
+ ProductionUpgradeLevel × 0.02
```

ROOT豊穣：

```text
RootAbundanceMultiplier
=
1.25 ^ RootAbundanceLevel
```

最終：

```text
ProductionPerSecond
=
ProductionBeforeSeedPower
× SeedProductionMultiplier
× RootAbundanceMultiplier
```

---

# 30. 種芋獲得計算

基礎：

```text
BaseSeedGain
=
floor(
    sqrt(
        RunProducedPotato
        / 9.000e+10
    )
)
```

ROOT種の祝福：

```text
RootSeedMultiplier
=
1.12 ^ RootSeedBlessingLevel
```

最終：

```text
FinalSeedGain
=
floor(
    BaseSeedGain
    × RootSeedMultiplier
)
```

REPLANT可能判定は、

```text
BaseSeedGain >= 1
```

または最終値1以上で判定してよいが、
現行仕様との互換性を優先するなら基礎REPLANT条件は `9.000e+10` のままとする。

---

# 31. ASCENT条件判定

ASCENT条件はROOT種の祝福による種芋獲得倍率とは無関係。

常に、

```text
RunProducedPotato >= 9.000e+14
```

で判定する。

つまりROOT種の祝福が強くなってもASCENT条件自体は変わらない。

---

# 32. セーブデータ

## 32.1 SaveVersion

現行：

```text
SaveVersion = 4
```

v5では、

```text
SaveVersion = 5
```

へ更新。

---

## 32.2 新規保存項目

最低限：

```text
AscentCount
CurrentRoot
TotalRootEarned

RootAbundanceLevel
RootFertilityLevel
RootRetillLevel
RootSeedBlessingLevel

AutoBuyUnlocked
AutoBuyEnabled

AutoRetillUnlocked
AutoRetillEnabled

CurrentAscentReplantCount
CurrentAscentStartedAtUtc

BestAscentSeconds

AscentHistory
```

必要に応じて、

```text
LastAscentAtUtc
```

なども追加。

---

## 32.3 使用済みROOT

保存せず派生計算を推奨。

```text
UsedRoot
=
RootAbundanceLevel
+ RootFertilityLevel
+ RootRetillLevel
+ RootSeedBlessingLevel
+ AutoBuyUnlocked
+ AutoRetillUnlocked
```

---

# 33. v4 → v5 セーブ移行

v4 SaveVersion 4 を読み込んだ場合：

```text
AscentCount = 0
CurrentRoot = 0
TotalRootEarned = 0

RootAbundanceLevel = 0
RootFertilityLevel = 0
RootRetillLevel = 0
RootSeedBlessingLevel = 0

AutoBuyUnlocked = false
AutoBuyEnabled = false

AutoRetillUnlocked = false
AutoRetillEnabled = false
```

既存v4進行：

- 現在芋
- 畑Lv
- RETILL
- 種芋
- 種芋強化
- REPLANT回数
- 実績
- 図鑑

などはそのまま維持する。

v5移行時に強制ASCENTしない。

---

# 34. サーバーセーブ

ASCENT関連情報も正式なサーバーセーブへ追加。

既存Revision方式を維持する。

特に、

```text
ASCENT実行
ROOT獲得
ROOT消費
ROOT強化
AUTO能力解放
```

は重要な永続変更。

クライアントだけ更新され、
サーバー保存が失敗する状態を避ける。

---

# 35. セーブ整合性チェック

v5で最低限確認：

```text
AscentCount >= 0
CurrentRoot >= 0
TotalRootEarned >= 0

0 <= RootAbundanceLevel <= 30
0 <= RootFertilityLevel <= 20
0 <= RootRetillLevel <= 24
0 <= RootSeedBlessingLevel <= 24
```

ROOT整合：

```text
UsedRoot <= TotalRootEarned
CurrentRoot
= TotalRootEarned - UsedRoot
```

を基本とする。

履歴：

```text
AscentHistory.Count <= 10
```

---

# 36. ランキング

v5.0.0では既存ランキングを変更しない。

```text
TOTAL POTATO
BEST POTATO / SEC
REPLANT
```

ASCENTランキングは未実装。

ただし将来用に、

```text
ASCENT累計回数
最速ASCENT
```

を保存しておく。

---

# 37. MENU

例：

```text
MENU

種芋強化
いも図鑑
実績
統計
プロフィール
ランキング
REPLANT HISTORY
ASCENT
設定
アカウント
```

ASCENTは主要項目として見つけやすく配置。

---

# 38. v5.0.0で追加しないもの

```text
新しい畑
RETILL上限追加
種芋強化追加
ROOT振り直し
ROOTプリセット
AUTO REPLANT
AUTO ASCENT
複雑なAUTO BUY
ASCENTランキング
探索
ガチャ
装備
期間イベント
第3転生
```

ASCENT 1〜100の進行ループを安定させることを優先する。

---

# 39. 実装優先順位

## Phase 1

- SaveVersion 5
- ASCENTデータモデル
- ASCENT条件
- ASCENTリセット
- ROOT付与

## Phase 2

- ROOT強化4種類
- 現行生産計算への組み込み
- 現行畑価格計算への組み込み
- 現行RETILL計算への組み込み
- 現行種芋計算への組み込み

## Phase 3

- AUTO BUY
- AUTO RETILL

## Phase 4

- ASCENT画面
- ROOT画面
- ASCENT READY
- 確認
- 結果カード

## Phase 5

- ASCENT履歴
- ASCENT統計
- 実績
- 称号

## Phase 6

- v4 → v5移行
- サーバーセーブ
- Revision競合確認
- セーブ整合性チェック

## Phase 7

- 初ASCENTまでの実プレイ時間確認
- ASCENT 2〜10回の高速化確認
- ROOT振り分けによる極端な破綻確認
- AUTO BUY / AUTO RETILL動作確認
- モバイルUI確認

---

# 40. v5.0.0 完了条件

- `9.000e+14` でASCENT可能
- ASCENT条件が固定
- ASCENTでROOT +1
- 現在芋リセット
- 畑1〜8リセット
- RETILLリセット
- 現在種芋リセット
- 種芋強化5種Lv.0
- lifetime REPLANTは維持
- ROOT維持
- ROOT強化維持
- 豊穣 最大Lv.30
- 肥沃 最大Lv.20
- 再耕 最大Lv.24
- 種の祝福 最大Lv.24
- AUTO BUY ROOT 1
- AUTO RETILL ROOT 1
- 総必要ROOT 100
- ROOT POWER最大100
- AUTO BUYがLv上限を超えない
- AUTO RETILLが10回を超えない
- ASCENT確認あり
- ASCENT結果カードあり
- ASCENT履歴10件
- ASCENT統計更新
- v4 SaveVersion 4から移行可能
- サーバーセーブ正常
- NaN / Infinity / overflow等の重大不具合なし
- スマートフォンで操作可能

---

# 41. 数値仕様まとめ

```text
CURRENT REPLANT BASE
9.000e+10

ASCENT TARGET
9.000e+14

ASCENT TARGET MEANING
通常REPLANT時の基礎種芋100個相当

ASCENT TARGET SCALE
固定

ASCENT REWARD
ROOT +1


ROOT ABUNDANCE / 豊穣
Max Lv.30
×1.25 / Lv
Max ≈ ×807.8


ROOT FERTILITY / 肥沃
Max Lv.20
-2.5% field cost / Lv
Max -50%


ROOT RETILL / 再耕
Max Lv.24
RETILL base +0.015 / Lv
×1.25 → ×1.61


ROOT SEED BLESSING / 種の祝福
Max Lv.24
×1.12 seed gain / Lv
Max ≈ ×15.18


AUTO BUY
1 ROOT

AUTO RETILL
1 ROOT

TOTAL ROOT COST
30 + 20 + 24 + 24 + 1 + 1
= 100
```

---

# 42. v5の最終ゲームループ

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
種芋強化5種
↓
REPLANT周回
↓
現在周回 9.000e+14
↓
ASCENT
↓
ROOT +1
↓
ROOT永久強化
↓
再び最初から
↓
以前より高速にASCENT
```

長期目標：

```text
ASCENT 100回
↓
ROOT POWER 100 / 100
```

---

# 43. 実装チャットへの引き継ぎメモ

v5では、データの寿命を明確に3層へ分離する。

```text
RUN SCOPE
現在芋
畑Lv
PurchaseCount
RETILL
現在周回生産


REPLANT SCOPE
現在種芋
種芋強化
今回ASCENT中REPLANT回数


ASCENT / PERMANENT SCOPE
ROOT
ROOT強化
AUTO能力
ASCENT回数
実績
図鑑
長期統計
```

特に、

```text
Replant()
Ascent()
```

で同じリセット処理を雑に共有すると、
消してはいけないデータまで消す危険がある。

推奨：

```text
ResetRunProgress()
ResetReplantLayer()
PerformReplant()
PerformAscent()
```

など、責務を明確に分ける。

また現行v4.2.1では、

```text
ReplantBaseProduction = 9.000e10
FarmCostMultiplier = 1.005
RetillProductionMultiplier = 1.25
RetillEfficiencyBonusPerLevelPerRetill = 0.005
FieldCostReductionPerLevel = 0.0025
```

が実装済み。

v5ではこれらを基準値として扱い、
古い仕様書上の `5.000e+19` や旧畑コストを参照しないこと。

v5.0.0では新要素を増やしすぎず、
ASCENT 1〜100周の成長ループを安定させることを最優先とする。
