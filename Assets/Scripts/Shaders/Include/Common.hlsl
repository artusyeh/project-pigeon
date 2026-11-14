#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

#define SHAPE_SPHERE 0
#define SHAPE_CUBE   1
#define SHAPE_CLOUD  2

#define CLOUD_WOBBLE 0.08 
#define CLOUD_SPEED  0.4 

// distance from a point to the shape's edge: negative inside, positive outside
float ShapeDistance(float3 p, float3 center, float radius, int shape, float time) {
    float3 local = p - center;

    if (shape == SHAPE_CUBE) {
        float3 q = abs(local) - radius; // radius is half the cube's width
        return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
    }

    if (shape == SHAPE_CLOUD) {
        float3 d = normalize(local + 0.0001); // direction from center
        float t = time * CLOUD_SPEED;
        float wobble = sin(d.x * 5.0 + t) * sin(d.y * 4.0 - t * 0.7) * sin(d.z * 6.0 + t * 0.5); // slow lumpy movement
        return length(local) - radius * (1.0 + wobble * CLOUD_WOBBLE);
    }

    return length(local) - radius; // sphere
}

float GetLuminance(float3 linearColor) {
    return dot(linearColor, float3(0.2126, 0.7152, 0.0722)); // basically checks how bright
}

// turns any 2D input into a random-looking number between 0 and 1
// referenced https://www.shadertoy.com/view/4djSRW
float Hash12(float2 p) {
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

#endif