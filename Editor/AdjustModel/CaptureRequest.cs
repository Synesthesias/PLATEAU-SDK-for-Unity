using PLATEAU.Util;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Editor.AdjustModel
{
    public class CaptureRequest
    {
        /// <summary>
        /// 一つの面に関するリクエスト情報
        /// </summary>
        public record FaceRequest
        {
            /// <summary>
            /// 面の法線方向
            /// </summary>
            public Vector3 Direction { get; }

            /// <summary>
            /// Upベクトル
            /// </summary>
            public Vector3 Up { get; }

            /// <summary>
            /// 画像サイズ
            /// </summary>
            public Vector2Int ImageSize { get; }

            public FaceRequest(Vector3 direction, Vector3 up, Vector2 imageSize)
            {
                Direction = direction;
                Up = up;
                ImageSize = imageSize.FloorToInt();
            }
        }

        /// <summary>
        /// メッシュコード
        /// </summary>
        public string MeshCode { get; }

        /// <summary>
        /// バウンディングボックス
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// シェーダパラメータ) X方向の正規化係数
        /// </summary>
        public Vector3 XCoef { get; }

        /// <summary>
        /// シェーダパラメータ) Y方向の正規化係数
        /// </summary>
        public Vector3 YCoef { get; }

        /// <summary>
        /// シェーダパラメータ) Z方向の正規化係数
        /// </summary>
        public Vector3 ZCoef { get; }

        /// <summary>
        /// 解像度用
        /// </summary>
        public float PixelsPerMeter { get; }

        /// <summary>
        /// 各面ごとの情報
        /// </summary>
        public IReadOnlyDictionary<Lod1TextureCaptureService.Face, FaceRequest> Faces { get; }

        public CaptureRequest(string meshCode, Bounds bounds, float pixelsPerMeter)
        {
            MeshCode = meshCode;
            Bounds = bounds;
            PixelsPerMeter = pixelsPerMeter;

            var size = Vector3.Max(Bounds.size, Vector3.one * 1e-6f);
            XCoef = size.RevScaled();
            YCoef = size.RevScaled();
            ZCoef = size.RevScaled();
            Faces = new Dictionary<Lod1TextureCaptureService.Face, FaceRequest>
            {
                [Lod1TextureCaptureService.Face.Front] = new FaceRequest(Vector3.forward, Vector3.up, size.Xy()),
                [Lod1TextureCaptureService.Face.Back] = new FaceRequest(Vector3.back, Vector3.up, size.Xy()),
                [Lod1TextureCaptureService.Face.Left] = new FaceRequest(Vector3.left, Vector3.up, size.Zy()),
                [Lod1TextureCaptureService.Face.Right] = new FaceRequest(Vector3.right, Vector3.up, size.Zy()),
                [Lod1TextureCaptureService.Face.Top] = new FaceRequest(Vector3.up, Vector3.forward, size.Xz())
            };
        }
    }
}