using System;
using System.IO;
using System.Linq;

namespace ReflectionTools
{
	/// <summary>
	/// Provides methods for reading and validating CLR assembly files.
	/// </summary>
	public class AssemblyFile
	{
		// A 2-byte signature that identifies the file as an MS-DOS compatible image file.
		// This signature is "MZ" (which stands for Mark Zbikowski).
		private static readonly byte[] MZSignature = { 0x4D, 0x5A };
		// A 4-byte signature that identifies the file as a PE format image file.
		// This signature is "PE\0\0" (the letters "P" and "E" followed by two null bytes).
		private static readonly byte[] PESignature = { 0x50, 0x45, 0x00, 0x00 };
		// A 2-byte value that specifies the target machine of the image.
		private static readonly byte[] AMD64Machine = { 0x64, 0x86 };
		// A 2-byte magic number that determines whether it is a PE32 or PE32+ image.
		private static readonly byte[] PEMagic = { 0x0b, 0x01 };
		// Section name of the .text section. We search this to find the CLR header.
		private static readonly byte[] TextSection = { 0x2e, 0x74, 0x65, 0x78, 0x74, 0x00, 0x00, 0x00 };
		// An 8-byte signature that identifies the file as a CLR assembly image.
		// The 48 is actually the fixed size of the CLR header and the other numbers represent
		// the CLR version, which is always fixed at 2.5.
		private static readonly byte[] CLRSignature = { 0x48, 0x00, 0x00, 0x00, 0x02, 0x00, 0x05, 0x00 };

		/// <summary>
		/// Returns a value indicating whether or not the specified file contains a valid CLR assembly.
		/// </summary>
		/// <param name="path">
		/// The file system location of the file to check.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the specified <paramref name="path"/> contains a valid CLR assembly;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This method provides a method to determine whether a specified file is a CLR assembly. Using
		/// this method is significantly faster and safer than trying to load an assembly and catching
		/// the <see cref="BadImageFormatException"/> that would occur when the specified file is not
		/// a valid assembly.
		/// </para>
		/// <para>
		/// When running in an x86 context, this method will also return <see langword="true"/> when the 
		/// specified file is in fact an x64 CLR assembly, even though you won't be able to load it.
		/// </para>
		/// <para>
		/// Additionally, this method might not detect assemblies linked to CLR version 1.1 or below.
		/// </para>
		/// </remarks>
		public static bool IsAssembly(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			using (var stream = File.OpenRead(path))
			{
				using (var reader = new BinaryReader(stream))
				{
					// The MS-DOS stub is a valid application that runs under MS-DOS. It is placed at the front of 
					// the EXE image.The linker places a default stub here, which prints out the message 
					// "This program cannot be run in DOS mode" when the image is run in MS-DOS.
					var mz = reader.ReadBytes(2);
					if (!MZSignature.SequenceEqual(mz))
					{
						return false;
					}

					// At location 0x3C, the stub has the file offset to the PE signature.
					stream.Position = 0x3C;
					var offset = reader.ReadUInt32();

					// After the MS-DOS stub, at the file offset specified at offset 0x3c, is a 4-byte signature
					// that identifies the file as a PE format image file.
					stream.Position = offset;
					var pe = reader.ReadBytes(4);
					if (!PESignature.SequenceEqual(pe))
					{
						return false;
					}

					// The number that identifies the type of target machine.
					var machine = reader.ReadBytes(2);

					// The number of sections. This indicates the size of the section table.
					var numberOfSections = reader.ReadUInt16();
					// The size of the optional header, which is required for executable files but not for object files.
					// This determines the start location of the section table.
					stream.Position += 12;
					var optionalHeaderSize = reader.ReadUInt16();
					stream.Position += 2;

					// The optional header magic number determines whether an image is a PE32 or PE32+ executable.
					var optionalHeaderOffset = stream.Position;
					var magic = reader.ReadBytes(2);
					offset = (uint)(PEMagic.SequenceEqual(magic) ? 94 : 110);

					// Read the 15th data directory entry to test for CLR header.
					stream.Position += offset + (14 * 8);
					var header = reader.ReadUInt64();
					if (header == 0)
					{
						return false;
					}

					// Skip past the optional header to the start of the section table.
					stream.Position = optionalHeaderOffset + optionalHeaderSize;

					// Read the section table, which is located directly after the PE header.
					bool sectionFound = false;
					for (int sectionIndex = 0; sectionIndex < numberOfSections; sectionIndex++)
					{
						var section = reader.ReadBytes(8);
						if (TextSection.SequenceEqual(section))
						{
							// We found the .text section in the section table.
							sectionFound = true;

							// The file pointer to the first page of the section within the COFF file.
							stream.Position += 12;
							offset = reader.ReadUInt32();

							stream.Position = offset;
							break;
						}

						// Skip past the section table entry.
						stream.Position += 32;
					}

					if (!sectionFound)
					{
						// No .text section was found, so we bail out.
						return false;
					}

					// We are now positioned right at the start of the .text section.
					if (!AMD64Machine.SequenceEqual(machine))
					{
						// Skip past the CLR loader stub. This contains a jump instruction that is used
						// to load mscoree.dll on 32-bit Windows.
						stream.Position += 8;
					}

					// Read the CLR header and verify if this is a true CLR assembly.
					var clr = reader.ReadBytes(8);
					if (CLRSignature.SequenceEqual(clr))
					{
						return true;
					}
				}
			}

			// We tried just about everything, but failed to identify this as a CLR assembly.
			return false;
		}
	}
}