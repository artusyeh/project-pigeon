#ifndef DITHER_INCLUDED
#define DITHER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Common.hlsl"

TEXTURE2D(_BlueNoiseTex);
float4 _BlueNoiseTex_TexelSize;

// the recursive Bayer matrix; reference: https://surma.dev/things/ditherpunk/
// returns a threshold between 0 and 1 for this pixel
float BayerThreshold(uint2 pixel, uint level) {
    uint value = 0;

    for (uint i = 0; i <= level; i++) {
        uint bx = (pixel.x >> i) & 1; // shift x right by i bits, & 1 keeps the lowest bit: left or right half of the block at level i
        uint by = (pixel.y >> i) & 1; // same for y: top or bottom half
        uint b0 = ((bx ^ by) << 1) | by; // compact lookup into the 2x2 level 0 grid

        value = value * 4 + b0; // 4 * previous level + offset
    }

    float count = (float)(1u << (2 * (level + 1))); // number of cells in the matrix
    return (value + 0.5) / count;
}

// reads this pixel's threshold from the tiling blue noise texture
float BlueNoiseThreshold(uint2 pixel) {
    uint2 size = (uint2)_BlueNoiseTex_TexelSize.zw; // texture width and height in pixels
    return LOAD_TEXTURE2D(_BlueNoiseTex, pixel % size).r; // % wraps around so the texture tiles
}

#endif