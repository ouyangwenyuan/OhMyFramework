Shader "Custom/Unlit/SkyboxShader"
{
    Properties
    {
        _SkyBoxTexture ("Skybox Texture", Cube) = "white" {}
    }
    SubShader
    {
        Cull Off 
        ZWrite Off
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // sampler2D _MainTex;
            // float4 _MainTex_ST;
            float4 _Corner[4];
            TextureCube _SkyBoxTexture;
            SamplerState sample_SkyBoxTexture;

            v2f vert (appdata v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                o.vertex = v.vertex;
                o.worldPos = _Corner[v.uv.x + v.uv.y*2].xyz;
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);
                return _SkyBoxTexture.Sample(sample_SkyBoxTexture,viewDir);
            }
            ENDCG
        }
    }
}
