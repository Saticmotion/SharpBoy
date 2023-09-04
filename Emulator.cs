using System.Diagnostics;

namespace SharpBoy;

public class Emulator
{
	public static int screenWidth = 160;
	public static int screenHeight = 144;

	private byte[] memory = new byte[0x10000];

	public byte[] screen = new byte[160 * 144 * 3];
	private byte[] backbuffer = new byte[256 * 256 * 3];

	private byte A;
	private byte B;
	private byte C;
	private byte D;
	private byte E;
	private byte F;
	private byte H;
	private byte L;

	private ushort AF
	{
		get => (ushort)((A << 8) | F);
		set
		{
			A = (byte)(value >> 8);
			F = (byte)value;
		}
	}

	private ushort BC
	{
		get => (ushort)((B << 8) | C);
		set
		{
			B = (byte)(value >> 8);
			C = (byte)value;
		}
	}

	private ushort DE
	{
		get => (ushort)((D << 8) | E);
		set
		{
			D = (byte)(value >> 8);
			E = (byte)value;
		}
	}

	private ushort HL
	{
		get => (ushort)((H << 8) | L);
		set
		{
			H = (byte)(value >> 8);
			L = (byte)value;
		}
	}

	private ushort StackPointer;
	private bool IME = true;
	private bool IMEScheduled = false;

	private ushort PC;
	private int scanlineCounter;

	//NOTE(Simon): Special memory registers
	private const int IF = 0xFF0F;
	private const int LCDC = 0xFF40;
	private const int STAT = 0xFF41;
	private const int SCY = 0xFF42;
	private const int SCX = 0xFF43;
	private const int LY = 0xFF44;
	private const int LYC = 0xFF45;
	private const int IE = 0xFFFF;

	int i = 0;

	public Emulator()
	{
		PC = 0x100;
		scanlineCounter = 114;

		AF = 0x01B0;
		BC = 0x0013;
		DE = 0x00D8;
		HL = 0x014D;

		StackPointer = 0xFFFE;

		memory[0xFF05] = 0x00;
		memory[0xFF06] = 0x00;
		memory[0xFF07] = 0x00;
		memory[0xFF10] = 0x80;
		memory[0xFF11] = 0xBF;
		memory[0xFF12] = 0xF3;
		memory[0xFF14] = 0xBF;
		memory[0xFF16] = 0x3F;
		memory[0xFF17] = 0x00;
		memory[0xFF19] = 0xBF;
		memory[0xFF1A] = 0x7F;
		memory[0xFF1B] = 0xFF;
		memory[0xFF1C] = 0x9F;
		memory[0xFF1E] = 0xBF;
		memory[0xFF20] = 0xFF;
		memory[0xFF21] = 0x00;
		memory[0xFF22] = 0x00;
		memory[0xFF23] = 0xBF;
		memory[0xFF24] = 0x77;
		memory[0xFF25] = 0xF3;
		memory[0xFF26] = 0xF1;
		memory[LCDC] = 0x91;
		memory[SCY] = 0x00;
		memory[SCX] = 0x00;
		memory[LYC] = 0x00;
		memory[0xFF47] = 0xFC;
		memory[0xFF48] = 0xFF;
		memory[0xFF49] = 0xFF;
		memory[0xFF4A] = 0x00;
		memory[0xFF4B] = 0x00;
		memory[IE] = 0x00;
	}

	public void LoadProgram(byte[] data)
	{
		Array.Copy(data, memory, 0x800);
		PC = 0x100;
	}

	public int Simulate(bool[] input, int maxCycles)
	{
		int realCycles = 0;
		while (realCycles < maxCycles)
		{
			int cycles = HandleInterrupts();
			cycles += SimulateNextOpcode();
			SimulateScreen(cycles);
			realCycles += cycles;

			//NOTE(Simon): By this point we will have advanced PC by 2 instructions, but only 1 instruction has actually executed. (Hence PC - 2)
			//NOTE(Simon): So at this point check if previous instruction was EI, and if so enable IME
			if (IMEScheduled && memory[PC - 2] == 0xDF)
			{
				IME = true;
				IMEScheduled = false;
			}
		}

		screen[i++] = 255;

		return realCycles;
	}

	private void SimulateScreen(int cycles)
	{
		//NOTE(Simon): If updating screen
		if (GetBit(ReadMemory(LCDC), 7) != 1)
		{
			return;
		}

		scanlineCounter -= cycles;

		if (scanlineCounter <= 0)
		{
			memory[LY]++;
		}

		int currentLine = memory[LY];

		if (currentLine == 144)
		{
			//Interrupt
		}

		if (currentLine > 153)
		{
			memory[LY] = 0;
		}

		if (currentLine < 144)
		{
			//DrawScanline
		}
	}

