float GetColorDistance(float3 c1, float3 c2)
{
    float r = pow(c1.r - c2.r, 2);
    float g = pow(c1.g - c2.g, 2);
    float b = pow(c1.b - c2.b, 2);

    return sqrt(r + g + b);
}

float CalculateColorChangeRate(bool hasIntersection, float intersectionDistance, float distance, float graphWeight, float minValue)
{
    if (!hasIntersection || abs(intersectionDistance) < 1e-6) return 1.0;
    float changeRate = pow(saturate(1.0 - (distance / intersectionDistance)), graphWeight);
    return max(minValue, changeRate);
}

void GetRGBIntersectionDistance(
    float3 baseColor,
    float3 targetColor,
    out bool hasIntersection,
    out float intersectionDistance
)
{
    float dx = targetColor.r - baseColor.r;
    float dy = targetColor.g - baseColor.g;
    float dz = targetColor.b - baseColor.b;

    float t_values[6];
    int count = 0;
    
    if (abs(dx) > 1e-6)
    {
        t_values[count++] = (0.0 - baseColor.r) / dx;
        t_values[count++] = (255.0 - baseColor.r) / dx;
    }
    
    if (abs(dy) > 1e-6)
    {
        t_values[count++] = (0.0 - baseColor.g) / dy;
        t_values[count++] = (255.0 - baseColor.g) / dy;
    }
    
    if (abs(dz) > 1e-6)
    {
        t_values[count++] = (0.0 - baseColor.b) / dz;
        t_values[count++] = (255.0 - baseColor.b) / dz;
    }

    float minPositiveT = 1e30;
    //[unroll]
    for (int i = 0; i < count; i++)
    {
        float t = t_values[i];
        if (t > 0.0)
        {
            float x = baseColor.r + (t * dx);
            float y = baseColor.g + (t * dy);
            float z = baseColor.b + (t * dz);

            if (
                x >= 0.0 && x <= 255.0 &&
                y >= 0.0 && y <= 255.0 &&
                z >= 0.0 && z <= 255.0 &&
                t < minPositiveT
            )
            {
                minPositiveT = t;
            }
        }
    }

    if (abs(minPositiveT - 1e30) > 1e-6)
    {
        float length = sqrt((dx * dx) + (dy * dy) + (dz * dz));
        intersectionDistance = minPositiveT * length;
        hasIntersection = true;
    }
    else
    {
        intersectionDistance = -1.0;
        hasIntersection = false;
    }
}

float3 BalanceColorAdjustment(
    float3 pixel, float3 prevColor, float3 diff,
    int modeVersion,
    float v1Weight,
    float v1MinimumValue,
    float v2Radius,
    float v2Weight,
    float v2MinimumValue,
    bool v2IncludeOutside,
    RWTexture2D<float4> v3Gradient,
    int v3PreviewResolution
)
{
    float adjustmentFactor = 0.0;

    float distance = GetColorDistance(pixel, prevColor);

    if (modeVersion == 1)
    {
        bool hasIntersection = false;
        float intersectionDistance = 1.0;
        GetRGBIntersectionDistance(prevColor, pixel, hasIntersection, intersectionDistance);

        adjustmentFactor = CalculateColorChangeRate(
            hasIntersection,
            intersectionDistance,
            distance,
            v1Weight,
            v1MinimumValue
        );
            
        pixel.r = pixel.r + (diff.r * adjustmentFactor);
        pixel.g = pixel.g + (diff.g * adjustmentFactor);
        pixel.b = pixel.b + (diff.b * adjustmentFactor);

        return pixel;
    }
    else if (modeVersion == 2)
    {
        if (distance <= v2Radius)
        {
            adjustmentFactor = CalculateColorChangeRate(
                true,
                v2Radius,
                distance,
                v2Weight,
                v2MinimumValue
            );
        }
        else if (v2IncludeOutside)
        {
            adjustmentFactor = v2MinimumValue;
        }
        
            
        pixel.r = pixel.r + (diff.r * adjustmentFactor);
        pixel.g = pixel.g + (diff.g * adjustmentFactor);
        pixel.b = pixel.b + (diff.b * adjustmentFactor);

        return pixel;
    }
    else if (modeVersion == 3)
    {
        const float grayScaleWeightR = 0.299;
        const float grayScaleWeightG = 0.587;
        const float grayScaleWeightB = 0.114;

        float grayScale =
            grayScaleWeightR * (pixel.r / 255.0) +
            grayScaleWeightG * (pixel.g / 255.0) +
            grayScaleWeightB * (pixel.b / 255.0);
        
        int x = (int) (saturate(grayScale) * (v3PreviewResolution - 1));
        int y = 0;
        
        float4 gradientColor = v3Gradient[int2(x, y)];
        
        return gradientColor.rgb * 255.0;
    }
    
    pixel.r = pixel.r + (diff.r * adjustmentFactor);
    pixel.g = pixel.g + (diff.g * adjustmentFactor);
    pixel.b = pixel.b + (diff.b * adjustmentFactor);

    return pixel;
}
