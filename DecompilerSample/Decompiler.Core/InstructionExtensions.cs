using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace Decompiler.Core
{
	public static class InstructionExtensions
	{
		public static IEnumerable<Instruction> MoveBackwardFromSelf( this Instruction instruction )
		{
			yield return instruction;
			foreach ( var i in instruction.MoveBackward() )
			{
				yield return i;
			}
		}

		public static IEnumerable<Instruction> MoveBackward( this Instruction instruction )
		{
			var current = instruction;
			while ( current.Previous != null )
			{
				yield return current.Previous;
				current = current.Previous;
			}
		}
	}
}