# Tekla 导出器

这个项目是给当前识别算法准备真实样本数据用的 Tekla Open API 导出器。

它做的事情很单纯：

- 从当前打开的 Tekla 模型里读取你选中的 assembly。
- 导出算法需要的 `parts` 和 `relationships` JSON。
- 同时为每个 assembly 生成一个人工标注模板，方便你补“期望结果”。

## 适用方式

这是一个独立的 Tekla Open API 小工具，不依赖当前 `net9.0` 的算法项目运行。
当前已经按 Tekla 2017 的 `plugins + macros` 部署方式配置。

建议使用方式：

1. 在 Tekla Structures 里打开模型。
2. 在模型视图里选中要导出的 assembly，或者选中 assembly 内任意 part。
3. 运行部署后的宏 `TeklaBodyBracketExportMacro`，或者直接启动部署到 `plugins` 目录里的导出器 EXE。
4. 选择一个输出目录。
5. 导出后把生成的整个目录发给我。

## 构建前准备

项目默认以你的 Tekla 2017 环境为准：

- 安装根目录：`D:\Program Files\Tekla Structures\2017`
- Open API 引用目录：`D:\Program Files\Tekla Structures\2017\nt\bin\plugins`
- 宏部署目录：`D:\Program Files\Tekla Structures\2017\Environments\common\macros\modeling`

如果你的安装目录不一样，构建时显式传入：

```powershell
dotnet build .\src\TeklaBodyBracketRecognition.TeklaExporter\TeklaBodyBracketRecognition.TeklaExporter.csproj -c Release /p:TeklaStructuresPath="D:\Program Files\Tekla Structures\2017"
```

如果你更习惯在 Visual Studio 里构建，也可以直接在项目属性里加一个 MSBuild 属性：

- `TeklaStructuresPath=D:\Program Files\Tekla Structures\2017`

## 运行方式

运行方式有两种：

- 在 Tekla 里执行宏 `TeklaBodyBracketExportMacro`
- 直接运行 `D:\Program Files\Tekla Structures\2017\nt\bin\plugins\TeklaBodyBracketRecognition.TeklaExporter.Gui.exe`

运行后会弹出一个目录选择框。

导出器会在你选中的目录下新建一个时间戳文件夹，并生成这些文件：

- `tekla-body-bracket-export.bundle.json`
- `assembly-<assemblyId>.labels.template.json`
- `README-export.txt`

如果使用 `Release` 构建，项目会自动把这些文件部署到 Tekla 目录：

- `TeklaBodyBracketRecognition.TeklaExporter.Gui.exe`
- `TeklaBodyBracketRecognition.TeklaExporter.pdb`
- `README.md`
- `TeklaBodyBracketExportMacro.cs`

## 当前导出内容

`bundle.json` 里每个 assembly 会包含：

- `assemblyId`
- `mainPartId`
- `parts[]`
- `relationships[]`

其中 `parts[]` 已经覆盖当前算法最需要的字段：

- `runtimeType`
- `material`
- `profileString`
- `isPlateLike`
- `isSpecialShape`
- `centroid`
- `obbDims`
- `boundingBox`
- `volume`
- `surfaceArea`
- `thickness`
- `plateNormal`
- `plateLongDirection`
- `plateWidthDirection`
- `contourVertexCount`
- `concaveCornerCount`
- `holeLikeFeatureCount`
- `booleanCutCount`
- `booleanAddCount`
- `shopWeldDegree`
- `siteWeldDegree`

`relationships[]` 当前会导出：

- `Weld`
- `Boolean`
- `Bolt`

接触关系 `Contact` 先不从 Tekla 直接导，后续由算法侧根据包围盒补充。

## 建议你优先导出的样本

第一批尽量给这些：

- 2 个焊接 H
- 1 到 2 个焊接箱
- 2 个简单牛腿或带肋牛腿
- 2 个容易和牛腿混淆的负例，比如加劲板、隔板、端板

## 人工标注模板怎么填

每个 `assembly-*.labels.template.json` 里你只需要补这些最关键字段：

- `expectedBodyFamily`
- `expectedCoreBodyPartIds`
- `expectedAccessoryPartIds`
- `expectedBrackets[].type`
- `expectedBrackets[].partIds`

如果某个样本你觉得“很容易误判”，把原因写到 `notes` 就行。

## 已知限制

- 这版导出器优先服务当前算法迭代，不是最终正式发布版插件。
- `centroid` 目前使用 solid 包围盒中心近似，不是严格体质心。
- `holeLikeFeatureCount` 目前主要来自 bolt group 和 boolean cut。
- `ContourPlate` 以外的异形件导出会尽量保底，但首批调试仍建议先以常见焊接板构件为主。