	//NOTE(Simon): Return cycles taken
	private int SimulateNextOpcode()
	{
		byte opcode = memory[PC];
		Logger.WriteLine($"{opcode:X2}, {PC:X}");
		Logger.WriteLine($"AF: {AF:X4}");
		PC++;

		//NOTE(Simon): Needed for JR cc, e instructions. Can't be declared in each case scope
		bool wasTrue;

		switch (opcode)
		{
			#region 0x DONE

			case 0x00:
				//NOTE(Simon): NOP
				return 1;
			case 0x01:
				//NOTE(Simon): LD BC, nn
				BC = ReadImmediate16();
				return 3;
			case 0x02:
				//NOTE(Simon): LD (BC), A
				WriteMemory(BC, A);
				return 2;
			case 0x03:
				//NOTE(Simon): INC BC
				BC = IncrementRegister16(BC);
				return 2;
			case 0x04:
				//NOTE(Simon): INC, B
				B = IncrementRegister(B);
				return 1;
			case 0x05:
				//NOTE(Simon): DEC, B
				B = DecrementRegister(B);
				return 1;
			case 0x06:
				//NOTE(Simon): LD B, n
				B = ReadImmediate8();
				return 2;
			case 0x07:
				//NOTE(Simon): RLCA
				A = RotateLeftCarry(A);
				return 1;
			case 0x08:
				//NOTE(Simon): LD(nn), SP
				WriteToAddress16(ReadImmediate16(), StackPointer);
				return 5;
			case 0x09:
				//NOTE(Simon): ADD HL, BC
				AddHL(BC);
				return 2;
			case 0x0A:
				//NOTE(Simon): LD A, (BC)
				A = LoadFromAddress(BC);
				return 2;
			case 0x0B:
				//NOTE(Simon): DEC BC
				BC = DecrementRegister16(BC);
				return 2;
			case 0x0C:
				//NOTE(Simon): INC, C
				C = IncrementRegister(C);
				return 1;
			case 0x0D:
				//NOTE(Simon): DEC, C
				C = DecrementRegister(C);
				return 1;
			case 0x0E:
				//NOTE(Simon): LD C, n
				C = ReadImmediate8();
				return 2;
			case 0x0F:
				//NOTE(Simon): RRCA
				A = RotateRightCarry(A);
				return 1;

			#endregion

			#region 1x

			case 0x10:
				throw new NotImplementedException();
			case 0x11:
				//NOTE(Simon): LD DE, nn
				DE = ReadImmediate16();
				return 3;
			case 0x12:
				//NOTE(Simon): LD (DE), A
				WriteMemory(DE, A);
				return 2;
			case 0x13:
				//NOTE(Simon): INC DE
				DE = IncrementRegister16(DE);
				return 2;
			case 0x14:
				//NOTE(Simon): INC, D
				D = IncrementRegister(D);
				return 1;
			case 0x15:
				//NOTE(Simon): DEC, D
				D = DecrementRegister(D);
				return 1;
			case 0x16:
				//NOTE(Simon): LD D, n
				D = ReadImmediate8();
				return 2;
			case 0x17:
				//NOTE(Simon): RLA
				A = RotateLeftThroughCarry(A);
				return 1;
			case 0x18:
				//NOTE(Simon): JR e
				JumpRelative();
				return 3;
			case 0x19:
				//NOTE(Simon): ADD HL, DE
				AddHL(DE);
				return 2;
			case 0x1A:
				//NOTE(Simon): LD A, (DE)
				A = LoadFromAddress(DE);
				return 2;
			case 0x1B:
				//NOTE(Simon): DEC DE
				DE = DecrementRegister16(DE);
				return 2;
			case 0x1C:
				//NOTE(Simon): INC, E
				E = IncrementRegister(E);
				return 1;
			case 0x1D:
				//NOTE(Simon): DEC, E
				E = DecrementRegister(E);
				return 1;
			case 0x1E:
				//NOTE(Simon): LD E, n
				E = ReadImmediate8();
				return 2;
			case 0x1F:
				//NOTE(Simon): RRA
				A = RotateRightThroughCarry(A);
				return 1;

			#endregion

			#region 2x DONE

			case 0x20:
				//NOTE(Simon): JR NZ, e
				wasTrue = JumpRelativeConditional(GetFlagZero(), 0);
				return wasTrue ? 3 : 2;
			case 0x21:
				//NOTE(Simon): LD BC, nn
				HL = ReadImmediate16();
				return 3;
			case 0x22:
				//NOTE(Simon): LD (HL+), A
				WriteToAddress(HL++, A);
				return 2;
			case 0x23:
				//NOTE(Simon): INC HL
				HL = IncrementRegister16(HL);
				return 2;
			case 0x24:
				//NOTE(Simon): INC, H
				H = IncrementRegister(H);
				return 1;
			case 0x25:
				//NOTE(Simon): DEC, H
				H = DecrementRegister(H);
				return 1;
			case 0x26:
				//NOTE(Simon): LD H, n
				H = ReadImmediate8();
				return 2;
			case 0x27:
				//NOTE(Simon): DAA
				DecimalAdjustAccumulator();
				return 1;
			case 0x28:
				//NOTE(Simon): JR Z, e
				wasTrue = JumpRelativeConditional(GetFlagZero(), 1);
				return wasTrue ? 3 : 2;
			case 0x29:
				//NOTE(Simon): ADD HL, HL
				AddHL(HL);
				return 2;
			case 0x2A:
				//NOTE(Simon): LD A, (HL+)
				A = LoadFromAddress(HL++);
				return 2;
			case 0x2B:
				//NOTE(Simon): DEC HL
				HL = DecrementRegister16(HL);
				return 2;
			case 0x2C:
				//NOTE(Simon): INC, L
				L = IncrementRegister(L);
				return 1;
			case 0x2D:
				//NOTE(Simon): DEC, L
				L = DecrementRegister(L);
				return 1;
			case 0x2E:
				//NOTE(Simon): LD L, n
				L = ReadImmediate8();
				return 2;
			case 0x2F:
				//NOTE(Simon): CPL
				ComplementAccumulator();
				return 1;

			#endregion

			#region 3x DONE

			case 0x30:
				//NOTE(Simon): JR NC, e
				wasTrue = JumpRelativeConditional(GetFlagCarry(), 0);
				return wasTrue ? 3 : 2;
			case 0x31:
				//NOTE(Simon): LD BC, nn
				StackPointer = ReadImmediate16();
				return 3;
			case 0x32:
				//NOTE(Simon): LD (HL-), A
				WriteToAddress(HL--, A);
				return 2;
			case 0x33:
				//NOTE(Simon): INC SP
				StackPointer = IncrementRegister16(StackPointer);
				return 2;
			case 0x34:
				//NOTE(Simon): INC (HL)
				IncrementIndirect();
				return 3;
			case 0x35:
				//NOTE(Simon): DEC (HL)
				DecrementIndirect();
				return 3;
			case 0x36:
				//NOTE(Simon): LD (HL), n
				WriteToAddress(HL, ReadImmediate8());
				return 3;
			case 0x37:
				//NOTE(Simon): SCF
				//NOTE(Simon): Not to be confused with SetFlagCarry
				SetCarryFlag();
				return 1;
			case 0x38:
				//NOTE(Simon): JR C, e
				wasTrue = JumpRelativeConditional(GetFlagCarry(), 1);
				return wasTrue ? 3 : 2;
			case 0x39:
				//NOTE(Simon): ADD HL, SP
				AddHL(StackPointer);
				return 2;
			case 0x3A:
				//NOTE(Simon): LD A, (HL-)
				A = LoadFromAddress(HL--);
				return 2;
			case 0x3B:
				//NOTE(Simon): DEC SP
				StackPointer = DecrementRegister16(StackPointer);
				return 2;
			case 0x3C:
				//NOTE(Simon): INC, A
				A = IncrementRegister(A);
				return 1;
			case 0x3D:
				//NOTE(Simon): DEC, A
				A = DecrementRegister(A);
				return 1;
			case 0x3E:
				//NOTE(Simon): LD A, n
				A = ReadImmediate8();
				return 2;
			case 0x3F:
				//NOTE(Simon): CCF
				ComplementCarryFlag();
				return 1;

			#endregion

			#region 4x DONE

			case 0x40:
				//NOTE(Simon): LD B, B
				B = B;
				return 1;
			case 0x41:
				//NOTE(Simon): LD B, C
				B = C;
				return 1;
			case 0x42:
				//NOTE(Simon): LD B, D
				B = D;
				return 1;
			case 0x43:
				//NOTE(Simon): LD B, E
				B = E;
				return 1;
			case 0x44:
				//NOTE(Simon): LD B, H
				B = H;
				return 1;
			case 0x45:
				//NOTE(Simon): LD B, L
				B = L;
				return 1;
			case 0x46:
				//NOTE(Simon): LD B, (HL)
				B = LoadFromAddress(HL);
				return 2;
			case 0x47:
				//NOTE(Simon): LD B, A
				B = A;
				return 1;
			case 0x48:
				//NOTE(Simon): LD C, B
				C = B;
				return 1;
			case 0x49:
				//NOTE(Simon): LD C, C
				C = C;
				return 1;
			case 0x4A:
				//NOTE(Simon): LD C, D
				C = D;
				return 1;
			case 0x4B:
				//NOTE(Simon): LD C, E
				C = E;
				return 1;
			case 0x4C:
				//NOTE(Simon): LD C, H
				C = H;
				return 1;
			case 0x4D:
				//NOTE(Simon): LD C, L
				C = L;
				return 1;
			case 0x4E:
				//NOTE(Simon): LD C, (HL)
				C = LoadFromAddress(HL);
				return 2;
			case 0x4F:
				//NOTE(Simon): LD C, A
				C = A;
				return 1;

			#endregion

			#region 5x DONE

			case 0x50:
				//NOTE(Simon): LD D, B
				D = B;
				return 1;
			case 0x51:
				//NOTE(Simon): LD D, C
				D = C;
				return 1;
			case 0x52:
				//NOTE(Simon): LD D, D
				D = D;
				return 1;
			case 0x53:
				//NOTE(Simon): LD D, E
				D = E;
				return 1;
			case 0x54:
				//NOTE(Simon): LD D, H
				D = H;
				return 1;
			case 0x55:
				//NOTE(Simon): LD D, L
				D = L;
				return 1;
			case 0x56:
				//NOTE(Simon): LD D, (HL)
				D = LoadFromAddress(HL);
				return 2;
			case 0x57:
				//NOTE(Simon): LD D, A
				D = A;
				return 1;
			case 0x58:
				//NOTE(Simon): LD E, B
				E = B;
				return 1;
			case 0x59:
				//NOTE(Simon): LD E, C
				E = C;
				return 1;
			case 0x5A:
				//NOTE(Simon): LD E, D
				E = D;
				return 1;
			case 0x5B:
				//NOTE(Simon): LD E, E
				E = E;
				return 1;
			case 0x5C:
				//NOTE(Simon): LD E, H
				E = H;
				return 1;
			case 0x5D:
				//NOTE(Simon): LD E, L
				E = L;
				return 1;
			case 0x5E:
				//NOTE(Simon): LD E, (HL)
				E = LoadFromAddress(HL);
				return 2;
			case 0x5F:
				//NOTE(Simon): LD E, A
				E = A;
				return 1;

			#endregion

			#region 6x DONE

			case 0x60:
				//NOTE(Simon): LD H, B
				H = B;
				return 1;
			case 0x61:
				//NOTE(Simon): LD H, C
				H = C;
				return 1;
			case 0x62:
				//NOTE(Simon): LD H, D
				H = D;
				return 1;
			case 0x63:
				//NOTE(Simon): LD H, E
				H = E;
				return 1;
			case 0x64:
				//NOTE(Simon): LD H, H
				H = H;
				return 1;
			case 0x65:
				//NOTE(Simon): LD H, L
				H = L;
				return 1;
			case 0x66:
				//NOTE(Simon): LD H, (HL)
				H = LoadFromAddress(HL);
				return 2;
			case 0x67:
				//NOTE(Simon): LD H, A
				H = A;
				return 1;
			case 0x68:
				//NOTE(Simon): LD L, B
				L = B;
				return 1;
			case 0x69:
				//NOTE(Simon): LD L, C
				L = C;
				return 1;
			case 0x6A:
				//NOTE(Simon): LD L, D
				L = D;
				return 1;
			case 0x6B:
				//NOTE(Simon): LD L, E
				L = E;
				return 1;
			case 0x6C:
				//NOTE(Simon): LD L, H
				L = H;
				return 1;
			case 0x6D:
				//NOTE(Simon): LD L, L
				L = L;
				return 1;
			case 0x6E:
				//NOTE(Simon): LD L, (HL)
				L = LoadFromAddress(HL);
				return 2;
			case 0x6F:
				//NOTE(Simon): LD L, A
				L = A;
				return 1;

			#endregion

			#region 7x

			case 0x70:
				//NOTE(Simon): LD (HL), B
				WriteMemory(HL, B);
				return 2;
			case 0x71:
				//NOTE(Simon): LD (HL), C
				WriteMemory(HL, C);
				return 2;
			case 0x72:
				//NOTE(Simon): LD (HL), D
				WriteMemory(HL, D);
				return 2;
			case 0x73:
				//NOTE(Simon): LD (HL), E
				WriteMemory(HL, E);
				return 2;
			case 0x74:
				//NOTE(Simon): LD (HL), H
				WriteMemory(HL, H);
				return 2;
			case 0x75:
				//NOTE(Simon): LD (HL), L
				WriteMemory(HL, L);
				return 2;
			case 0x76:
				throw new NotImplementedException();
			case 0x77:
				//NOTE(Simon): LD (HL), A
				WriteMemory(HL, A);
				return 2;
			case 0x78:
				//NOTE(Simon): LD A, B
				A = B;
				return 1;
			case 0x79:
				//NOTE(Simon): LD A, C
				A = C;
				return 1;
			case 0x7A:
				//NOTE(Simon): LD A, D
				A = D;
				return 1;
			case 0x7B:
				//NOTE(Simon): LD A, E
				A = E;
				return 1;
			case 0x7C:
				//NOTE(Simon): LD A, H
				A = H;
				return 1;
			case 0x7D:
				//NOTE(Simon): LD A, L
				A = L;
				return 1;
			case 0x7E:
				//NOTE(Simon): LD A, (HL)
				A = LoadFromAddress(HL);
				return 2;
			case 0x7F:
				//NOTE(Simon): LD A, A
				A = A;
				return 1;

			#endregion

			#region 8x DONE

			case 0x80:
				//NOTE(Simon): ADD B
				AddRegister(B);
				return 1;
			case 0x81:
				//NOTE(Simon): ADD C
				AddRegister(C);
				return 1;
			case 0x82:
				//NOTE(Simon): ADD D
				AddRegister(D);
				return 1;
			case 0x83:
				//NOTE(Simon): ADD E
				AddRegister(E);
				return 1;
			case 0x84:
				//NOTE(Simon): ADD H
				AddRegister(H);
				return 1;
			case 0x85:
				//NOTE(Simon): ADD L
				AddRegister(L);
				return 1;
			case 0x86:
				//NOTE(Simon): ADD (HL)
				AddRegisterIndirect();
				return 2;
			case 0x87:
				//NOTE(Simon): ADD A
				AddRegister(A);
				return 1;
			case 0x88:
				//NOTE(Simon): ADC B
				AddWithCarryRegister(B);
				return 2;
			case 0x89:
				//NOTE(Simon): ADC C
				AddWithCarryRegister(C);
				return 2;
			case 0x8A:
				//NOTE(Simon): ADC D
				AddWithCarryRegister(D);
				return 2;
			case 0x8B:
				//NOTE(Simon): ADC E
				AddWithCarryRegister(E);
				return 2;
			case 0x8C:
				//NOTE(Simon): ADC H
				AddWithCarryRegister(H);
				return 2;
			case 0x8D:
				//NOTE(Simon): ADC L
				AddWithCarryRegister(L);
				return 2;
			case 0x8E:
				//NOTE(Simon): ADC (HL)
				AddWithCarryRegisterIndirect();
				return 2;
			case 0x8F:
				//NOTE(Simon): ADC A
				AddWithCarryRegister(A);
				return 2;

			#endregion

			#region 9x DONE

			case 0x90:
				//NOTE(Simon): SUB, B
				SubtractRegister(B);
				return 1;
			case 0x91:
				//NOTE(Simon): SUB, C
				SubtractRegister(C);
				return 1;
			case 0x92:
				//NOTE(Simon): SUB, D
				SubtractRegister(D);
				return 1;
			case 0x93:
				//NOTE(Simon): SUB, E
				SubtractRegister(E);
				return 1;
			case 0x94:
				//NOTE(Simon): SUB, H
				SubtractRegister(H);
				return 1;
			case 0x95:
				//NOTE(Simon): SUB, L
				SubtractRegister(L);
				return 1;
			case 0x96:
				//NOTE(Simon): SUB (HL)
				SubtractRegisterIndirect();
				return 2;
			case 0x97:
				//NOTE(Simon): SUB, A
				SubtractRegister(A);
				return 1;
			case 0x98:
				//NOTE(Simon): SBC, B
				SubtractWithCarryRegister(B);
				return 1;
			case 0x99:
				//NOTE(Simon): SBC, C
				SubtractWithCarryRegister(C);
				return 1;
			case 0x9A:
				//NOTE(Simon): SBC, D
				SubtractWithCarryRegister(D);
				return 1;
			case 0x9B:
				//NOTE(Simon): SBC, E
				SubtractWithCarryRegister(E);
				return 1;
			case 0x9C:
				//NOTE(Simon): SBC, H
				SubtractWithCarryRegister(H);
				return 1;
			case 0x9D:
				//NOTE(Simon): SBC, L
				SubtractWithCarryRegister(L);
				return 1;
			case 0x9E:
				//NOTE(Simon): SBC (HL)
				SubtractWithCarryRegisterIndirect();
				return 2;
			case 0x9F:
				//NOTE(Simon): SBC, A
				SubtractWithCarryRegister(A);
				return 1;

			#endregion

			#region Ax DONE

			case 0xA0:
				//NOTE(Simon): AND B
				AndRegister(B);
				return 1;
			case 0xA1:
				//NOTE(Simon): AND C
				AndRegister(C);
				return 1;
			case 0xA2:
				//NOTE(Simon): AND D
				AndRegister(D);
				return 1;
			case 0xA3:
				//NOTE(Simon): AND E
				AndRegister(E);
				return 1;
			case 0xA4:
				//NOTE(Simon): AND H
				AndRegister(H);
				return 1;
			case 0xA5:
				//NOTE(Simon): AND L
				AndRegister(L);
				return 1;
			case 0xA6:
				//NOTE(Simon): AND (HL)
				AndIndirect();
				return 2;
			case 0xA7:
				//NOTE(Simon): AND A
				AndRegister(A);
				return 1;
			case 0xA8:
				//NOTE(Simon): XOR B
				XorRegister(B);
				return 1;
			case 0xA9:
				//NOTE(Simon): XOR C
				XorRegister(C);
				return 1;
			case 0xAA:
				//NOTE(Simon): XOR D
				XorRegister(D);
				return 1;
			case 0xAB:
				//NOTE(Simon): XOR E
				XorRegister(E);
				return 1;
			case 0xAC:
				//NOTE(Simon): XOR H
				XorRegister(H);
				return 1;
			case 0xAD:
				//NOTE(Simon): XOR L
				XorRegister(L);
				return 1;
			case 0xAE:
				//NOTE(Simon): XOR (HL)
				XorIndirect();
				return 2;
			case 0xAF:
				//NOTE(Simon): XOR A
				XorRegister(A);
				return 1;

			#endregion

			#region Bx DONE

			case 0xB0:
				//NOTE(Simon): OR B
				OrRegister(B);
				return 1;
			case 0xB1:
				//NOTE(Simon): OR C
				OrRegister(C);
				return 1;
			case 0xB2:
				//NOTE(Simon): OR D
				OrRegister(D);
				return 1;
			case 0xB3:
				//NOTE(Simon): OR E
				OrRegister(E);
				return 1;
			case 0xB4:
				//NOTE(Simon): OR H
				OrRegister(H);
				return 1;
			case 0xB5:
				//NOTE(Simon): OR L
				OrRegister(L);
				return 1;
			case 0xB6:
				//NOTE(Simon): OR (HL)
				OrIndirect();
				return 2;
			case 0xB7:
				//NOTE(Simon): OR A
				OrRegister(A);
				return 1;
			case 0xB8:
				//NOTE(Simon): CP B
				CompareRegister(B);
				return 1;
			case 0xB9:
				//NOTE(Simon): CP C
				CompareRegister(C);
				return 1;
			case 0xBA:
				//NOTE(Simon): CP D
				CompareRegister(D);
				return 1;
			case 0xBB:
				//NOTE(Simon): CP E
				CompareRegister(E);
				return 1;
			case 0xBC:
				//NOTE(Simon): CP H
				CompareRegister(H);
				return 1;
			case 0xBD:
				//NOTE(Simon): CP L
				CompareRegister(L);
				return 1;
			case 0xBE:
				//NOTE(Simon): CP (HL)
				CompareIndirect();
				return 2;
			case 0xBF:
				//NOTE(Simon): CP A
				CompareRegister(A);
				return 1;

			#endregion

			#region Cx

			case 0xC0:
				//NOTE(Simon): RET NZ
				wasTrue = ReturnConditional(GetFlagZero(), 0);
				return wasTrue ? 5 : 2;
			case 0xC1:
				//NOTE(Simon): POP BC
				BC = PopStack();
				return 3;
			case 0xC2:
				//NOTE(Simon): JP NZ, nn
				wasTrue = JumpConditional(GetFlagZero(), 0);
				return wasTrue ? 4 : 3;
			case 0xC3:
				//NOTE(Simon): JP nn
				JumpImmediate();
				return 4;
			case 0xC4:
				throw new NotImplementedException();
			case 0xC5:
				//NOTE(Simon): PUSH BC
				PushStack(BC);
				return 4;
			case 0xC6:
				throw new NotImplementedException();
			case 0xC7:
				//NOTE(Simon): RST, 00
				Restart(0x00);
				return 4;
			case 0xC8:
				//NOTE(Simon): RET Z
				wasTrue = ReturnConditional(GetFlagZero(), 1);
				return wasTrue ? 5 : 2;
			case 0xC9:
				//NOTE(Simon): RET
				Return();
				return 4;
			case 0xCA:
				//NOTE(Simon): JP Z, nn
				wasTrue = JumpConditional(GetFlagZero(), 1);
				return wasTrue ? 4 : 3;
			case 0xCB:
				return SimulateExtendedOpcodes();
			case 0xCC:
				throw new NotImplementedException();
			case 0xCD:
				//NOTE(Simon): Call nn
				CallFunctionImmediate();
				return 6;
			case 0xCE:
				throw new NotImplementedException();
			case 0xCF:
				//NOTE(Simon): RST, 08
				Restart(0x08);
				return 4;

			#endregion

			#region Dx

			case 0xD0:
				//NOTE(Simon): RET NC
				wasTrue = ReturnConditional(GetFlagCarry(), 0);
				return wasTrue ? 5 : 2;
			case 0xD1:
				//NOTE(Simon): POP DE
				DE = PopStack();
				return 3;
			case 0xD2:
				//NOTE(Simon): JP NC, nn
				wasTrue = JumpConditional(GetFlagCarry(), 0);
				return wasTrue ? 4 : 3;
			case 0xD3:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xD4:
				throw new NotImplementedException();
			case 0xD5:
				//NOTE(Simon): PUSH DE
				PushStack(DE);
				return 4;
			case 0xD6:
				throw new NotImplementedException();
			case 0xD7:
				//NOTE(Simon): RST, 10
				Restart(0x10);
				return 4;
			case 0xD8:
				//NOTE(Simon): RET C
				wasTrue = ReturnConditional(GetFlagCarry(), 1);
				return wasTrue ? 5 : 2;
			case 0xD9:
				throw new NotImplementedException();
			case 0xDA:
				//NOTE(Simon): JP C, nn
				wasTrue = JumpConditional(GetFlagCarry(), 1);
				return wasTrue ? 4 : 3;
			case 0xDB:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xDC:
				throw new NotImplementedException();
			case 0xDD:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xDE:
				throw new NotImplementedException();
			case 0xDF:
				//NOTE(Simon): RST, 18
				Restart(0x18);
				return 4;

			#endregion

			#region Ex

			case 0xE0:
				//NOTE(Simon): LDH (n), A
				WriteToAddressPart(ReadImmediate8(), A);
				return 3;
			case 0xE1:
				//NOTE(Simon): POP HL
				HL = PopStack();
				return 3;
			case 0xE2:
				//NOTE(Simon): LDH (C), A
				WriteToAddressPart(C, A);
				return 2;
			case 0xE3:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xE4:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xE5:
				//NOTE(Simon): PUSH HL
				PushStack(HL);
				return 4;
			case 0xE6:
				throw new NotImplementedException();
			case 0xE7:
				//NOTE(Simon): RST, 20
				Restart(0x20);
				return 4;
			case 0xE8:
				throw new NotImplementedException();
			case 0xE9:
				//NOTE(Simon): JP HL
				JumpAddress(HL);
				return 1;
			case 0xEA:
				//NOTE(Simon): LD (nn), A
				WriteToAddress(ReadImmediate16(), A);
				return 4;
			case 0xEB:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xEC:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xED:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xEE:
				throw new NotImplementedException();
			case 0xEF:
				//NOTE(Simon): RST, 28
				Restart(0x28);
				return 4;

			#endregion

			#region Fx

			case 0xF0:
				//NOTE(Simon): LDH A, (n)
				A = LoadFromAddressPart();
				return 2;
			case 0xF1:
				//NOTE(Simon): POP AF
				AF = PopStack();
				return 3;
			case 0xF2:
				throw new NotImplementedException();
			case 0xF3:
				//NOTE(Simon): DI
				IME = false;
				return 1;
			case 0xF4:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xF5:
				//NOTE(Simon): PUSH AF
				PushStack(AF);
				return 4;
			case 0xF6:
				throw new NotImplementedException();
			case 0xF7:
				//NOTE(Simon): RST, 30
				Restart(0x30);
				return 4;
			case 0xF8:
				throw new NotImplementedException();
			case 0xF9:
				//NOTE(Simon): LD SP, HL
				StackPointer = HL;
				return 2;
			case 0xFA:
				//NOTE(Simon): LD A, (nn)
				A = LoadFromAddress(ReadImmediate16());
				return 4;
			case 0xFB:
				//NOTE(Simon): EI
				IMEScheduled = true;
				return 1;
			case 0xFC:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xFD:
				//NOTE(Simon): No opcode
				Console.WriteLine($"Encountered unknown opcode {opcode:X}");
				break;
			case 0xFE:
				//NOTE(Simon): CP n
				CompareImmediate();
				return 2;
			case 0xFF:
				//NOTE(Simon): RST, 38
				Restart(0x38);
				return 4;

			#endregion

			default:
				throw new NotImplementedException();
		}

		return 0;
	}

