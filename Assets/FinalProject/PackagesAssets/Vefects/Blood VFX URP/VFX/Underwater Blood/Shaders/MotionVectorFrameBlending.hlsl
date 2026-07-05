float DecodeVec2(in float2 enc)
{
	float2 kDecodeDot = float2(1.0, 1 / 255.0);
	return dot(enc, kDecodeDot);
}

void ComputeMotionFlow_float(in float2 currentFrameUV, in float2 nextFrameUV, in float4 currentMV, in float2 nextMV, in float isIntensityEncoded, in float motionIntensity, in float columns, in float rows, in float blendFactor, out float2 currentFrameMVUV, out float2 nextFrameMVUV)
{
    currentFrameMVUV = float2(0.0, 0.0);
    nextFrameMVUV = float2(0.0, 0.0);

    //Decode intensity from blue and alpha if enabled
    float decodedIntensity = DecodeVec2(currentMV.ba);
    motionIntensity = lerp(motionIntensity, decodedIntensity, isIntensityEncoded);
	
    float2 strength = float2(motionIntensity, motionIntensity) * (float2(1.0, 1.0) / float2(columns, rows));
	
    currentFrameMVUV = currentMV.rg * 2.0 - 1.0;
    nextFrameMVUV = nextMV.rg * 2.0 - 1.0;
    currentFrameMVUV = currentFrameMVUV * (-strength * blendFactor);
    nextFrameMVUV = nextFrameMVUV * (strength * (1.0 - blendFactor));
    currentFrameMVUV += currentFrameUV;
    nextFrameMVUV += nextFrameUV;
}
