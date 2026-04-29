# 下一批导数请求

## 优先级最高的两条路

### 路线 A：不重新导数据，直接回填最小真值

这是最快的路径，只需要补：

- [priority-bracket-answers.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-bracket-answers.csv>)：`10` 条牛腿判断
- [priority-body-answers.csv](</I:/autoteklasuanfa/.tmpresults/run_body_bracket_real_01_workflow/priority-body-answers.csv>)：`8` 条主体家族判断

### 路线 B：重新导一小批更有监督价值的数据

如果你更愿意重新导数据，而不是人工回填，那么我现在最需要的是：

- `6-10` 个明确有牛腿的 `GKZ` 紧凑双板对样本
- `6-10` 个明确没有牛腿、但形态接近 `GL / BuiltUpT + 角钢短板簇` 的硬负例

## 最精确的下一批导数对象

如果只能先导一小批，请优先覆盖下面这些 assembly。

### A. 牛腿高优先样本

这些样本当前都被识别成“有 1 个牛腿”，最需要确认真阳性还是残余误报。

| AssemblyId | 当前文件 | 预测主体 | 预测牛腿件组 | 建议用途 |
| --- | --- | --- | --- | --- |
| 262948748 | `member_T2-3GKZ-5.json` | `BuiltUpH` | `262317231,262317238` | `GKZ` 真牛腿优先 |
| 262960295 | `member_T2-3GKZ-3.json` | `BuiltUpH` | `261599364,261599370` | `GKZ` 真牛腿优先 |
| 262970541 | `member_T2-3GKZ-15.json` | `BuiltUpH` | `261594522,261594528` | `GKZ` 真牛腿优先 |
| 260923319 | `member_T2-3GL-76.json` | `Irregular` | `263024889,263025492` | `GL` 可疑牛腿/负例 |
| 260923860 | `member_T2-3GL-18.json` | `Irregular` | `262074060,262074068,262074076,262074114` | `GL` 可疑牛腿/负例 |
| 260925586 | `member_T2-3GL-50.json` | `BuiltUpT` | `262077056,262077064,262077072,262077079,262077110` | `BuiltUpT` 硬负例优先 |
| 260925609 | `member_T2-3GL-53.json` | `BuiltUpT` | `262077208,262077216,262077224,262077262` | `BuiltUpT` 硬负例优先 |
| 260925691 | `member_T2-3GL-22.json` | `BuiltUpT` | `262074364,262074372,262074380,262074418` | `BuiltUpT` 硬负例优先 |
| 263004765 | `member_T2-3GL-12.json` | `BuiltUpH` | `267382447` | 单件簇误报核对 |
| 263112220 | `member_T2-3GL-63.json` | `Irregular` | `263100710` | 单件簇误报核对 |

### B. 主体家族高优先样本

这些样本当前没有牛腿争议，但主体家族判断发生了变化，最适合补主体真值。

| AssemblyId | 当前文件 | 当前预测 | 变化方向 | 建议用途 |
| --- | --- | --- | --- | --- |
| 262965494 | `member_T2-3GKZ-11.json` | `BuiltUpH` | `BuiltUpT -> BuiltUpH` | `GKZ` 主体真值优先 |
| 260923465 | `member_T2-3GL-62.json` | `BuiltUpT` | `Irregular -> BuiltUpT` | `GL/BuiltUpT` 主体真值优先 |
| 263004749 | `member_T2-3GL-56.json` | `BuiltUpT` | `BuiltUpH -> BuiltUpT` | `GL/BuiltUpT` 主体真值优先 |
| 260925150 | `member_T2-3GL-35.json` | `BuiltUpH` | `Irregular -> BuiltUpH` | `GL/BuiltUpH` 主体真值优先 |
| 262849548 | `member_T2-3GL-181.json` | `Irregular` | `BuiltUpH -> Irregular` | 边界异形核对 |
| 263004753 | `member_T2-3GL-165.json` | `BuiltUpH` | `Irregular -> BuiltUpH` | `GL/BuiltUpH` 主体真值优先 |
| 266028106 | `member_T2-3GL-3.json` | `Irregular` | `BuiltUpH -> Irregular` | 边界异形核对 |
| 268064329 | `member_T2-3GL-27.json` | `BuiltUpH` | `Irregular -> BuiltUpH` | `GL/BuiltUpH` 主体真值优先 |

## 重新导数时，最好补哪些字段

如果导出器能补 richer data，我最希望这批样本补这些字段：

- weld 关系
- boolean cut / boolean add
- bolt / hole group 信息
- part-to-part contact / attachment 关系
- 轮廓点
- 局部坐标系

## 最小人工标签要求

即使没有 richer data，只要能补下面这些人工真值，也足够我继续迭代：

- 是否有牛腿
- 如果有牛腿，牛腿 part id 分组
- 主体家族：`BuiltUpH / BuiltUpBox / BuiltUpT / BuiltUpCross / Irregular`

## 推荐顺序

如果你时间很紧，先按这个顺序来：

1. `GKZ` 牛腿样本：`262948748, 262960295, 262970541`
2. `BuiltUpT` 硬负例：`260925586, 260925609, 260925691`
3. 单件簇误报边界：`263004765, 263112220`
4. 主体边界样本：`262965494, 260923465, 263004749`
