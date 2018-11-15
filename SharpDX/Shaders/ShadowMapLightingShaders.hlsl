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

cbuffer ShadowTransform : register (b3)
{
	float4x4 ShadowTransform;
};

struct Flags
{
	float IsCubeTexture;
	float IsLighten;
};

cbuffer FLAGSBUF : register(b4)
{
	Flags flags;
};

Texture2D ShaderTexture : register(t0);
SamplerState Sampler : register(s0);

Texture2D ShadowMap : register(t1);
SamplerComparisonState samShadow : register(s1);

TextureCube CubeTexture : register(t2);
// SamplerState CubeSampler : register(s2);

struct VS_IN
{
	float4 pos : POSITION;
	float4 normal : NORMAL;
	float2 text : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 normal : TEXCOORD2;
	float2 text : TEXCOORD0;
	float4 worldPos : POSITION;
	float4 lightCoord : TEXCOORD1;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

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

static const float SMAP_SIZE = 2048.0f;
static const float SMAP_DX = 1.0f / SMAP_SIZE;

float CalcShadowFactor(SamplerComparisonState samShadow, Texture2D shadowMap, float4 shadowPos)
{
	// Complete projection by doing division by w.
	//shadowPos.xyz /= shadowPos.w;

	// Depth in NDC space.
	float depth = shadowPos.z;

	shadowPos.xy = (shadowPos.xy + float2(1, 1)) * 0.5f;
	shadowPos.y = 1 - shadowPos.y;

	// Texel size.
	float dx = SMAP_DX;

	float percentLit = 0.0f;
	float2 offsets[9] =
	{
		float2(-dx,  -dx), float2(0.0f,  -dx), float2(dx,  -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx,  +dx), float2(0.0f,  +dx), float2(dx,  +dx)
	};

	for (int i = 0; i < 9; i++)
	{
		percentLit += shadowMap.SampleCmpLevelZero(samShadow,
			shadowPos.xy + offsets[i], depth).r;
	}

	percentLit = shadowMap.SampleCmpLevelZero(samShadow, shadowPos.xy, depth).r;

	return percentLit /= 9.0f;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 color;
	[branch]
	if (flags.IsCubeTexture == 1.0f)
	{
		//color = float4(1.0f, 0.0f, 0.0f, 1.0f);
		color = CubeTexture.Sample(Sampler, input.normal.xyz);
		//return color;
	}
	else
	{
		color = ShaderTexture.Sample(Sampler, input.text);
	}

	if (flags.IsLighten == 1.0f)
	{
		float4 l = light.pos;
		l.w = 0;
		l = normalize(l);
		float4 r = reflect(-l, input.normal);
		r = normalize(r);
		float4 v = light.EyePos - input.worldPos;
		v.w = 0;
		v = normalize(v);

		float shadow = CalcShadowFactor(samShadow, ShadowMap, input.lightCoord);

		//return float4(shadow, shadow, shadow, 1.0f);

		float IdiffR = mat.diff.x * (dot(l, input.normal));
		float IdiffG = mat.diff.y * (dot(l, input.normal));
		float IdiffB = mat.diff.z * (dot(l, input.normal));

		float IspecR = mat.absorp.x * pow(saturate(dot(v, r)), mat.shine);
		float IspecG = mat.absorp.y * pow(saturate(dot(v, r)), mat.shine);
		float IspecB = mat.absorp.z * pow(saturate(dot(v, r)), mat.shine);

		float IambR = mat.amb.x;
		float IambG = mat.amb.y;
		float IambB = mat.amb.z;

		float IR = (IdiffR * shadow + IspecR * shadow) * light.color.x * light.intens + IambR;
		float IG = (IdiffG * shadow + IspecG * shadow) * light.color.y * light.intens + IambG;
		float IB = (IdiffB * shadow + IspecB * shadow) * light.color.z * light.intens + IambB;

		float4 res;
	    res.x = IR * color.r;
	    res.y = IG * color.g;
	    res.z = IB * color.b;
	    res.w = 1.0f; 
		return res;
	}
	else
	{
		return color;
	}
		//return float4(color.rgb*I, 1.0f);
}


