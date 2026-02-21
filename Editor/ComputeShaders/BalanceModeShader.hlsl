float GetColorDistance(int3 c1, int3 c2)
{
    float r = pow(float(c1.r - c2.r), 2.0);
    float g = pow(float(c1.g - c2.g), 2.0);
    float b = pow(float(c1.b - c2.b), 2.0);

    return sqrt(r + g + b);
}

float CalculateColorChangeRate(bool hasIntersection, float intersectionDistance, float distance, float graphWeight, float minValue)
{
    if (!hasIntersection || abs(intersectionDistance) < 1e-6) return 1.0;
    float changeRate = pow(saturate(1.0 - (distance / intersectionDistance)), graphWeight);
    return max(minValue, changeRate);
}

void GetRGBIntersectionDistance(
    int3 baseColor,
    int3 targetColor,
    out bool hasIntersection,
    out float intersectionDistance
)
{
    int dx = targetColor.r - baseColor.r;
    int dy = targetColor.g - baseColor.g;
    int dz = targetColor.b - baseColor.b;

    float t_values[6];
    int count = 0;
    
    if (abs(dx) > 1e-6)
    {
        t_values[count++] = (0 - baseColor.r) / (float)dx;
        t_values[count++] = (255 - baseColor.r) / (float)dx;
    }
    
    if (abs(dy) > 1e-6)
    {
        t_values[count++] = (0 - baseColor.g) / (float)dy;
        t_values[count++] = (255 - baseColor.g) / (float)dy;
    }
    
    if (abs(dz) > 1e-6)
    {
        t_values[count++] = (0 - baseColor.b) / (float)dz;
        t_values[count++] = (255 - baseColor.b) / (float)dz;
    }

    float minPositiveT = 1e30;
    bool foundAny = false;

    [unroll]
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
                foundAny = true;
            }
        }
    }

    if (foundAny)
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

int3 BalanceColorAdjustment(
    int3 pixel, int3 prevColor, int3 diff,
    int modeVersion,
    float v1Weight,
    float v1MinimumValue,
    float v2Radius,
    float v2Weight,
    float v2MinimumValue,
    bool v2IncludeOutside,
    Texture2D<float4> v3Gradient,
    int v3PreviewResolution
)
{
    int3 result = pixel;
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

        result.r = (int)(pixel.r + (diff.r * adjustmentFactor));
        result.g = (int)(pixel.g + (diff.g * adjustmentFactor));
        result.b = (int)(pixel.b + (diff.b * adjustmentFactor));
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

        result.r = (int)(pixel.r + (diff.r * adjustmentFactor));
        result.g = (int)(pixel.g + (diff.g * adjustmentFactor));
        result.b = (int)(pixel.b + (diff.b * adjustmentFactor));
    }
    else if (modeVersion == 3)
    {
        const float grayScaleWeightR = 0.299;
        const float grayScaleWeightG = 0.587;
        const float grayScaleWeightB = 0.114;

        float grayScale = (
            grayScaleWeightR * (pixel.r / 255.0) +
            grayScaleWeightG * (pixel.g / 255.0) +
            grayScaleWeightB * (pixel.b / 255.0)
        );
        
        int x = (int)(saturate(grayScale) * (v3PreviewResolution - 1));
        int y = 0;
        
        float4 gradientColor = LinearToGammaSpaceExact(v3Gradient[int2(x, y)]);
        
        result = int3(gradientColor.rgb * 255.0);
    }

    return result;
}
