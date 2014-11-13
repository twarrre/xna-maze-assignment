#define MaxBones 60 
 
float4x3 Bones[MaxBones];

float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float4x4 WorldInverseTranspose;
float3 DiffuseLightDirection = float3(1, 1, 1);
float3 DiffusePosition = float3(0, 0, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseLightRadius;
float DiffuseLightAngleCosine;
float DiffuseLightDecayExponent;
float DiffuseIntensity = 6.0;

float Shininess = 200;
float4 SpecularColor = float4(1, 1, 1, 1);    
float SpecularIntensity = 1;
float3 ViewVector = float3(1, 0, 0);

float FarPlane;
float4 FogColor;
bool FogEnabled;
bool DayEnabled;
float DaylightIntensity;

texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
	int4   Indices  : BLENDINDICES0; 
    float4 Weights  : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
	float2 TextureCoordinate : TEXCOORD1;
	float ViewSpaceZ : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
};

void Skin(inout VertexShaderInput vin, uniform int boneCount) 
{ 
    float4x3 skinning = 0; 
 
    [unroll] 
    for (int i = 0; i < boneCount; i++) 
    { 
        skinning += Bones[vin.Indices[i]] * vin.Weights[i]; 
    } 
 
    vin.Position.xyz = mul(vin.Position, skinning); 
    vin.Normal = mul(vin.Normal, (float3x3)skinning); 
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	Skin(input, 4);
	float4 worldPosition = input.Position;

    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	float4 normal = mul(input.Normal, WorldInverseTranspose);
    float lightIntensity = dot(normal, DiffuseLightDirection);
    output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);

	output.Normal = normal;
	output.TextureCoordinate = input.TextureCoordinate;
	output.ViewSpaceZ = (output.Position.z) / FarPlane;
	output.WorldPosition = worldPosition;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	const float FOG_MIN = 0.49;
    const float FOG_MAX = 0.99;

	float4 finalColor = float4(0, 0, 0, 0);
	float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);

	float3 lightDirection = DiffusePosition - input.WorldPosition;
	float attenuation = saturate(1.0f - length(lightDirection) / DiffuseLightRadius);
	float3 light = normalize(lightDirection);
	float dotProduct = dot(DiffuseLightDirection, -light);

	if(dotProduct > DiffuseLightAngleCosine)
	{
		float spotIntensity = pow(dotProduct, DiffuseLightDecayExponent);
		float3 normal = normalize(input.Normal);
		float3 viewDir = normalize(mul(normalize(ViewVector), World));
		float diff = saturate(dot(normal, light));
		float3 reflect = normalize(2 * diff * normal - light);
		float specular = max(pow(dotProduct, Shininess), 0) * length(input.Color);
		finalColor = AmbientColor * AmbientIntensity + 
		spotIntensity
		* DiffuseColor
		* DiffuseIntensity 
		* textureColor * diff + SpecularIntensity * SpecularColor * specular;
	}

	if(DayEnabled)
	{
		finalColor = saturate(finalColor * DaylightIntensity);
	}


	if(FogEnabled)
		return lerp(finalColor, FogColor, lerp(FOG_MIN, FOG_MAX, input.ViewSpaceZ));
	else
		return finalColor;
}

technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
