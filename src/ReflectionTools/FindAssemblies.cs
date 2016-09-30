using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ReflectionTools
{
	/// <summary>
	/// Provides methods for finding and loading assemblies at various locations.
	/// </summary>
	public static class FindAssemblies
	{
		private static readonly string[] ExecutableExensions = { ".exe", ".dll" };

		/// <summary>
		/// Returns the assemblies loaded in the execution context of the current application domain.
		/// </summary>
		/// <returns>
		/// The collection of assemblies loaded in the execution context of the current application domain.
		/// </returns>
		public static IEnumerable<Assembly> InCurrentAppDomain()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		/// <summary>
		/// Returns the assemblies located in the base directory of the current application domain.
		/// </summary>
		/// <returns>
		/// The assemblies located in the base directory of the current application domain.
		/// </returns>
		public static IEnumerable<Assembly> InBaseDirectory()
		{
			return InDirectory(AppDomain.CurrentDomain.BaseDirectory);
		}

		/// <summary>
		/// Returns the assemblies located in the specified directory.
		/// </summary>
		/// <returns>
		/// The assemblies located in the specified directory.
		/// </returns>
		public static IEnumerable<Assembly> InDirectory(string path)
		{
			var directory = new DirectoryInfo(path);

			foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
			{
				if (HasExecutableExtension(file.FullName))
				{
					if (AssemblyFile.IsAssembly(file.FullName))
					{
						yield return Assembly.LoadFile(file.FullName);
					}
				}
			}
		}

		/// <summary>
		/// Returns the assemblies located in the global assembly cache (GAC).
		/// </summary>
		/// <returns>
		/// The assemblies located in the global assembly cache (GAC).
		/// </returns>
		public static IEnumerable<Assembly> InGlobalAssemblyCache()
		{
			Fusion.IAssemblyEnum assemblies;
			Fusion.CreateAssemblyEnum(out assemblies, IntPtr.Zero, null, Fusion.ASM_CACHE_FLAGS.ASM_CACHE_GAC, IntPtr.Zero);
			Fusion.IAssemblyName assembly;

			while (assemblies.GetNextAssembly(IntPtr.Zero, out assembly, 0U) == 0)
			{
				uint size = 0;
				const Fusion.ASM_DISPLAY_FLAGS displayFlags =
					Fusion.ASM_DISPLAY_FLAGS.VERSION |
					Fusion.ASM_DISPLAY_FLAGS.CULTURE |
					Fusion.ASM_DISPLAY_FLAGS.PUBLIC_KEY_TOKEN;

				assembly.GetDisplayName(null, ref size, displayFlags);
				StringBuilder assemblyNameBuilder = new StringBuilder((int)size);
				assembly.GetDisplayName(assemblyNameBuilder, ref size, displayFlags);

				AssemblyName assemblyName = new AssemblyName(assemblyNameBuilder.ToString());
				yield return Assembly.Load(assemblyName);
			}
		} 

		private static bool HasExecutableExtension(string path)
		{
			return ExecutableExensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
		}
	}
}