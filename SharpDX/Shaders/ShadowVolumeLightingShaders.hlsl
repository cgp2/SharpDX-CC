struct TransformMatrices
{
    float4x4 World;
    float4x4 View;
    float4x4 Proj;
};
cbuffer MATRICESBUF : register(b0)
{
    TransformMatrices matrices;
};

struct Light
{
    float4 pos;
    float4 color;
    float4 EyePos;
    float intens;
};
cbuffer LIGHTBUF : register(b1)
{
    Light light;
};

struct Material
{
    float4 diff;
    float4 absorp;
    float4 amb;
    float shine;
};
cbuffer MATERIALBUF : register(b2)
{
    Material mat;
};


cbuffer ShadowTransform : register(b3)
{
    float4x4 ShadowTransform;
};

Texture2D ShaderTexture : register(t0);
SamplerState Sampler : register(s0);

struct VS_IN
{
    float4 pos : POSITION;
    float4 normal : NORMAL;
    float2 text : TEXCOORD;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 normal : NORMAL;
    float2 text : TEXCOORD0;
    float4 worldPos : POSITION;
    float4 lightCoord : TEXCOORD1;
};

PS_IN VS(VS_IN input)
{
    PS_IN output = (PS_IN) 0;

    float4x4 viewProj = mul(matrices.View, matrices.Proj);
    float4x4 worldView = mul(matrices.World, viewProj);
    output.pos = mul(input.pos, worldView);
    output.normal = mul(input.normal, matrices.World);
    output.normal = normalize(output.normal);

    output.worldPos = mul(input.pos, matrices.World);

    output.text = input.text.xy;

    output.lightCoord = mul(input.pos, ShadowTransform);

    output.lightCoord /= output.lightCoord.w;

    return output;
}

float4 PS(PS_IN input) : SV_Target
{
    float4 l = light.pos;
    l.w = 0;
    l = normalize(l);
    float4 r = reflect(-l, input.normal);
    r = normalize(r);
    float4 v = light.EyePos - input.worldPos;
    v.w = 0;
    v = normalize(v);

	//return float4(shadow, shadow, shadow, 1.0f);

    float IambR = mat.amb.x;
    float IambG = mat.amb.y;
    float IambB = mat.amb.z;

    float IR = IambR;
    float IG = IambG;
    float IB = IambB;

    float4 color = ShaderTexture.Sample(Sampler, input.text);

    float4 res;
    res.x = IR * color.r;
    res.y = IG * color.g;
    res.z = IB * color.b;
    res.w = 1.0f;

    return res;
	//return float4(color.rgb*I, 1.0f);
}


