//  Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/6a63f93bc1f20ce6cd47f981c7494e8328915621/license.txt)
//  https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/6a63f93bc1f20ce6cd47f981c7494e8328915621/CGIncludes/UnityStandardParticles.cginc#L147
float3 RGBtoHSV(float3 arg1)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 P = lerp(float4(arg1.bg, K.wz), float4(arg1.gb, K.xy), step(arg1.b, arg1.g));
    float4 Q = lerp(float4(P.xyw, arg1.r), float4(arg1.r, P.yzx), step(P.x, arg1.r));
    float D = Q.x - min(Q.w, Q.y);
    float E = 1e-10;
    return float3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
}
float3 HSVtoRGB(float3 arg1)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 P = abs(frac(arg1.xxx + K.xyz) * 6.0 - K.www);
    return arg1.z * lerp(K.xxx, saturate(P - K.xxx), arg1.y);
}
