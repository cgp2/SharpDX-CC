cbuffer EYEBUF : register (b1)
{
	float4 EyePos;
};

struct LightSource
{
	float4 pos;
	float4 color;
	float intens;
};

cbuffer LIGHTBUF : register (b2)
{
	LightSource light;
};

struct Material
{
	float4 diff;
	float4 absorp;
	float4 amb;
	float shine;
};

cbuffer MATERIALBUF : register (b0)
{
	Material mat;
};

cbuffer World : register (b4)
{
	float4x4 World;
};

cbuffer View : register (b5)
{
	float4x4 View;
};

cbuffer Proj : register (b6)
{
	float4x4 Proj;
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
	PS_IN output = (PS_IN)0;

	float4x4 viewProj = mul(View, Proj);
	float4x4 worldView = mul(World, viewProj);
	output.pos = mul(input.pos, worldView);
	output.normal = mul(input.normal, World);
	output.normal = normalize(output.normal);

	output.worldPos = mul(input.pos, World);

	output.text = input.text.xy;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 l = light.pos;
	l.w = 0;
	l = normalize(l);
	float4 r = reflect(-l, input.normal);
	r = normalize(r);
	float4 v = EyePos - input.worldPos;
	v.w = 0;
	v = normalize(v);

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

	float IR = (IdiffR + IspecR) * light.color.x * light.intens + IambR;
	float IG = (IdiffG + IspecG) * light.color.y * light.intens + IambG;
	float IB = (IdiffB + IspecB) * light.color.z * light.intens + IambB;

	float4 color = ShaderTexture.Sample(Sampler, input.text);

	float4 res;
	res.x = IR* color.r;
	res.y = IG* color.g;
	res.z = IB* color.b ;
	res.w = 1.0f;

	return res ;
	//return float4(color.rgb*I, 1.0f);
}


