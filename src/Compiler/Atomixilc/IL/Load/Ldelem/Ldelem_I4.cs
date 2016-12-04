﻿using System;
using System.Collections.Generic;
using System.Reflection;

using Atomixilc.Machine;
using Atomixilc.Attributes;
using Atomixilc.Machine.x86;
using Atomixilc.IL.CodeType;

namespace Atomixilc.IL
{
    [ILImpl(ILCode.Ldelem_I4)]
    internal class Ldelem_I4_il : MSIL
    {
        internal Ldelem_I4_il()
            : base(ILCode.Ldelem_I4)
        {

        }

        /*
         * URL : https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.Ldelem_I4(v=vs.110).aspx
         * Description : Loads the element with type int32 at a specified array index onto the top of the evaluation stack as an int32.
         */
        internal override void Execute(Options Config, OpCodeType xOp, MethodBase method, Optimizer Optimizer)
        {
            if (Optimizer.vStack.Count < 2)
                throw new Exception("Internal Compiler Error: vStack.Count < 2");

            /* The stack transitional behavior, in sequential order, is:
             * An object reference array is pushed onto the stack.
             * An index value index is pushed onto the stack.
             * index and array are popped from the stack; the value stored at position index in array is looked up.
             * The value is pushed onto the stack.
             */

            Optimizer.vStack.Pop();
            Optimizer.vStack.Pop();

            switch (Config.TargetPlatform)
            {
                case Architecture.x86:
                    {
                        Ldelem_il.Executex86(4, true);
                        Optimizer.vStack.Push(new StackItem(typeof(int)));
                    }
                    break;
                default:
                    throw new Exception(string.Format("Unsupported target platform '{0}' for MSIL '{1}'", Config.TargetPlatform, msIL));
            }
        }
    }
}