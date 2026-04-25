# 主材识别输出契约

## 文档目的

本文档用于规范主材识别相关输出，避免再出现以下混淆：

- 把导入阶段重建类型当成最终工程结论
- 把虚拟件编号当成真实零件编号
- 把当前主体描述当成定义驱动家族判断

本契约服务于三个目标：

1. 对工程人员可读
2. 对算法调试可追溯
3. 对后续重构兼容

---

## 一、输出分层

主材识别输出必须拆成四层：

### 1. 输入来源层

描述当前样本来自哪里、主零件是什么。

必备字段：

- `SourceFile`
- `MemberId`
- `AssemblyId`
- `SourceMainPartId`
- `SourceMainPartName`
- `SourceMainPartProfileString`

### 2. 导入解释层

描述导入阶段为了跑离线识别做了什么中间处理。

必备字段：

- `SynthesizedBody`
- `ImportSynthesisKind`
- `SourceBodySeedPartIds`
- `SourceBodySeedParts`

说明：

- `ImportSynthesisKind` 只表示“导入阶段按什么方式重建了虚拟主体”
- 它不是最终工程截面结论

### 3. 当前主体描述层

描述当前阶段 5 proof 主链汇总出的主体描述。

必备字段：

- `BodyDescriptorFamily`
- `BodyDescriptorSectionType`
- `BodyDescriptorConfidence`
- `BodyDescriptorCorePartIds`
- `BodyDescriptorCoreParts`
- `BodyDescriptorReviewRequired`
- `BodyDescriptorReviewReasons`

说明：

- 这层是当前系统已有的主体描述摘要
- 它不等于导入重建类型，也不等于最终定义驱动家族结论

### 4. 未来定义驱动识别层

为后续新基座预留。

预留字段：

- `DefinitionBodyFamily`
- `DefinitionSatisfied`
- `DefinitionFailReasons`
- `DefinitionCoreBodyPartIds`
- `DefinitionAccessoryPartIds`
- `DefinitionStableStations`
- `DefinitionReviewRequired`
- `DefinitionReviewReasons`

当前若尚未实现，应允许为空。

---

## 二、最终对外展示原则

对外给工程人员的默认展示顺序应为：

1. 真实构件编号与真实零件编号
2. 当前真实主材种子零件
3. 当前主体描述
4. 未来定义驱动结论

禁止默认先展示：

- `ImportSynthesisKind`
- 虚拟件 `-1001/-1002/...`

除非用户明确在看算法中间层。

---

## 三、字段命名规范

## 3.1 强制区分的字段

必须显式区分以下字段，不允许再混用：

- `ImportSynthesisKind`
- `BodyDescriptorFamily`
- `DefinitionBodyFamily`

其中：

- `ImportSynthesisKind`：导入阶段重建方式
- `BodyDescriptorFamily`：当前主体描述家族
- `DefinitionBodyFamily`：未来按工程定义证明出的家族结论

## 3.2 真实零件字段

凡涉及真实零件，统一使用：

- `PartId`
- `PartName`
- `ProfileString`
- `Material`

若为虚拟对象，必须额外携带：

- `SourcePartIds`

---

## 四、推荐输出文件

## 4.1 构件级汇总表

推荐文件：

- `body-main-material-summary.csv`

推荐字段：

- `MemberId`
- `AssemblyId`
- `SourceMainPartId`
- `SourceMainPartProfileString`
- `SynthesizedBody`
- `ImportSynthesisKind`
- `BodyDescriptorFamily`
- `BodyDescriptorSectionType`
- `BodyDescriptorConfidence`
- `SourceBodySeedPartIds`
- `SourceBodySeedParts`
- `BodyDescriptorCorePartIds`
- `BodyDescriptorCoreParts`

## 4.2 真实主材种子零件表

推荐文件：

- `body-main-material-source-seeds.csv`

推荐字段：

- `MemberId`
- `AssemblyId`
- `SourceMainPartId`
- `SeedOrder`
- `PartId`
- `PartName`
- `ProfileString`
- `Material`
- `Thickness`
- `SemanticRole`
- `SemanticRoleScore`
- `IsSourceMainPart`

## 4.3 当前主体核心件表

推荐文件：

- `body-main-material-parts.csv`

推荐字段：

- `MemberId`
- `AssemblyId`
- `BodyDescriptorFamily`
- `BodyDescriptorSectionType`
- `CorePartOrder`
- `PartId`
- `PartName`
- `ProfileString`
- `IsSyntheticPart`

## 4.4 主体解释摘要

推荐文件：

- `body-main-material-explanation.json`

推荐内容：

- 当前为什么被当成主体候选
- 当前为什么被判成某家族
- 当前真实种子零件与主体核心件的差异
- 是否依赖上游缓存 `MainClass`

---

## 五、典型误导示例与纠正方式

## 示例 1：导入是 BOX，最终却不是 BOX

如果出现：

- `ImportSynthesisKind = BOX`
- `BodyDescriptorFamily = BuiltUpH`

那么系统必须明确展示为：

- 导入阶段按 `BOX` 重建
- 当前启发式主体猜测为 `BuiltUpH`

不得只在界面里显示一个“BOX”或一个“H”。

## 示例 2：核心件全是负号编号

如果出现：

- `BodyDescriptorCorePartIds = -1001|-1002|-1003`

那么必须同时展示：

- `SourceBodySeedPartIds = 真实零件编号`

避免工程人员无法回到 Tekla 真零件。

---

## 六、对当前代码的落地要求

当前代码在输出侧必须尽快完成：

1. 将 `SynthesisKind` 更名或镜像为 `ImportSynthesisKind`
2. 在汇总输出里显式加入 `BodyDescriptorFamily`
3. 为主体描述核心件补上对应真实来源说明
4. 增加主体解释摘要文件

---

## 七、验收标准

本输出契约落地后，需满足：

1. 任意样本都能直接看到真实构件编号与真实零件编号
2. 任意样本都能分清导入重建类型与最终启发式结论
3. 任意合成主体样本都能追溯到真实零件来源
4. 对 `T3-1HXZ-1` 这类边界样本，工程人员无需读代码即可知道：
   - 当前启发式为什么这么判
   - 它依赖哪些真实零件

---

## 八、当前实施优先级

### P0

- 字段语义拆分
- 真实零件映射
- 汇总表可读性提升

### P1

- 主体解释摘要
- 启发式核心件与真实种子零件对照

### P2

- 未来定义驱动结论字段预留
