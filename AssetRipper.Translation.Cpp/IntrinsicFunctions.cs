﻿using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetRipper.Translation.Cpp;

#pragma warning disable IDE0060 // Remove unused parameter
internal static partial class IntrinsicFunctions
{
	public static class Implemented
	{
	}
	public static class Unimplemented
	{
	}

	[MangledName("puts")]
	public unsafe static int PutString(sbyte* P_0)
	{
		try
		{
			string? s = Marshal.PtrToStringAnsi((IntPtr)P_0); // Maybe UTF-8?
			Console.WriteLine(s);
			return 0;
		}
		catch
		{
			return -1;
		}
	}

	[DoesNotReturn]
	[MangledName("_wassert")]
	public unsafe static void Assert(char* message, char* file, uint line)
	{
		// This needs to be switched to an emulated exception because _wassert exceptions can be caught by C++ code.
		throw new Exception($"Assertion failed: {Marshal.PtrToStringUni((IntPtr)message)} at {Marshal.PtrToStringUni((IntPtr)file)}:{line}");
	}

	/// <summary>
	/// Triggers a fatal exception, indicating a critical assertion failure in the application.
	/// </summary>
	/// <remarks>
	/// This aligns with the C++ behavior, which causes the application to crash and triggers the Windows Error Reporting (WER) system (aka "Watson").
	/// </remarks>
	/// <param name="expression">A pointer to the string representation of the failed assertion expression.</param>
	/// <param name="function">A pointer to the string representation of the function name where the assertion failed.</param>
	/// <param name="file">A pointer to the string representation of the file name where the assertion failed.</param>
	/// <param name="line">The line number in the file where the assertion failed.</param>
	/// <param name="reserved">Reserved for future use. Currently unused. C++ type is uintptr_t.</param>
	/// <exception cref="FatalException">
	/// Always thrown to indicate a fatal assertion failure. The exception message includes details about the failed
	/// assertion, such as the expression, function, file, and line number.
	/// </exception>
	[DoesNotReturn]
	[MangledName("_invoke_watson")]
	public unsafe static void InvokeWatson(char* expression, char* function, char* file, int line, long reserved)
	{
		throw new FatalException($"Fatal assertion failed: {Marshal.PtrToStringUni((IntPtr)expression)} in {Marshal.PtrToStringUni((IntPtr)function)} at {Marshal.PtrToStringUni((IntPtr)file)}:{line}");
	}

	[DoesNotReturn]
	[MangledName("__std_terminate")]
	public static void Terminate()
	{
		throw new FatalException(nameof(Terminate));
	}

	[MangledName("llvm.va.start")]
	public unsafe static void llvm_va_start(void** va_list)
	{
		// Handled elsewhere.
		throw new NotSupportedException();
	}

	[MangledName("llvm.va.copy")]
	public unsafe static void llvm_va_copy(void** destination, void** source)
	{
		*destination = *source;
	}

	[MangledName("llvm.va.end")]
	public unsafe static void llvm_va_end(void** va_list)
	{
		// Do nothing because it's freed automatically.
	}

	[MangledName("llvm.memcpy.p0.p0.i32")]
	public unsafe static void llvm_memcpy_p0_p0_i32(void* destination, void* source, int length, bool isVolatile)
	{
		Unsafe.CopyBlock(destination, source, (uint)length);
	}

	[MangledName("llvm.memcpy.p0.p0.i64")]
	public unsafe static void llvm_memcpy_p0_p0_i64(void* destination, void* source, long length, bool isVolatile)
	{
		Unsafe.CopyBlock(destination, source, (uint)length);
	}

	[MangledName("llvm.memmove.p0.p0.i32")]
	public unsafe static void llvm_memmove_p0_p0_i32(void* destination, void* source, int length, bool isVolatile)
	{
		// Same as memcpy, except that the source and destination are allowed to overlap.
		byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
		Span<byte> span = new(buffer, 0, length);
		new ReadOnlySpan<byte>(source, length).CopyTo(span);
		span.CopyTo(new Span<byte>(destination, length));
		ArrayPool<byte>.Shared.Return(buffer);
	}

	[MangledName("llvm.memmove.p0.p0.i64")]
	public unsafe static void llvm_memmove_p0_p0_i64(void* destination, void* source, long length, bool isVolatile)
	{
		llvm_memmove_p0_p0_i32(destination, source, (int)length, isVolatile);
	}

	[MangledName("llvm.memset.p0.i32")]
	public unsafe static void llvm_memset_p0_i32(void* destination, sbyte value, int length, bool isVolatile)
	{
		new Span<byte>(destination, length).Fill(unchecked((byte)value));
	}

	[MangledName("llvm.memset.p0.i64")]
	public unsafe static void llvm_memset_p0_i64(void* destination, sbyte value, long length, bool isVolatile)
	{
		llvm_memset_p0_i32(destination, value, (int)length, isVolatile);
	}

	[MangledName("malloc")]
	[MangledName("??2@YAPEAX_K@Z")] // new
	public unsafe static void* Alloc(long size)
	{
		return (void*)Marshal.AllocHGlobal((nint)size);
	}

	[MangledName("realloc")]
	public unsafe static void* ReAlloc(void* ptr, long size)
	{
		return (void*)Marshal.ReAllocHGlobal((nint)ptr, (nint)size);
	}

	[MangledName("free")]
	public unsafe static void Free(void* ptr)
	{
		Marshal.FreeHGlobal((IntPtr)ptr);
	}

	[MangledName("??3@YAXPEAX_K@Z")]
	public unsafe static void Delete(void* ptr, long size)
	{
		Marshal.FreeHGlobal((IntPtr)ptr);
	}

	[MangledName("expand")]
	public unsafe static void* Expand(void* ptr, long size)
	{
		// _expand is a non-standard function available in some C++ implementations, particularly in Microsoft C Runtime Library (CRT).
		// It is used to resize a previously allocated memory block without moving it, meaning it tries to expand or shrink the allocated memory in place.
		// _expand is mainly useful for optimizing performance in memory management when using Microsoft CRT.
		// If the block cannot be resized in place, _expand returns NULL, but the original block remains valid.

		// We take advantage of the fact that it's just an optimization and return null, signaling that we can't expand the memory in place.
		return null;
	}
}
#pragma warning restore IDE0060 // Remove unused parameter
