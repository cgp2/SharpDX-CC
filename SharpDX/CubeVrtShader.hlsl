struct VSOut
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

float4x4 worldViewProj;

VSOut main(float4 position : POSITION, float4 color : COLOR)
{
	VSOut output = (VSOut)0;
	output.position = mul(position, worldViewProj);
	output.position = position;
	output.color = color;

	return output;
}