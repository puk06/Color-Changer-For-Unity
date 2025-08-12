float4 ApplyBrightness(float4 color, float brightness)
{
    return float4(color.rgb * brightness, color.a);
}

float4 ApplyContrast(float4 color, float contrast)
{
    float3 contrasted = ((color.rgb - 128.0) * contrast) + 128.0;
    return float4(saturate(contrasted), color.a);
}

float4 ApplyGamma(float4 color, float gamma)
{
    float3 gammaCorrected = pow(color.rgb / 255.0, float3(gamma, gamma, gamma)) * float3(255.0, 255.0, 255.0);
    return float4(gammaCorrected, color.a);
}

float4 ApplyExposure(float4 color, float exposure)
{
    float exposureFactor = pow(2.0, exposure);
    return float4(color.rgb * exposureFactor, color.a);
}

float4 ApplyTransparency(float4 color, float transparency)
{
    float alpha = color.a * (1.0 - transparency);
    return float4(color.rgb, alpha);
}

float4 AdvancedColorAdjustment(
    float4 color,
    float brightness,
    float contrast,
    float gamma,
    float exposure,
    float transparency
)
{
    if (brightness != 1.0)
        color = ApplyBrightness(color, brightness);

    if (contrast != 1.0)
        color = ApplyContrast(color, contrast);

    if (gamma != 1.0)
        color = ApplyGamma(color, gamma);

    if (abs(exposure) > 1e-6)
        color = ApplyExposure(color, exposure);

    if (abs(transparency) > 1e-6)
        color = ApplyTransparency(color, transparency);

    return color;
}