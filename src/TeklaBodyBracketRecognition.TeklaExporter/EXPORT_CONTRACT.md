# 导出 JSON 契约

当前导出器会生成一个 `tekla-body-bracket-export.bundle.json`。

顶层结构：

```json
{
  "schemaVersion": "tekla-body-bracket-export.v1",
  "exportedAtUtc": "2026-04-18T08:30:00.0000000Z",
  "source": {
    "modelPath": "C:\\TeklaModels\\Demo",
    "selectionMode": "selected",
    "exporterVersion": "0.1.0"
  },
  "assemblies": [
    {
      "assemblyId": "12345",
      "mainPartId": 101,
      "parts": [],
      "relationships": [],
      "metadata": {}
    }
  ]
}
```

单个 `part` 结构：

```json
{
  "partId": 101,
  "runtimeType": "ContourPlate",
  "name": "Web",
  "material": "Q355B",
  "profileString": "PL10",
  "isPlateLike": true,
  "isSpecialShape": false,
  "centroid": { "x": 0.0, "y": 0.0, "z": 0.0 },
  "obbDims": { "x": 6000.0, "y": 500.0, "z": 10.0 },
  "boundingBox": {
    "min": { "x": -3000.0, "y": -5.0, "z": -250.0 },
    "max": { "x": 3000.0, "y": 5.0, "z": 250.0 }
  },
  "volume": 30000000.0,
  "surfaceArea": 6000000.0,
  "thickness": 10.0,
  "plateNormal": { "x": 0.0, "y": 1.0, "z": 0.0 },
  "plateLongDirection": { "x": 1.0, "y": 0.0, "z": 0.0 },
  "plateWidthDirection": { "x": 0.0, "y": 0.0, "z": 1.0 },
  "contourVertexCount": 4,
  "concaveCornerCount": 0,
  "holeLikeFeatureCount": 0,
  "booleanCutCount": 0,
  "booleanAddCount": 0,
  "shopWeldDegree": 2.8,
  "siteWeldDegree": 0.0,
  "contourPoints": []
}
```

单个 `relationship` 结构：

```json
{
  "partIdA": 101,
  "partIdB": 102,
  "edgeType": "Weld",
  "strength": 0.95,
  "geometricSupport": 1.0,
  "meta": "Weld|shop=True|sizeAbove=6|sizeBelow=6|around=False"
}
```

## 设计说明

- `parts` 的字段已经和当前算法 `PartInput`/`PartFeature` 所需字段对齐。
- `relationships` 先导出确定性最强的 `Weld / Boolean / Bolt`。
- `Contact` 关系暂时不由 Tekla 侧直接导出，后续由算法端用包围盒和几何阈值补足。
- 如果某些 report property 在你的 Tekla 版本里取不到，导出器会尽量用 profile 或局部包围盒做保底。
