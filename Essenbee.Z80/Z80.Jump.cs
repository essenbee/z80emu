namespace Essenbee.Z80
{
    public partial class Z80
    {
        // ========================================
        // Jump Group
        // ========================================

        // Instruction    : JP nn
        // Operation      : PC <- nn
        // Flags Affected : None
        private byte JPNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = Fetch1(RootInstructions);
            var addr = (ushort)((hiByte << 8) + loByte);

            PC = addr;

            ResetQ();
            MEMPTR = addr;

            return 0;
        }

        // Instruction    : JP cc, nn
        // Operation      : PC <- nn if cc is true
        // Flags Affected : None
        private byte JPCCNN(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = Fetch1(RootInstructions);
            var addr = (ushort)((hiByte << 8) + loByte);

            var cc = (opCode & 0b00111000) >> 3;

            if (EvaluateCC(cc))
            {
                PC = addr;
            }

            MEMPTR = addr;

            ResetQ();
            return 0;
        }

        // Instruction    : JR e
        // Operation      : PC <- PC + e
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JR(byte opCode)
        {
            var e = (sbyte)Fetch1(RootInstructions);

            PC = (ushort)(PC + e);

            ResetQ();
            MEMPTR = PC;

            return 0;
        }

        // Instruction    : JR C, e
        // Operation      : PC <- PC + e if Carry set
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JRC(byte opCode)
        {
            byte additionalTStates = 0;
            var e = (sbyte)Fetch1(RootInstructions);

            if (CheckFlag(Flags.C))
            {
                additionalTStates = 5;
                PC = (ushort)(PC + e);
                MEMPTR = PC;
            }

            ResetQ();
            return additionalTStates;
        }

        // Instruction    : JR NC, e
        // Operation      : PC <- PC + e if Carry not set
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JRNC(byte opCode)
        {
            byte additionalTStates = 0;
            var e = (sbyte)Fetch1(RootInstructions);

            if (!CheckFlag(Flags.C))
            {
                additionalTStates = 5;
                PC = (ushort)(PC + e);
                MEMPTR = PC;
            }

            ResetQ();
            return additionalTStates;
        }

        // Instruction    : JR Z, e
        // Operation      : PC <- PC + e if Zero flag set
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JRZ(byte opCode)
        {
            byte additionalTStates = 0;
            var e = (sbyte)Fetch1(RootInstructions);

            if (CheckFlag(Flags.Z))
            {
                additionalTStates = 5;
                PC = (ushort)(PC + e);
                MEMPTR = PC;
            }

            ResetQ();
            return additionalTStates;
        }

        // Instruction    : JR NZ, e
        // Operation      : PC <- PC + e if Zero flag not set
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte JRNZ(byte opCode)
        {
            byte additionalTStates = 0;
            var e = (sbyte)Fetch1(RootInstructions);

            if (!CheckFlag(Flags.Z))
            {
                additionalTStates = 5;
                PC = (ushort)(PC + e);
                MEMPTR = PC;
            }

            ResetQ();
            return additionalTStates;
        }

        // Instruction    : JP HL
        // Operation      : PC <- HL
        // Flags Affected : None
        // Notes          : -
        private byte JPHL(byte opCode)
        {
            PC = HL;

            ResetQ();
            return 0;
        }

        // Instruction    : JP IX
        // Operation      : PC <- IX
        // Flags Affected : None
        // Notes          : -
        private byte JPIX(byte opCode)
        {
            PC = IX;

            ResetQ();
            return 0;
        }

        // Instruction    : JP IY
        // Operation      : PC <- IY
        // Flags Affected : None
        // Notes          : -
        private byte JPIY(byte opCode)
        {
            PC = IY;

            ResetQ();
            return 0;
        }

        // Instruction    : DJNZ e
        // Operation      : Decrement B and jump if NZ (PC <- PC + e)
        // Flags Affected : None
        // Notes          : Assembler with compensate automatically for the twice-incremented PC
        private byte DJNZ(byte opCode)
        {
            byte additionalTStates = 0;
            var e = (sbyte)Fetch1(RootInstructions);

            var initialVal = B;
            B--;
            SetDecFlags(initialVal, B);

            if (!CheckFlag(Flags.Z))
            {
                additionalTStates = 5;
                PC = (ushort)(PC + e);
                MEMPTR = PC;
            }

            ResetQ();

            return additionalTStates;
        }

        // ========================================
        // Call and Return Group
        // ========================================

        // Instruction    : CALL nn
        // Operation      : Push PC onto Stack, then PC <- nn
        // Flags Affected : None

        private byte CALL(byte opCode)
        {
            var loByte = Fetch1(RootInstructions);
            var hiByte = Fetch1(RootInstructions);

            PushProgramCounter();
            var addr = (ushort)((hiByte << 8) + loByte);
            PC = addr;
            MEMPTR = addr;
            ResetQ();

            return 0;
        }

        // Instruction    : CALL cc,nn
        // Operation      : Conditionally push PC onto Stack, then PC <- nn
        // Flags Affected : None

        private byte CALLCC(byte opCode)
        {
            byte additionalTStates = 0;
            var loByte = Fetch1(RootInstructions);
            var hiByte = Fetch1(RootInstructions);

            var cc = (opCode & 0b00111000) >> 3;
            var addr = (ushort)((hiByte << 8) + loByte);

            if (EvaluateCC(cc))
            {
                PushProgramCounter();
                PC = addr;
                additionalTStates = 7;
            }

            MEMPTR = addr;
            ResetQ();

            return additionalTStates;
        }

        // Instruction    : RET
        // Operation      : Pop PC
        // Flags Affected : None

        private byte RET(byte opCode)
        {
            PopProgramCounter();
            MEMPTR = PC;
            ResetQ();

            return 0;
        }

        // Instruction    : RET cc
        // Operation      : Conditionally POP PCn
        // Flags Affected : None

        private byte RETCC(byte opCode)
        {
            byte additionalTStates = 0;
            var cc = (opCode & 0b00111000) >> 3;

            if (EvaluateCC(cc))
            {
                PopProgramCounter();
                MEMPTR = PC;
                additionalTStates = 6;
            }

            ResetQ();

            return additionalTStates;
        }

        // Instruction    : RETI
        // Operation      : Return from Interrupt
        // Flags Affected : None

        private byte RETI(byte opCode)
        {
            PopProgramCounter();
            MEMPTR = PC;
            ResetQ();

            // ToDo: signal interrupting device that interrupt is complete

            return 0;
        }

        // Instruction    : RETN
        // Operation      : Return from NMI
        // Flags Affected : None

        private byte RETN(byte opCode)
        {
            PopProgramCounter();
            IFF1 = IFF2;
            MEMPTR = PC;
            ResetQ();

            return 0;
        }

        // Instruction    : RST p
        // Operation      : Restart at page zero address p
        // Flags Affected : None

        private byte RST(byte opCode)
        {
            var val = (byte)((opCode & 0b00111000) >> 3);
            var addr = GetPageZeroAddress(val);

            PushProgramCounter();
            PC = addr;
            MEMPTR = addr;
            ResetQ();

            return 0;
        }
    }
}
