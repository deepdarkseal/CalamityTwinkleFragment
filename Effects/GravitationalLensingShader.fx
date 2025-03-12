// GravitationalLensingShader.fx
sampler2D uImage0;
float4 uColor;
float4 uSecondaryColor;
float2 aspectRatioCorrectionFactor;
float2 sourcePosition;
float blackRadius;
float distortionStrength;
float globalTime;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = sourcePosition;
    float2 delta = uv - center;
    float distance = length(delta * aspectRatioCorrectionFactor);

    // 扭曲效果
    float distortion = distortionStrength * (1.0 - smoothstep(0.0, blackRadius, distance));
    float2 distortedUV = uv + delta * distortion;

    // 颜色混合
    float4 color = lerp(uColor, uSecondaryColor, distance);
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}