	//NOTE(Simon): Return cycles taken
	private int SimulateExtendedOpcodes()
	{
		byte opcode = memory[PC];
		PC++;

		throw new NotImplementedException();
		switch (opcode)
		{
		}

		return 0;
	}

	private int HandleInterrupts()
	{
		if (!IME)
		{
			return 0;
		}

		byte ifValue = ReadMemory(IF);
		byte ieValue = ReadMemory(IE);

		//NOTE(Simon): No interrupts (allowed) to be handled, so return immediately
		if (ifValue == 0 || ieValue == 0)
		{
			return 0;
		}

		//NOTE(Simon): Handle VBLANK interrupt
		if (GetBit(ifValue, 0) == 1 && GetBit(ieValue, 0) == 1)
		{
			ServiceInterrupt(0x40);
			ModifyBit(memory[IF], 0, 0);
		}
		//NOTE(Simon): Handle STAT interrupt
		else if (GetBit(ifValue, 1) == 1 && GetBit(ieValue, 1) == 1)
		{
			ServiceInterrupt(0x48);
			ModifyBit(memory[IF], 1, 0);
		}
		//NOTE(Simon): Handle TIMER interrupt
		else if (GetBit(ifValue, 2) == 1 && GetBit(ieValue, 2) == 1)
		{
			ServiceInterrupt(0x50);
			ModifyBit(memory[IF], 2, 0);
		}
		//NOTE(Simon): Handle SERIAL interrupt
		else if (GetBit(ifValue, 3) == 1 && GetBit(ieValue, 3) == 1)
		{
			ServiceInterrupt(0x58);
			ModifyBit(memory[IF], 3, 0);
		}
		//NOTE(Simon): Handle JOYPAD interrupt
		else if (GetBit(ifValue, 4) == 1 && GetBit(ieValue, 4) == 1)
		{
			ServiceInterrupt(0x60);
			ModifyBit(memory[IF], 4, 0);
		}
		else
		{
			throw new InvalidOperationException();
		}

		return 5;
	}

