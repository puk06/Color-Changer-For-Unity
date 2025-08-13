int4 ApplyBrightness(int4 color, float brightness)
{
    return int4((int3)(color.rgb * brightness), color.a);
}

int4 ApplyContrast(int4 color, float contrast)
{
    int3 contrasted = int3(float3(color.rgb - 128) * contrast + 128.0);
    return int4(contrasted, color.a);
}

int4 ApplyGamma(int4 color, float gamma)
{
    int3 gammaCorrected = (int3)(pow(saturate(color.rgb / 255.0), gamma) * 255.0);
    return int4(gammaCorrected, color.a);
}

int4 ApplyExposure(int4 color, float exposure)
{
    float exposureFactor = pow(2.0, exposure);
    return int4((int3)(color.rgb * exposureFactor), color.a);
}

int4 ApplyTransparency(int4 color, float transparency)
{
    int alpha = (int)(color.a * (1.0 - transparency));
    return int4(color.rgb, alpha);
}

int4 AdvancedColorAdjustment(
    int4 color,
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
