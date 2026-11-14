#ifndef WORLD_WINDOW_INCLUDED
#define WORLD_WINDOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Common.hlsl"

#define MAX_WORLD_AREAS 8 
#define ANY_WORLD -1

// set by worldarea script
float4 _WorldAreaData[MAX_WORLD_AREAS]; // xyz = center, w = radius
float4 _WorldAreaSettings[MAX_WORLD_AREAS]; // x = edge softness, y = world ID, z = enclosed (1 or 0)
float  _WorldAreaCount; // how many entries are actually in use

// turns a screen pixel back into a 3D world position
float3 ReconstructWorldPosition(float2 uv) {
    float deviceDepth = SampleSceneDepth(uv); // how far the pixel surface is
    return ComputeWorldSpacePosition(uv, deviceDepth, UNITY_MATRIX_I_VP); // gets back the surface world pos; UNITY_MATRIX_I_VP is the camera's 3d to screen math but inverted
}

// checks line of sight from cam through one sphere
float SphereWindowAmount(float3 camPos, float3 surfacePos, float3 center, float radius, float softness, int shape) {
    float3 toSurface = surfacePos - camPos; // line from cam to surface
    float rayLength = length(toSurface); // distance to surface of sightline
    float3 rayDir = toSurface / max(rayLength, 0.0001); // length of 1

    float3 checkPoint;

    if (shape == SHAPE_CUBE) {
        float3 inv = 1.0 / (rayDir + 0.001);
        float3 tA = (center - radius - camPos) * inv;
        float3 tB = (center + radius - camPos) * inv;
        float3 tMin = min(tA, tB);
        float3 tMax = max(tA, tB);

        float tEnter = max(max(max(tMin.x, tMin.y), tMin.z), 0.0); // can't start behind the camera
        float tExit = min(min(tMax.x, tMax.y), tMax.z);
        tExit = min(tExit, rayLength); // can't go past the surface

        if (tEnter > tExit)
            return 0.0; // ray misses the cube

        checkPoint = camPos + rayDir * (tEnter + tExit) * 0.5; // halfway through the part inside the cube
    } else {
        float t = clamp(dot(center - camPos, rayDir), 0.0, rayLength); // how far along the line of sight is the point nearest area center
        checkPoint = camPos + rayDir * t; // walk t meters from the camera along the line to get to the closest 3d point 
    }
    
    float dist = ShapeDistance(checkPoint, center, radius, shape, _Time.y); // how close the line of sight can get to the center of the area
    softness = max(softness, 0.001); // width of the fading edge
    return 1.0 - smoothstep(-softness, 0.0, dist); // 1 = passes through, 0 = misses, and smooth curve across edge band
}

// checks every sphere tagged with worldID for a surface position already reconstructed
float GetWindowAmountAt(float3 surfacePos, int worldID)
{
    float3 camPos = _WorldSpaceCameraPos;
    float amount = 0.0;

    int count = min((int)_WorldAreaCount, MAX_WORLD_AREAS);
    for (int i = 0; i < count; i++)
    {
        // skip spheres belonging to a different world
        int areaWorld = (int)_WorldAreaSettings[i].y;
        if (worldID != ANY_WORLD && areaWorld != worldID)
            continue;

        float a = SphereWindowAmount(camPos, 
                                    surfacePos,
                                    _WorldAreaData[i].xyz, // center
                                    _WorldAreaData[i].w, // radius
                                    _WorldAreaSettings[i].x, // edge softness
                                    (int)_WorldAreaSettings[i].w); // shape

        amount = max(amount, a); // keep the strongest, so if any matching sphere covers this pixel it counts
    }

    return amount;
}

// checks how much a surface was cut away by an enclosed world; 1 = show sky instead, 0 = keep it
float GetCutAmount(float3 surfacePos, int worldID)
{
    float3 camPos = _WorldSpaceCameraPos;
    float enclosedWindow = 0.0; // seen through any enclosed sphere of this world
    float inside = 0.0; // surface itself inside any sphere of this world

    int count = min((int)_WorldAreaCount, MAX_WORLD_AREAS);
    for (int i = 0; i < count; i++)
    {
        // skip spheres belonging to a different world
        int areaWorld = (int)_WorldAreaSettings[i].y;
        if (worldID != ANY_WORLD && areaWorld != worldID)
            continue;

        float3 center = _WorldAreaData[i].xyz; // sphere center
        float radius = _WorldAreaData[i].w; // sphere radius
        float softness = max(_WorldAreaSettings[i].x, 0.001); // width of the fading edge

        if (_WorldAreaSettings[i].z > 0.5) // only enclosed spheres can cut things away
            enclosedWindow = max(enclosedWindow, SphereWindowAmount(camPos, surfacePos, center, radius, softness, (int)_WorldAreaSettings[i].w)); // line of sight test

        float dist = ShapeDistance(surfacePos, center, radius, (int)_WorldAreaSettings[i].w, _Time.y); // how far the surface is from the center
        inside = max(inside, 1.0 - smoothstep(-softness, 0.0, dist)); // inside test, 1 = inside, fades across the edge
    }

    return (enclosedWindow > 0.0) ? (1.0 - inside) : 0.0; // cut by however much the surface is outside
}

// reconstructs the position from the screen uv, then checks
float GetWindowAmount(float2 uv, int worldID)
{
    return GetWindowAmountAt(ReconstructWorldPosition(uv), worldID);
}

#endif