# Body Family Proof Summary

## 总览

- 构件数：`377`
- 已进入家族判定：`328`
- 暂缓进入家族判定：`49`

## Family 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| STANDARD_SECTION | 标准截面型材主体 | 203 |
| H | H型主体 | 118 |
| PRIMARY_PLATE_BODY | 单主板主体 | 26 |
| BOX | 闭合箱形主体 | 19 |
| NONE | 未进入家族判定 | 11 |

## LongitudinalType 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| STRAIGHT | 直线主线 | 279 |
| POLYLINE | 折线主线 | 98 |

## Subtype 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| STANDARD_IH | 标准工字/H 型材 | 112 |
| STANDARD_ANGLE | 标准角钢 | 89 |
| FOLDED_FLANGE_H | 折型翼缘 H | 68 |
| GENERAL_BUILTUP_H | 一般 built-up H | 50 |
| MAJORITY_CONTINUITY | 多数站位持续主板 | 26 |
| CLOSED_LOOP_BOX | 闭合箱形截面 | 19 |
| NONE | 无子类 | 11 |
| STANDARD_ROD | 标准圆钢 | 2 |

## DecisionStatus 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| Adjudicated | 已进入家族判定 | 328 |
| Deferred | 暂缓进入家族判定 | 49 |

## DecisionReason 分布

| Code | LabelZh | Count |
| --- | --- | ---: |
| SOURCE_STANDARD_SECTION_READY | source semantic 已明确标准截面，可直接进入标准型材家族 | 203 |
| READINESS_NOT_PROMOTABLE | 当前仍未达到 ReadyForPromotion | 48 |
| H_CLAUSE_BROKEN_READY | 腹板/翼缘连续性条款已稳定破坏，可进入折型翼缘 H 家族 | 41 |
| H_CLAUSE_READY | 腹板/翼缘连续性条款已稳定满足，可进入 H 家族 | 27 |
| PRIMARY_PLATE_CLAUSE_READY | 主板多数站位持续条款已稳定，可进入单主板家族 | 26 |
| BOX_CLAUSE_READY | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | 19 |
| PRIMARY_PLATE_OVERRIDDEN_BY_H_SEMANTIC | 上游稳定语义已明确为 H 主类，单主板条款不再覆盖 H 家族归属 | 8 |
| H_REVIEW_READY_OVERRIDDEN_BY_STABLE_H_SEMANTIC | H 连续性条款已稳定指向 H 主类，BuiltUpT 启发式不再继续压入人工暂缓 | 4 |
| LEAD_CLAUSE_NOT_MAPPED | 当前主条款尚未接入阶段 6 家族映射 | 1 |

## 构件级结果

| MemberId | AssemblyId | Family | LongitudinalType | LongitudinalSubtype | Subtype | LeadClause | LeadClauseVerdict | PromotionReadiness | DecisionStatus | DecisionReason | SatisfiedConditions | MissingConditions |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| T3-4GZ-1 | 35683548 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-10 | 31967532 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-11 | 31784557 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-12 | 31811721 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-13 | 32140979 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-14 | 35401243 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-15 | 31807625 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-16 | 31808990 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-2 | 31923523 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-3 | 31930500 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-4 | 31919209 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-5 | 31751819 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-6 | 31916543 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-7 | 31779253 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-8 | 31915177 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4GZ-9 | 31781014 | 闭合箱形主体 | 直线主线 | 主轴折向直线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HMS-1 | 36152412 | 标准截面型材主体 | 折线主线 | 无长度方向细分类 | 标准圆钢 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_ROD; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HMS-2 | 36358402 | 标准截面型材主体 | 折线主线 | 无长度方向细分类 | 标准圆钢 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_ROD; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-1 | 30096906 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款破坏 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Broken; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-10 | 30076627 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-15 | 30076127 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 主板：多数站位持续性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 主板多数站位持续条款已稳定，可进入单主板家族 | LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-2 | 30076477 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 主板：多数站位持续性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 主板多数站位持续条款已稳定，可进入单主板家族 | LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-3 | 30076577 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 主板：多数站位持续性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 主板多数站位持续条款已稳定，可进入单主板家族 | LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-4 | 30076032 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 主板：多数站位持续性条款 | 条款破坏 | 可进入阶段 6 提升 | 已进入家族判定 | 主板多数站位持续条款已稳定，可进入单主板家族 | LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE; LeadClauseVerdict = Broken; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-5 | 32021736 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-6 | 30076377 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 主板：多数站位持续性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 主板多数站位持续条款已稳定，可进入单主板家族 | LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-7 | 30076647 | 单主板主体 | 直线主线 | 一般直线主线 | 多数站位持续主板 | 主板：多数站位持续性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | 主板多数站位持续条款已稳定，可进入单主板家族 | LeadClause = PRIMARY_PLATE_CONTINUITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-8 | 35179365 | 闭合箱形主体 | 直线主线 | 一般直线主线 | 闭合箱形截面 | 箱型：闭合/对边稳定条款 | 条款破坏 | 可进入阶段 6 提升 | 已进入家族判定 | 箱型闭合/对壁稳定条款已稳定，可进入 BOX 家族 | LeadClause = BOX_CLOSED_LOOP_OPPOSITE_WALL_CLAUSE; LeadClauseVerdict = Broken; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-4HXZ-9 | 30085855 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-1 | 36865105 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-10 | 36865384 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-2 | 36864778 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-3 | 36864836 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-4 | 36865215 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-5 | 36865271 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-6 | 36865440 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-7 | 36864891 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-8 | 36864946 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5DZ-9 | 36865328 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-1 | 28151091 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-10 | 28151371 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-11 | 28152669 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-12 | 30126470 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-13 | 28152900 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-14 | 28152955 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-15 | 28153864 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-16 | 35224469 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-17 | 35216274 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-18 | 28153489 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-19 | 28153933 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-2 | 28150744 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-20 | 28152763 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-3 | 28151762 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-4 | 35225194 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-5 | 35231451 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-6 | 28150831 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-7 | 28151384 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-8 | 28152628 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GKL-9 | 28151425 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
| T3-5GL-1 | 30181144 | 标准截面型材主体 | 直线主线 | 一般直线主线 | 标准工字/H 型材 | 标准截面：源主截面同一性条款 | 条款满足 | 可进入阶段 6 提升 | 已进入家族判定 | source semantic 已明确标准截面，可直接进入标准型材家族 | SourceSemanticBodyFamily = StandardSection; SourceSemanticSectionType = STANDARD_IH; LeadClause = STANDARD_SECTION_SOURCE_IDENTITY_CLAUSE; LeadClauseVerdict = Satisfied; LeadClausePromotionReadiness = ReadyForPromotion | (empty) |
