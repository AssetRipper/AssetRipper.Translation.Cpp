﻿using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using LLVMSharp.Interop;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Translation.Cpp;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class InstructionContext
{
	protected InstructionContext(LLVMValueRef instruction, BasicBlockContext block, FunctionContext function)
	{
		Instruction = instruction;
		Function = function;
		Block = block;
		Operands = instruction.GetOperands();
	}

	public static InstructionContext Create(LLVMValueRef instruction, BasicBlockContext block, FunctionContext function)
	{
		return instruction.InstructionOpcode switch
		{
			LLVMOpcode.LLVMAlloca => new AllocaInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMLoad => new LoadInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMStore => new StoreInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMCall => new CallInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMICmp => new IntegerComparisonInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMFCmp => new FloatComparisonInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMZExt or LLVMOpcode.LLVMSExt or LLVMOpcode.LLVMTrunc or LLVMOpcode.LLVMFPExt or LLVMOpcode.LLVMFPTrunc => new NumericConversionInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMBr => instruction.IsConditional
				? new ConditionalBranchInstructionContext(instruction, block, function)
				: new UnconditionalBranchInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMRet => new ReturnInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMPHI => new PhiInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMGetElementPtr => new GetElementPointerInstructionContext(instruction, block, function),
			LLVMOpcode.LLVMSwitch => new SwitchBranchInstructionContext(instruction, block, function),
			_ when UnaryMathInstructionContext.Supported(instruction.InstructionOpcode) => new UnaryMathInstructionContext(instruction, block, function),
			_ when BinaryMathInstructionContext.Supported(instruction.InstructionOpcode) => new BinaryMathInstructionContext(instruction, block, function),
			_ => new InstructionContext(instruction, block, function),
		};
	}

	public LLVMOpcode Opcode => Instruction.InstructionOpcode;
	public LLVMValueRef Instruction { get; }
	public CilInstructionCollection CilInstructions => Function.CilInstructions;
	public FunctionContext Function { get; }
	public BasicBlockContext Block { get; }
	public LLVMValueRef[] Operands { get; }
	public List<InstructionContext> Loads { get; } = new();
	public List<InstructionContext> Stores { get; } = new();
	public List<InstructionContext> Accessors { get; } = new();
	public TypeSignature? ResultTypeSignature { get; set; }
	[MemberNotNullWhen(true, nameof(ResultTypeSignature))]
	public bool HasResult => ResultTypeSignature is not null and not CorLibTypeSignature { ElementType: ElementType.Void };

	public TypeSignature GetOperandTypeSignature(int index)
	{
		return Function.GetOperandTypeSignature(Operands[index]);
	}

	public void LoadOperand(int index, out TypeSignature typeSignature)
	{
		Function.LoadOperand(Operands[index], out typeSignature);
	}

	public void LoadOperand(int index)
	{
		Function.LoadOperand(Operands[index]);
	}

	private string GetDebuggerDisplay()
	{
		return Instruction.ToString();
	}
}
