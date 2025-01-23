﻿using AssetRipper.Translation.Cpp.Extensions;
using AssetRipper.Translation.Cpp.Instructions;
using LLVMSharp.Interop;

namespace AssetRipper.Translation.Cpp;

internal sealed class BasicBlockContext
{
	public LLVMBasicBlockRef Block { get; }
	public FunctionContext Function { get; }
	public List<InstructionContext> Instructions { get; } = new();
	public List<BasicBlockContext> Predecessors { get; } = new();
	public List<BasicBlockContext> Successors { get; } = new();

	private BasicBlockContext(LLVMBasicBlockRef block, FunctionContext function)
	{
		Block = block;
		Function = function;
	}

	public static BasicBlockContext Create(LLVMBasicBlockRef block, FunctionContext function)
	{
		Stack<InstructionContext> stack = new();
		BasicBlockContext context = new(block, function);
		foreach (LLVMValueRef instruction in block.GetInstructions())
		{
			stack.Push(InstructionContext.Create(instruction, context, function));
			MaybeAddOperandsToStack(stack.Peek(), stack);
			context.Instructions.AddRange(stack);
			stack.Clear();
		}
		return context;

		static void MaybeAddOperandsToStack(InstructionContext instructionContext, Stack<InstructionContext> stack)
		{
			for (int i = instructionContext.Operands.Length - 1; i >= 0; i--)
			{
				LLVMValueRef operand = instructionContext.Operands[i];
				if (operand.Kind == LLVMValueKind.LLVMConstantExprValueKind)
				{
					InstructionContext operandInstruction = InstructionContext.Create(operand, instructionContext.Block, instructionContext.Function);
					stack.Push(operandInstruction);
					MaybeAddOperandsToStack(operandInstruction, stack);
				}
			}
		}
	}
}
