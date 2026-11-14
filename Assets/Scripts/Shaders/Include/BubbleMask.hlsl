#ifndef BUBBLE_MASK_INCLUDED
#define BUBBLE_MASK_INCLUDED

#include "Common.hlsl"

#define BUBBLE_MASK_MAX_AREAS 8

// set by worldarea script
float4 _WorldAreaData[BUBBLE_MASK_MAX_AREAS]; // xyz = center, w = radius
float4 _WorldAreaSettings[BUBBLE_MASK_MAX_AREAS]; // x = edge softness
float  _WorldAreaCount; // how many entries are actually in use



// for shader graph use since terrains need special kind of shader holy shit
// ShowOnlyInside 0 = hidden inside bubbles, 1 = only visible inside bubbles
void BubbleMask_float(float3 PositionWS, float ShowOnlyInside, out float Alpha)
{
    float inside = 0.0;

    int count = min((int)_WorldAreaCount, BUBBLE_MASK_MAX_AREAS);

    for (int i = 0; i < count; i++) {
        float softness = max(_WorldAreaSettings[i].x, 0.001);
        float dist = ShapeDistance(PositionWS, _WorldAreaData[i].xyz, _WorldAreaData[i].w, (int)_WorldAreaSettings[i].w, _Time.y);
        inside = max(inside, 1.0 - smoothstep(-softness, 0.0, dist));
    }

    Alpha = lerp(1.0 - inside, inside, ShowOnlyInside); // pick hide or show
}

#endif