using System;
using System.Runtime.InteropServices;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.Texture
{
    public class HeightmapGenerator
    {
        public void GenerateFromMesh(PolygonMesh.Mesh InMesh, int TextureWidth, int TextureHeight, 
            PlateauVector2d Margin, out PlateauVector3d Min, out PlateauVector3d Max, 
            out PlateauVector2f MinUV, out PlateauVector2f MaxUV, out UInt16[] HeightData )
        {
            var apiResult =
                NativeMethods.heightmap_generator_generate_from_mesh(InMesh.Handle, TextureWidth, TextureHeight, new PlateauVector3d(Margin.X, Margin.Y, 0), CoordinateSystem.EUN, 
                out Min, out Max , out MinUV, out MaxUV, out IntPtr HeightmapDataPtr, out int DataSize);
            DLLUtil.CheckDllError(apiResult);

            byte[] outData = DLLUtil.PtrToBytes(HeightmapDataPtr, sizeof(UInt16) * DataSize);
            NativeMethods.release_heightmap_data(HeightmapDataPtr);

            HeightData = new UInt16[DataSize];
            int byteIndex = 0;
            for (int i = 0; i < HeightData.Length; i++)
            {
                HeightData[i] = BitConverter.ToUInt16(outData, byteIndex);
                byteIndex += 2;
            }
        }

        static public float[,] ConvertTo2DFloatArray(UInt16[] HeightData, int TextureWidth, int TextureHeight)
        {
            float[,] textureData = new float[TextureWidth, TextureHeight];
            int index = 0;
            for (int y = TextureHeight - 1; y >=0; y--)
            {
                for (int x = 0; x < TextureWidth; x++)
                {
                    textureData[y, x] = (float)(HeightData[index] / 65535f);
                    index++;
                }
            }
            return textureData; ;
        }

        static public void SavePngFile(string FileName, int Width, int Height, UInt16[] Data)
        {
            IntPtr heightmapDataPtr = Marshal.AllocHGlobal(sizeof(UInt16) * Data.Length);

            byte[] outData = new byte[sizeof(UInt16) * Data.Length];
            int byteIndex = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                byte[] datasets = BitConverter.GetBytes(Data[i]);
                for(int j = 0; j < datasets.Length; j++)
                {
                    outData[byteIndex] = datasets[j];
                    byteIndex++;
                }
            }
  
            Marshal.Copy(outData, 0, heightmapDataPtr, sizeof(UInt16) * Data.Length);
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(FileName);
            var apiResult =
                NativeMethods.heightmap_save_png_file(fileNameUtf8, Width, Height, heightmapDataPtr);         

            Marshal.FreeHGlobal(heightmapDataPtr);
            DLLUtil.CheckDllError(apiResult);
        }

        static public void ReadPngFile(string FileName, int Width, int Height, out UInt16[] Data)
        {
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(FileName);
            var apiResult =
                NativeMethods.heightmap_read_png_file(fileNameUtf8, Width, Height, out IntPtr heightmapDataPtr, out int dataSize);
            DLLUtil.CheckDllError(apiResult);

            byte[] outData = DLLUtil.PtrToBytes(heightmapDataPtr, sizeof(UInt16) * dataSize);
            NativeMethods.release_heightmap_data(heightmapDataPtr);

            Data = new UInt16[dataSize];
            
            int byteIndex = 0;
            for(int i = 0; i < Data.Length; i++)
            {
                Data[i] = BitConverter.ToUInt16(outData, byteIndex);
                byteIndex += 2;
            }

        }

        static public void SaveRawFile(string FileName, int Width, int Height, UInt16[] Data)
        {
            IntPtr heightmapDataPtr = Marshal.AllocHGlobal(sizeof(UInt16) * Data.Length);

            byte[] outData = new byte[sizeof(UInt16) * Data.Length];
            int byteIndex = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                byte[] datasets = BitConverter.GetBytes(Data[i]);
                for (int j = 0; j < datasets.Length; j++)
                {
                    outData[byteIndex] = datasets[j];
                    byteIndex++;
                }
            }

            Marshal.Copy(outData, 0, heightmapDataPtr, sizeof(UInt16) * Data.Length);
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(FileName);
            var apiResult =
                NativeMethods.heightmap_save_raw_file(fileNameUtf8, Width, Height, heightmapDataPtr);

            Marshal.FreeHGlobal(heightmapDataPtr);
            DLLUtil.CheckDllError(apiResult);
        }

        static public void ReadRawFile(string FileName, int Width, int Height, out UInt16[] Data)
        {
            var fileNameUtf8 = DLLUtil.StrToUtf8Bytes(FileName);
            var apiResult =
                NativeMethods.heightmap_read_raw_file(fileNameUtf8, Width, Height, out IntPtr heightmapDataPtr, out int DataSize);
            DLLUtil.CheckDllError(apiResult);

            byte[] outData = DLLUtil.PtrToBytes(heightmapDataPtr, sizeof(UInt16) * DataSize);
            NativeMethods.release_heightmap_data(heightmapDataPtr);

            Data = new UInt16[DataSize];
            int byteIndex = 0;
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = BitConverter.ToUInt16(outData, byteIndex);
                byteIndex += 2;
            }
        }

        private static class NativeMethods
        {
            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_generator_generate_from_mesh(
                [In] IntPtr srcMeshPtr,
                [In] int TextureWidth,
                [In] int TextureHeight,
                [In] PlateauVector3d Margin,
                [In] CoordinateSystem Coordinate,
                out PlateauVector3d Min,
                out PlateauVector3d Max,
                out PlateauVector2f MinUV,
                out PlateauVector2f MaxUV,
                out IntPtr HeightmapDataPtr,
                out int DataSize
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_save_png_file(
             [In] byte[] FileName,
             [In] int Width,
             [In] int Height,
             [In] IntPtr Data
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_read_png_file(
             [In] byte[] FileName,
             [In] int Width,
             [In] int Height,
             out IntPtr OutData,
             out int DataSize
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_save_raw_file(
             [In] byte[] FileName,
             [In] int Width,
             [In] int Height,
             [In] IntPtr Data
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult heightmap_read_raw_file(
             [In] byte[] FileName,
             [In] int Width,
             [In] int Height,
             out IntPtr OutData,
             out int DataSize
            );

            [DllImport(DLLUtil.DllName)]
            internal static extern APIResult release_heightmap_data(
             [In] IntPtr Data
            );
        }
    }


}
