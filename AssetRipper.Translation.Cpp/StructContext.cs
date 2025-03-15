﻿using AsmResolver.DotNet;
using LLVMSharp.Interop;
using System.Diagnostics;

namespace AssetRipper.Translation.Cpp;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class StructContext : IHasName
{
	public string MangledName => Type.StructName;

	public string? DemangledName => null;

	public string CleanName { get; }

	public string Name
	{
		get => Definition.Name ?? "";
		set => Definition.Name = value;
	}

	public ModuleContext Module { get; }

	public TypeDefinition Definition { get; }

	public LLVMTypeRef Type { get; }

	public StructContext(ModuleContext module, TypeDefinition definition, LLVMTypeRef type)
	{
		Module = module;
		Definition = definition;
		Type = type;
		CleanName = ExtractCleanName(MangledName);
	}

	private static string RemovePrefix(string name, string prefix)
	{
		return name.StartsWith(prefix, StringComparison.Ordinal) ? name[prefix.Length..] : name;
	}

	private static string ExtractCleanName(string name)
	{
		name = RemovePrefix(name, "class.");
		name = RemovePrefix(name, "struct.");
		name = RemovePrefix(name, "union.");
		return NameGenerator.CleanName(name, "Struct");
	}

	private string GetDebuggerDisplay()
	{
		return CleanName;
	}
}
