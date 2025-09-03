Shader "Custom/InfiniteGridFullScreen"
{
    Properties
    {
        _GridScale ("Grid Scale", Float) = 1.0
        _GridThickness ("Grid Thickness", Float) = 0.01
        _GridColor ("Grid Color", Color) = (0.3, 0.3, 0.3, 1.0)
        _BackgroundColor ("Background Color", Color) = (0.1, 0.1, 0.1, 1.0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };
            
            float _GridScale;
            float _GridThickness;
            float4 _GridColor;
            float4 _BackgroundColor;
            float4 _CameraWorldPosition;
            float _CameraOrthoSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // Преобразуем UV в мировые координаты
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Получаем мировые координаты с учетом позиции камеры
                float2 worldPos = i.worldPos.xy;
                
                // Масштабируем координаты
                float2 scaledPos = worldPos / _GridScale;
                
                // Вычисляем дробные части
                float2 grid = abs(frac(scaledPos - 0.5) - 0.5);
                
                // Адаптивная толщина линий (учитываем масштаб)
                float lineWidth = _GridThickness;
                
                // Рисуем линии сетки
                float gridLines = 1.0 - smoothstep(0.0, lineWidth, min(grid.x, grid.y));
                
                // Смешиваем цвета
                fixed4 finalColor = lerp(_BackgroundColor, _GridColor, gridLines);
                
                return finalColor;
            }
            ENDCG
        }
    }
}