	private void ServiceInterrupt(ushort address)
	{
		PushStack(PC);
		PC = address;
		IME = false;
	}

	private void AddRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry((A & 0xF) + (value & 0xF) > 0xF ? 1 : 0);
		SetFlagCarry(A + value > 0xFF ? 1 : 0);

		A += value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void AddRegisterIndirect()
	{
		AddRegister(ReadMemory(HL));
	}

	//TODO(Simon): Verify HalfCarry flag
	private void AddHL(ushort value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry((HL & 0xF00) + (value & 0xF00) > 0xF00 ? 1 : 0);
		SetFlagCarry(HL + value > 0xFFFF ? 1 : 0);

		HL += value;
	}

	private void AddWithCarryRegisterIndirect()
	{
		AddWithCarryRegister(ReadMemory(HL));
	}

	private void AddWithCarryRegister(byte value)
	{
		AddRegister((byte)(value + GetFlagCarry()));
	}

	private void SubtractRegisterIndirect()
	{
		SubtractRegister(ReadMemory(HL));
	}

	private void SubtractRegister(byte value)
	{
		SetFlagSubtraction(1);
		SetFlagHalfCarry((value & 0xF) > (A & 0xF) ? 1 : 0);
		SetFlagCarry(value > A ? 1 : 0);

		A -= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void SubtractWithCarryRegisterIndirect()
	{
		SubtractWithCarryRegister(ReadMemory(HL));
	}

	private void SubtractWithCarryRegister(byte value)
	{
		SubtractRegister((byte)(value + GetFlagCarry()));
	}

	private void DecrementIndirect()
	{
		byte value = ReadMemory(HL);
		byte result = DecrementRegister(value);
		WriteMemory(HL, result);
	}

	private byte DecrementRegister(byte value)
	{
		SetFlagSubtraction(1);
		SetFlagHalfCarry((value & 0xF) == 0 ? 1 : 0);

		value--;

		SetFlagZero(value == 0 ? 1 : 0);

		return value;
	}

	private ushort DecrementRegister16(ushort value)
	{
		return (ushort)(value - 1);
	}

	private void IncrementIndirect()
	{
		byte value = ReadMemory(HL);
		byte result = IncrementRegister(value);
		WriteMemory(HL, result);
	}

	private byte IncrementRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry((value & 0xF) == 0xF ? 1 : 0);

		value++;

		SetFlagZero(A == 0 ? 1 : 0);

		return value;
	}

