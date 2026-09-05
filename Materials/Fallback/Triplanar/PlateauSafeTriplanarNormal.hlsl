#ifndef PLATEAU_SAFE_TRIPLANAR_NORMAL_INCLUDED
#define PLATEAU_SAFE_TRIPLANAR_NORMAL_INCLUDED

// 面積ゼロの面は幾何法線が (0,0,0) になり、Unity の Triplanar が blend /= |n| で NaN を出す。
// SafeNormalize だけではゼロのまま残るため、その場合だけオブジェクト/ワールドアップに置き換える。

void PlateauSafeTriplanarNormal_float(float3 _in, out float3 _out)
{
    float lenSq = dot(_in, _in);
    _out = (lenSq > 1e-8) ? _in * rsqrt(lenSq) : float3(0.0, 1.0, 0.0);
}

void PlateauSafeTriplanarNormal_half(half3 _in, out half3 _out)
{
    half lenSq = dot(_in, _in);
    _out = (lenSq > half(1e-5)) ? _in * rsqrt(lenSq) : half3(0.0, 1.0, 0.0);
}

#endif
