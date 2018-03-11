struct GS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
};

struct VS_IN
{
	float4 pos : POSITION;
	float4 normal : NORMAL;
	float2 text : TEXCOORD;
};

GS_IN VS(VS_IN input)
{
	GS_IN output = (GS_IN)0;

	output.pos = input.pos;
		output.col = float4(1, 1, 0, 0);

	return output;
}

[maxvertexcount(3)]
void GS(point GS_IN input[1], inout TriangleStream<GS_IN> stream)
{
	GS_IN p = input[0];

	GS_IN p0 = p;
	GS_IN p1 = p;
	GS_IN p2 = p;

	p1.pos += float4(2, 0, 0, 0);
	p2.pos += float4(0, 2, 0, 0);

	stream.Append(p0);
	stream.Append(p1);
	stream.Append(p2);

	//stream.RestartStrip();
}
