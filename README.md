# [Unity] Terrain Scaling Tool

Unity editor utility for offsetting and/or scaling one or more terrains while preserving the world positions of the existing content. Displays a bounding-box of the current maximum bounds of the terrain as well as a live preview of the changes.

## How to Use
After copying the TerrainScalingTool.cs script into your Unity project, the tool window can be accessed through [Tools] > Terrain > Scale. Enabling the preview option will store your terrains original data in-memory and show the results of any modifications. Since increasing the scale of your terrain decreases the precision of the height values, this can be useful to ensure signifigant details will not be lost.

**note:** Once details are applied, the previous terrain data can not be restored. if you do not leverage source control to revision your changes, it may be safer to first create a backup of your original data.

![default](https://github.com/null511/Unity.TerrainScalingTool/raw/master/media/default.png)

## Offset

Allows terrain tile bounds to be shifted vertically while preserving the existing terrain. Caution: May cause existing data to be truncated at the minimum/maximum bounds.

![offset](https://github.com/null511/Unity.TerrainScalingTool/raw/master/media/offset.png)

## Scale

Allows terrain tile bounds to be scaled vertically while preserving the existing terrain. Caution: Values less than 1.0f may cause existing data to be truncated at the maximum bounds.

![scale](https://github.com/null511/Unity.TerrainScalingTool/raw/master/media/scale.png)