	private ushort IncrementRegister16(ushort value)
	{
		return (ushort)(value + 1);
	}

	private byte RotateLeftCarry(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);

		byte bit7 = GetBit(value, 7);

		value <<= 1;

		SetFlagCarry(bit7);
		SetFlagZero(value == 0 ? 1 : 0);

		return value;
	}

	private byte RotateLeftThroughCarry(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);

		byte bit7 = GetBit(value, 7);

		value <<= 1;

		ModifyBit(value, 0, GetFlagCarry());
		SetFlagCarry(bit7);
		SetFlagZero(value == 0 ? 1 : 0);

		return value;
	}

	private byte RotateRightCarry(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);

		byte bit0 = GetBit(value, 0);

		value >>= 1;

		SetFlagCarry(bit0);
		SetFlagZero(value == 0 ? 1 : 0);

		return value;
	}

	private byte RotateRightThroughCarry(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);

		byte bit0 = GetBit(value, 0);

		value >>= 1;

		ModifyBit(value, 7, GetFlagCarry());
		SetFlagCarry(bit0);
		SetFlagZero(value == 0 ? 1 : 0);

		return value;
	}

	private void AndRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(1);
		SetFlagCarry(0);

		A &= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void AndIndirect()
	{
		AndRegister(ReadMemory(HL));
	}

	private void XorRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);
		SetFlagCarry(0);

		A ^= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void XorIndirect()
	{
		XorRegister(ReadMemory(HL));
	}

	private void OrRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);
		SetFlagCarry(0);

		A |= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void OrIndirect()
	{
		OrRegister(ReadMemory(HL));
	}

	private void CompareRegister(byte value)
	{
		SetFlagSubtraction(1);
		SetFlagHalfCarry((value & 0xF) > (A & 0xF) ? 1 : 0);
		SetFlagCarry(value > A ? 1 : 0);

		int result = A - value;
		SetFlagZero(result == 0 ? 1 : 0);
	}

	private void CompareIndirect()
	{
		CompareRegister(ReadMemory(HL));
	}

	private void JumpAddress(ushort address)
	{
		PC = address;
	}

	private void JumpImmediate()
	{
		JumpAddress(ReadImmediate16());
	}

	private bool JumpRelativeConditional(byte value, byte testValue)
	{
		if (value == testValue)
		{
			JumpRelative();
			return true;
		}

		return false;
	}

	private void JumpRelative()
	{
		short offset = ReadImmediate8Signed();
		PC = (ushort)(PC + offset);
	}

	private bool JumpConditional(byte value, byte testValue)
	{
		ushort address = ReadImmediate16();

		if (value == testValue)
		{
			JumpAddress(address);
			return true;
		}

		return false;
	}

	private bool ReturnConditional(byte value, byte testValue)
	{
		if (value == testValue)
		{
			Return();
			return true;
		}

		return false;
	}

	private void Return()
	{
		PC = ReadMemory16(StackPointer);
		StackPointer += 2;
	}

	private ushort PopStack()
	{
		ushort value = ReadMemory16(StackPointer);
		StackPointer += 2;
		return value;
	}

	private void ComplementAccumulator()
	{
		A = (byte)~A;
		SetFlagHalfCarry(1);
		SetFlagSubtraction(1);
	}

	private void ComplementCarryFlag()
	{
		SetFlagCarry(1 - GetFlagCarry());
		SetFlagHalfCarry(0);
		SetFlagSubtraction(0);
	}

	private void Restart(byte rstAddress)
	{
		PushStack(PC);
		PC = rstAddress;
	}

	private void WriteToAddressPart(byte addressPart, byte value)
	{
		ushort address = (ushort)(0xFF00 + addressPart);
		WriteMemory(address, value);
	}

	private void WriteToAddress(ushort address, byte value)
	{
		WriteMemory(address, value);
	}

	private void WriteToAddress16(ushort address, ushort value)
	{
		WriteMemory16(address, value);
	}

	private byte LoadFromAddressPart()
	{
		ushort address = (ushort)(0xFF00 + ReadImmediate8());
		return ReadMemory(address);
	}

	private byte LoadFromAddress(ushort address)
	{
		return ReadMemory(address);
	}

	private void CompareImmediate()
	{
		byte value = ReadImmediate8();

		SetFlagSubtraction(1);
		SetFlagHalfCarry((value & 0xF) > (A & 0xF) ? 1 : 0);
		SetFlagCarry(value > A ? 1 : 0);

		byte result = (byte)(A - value);

		SetFlagZero(result == 0 ? 1 : 0);
	}

	//NOTE(Simon): Probably correct??
	private void DecimalAdjustAccumulator()
	{
		byte value = A;

		int msb = value & 0xF0;
		int lsb = value & 0x0F;

		if (msb > 9)
		{
			A += 0x60;
		}

		if (lsb > 9)
		{
			A += 0x06;
			SetFlagCarry(1);
		}
		else
		{
			SetFlagCarry(0);
		}

		SetFlagZero(value == 0 ? 1 : 0);
		SetFlagHalfCarry(0);
	}

	private void CallFunctionImmediate()
	{
		ushort address = ReadImmediate16();
		PushStack(PC);
		JumpAddress(address);
	}

	//NOTE(Simon): Not to be confused with SetFlagCarry
	private void SetCarryFlag()
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);
		SetFlagCarry(1);
	}



	private void SetFlagZero(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 7, (byte)value);
	}

	private byte GetFlagZero()
	{
		return GetBit(F, 7);
	}

	//NOTE(Simon): Also known as flag N
	private void SetFlagSubtraction(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 6, (byte)value);
	}

	private byte GetFlagSubtraction()
	{
		return GetBit(F, 6);
	}

	private void SetFlagHalfCarry(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 5, (byte)value);
	}
	
	private byte GetFlagHalfCarry()
	{
		return GetBit(F, 5);
	}

	private void SetFlagCarry(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 4, (byte)value);
	}

	private byte GetFlagCarry()
	{
		return GetBit(F, 4);
	}


	private void PushStack(ushort value)
	{
		StackPointer--;
		WriteMemory(StackPointer--, MSB(value));
		WriteMemory(StackPointer, LSB(value));
	}



	private static byte ModifyBit(byte original, byte position, byte value)
	{
		int mask = 1 << position;
		return (byte)((original & ~mask) |
					((value << position) & mask));
	}

	private static byte GetBit(int value, int pos)
	{
		return (byte)((value >> pos) & 1);
	}

	private void WriteMemory16(ushort address, ushort value)
	{
		WriteMemory(address++, LSB(value));
		WriteMemory(address, MSB(value));
	}

	private void WriteMemory(ushort address, byte value)
	{
		if (address < 0x8000)
		{
			//NOTE(Simon): ROM, do nothing.
		}
		else if (address > 0xE000 && address < 0xFE00)
		{
			//NOTE(Simon): ECHO RAM, write to normal RAM too.
			memory[address] = value;
			memory[address - 0x2000] = value;
		}
		else if (address > 0xFEA0 && address < 0xFEFF)
		{
			//NOTE(Simon): Restricted, do nothing.
		}
		else if (address == LY)
		{
			//NOTE(Simon): Reset scanline register whenever it is written to
			memory[LY] = 0;
		}
		else
		{
			memory[address] = value;
		}
	}

	private byte ReadMemory(ushort address)
	{
		return memory[address];
	}

	private ushort ReadMemory16(ushort address)
	{
		return (ushort)(memory[address] | memory[address + 1] << 8);
	}

	//NOTE(Simon): Reads uint16 at PC. PC += 2
	private ushort ReadImmediate16()
	{
		ushort value = ReadMemory16(PC);
		PC += 2;
		return value;
	}

	private byte ReadImmediate8()
	{
		return ReadMemory(PC++);
	}

	private short ReadImmediate8Signed()
	{
		return (short)(ReadMemory(PC++) - 256);
	}

	private byte MSB(ushort value)
	{
		return (byte)(value >> 8);
	}

	private byte LSB(ushort value)
	{
		return (byte)value;
	}

}