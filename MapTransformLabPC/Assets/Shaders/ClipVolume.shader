Shader "Custom/ClipVolume" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

			// Expose parameters for the minimum x, minimum z,
			// maximum x, and maximum z of the rendered volume.
			_Corners("Min XZ / Max XZ", Vector) = (-1, -1, 1, 1)
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			// Allow back sides of the object to render.
			Cull Off

			CGPROGRAM

			#pragma surface surf Standard fullforwardshadows
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			// Read the min xz/ max xz material properties.
			float4 _Corners;

			void surf(Input IN, inout SurfaceOutputStandard o) {

				// Calculate a signed distance from the clipping volume.
				float2 offset;
				offset = IN.worldPos.xz - _Corners.zw;
				float outOfBounds = max(offset.x, offset.y);
				offset = _Corners.xy - IN.worldPos.xz;
				outOfBounds = max(outOfBounds, max(offset.x, offset.y));
				// Reject fragments that are outside the clipping volume.
				clip(-outOfBounds);

				// Default surface shading.
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			// Note that the non-clipped Diffuse material will be used for shadows.
			// If you need correct shadowing with clipped material, add a shadow pass
			// that includes the same clipping logic as above.
			FallBack "Diffuse"
}




/*
1. 
Clipping shader works on 3d objects like the cube, 
but for some reason doesn work on the map. 
Clue: mesh renderer doesn't work on wrld map object because toggling receive shadows doesn't affect it. 
maybe i neeed to go into the code to change the clipping stuff.
Another solution: dont attach shader to the map, but attach it to a separate box?
How to do this?

2. 
trouble with finding the api call to change the lat and long. 
couldnt find the function to change the latitude/longitude. Only found SetOriginPoint, which changes the center of the map to that latitude longitude but doesn't generate any new map.

3. 
Need help with the calculation part.
*/

