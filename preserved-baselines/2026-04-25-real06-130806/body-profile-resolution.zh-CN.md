# Body Profile Resolution Summary

## 总览

- 构件数：`377`
- 已完成自动细分：`328`
- 人工复核旁路：`49`

## ResolutionStatus 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| Resolved | 已完成自动细分 | 328 |
| ReviewBypass | 人工复核旁路 | 49 |

## LongitudinalType 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| STRAIGHT | 直线主线 | 279 |
| POLYLINE | 折线主线 | 98 |

## ProfileCategory 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| STANDARD_SECTION | 标准型材 | 203 |
| BUILTUP_H | H 型 built-up | 80 |
| MANUAL_REVIEW | 人工复核 | 49 |
| BUILTUP_PRIMARY_PLATE | 单主板 built-up | 26 |
| BUILTUP_BOX | 规则箱形 built-up | 19 |

## Profile 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| STANDARD_IH | 标准工字/H 型材 | 112 |
| STANDARD_ANGLE | 标准角钢 | 89 |
| FOLDED_FLANGE_H | 折型翼缘 H | 68 |
| MANUAL_REVIEW | 人工复核 | 49 |
| PRIMARY_PLATE_BODY | 单主板 built-up | 26 |
| BUILTUP_BOX | 闭合箱形 built-up | 19 |
| GENERAL_BUILTUP_H | 一般 built-up H | 12 |
| STANDARD_ROD | 标准圆钢 | 2 |

## ProfileSeries 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| BH_SERIES | BH 焊接 H/I 系列 | 112 |
| L_SERIES | 角钢系列 | 89 |
| H_FOLDED_FLANGE_SERIES | 折型翼缘 H 系列 | 68 |
| MANUAL_REVIEW | 人工复核 | 49 |
| PRIMARY_PLATE_SERIES | 单主板系列 | 26 |
| BOX_CLOSED_LOOP_SERIES | 闭合箱形系列 | 19 |
| H_BUILTUP_SERIES | 一般 H built-up 系列 | 12 |
| D_SERIES | 直径圆钢系列 | 2 |

## DimensionSource 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| SOURCE_MAIN_PART_PROFILE | 源主件截面字符串 | 203 |
| FAMILY_SUBTYPE_PROOF | 阶段 6 家族子类证明 | 99 |
| MANUAL_REVIEW | 人工复核 | 49 |
| FAMILY_PROOF_CHAIN | 阶段 5/6 主板证明链 | 26 |

## 构件级结果

| MemberId | AssemblyId | Family | LongitudinalType | LongitudinalSubtype | FamilySubtype | ResolutionStatus | ProfileCategory | Profile | ProfileSeries | DimensionSource | SimilarityBasis | SimilarityScore | EvidenceSummary |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | ---: | --- |
| T3-4GZ-1 | 35683548 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-10 | 31967532 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-11 | 31784557 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-12 | 31811721 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-13 | 32140979 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-14 | 35401243 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-15 | 31807625 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-16 | 31808990 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-2 | 31923523 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-3 | 31930500 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-4 | 31919209 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-5 | 31751819 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-6 | 31916543 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-7 | 31779253 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-8 | 31915177 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4GZ-9 | 31781014 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4HMS-1 | 36152412 | 标准截面型材主体 | 折线主线 | 无长度方向细分类 | 标准圆钢 | 已完成自动细分 | 标准型材 | 标准圆钢 | 直径圆钢系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=D24 |
| T3-4HMS-2 | 36358402 | 标准截面型材主体 | 折线主线 | 无长度方向细分类 | 标准圆钢 | 已完成自动细分 | 标准型材 | 标准圆钢 | 直径圆钢系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=D24 |
| T3-4HXZ-1 | 30096906 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4HXZ-10 | 30076627 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*250*40*60 |
| T3-4HXZ-15 | 30076127 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 已完成自动细分 | 单主板 built-up | 单主板 built-up | 单主板系列 | 阶段 5/6 主板证明链 | 条款与家族一致 | 0.90 | LeadClause=PRIMARY_PLATE_CONTINUITY_CLAUSE; Verdict=Satisfied |
| T3-4HXZ-2 | 30076477 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 已完成自动细分 | 单主板 built-up | 单主板 built-up | 单主板系列 | 阶段 5/6 主板证明链 | 条款与家族一致 | 0.90 | LeadClause=PRIMARY_PLATE_CONTINUITY_CLAUSE; Verdict=Satisfied |
| T3-4HXZ-3 | 30076577 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 已完成自动细分 | 单主板 built-up | 单主板 built-up | 单主板系列 | 阶段 5/6 主板证明链 | 条款与家族一致 | 0.90 | LeadClause=PRIMARY_PLATE_CONTINUITY_CLAUSE; Verdict=Satisfied |
| T3-4HXZ-4 | 30076032 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 已完成自动细分 | 单主板 built-up | 单主板 built-up | 单主板系列 | 阶段 5/6 主板证明链 | 条款与家族一致 | 0.90 | LeadClause=PRIMARY_PLATE_CONTINUITY_CLAUSE; Verdict=Broken |
| T3-4HXZ-5 | 32021736 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4HXZ-6 | 30076377 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 已完成自动细分 | 单主板 built-up | 单主板 built-up | 单主板系列 | 阶段 5/6 主板证明链 | 条款与家族一致 | 0.90 | LeadClause=PRIMARY_PLATE_CONTINUITY_CLAUSE; Verdict=Satisfied |
| T3-4HXZ-7 | 30076647 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 已完成自动细分 | 单主板 built-up | 单主板 built-up | 单主板系列 | 阶段 5/6 主板证明链 | 条款与家族一致 | 0.90 | LeadClause=PRIMARY_PLATE_CONTINUITY_CLAUSE; Verdict=Satisfied |
| T3-4HXZ-8 | 35179365 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 已完成自动细分 | 规则箱形 built-up | 闭合箱形 built-up | 闭合箱形系列 | 阶段 6 家族子类证明 | 阶段 6 子类严格命中 | 0.93 | LeadClause=BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; FamilySubtype=CLOSED_LOOP_BOX |
| T3-4HXZ-9 | 30085855 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH250*300*30*30 |
| T3-5DZ-1 | 36865105 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-10 | 36865384 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-2 | 36864778 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-3 | 36864836 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-4 | 36865215 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-5 | 36865271 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-6 | 36865440 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-7 | 36864891 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-8 | 36864946 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5DZ-9 | 36865328 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH200*200*8*10 |
| T3-5GKL-1 | 28151091 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-10 | 28151371 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-11 | 28152669 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-12 | 30126470 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH500*250*12*30 |
| T3-5GKL-13 | 28152900 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH500*250*12*30 |
| T3-5GKL-14 | 28152955 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-15 | 28153864 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-16 | 35224469 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-17 | 35216274 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-18 | 28153489 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-19 | 28153933 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-2 | 28150744 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-20 | 28152763 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-3 | 28151762 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-4 | 35225194 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*30*30 |
| T3-5GKL-5 | 35231451 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*30*40 |
| T3-5GKL-6 | 28150831 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH500*250*12*30 |
| T3-5GKL-7 | 28151384 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-8 | 28152628 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH950*400*16*30 |
| T3-5GKL-9 | 28151425 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH500*300*14*30 |
| T3-5GL-1 | 30181144 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 已完成自动细分 | 标准型材 | 标准工字/H 型材 | BH 焊接 H/I 系列 | 源主件截面字符串 | source semantic 精确命中 | 1.00 | SourceMainPartProfile=BH400*150*8*8 |
