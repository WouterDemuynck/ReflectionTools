using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ReflectionTools
{
	internal static class Fusion
	{
		[DllImport("fusion.dll", PreserveSig = false)]
		internal static extern void CreateAssemblyEnum(out Fusion.IAssemblyEnum ppEnum, IntPtr pUnkReserved, Fusion.IAssemblyName pName, Fusion.ASM_CACHE_FLAGS dwFlags, IntPtr pvReserved);

		[Flags]
		internal enum ASM_CACHE_FLAGS
		{
			ASM_CACHE_ZAP = 1,
			ASM_CACHE_GAC = 2,
			ASM_CACHE_DOWNLOAD = 4,
		}

		[Flags]
		internal enum ASM_DISPLAY_FLAGS
		{
			VERSION = 1,
			CULTURE = 2,
			PUBLIC_KEY_TOKEN = 4,
			PUBLIC_KEY = 8,
			CUSTOM = 16,
			PROCESSORARCHITECTURE = 32,
			LANGUAGEID = 64,
		}

		[Flags]
		internal enum ASM_CMP_FLAGS
		{
			NAME = 1,
			MAJOR_VERSION = 2,
			MINOR_VERSION = 4,
			BUILD_NUMBER = 8,
			REVISION_NUMBER = 16,
			PUBLIC_KEY_TOKEN = 32,
			CULTURE = 64,
			CUSTOM = 128,
			ALL = CUSTOM | CULTURE | PUBLIC_KEY_TOKEN | REVISION_NUMBER | BUILD_NUMBER | MINOR_VERSION | MAJOR_VERSION | NAME,
			DEFAULT = 256,
		}

		[Guid("21B8916C-F28E-11D2-A473-00C04F8EF448")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[ComImport]
		internal interface IAssemblyEnum
		{
			[MethodImpl(MethodImplOptions.PreserveSig)]
			int GetNextAssembly(IntPtr pvReserved, out Fusion.IAssemblyName ppName, uint dwFlags);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int Reset();

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int Clone(out Fusion.IAssemblyEnum ppEnum);
		}

		internal enum ASM_NAME
		{
			PUBLIC_KEY,
			PUBLIC_KEY_TOKEN,
			HASH_VALUE,
			NAME,
			MAJOR_VERSION,
			MINOR_VERSION,
			BUILD_NUMBER,
			REVISION_NUMBER,
			CULTURE,
			PROCESSOR_ID_ARRAY,
			OSINFO_ARRAY,
			HASH_ALGID,
			ALIAS,
			CODEBASE_URL,
			CODEBASE_LASTMOD,
			NULL_PUBLIC_KEY,
			NULL_PUBLIC_KEY_TOKEN,
			CUSTOM,
			NULL_CUSTOM,
			MVID,
		}

		[Guid("CD193BC0-B4BC-11D2-9833-00C04FC31D2E")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[ComImport]
		internal interface IAssemblyName
		{
			[MethodImpl(MethodImplOptions.PreserveSig)]
			int SetProperty(Fusion.ASM_NAME PropertyId, IntPtr pvProperty, uint cbProperty);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int GetProperty(Fusion.ASM_NAME PropertyId, StringBuilder pvProperty, ref uint pcbProperty);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int Finalize();

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int GetDisplayName([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder szDisplayName, ref uint pccDisplayName, Fusion.ASM_DISPLAY_FLAGS dwDisplayFlags);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int BindToObject(ref Guid refIID, [MarshalAs(UnmanagedType.IUnknown)] object pUnkSink, [MarshalAs(UnmanagedType.IUnknown)] object pUnkContext, [MarshalAs(UnmanagedType.LPWStr)] string szCodeBase, long llFlags, IntPtr pvReserved, uint cbReserved, out IntPtr ppv);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int GetName(ref uint lpcwBuffer, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pwzName);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int IsEqual(Fusion.IAssemblyName pName, Fusion.ASM_CMP_FLAGS dwCmpFlags);

			[MethodImpl(MethodImplOptions.PreserveSig)]
			int Clone(out Fusion.IAssemblyName pName);
		}
	}
}
