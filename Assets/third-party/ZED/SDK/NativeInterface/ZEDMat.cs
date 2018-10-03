//======= Copyright (c) Stereolabs Corporation, All rights reserved. ===============
//Wrapper to the Mat of the ZED

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace sl
{
    [StructLayout(LayoutKind.Sequential)]
    public struct char2
    {
        public byte r;
        public byte g;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct char3
    {
        public byte r;
        public byte g;
        public byte b;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct char4
    {
        [MarshalAs(UnmanagedType.U1)] public byte r;
        [MarshalAs(UnmanagedType.U1)] public byte g;
        [MarshalAs(UnmanagedType.U1)] public byte b;
        [MarshalAs(UnmanagedType.U1)] public byte a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct float2
    {
        public float r;
        public float g;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct float3
    {
        public float r;
        public float g;
        public float b;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct float4
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    public class ZEDMat
    {
        public enum COPY_TYPE
        {
            COPY_TYPE_CPU_CPU, /*!< copy data from CPU to CPU.*/
            COPY_TYPE_CPU_GPU, /*!< copy data from CPU to GPU.*/
            COPY_TYPE_GPU_GPU, /*!< copy data from GPU to GPU.*/
            COPY_TYPE_GPU_CPU /*!< copy data from GPU to CPU.*/
        }

        public enum MAT_TYPE
        {
            MAT_32F_C1, /*!< float 1 channel.*/
            MAT_32F_C2, /*!< float 2 channels.*/
            MAT_32F_C3, /*!< float 3 channels.*/
            MAT_32F_C4, /*!< float 4 channels.*/
            MAT_8U_C1, /*!< unsigned char 1 channel.*/
            MAT_8U_C2, /*!< unsigned char 2 channels.*/
            MAT_8U_C3, /*!< unsigned char 3 channels.*/
            MAT_8U_C4 /*!< unsigned char 4 channels.*/
        }

        public enum MEM
        {
            MEM_CPU = 1, /*!< CPU Memory (Processor side).*/
            MEM_GPU = 2 /*!< GPU Memory (Graphic card side).*/
        }

        private const string nameDll = "sl_unitywrapper";

        /// <summary>
        ///     Creates an empty mat
        /// </summary>
        public ZEDMat()
        {
            MatPtr = dllz_mat_create_new_empty();
        }

        /// <summary>
        ///     Creates a mat from
        /// </summary>
        /// <param name="ptr"></param>
        public ZEDMat(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) throw new Exception("ZED Mat not initialized");
            MatPtr = ptr;
        }

        /// <summary>
        ///     Creates a Mat with a resolution
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="type"></param>
        /// <param name="mem"></param>
        public ZEDMat(Resolution resolution, MAT_TYPE type, MEM mem = MEM.MEM_CPU)
        {
            MatPtr = dllz_mat_create_new(resolution, (int) type, (int) mem);
        }

        /// <summary>
        ///     Creates a Mat
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="type"></param>
        /// <param name="mem"></param>
        public ZEDMat(uint width, uint height, MAT_TYPE type, MEM mem = MEM.MEM_CPU)
        {
            MatPtr = dllz_mat_create_new(new Resolution(width, height), (int) type, (int) mem);
        }

        /// <summary>
        ///     Returns the internal ptr of a mat
        /// </summary>
        public IntPtr MatPtr { get; private set; }

        [DllImport(nameDll, EntryPoint = "dllz_mat_create_new")]
        private static extern IntPtr dllz_mat_create_new(Resolution resolution, int type, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_create_new_empty")]
        private static extern IntPtr dllz_mat_create_new_empty();


        [DllImport(nameDll, EntryPoint = "dllz_mat_is_init")]
        private static extern bool dllz_mat_is_init(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_free")]
        private static extern bool dllz_mat_free(IntPtr ptr, int type);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_infos")]
        private static extern bool dllz_mat_get_infos(IntPtr ptr, byte[] buffer);


        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_float")]
        private static extern int dllz_mat_get_value_float(IntPtr ptr, int x, int y, out float value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_float2")]
        private static extern int dllz_mat_get_value_float2(IntPtr ptr, int x, int y, out float2 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_float3")]
        private static extern int dllz_mat_get_value_float3(IntPtr ptr, int x, int y, out float3 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_float4")]
        private static extern int dllz_mat_get_value_float4(IntPtr ptr, int x, int y, out float4 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_uchar")]
        private static extern int dllz_mat_get_value_uchar(IntPtr ptr, int x, int y, out byte value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_uchar2")]
        private static extern int dllz_mat_get_value_uchar2(IntPtr ptr, int x, int y, out char2 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_uchar3")]
        private static extern int dllz_mat_get_value_uchar3(IntPtr ptr, int x, int y, out char3 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_value_uchar4")]
        private static extern int dllz_mat_get_value_uchar4(IntPtr ptr, int x, int y, out char4 value, int mem);


        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_float")]
        private static extern int dllz_mat_set_value_float(IntPtr ptr, int x, int y, ref float value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_float2")]
        private static extern int dllz_mat_set_value_float2(IntPtr ptr, int x, int y, ref float2 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_float3")]
        private static extern int dllz_mat_set_value_float3(IntPtr ptr, int x, int y, ref float3 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_float4")]
        private static extern int dllz_mat_set_value_float4(IntPtr ptr, int x, int y, ref float4 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_uchar")]
        private static extern int dllz_mat_set_value_uchar(IntPtr ptr, int x, int y, ref byte value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_uchar2")]
        private static extern int dllz_mat_set_value_uchar2(IntPtr ptr, int x, int y, ref char2 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_uchar3")]
        private static extern int dllz_mat_set_value_uchar3(IntPtr ptr, int x, int y, ref char3 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_value_uchar4")]
        private static extern int dllz_mat_set_value_uchar4(IntPtr ptr, int x, int y, ref char4 value, int mem);


        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_float")]
        private static extern int dllz_mat_set_to_float(IntPtr ptr, ref float value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_float2")]
        private static extern int dllz_mat_set_to_float2(IntPtr ptr, ref float2 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_float3")]
        private static extern int dllz_mat_set_to_float3(IntPtr ptr, ref float3 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_float4")]
        private static extern int dllz_mat_set_to_float4(IntPtr ptr, ref float4 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_uchar")]
        private static extern int dllz_mat_set_to_uchar(IntPtr ptr, ref byte value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_uchar2")]
        private static extern int dllz_mat_set_to_uchar2(IntPtr ptr, ref char2 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_uchar3")]
        private static extern int dllz_mat_set_to_uchar3(IntPtr ptr, ref char3 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_to_uchar4")]
        private static extern int dllz_mat_set_to_uchar4(IntPtr ptr, ref char4 value, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_update_cpu_from_gpu")]
        private static extern int dllz_mat_update_cpu_from_gpu(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_update_gpu_from_cpu")]
        private static extern int dllz_mat_update_gpu_from_cpu(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_read")]
        private static extern int dllz_mat_read(IntPtr ptr, string filePath);

        [DllImport(nameDll, EntryPoint = "dllz_mat_write")]
        private static extern int dllz_mat_write(IntPtr ptr, string filePath);

        [DllImport(nameDll, EntryPoint = "dllz_mat_copy_to")]
        private static extern int dllz_mat_copy_to(IntPtr ptr, IntPtr dest, int cpyType);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_width")]
        private static extern int dllz_mat_get_width(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_height")]
        private static extern int dllz_mat_get_height(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_channels")]
        private static extern int dllz_mat_get_channels(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_memory_type")]
        private static extern int dllz_mat_get_memory_type(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_pixel_bytes")]
        private static extern int dllz_mat_get_pixel_bytes(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_step")]
        private static extern int dllz_mat_get_step(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_step_bytes")]
        private static extern int dllz_mat_get_step_bytes(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_width_bytes")]
        private static extern int dllz_mat_get_width_bytes(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_is_memory_owner")]
        private static extern bool dllz_mat_is_memory_owner(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_resolution")]
        private static extern Resolution dllz_mat_get_resolution(IntPtr ptr);

        [DllImport(nameDll, EntryPoint = "dllz_mat_alloc")]
        private static extern void dllz_mat_alloc(IntPtr ptr, int width, int height, int type, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_set_from")]
        private static extern int dllz_mat_set_from(IntPtr ptr, IntPtr source, int copyType);

        [DllImport(nameDll, EntryPoint = "dllz_mat_get_ptr")]
        private static extern IntPtr dllz_mat_get_ptr(IntPtr ptr, int mem);

        [DllImport(nameDll, EntryPoint = "dllz_mat_clone")]
        private static extern void dllz_mat_clone(IntPtr ptr, IntPtr ptrSource);

        /// <summary>
        ///     Defines whether the Mat is initialized or not.
        /// </summary>
        /// <returns></returns>
        public bool IsInit()
        {
            return dllz_mat_is_init(MatPtr);
        }

        /// <summary>
        ///     Free the memory of the Mat
        /// </summary>
        /// <param name="mem"></param>
        public void Free(MEM mem = MEM.MEM_GPU | MEM.MEM_CPU)
        {
            dllz_mat_free(MatPtr, (int) mem);
            MatPtr = IntPtr.Zero;
        }

        /// <summary>
        ///     Downloads data from DEVICE (GPU) to HOST (CPU), if possible.
        /// </summary>
        /// <returns></returns>
        public ERROR_CODE UpdateCPUFromGPU()
        {
            return (ERROR_CODE) dllz_mat_update_cpu_from_gpu(MatPtr);
        }

        /// <summary>
        ///     Uploads data from HOST (CPU) to DEVICE (GPU), if possible.
        /// </summary>
        /// <returns></returns>
        public ERROR_CODE UpdateGPUFromCPU()
        {
            return (ERROR_CODE) dllz_mat_update_gpu_from_cpu(MatPtr);
        }

        /// <summary>
        ///     Return the informations about the Mat
        /// </summary>
        /// <returns></returns>
        public string GetInfos()
        {
            var buf = new byte[300];
            dllz_mat_get_infos(MatPtr, buf);
            return Encoding.ASCII.GetString(buf);
        }

        /// <summary>
        ///     Copies data an other Mat (deep copy).
        /// </summary>
        /// <param name="dest">dst : the Mat where the data will be copied.</param>
        /// <param name="copyType">cpyType : specify the memories that will be used for the copy.</param>
        /// <returns></returns>
        public ERROR_CODE CopyTo(ZEDMat dest, COPY_TYPE copyType = COPY_TYPE.COPY_TYPE_CPU_CPU)
        {
            return (ERROR_CODE) dllz_mat_copy_to(MatPtr, dest.MatPtr, (int) copyType);
        }

        /// <summary>
        ///     Reads an image from a file (only if \ref MEM_CPU is available on the current
        /// </summary>
        /// <param name="filePath"> file path including the name and extension.</param>
        /// <returns></returns>
        public ERROR_CODE Read(string filePath)
        {
            return (ERROR_CODE) dllz_mat_read(MatPtr, filePath);
        }

        /// <summary>
        ///     Writes the Mat (only if MEM_CPU is available) into a file as an image.
        /// </summary>
        /// <param name="filePath">file path including the name and extension.</param>
        /// <returns></returns>
        public ERROR_CODE Write(string filePath)
        {
            return (ERROR_CODE) dllz_mat_write(MatPtr, filePath);
        }

        /// <summary>
        ///     Returns the width of the matrix.
        /// </summary>
        /// <returns></returns>
        public int GetWidth()
        {
            return dllz_mat_get_width(MatPtr);
        }

        /// <summary>
        ///     Returns the height of the matrix.
        /// </summary>
        /// <returns></returns>
        public int GetHeight()
        {
            return dllz_mat_get_height(MatPtr);
        }

        /// <summary>
        ///     Returns the number of values stored in one pixel.
        /// </summary>
        /// <returns></returns>
        public int GetChannels()
        {
            return dllz_mat_get_channels(MatPtr);
        }

        /// <summary>
        ///     Returns the size in bytes of one pixel.
        /// </summary>
        /// <returns></returns>
        public int GetPixelBytes()
        {
            return dllz_mat_get_pixel_bytes(MatPtr);
        }

        /// <summary>
        ///     Returns the memory step in number of elements (the number of values in one pixel row).
        /// </summary>
        /// <returns></returns>
        public int GetStep()
        {
            return dllz_mat_get_step(MatPtr);
        }

        /// <summary>
        ///     Returns the memory step in Bytes (the Bytes size of one pixel row).
        /// </summary>
        /// <returns></returns>
        public int GetStepBytes()
        {
            return dllz_mat_get_step_bytes(MatPtr);
        }

        /// <summary>
        ///     Returns the size in bytes of a row.
        /// </summary>
        /// <returns></returns>
        public int GetWidthBytes()
        {
            return dllz_mat_get_width_bytes(MatPtr);
        }

        /// <summary>
        ///     Returns the type of memory (CPU and/or GPU).
        /// </summary>
        /// <returns></returns>
        public MEM GetMemoryType()
        {
            return (MEM) dllz_mat_get_memory_type(MatPtr);
        }

        /// <summary>
        ///     Returns whether the Mat is the owner of the memory it access.
        /// </summary>
        /// <returns></returns>
        public bool IsMemoryOwner()
        {
            return dllz_mat_is_memory_owner(MatPtr);
        }

        public Resolution GetResolution()
        {
            return dllz_mat_get_resolution(MatPtr);
        }

        /// <summary>
        ///     Allocates the Mat memory.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="matType"></param>
        /// <param name="mem"></param>
        public void Alloc(uint width, uint height, MAT_TYPE matType, MEM mem = MEM.MEM_CPU)
        {
            dllz_mat_alloc(MatPtr, (int) width, (int) height, (int) matType, (int) mem);
        }

        /// <summary>
        ///     Allocates the Mat memory.
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="matType"></param>
        /// <param name="mem"></param>
        public void Alloc(Resolution resolution, MAT_TYPE matType, MEM mem = MEM.MEM_CPU)
        {
            dllz_mat_alloc(MatPtr, (int) resolution.width, (int) resolution.height, (int) matType, (int) mem);
        }

        /// <summary>
        ///     Copies data from an other Mat (deep copy).
        /// </summary>
        /// <param name="src"></param>
        /// <param name="copyType"></param>
        /// <returns></returns>
        public int SetFrom(ZEDMat src, COPY_TYPE copyType = COPY_TYPE.COPY_TYPE_CPU_CPU)
        {
            return dllz_mat_set_from(MatPtr, src.MatPtr, (int) copyType);
        }

        public IntPtr GetPtr(MEM mem = MEM.MEM_CPU)
        {
            return dllz_mat_get_ptr(MatPtr, (int) mem);
        }

        /// <summary>
        ///     Duplicates Mat by copy (deep copy).
        /// </summary>
        /// <param name="source"></param>
        public void Clone(ZEDMat source)
        {
            dllz_mat_clone(MatPtr, source.MatPtr);
        }

        /************ GET VALUES *********************/
        // Cannot send values by template, covariant issue with a out needed

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out float value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_float(MatPtr, x, y, out value, (int) mem);
        }

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out float2 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_float2(MatPtr, x, y, out value, (int) mem);
        }

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out float3 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_float3(MatPtr, x, y, out value, (int) mem);
        }

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out float4 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_float4(MatPtr, x, y, out value, (int) mem);
        }

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out byte value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_uchar(MatPtr, x, y, out value, (int) mem);
        }

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out char2 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_uchar2(MatPtr, x, y, out value, (int) mem);
        }

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out char3 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_uchar3(MatPtr, x, y, out value, (int) mem);
        }

        /// <summary>
        ///     Returns the value of a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE GetValue(int x, int y, out char4 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_get_value_uchar4(MatPtr, x, y, out value, (int) mem);
        }
        /***************************************************************************************/


        /************ SET VALUES *********************/
        // Cannot send values by template, covariant issue with a out needed

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, ref float value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_float(MatPtr, x, y, ref value, (int) mem);
        }

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, ref float2 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_float2(MatPtr, x, y, ref value, (int) mem);
        }

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, ref float3 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_float3(MatPtr, x, y, ref value, (int) mem);
        }

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, float4 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_float4(MatPtr, x, y, ref value, (int) mem);
        }

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, ref byte value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_uchar(MatPtr, x, y, ref value, (int) mem);
        }

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, ref char2 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_uchar2(MatPtr, x, y, ref value, (int) mem);
        }

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, ref char3 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_uchar3(MatPtr, x, y, ref value, (int) mem);
        }

        /// <summary>
        ///     Sets a value to a specific point in the matrix.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetValue(int x, int y, ref char4 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_value_uchar4(MatPtr, x, y, ref value, (int) mem);
        }
        /***************************************************************************************/

        /************ SET TO *********************/
        // Cannot send values by template, covariant issue with a out needed

        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref float value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_float(MatPtr, ref value, (int) mem);
        }


        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref float2 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_float2(MatPtr, ref value, (int) mem);
        }

        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref float3 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_float3(MatPtr, ref value, (int) mem);
        }

        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref float4 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_float4(MatPtr, ref value, (int) mem);
        }

        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref byte value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_uchar(MatPtr, ref value, (int) mem);
        }

        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref char2 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_uchar2(MatPtr, ref value, (int) mem);
        }

        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref char3 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_uchar3(MatPtr, ref value, (int) mem);
        }

        /// <summary>
        ///     ills the Mat with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mem"></param>
        /// <returns></returns>
        public ERROR_CODE SetTo(ref char4 value, MEM mem)
        {
            return (ERROR_CODE) dllz_mat_set_to_uchar4(MatPtr, ref value, (int) mem);
        }

        /***************************************************************************************/
    }
}