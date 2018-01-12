struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float brithgness;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float brithgness;
};

float4x4 worldViewProj;
float brightness = 1.1f;

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(input.pos, worldViewProj);
	output.col = input.col;
	//output.isLighten = input.isLighten;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	float4 fl = input.col;
	return fl;
}