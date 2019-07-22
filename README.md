# [Unity] Terrain Scaling Tool

Unity editor utility for offsetting and/or scaling one or more terrains while preserving the world positions of the existing content. Displays a bounding-box of the current maximum bounds of the terrain as well as a live preview of the changes.

![default](https://github.com/null511/Unity.TerrainScalingTool/raw/master/media/default.png)

## Offset

Allows terrain tile bounds to be shifted vertically while preserving the existing terrain. Caution: May cause existing data to be truncated at the minimum/maximum bounds.

![offset](https://github.com/null511/Unity.TerrainScalingTool/raw/master/media/offset.png)

## Scale

Allows terrain tile bounds to be scaled vertically while preserving the existing terrain. Caution: Values less than 1.0f may cause existing data to be truncated at the maximum bounds.

![scale](https://github.com/null511/Unity.TerrainScalingTool/raw/master/media/scale.png)
