﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using LLVMSharp.Interop;
using System.Diagnostics;

namespace AssetRipper.Translation.Cpp.Instructions;

internal sealed class GetElementPointerInstructionContext : InstructionContext
{
	internal GetElementPointerInstructionContext(LLVMValueRef instruction, BasicBlockContext block, FunctionContext function) : base(instruction, block, function)
	{
		Debug.Assert(Operands.Length >= 2);

		SourceElementTypeSignature = function.Module.GetTypeSignature(SourceElementType);
		FinalType = CalculateFinalType();
		ResultTypeSignature = FinalType.MakePointerType();
	}
	/// <summary>
	/// This is the pointer. It's generally void* due to stripping.
	/// </summary>
	public LLVMValueRef SourceOperand => Operands[0];
	public unsafe LLVMTypeRef SourceElementType => LLVM.GetGEPSourceElementType(Instruction);
	public TypeSignature SourceElementTypeSignature { get; set; }
	public TypeSignature FinalType { get; set; }
	public ReadOnlySpan<LLVMValueRef> IndexOperands => Operands.AsSpan()[1..];

	private TypeSignature CalculateFinalType()
	{
		TypeSignature currentType = SourceElementTypeSignature;
		for (int i = 2; i < Operands.Length; i++)
		{
			LLVMValueRef operand = Operands[i];
			if (currentType is TypeDefOrRefSignature structTypeSignature)
			{
				TypeDefinition structType = (TypeDefinition)structTypeSignature.ToTypeDefOrRef();
				if (operand.Kind == LLVMValueKind.LLVMConstantIntValueKind)
				{
					long index = operand.ConstIntSExt;
					string fieldName = $"field_{index}";
					FieldDefinition field = structType.Fields.First(t => t.Name == fieldName);
					currentType = field.Signature!.FieldType;
				}
				else
				{
					throw new NotSupportedException();
				}
			}
			else if (currentType is CorLibTypeSignature)
			{
				throw new NotSupportedException();
			}
			else
			{
				throw new NotSupportedException();
			}
		}
		return currentType;
	}
}
