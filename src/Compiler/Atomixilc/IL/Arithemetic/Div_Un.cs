﻿using System;
using System.Collections.Generic;
using System.Reflection;

using Atomixilc.Machine;
using Atomixilc.Attributes;
using Atomixilc.Machine.x86;

namespace Atomixilc.IL
{
    [ILImpl(ILCode.Div_Un)]
    internal class Div_Un_il : MSIL
    {
        internal Div_Un_il()
            : base(ILCode.Div_Un)
        {

        }

        /*
         * URL : https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.Div_Un(v=vs.110).aspx
         * Description : Divides two unsigned integer values and pushes the result (int32) onto the evaluation stack.
         */
        internal override void Execute(Options Config, OpCodeType xOp, MethodBase method, Optimizer Optimizer)
        {
            if (Optimizer.vStack.Count < 2)
                throw new Exception("Internal Compiler Error: vStack.Count < 2");

            var divisor = Optimizer.vStack.Pop();
            var dividend = Optimizer.vStack.Pop();

            var size = Math.Max(Helper.GetTypeSize(divisor.OperandType, Config.TargetPlatform),
                Helper.GetTypeSize(dividend.OperandType, Config.TargetPlatform));

            /* The stack transitional behavior, in sequential order, is:
             * value1 is pushed onto the stack.
             * value2 is pushed onto the stack.
             * value2 and value1 are popped from the stack; value1 is divided by value2.
             * The result is pushed onto the stack.
             */

            switch (Config.TargetPlatform)
            {
                case Architecture.x86:
                    {
                        if (divisor.IsFloat || dividend.IsFloat || size > 4)
                            throw new Exception(string.Format("UnImplemented '{0}'", msIL));

                        if (divisor.RegisterRef.HasValue)
                            Optimizer.FreeRegister(divisor.RegisterRef.Value);

                        if (dividend.RegisterRef.HasValue)
                            Optimizer.FreeRegister(dividend.RegisterRef.Value);

                        if (divisor.SystemStack)
                        {
                            new Pop { DestinationReg = Register.ESI };
                        }

                        if (dividend.SystemStack)
                        {
                            new Pop { DestinationReg = Register.EAX };
                        }
                        else
                        {
                            new Mov
                            {
                                DestinationReg = Register.EAX,
                                SourceReg = dividend.RegisterRef,
                                SourceIndirect = dividend.IsIndirect,
                                SourceDisplacement = dividend.Displacement,
                                SourceRef = dividend.AddressRef
                            };
                        }

                        new Xor { DestinationReg = Register.EDX, SourceReg = Register.EDX };

                        if (divisor.SystemStack)
                        {
                            new Div { DestinationReg = Register.ESI };
                        }
                        else
                        {
                            new Div
                            {
                                DestinationReg = dividend.RegisterRef,
                                DestinationIndirect = dividend.IsIndirect,
                                DestinationDisplacement = dividend.Displacement,
                                DestinationRef = dividend.AddressRef
                            };
                        }

                        Register? NonVolatileRegister = null;
                        Optimizer.GetNonVolatileRegister(ref NonVolatileRegister);

                        if (NonVolatileRegister.HasValue)
                        {
                            new Mov
                            {
                                DestinationReg = NonVolatileRegister.Value,
                                SourceReg = Register.EAX
                            };
                            Optimizer.AllocateRegister(NonVolatileRegister.Value);
                            Optimizer.vStack.Push(new StackItem(NonVolatileRegister.Value, typeof(UInt32)));
                        }
                        else
                        {
                            new Push { DestinationReg = Register.EAX };
                            Optimizer.vStack.Push(new StackItem(typeof(UInt32)));
                        }
                    }
                    break;
                default:
                    throw new Exception(string.Format("Unsupported target platform '{0}' for MSIL '{1}'", Config.TargetPlatform, msIL));
            }
        }
    }
}