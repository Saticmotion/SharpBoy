using System.Buffers.Binary;
using System.Diagnostics;

namespace ChipSharp;

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

	private ushort PC;

	int i = 0;

	public Emulator()
	{
		PC = 0x100;
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
		memory[0xFF40] = 0x91;
		memory[0xFF42] = 0x00;
		memory[0xFF43] = 0x00;
		memory[0xFF45] = 0x00;
		memory[0xFF47] = 0xFC;
		memory[0xFF48] = 0xFF;
		memory[0xFF49] = 0xFF;
		memory[0xFF4A] = 0x00;
		memory[0xFF4B] = 0x00;
		memory[0xFFFF] = 0x00;
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
			int cycles = SimulateNextOpcode();
			SimulateScreen(cycles);
			realCycles += cycles;
		}

		screen[i++] = 255;

		return realCycles;
	}

	//NOTE(Simon): Return cycles taken
	private int SimulateNextOpcode()
	{
		byte opcode = memory[PC];
		Console.WriteLine($"{opcode:X2}");
		PC++;

		switch (opcode)
		{
			#region 0x
			case 0x00:
				//NOTE(Simon): NOP
				return 1;
			case 0x01:
				//NOTE(Simon): LD BC, nn
				BC = ReadImmediate();
				return 3;
			case 0x02:
				//NOTE(Simon): LD (BC), A
				WriteMemory(BC, A);
				return 2;
			case 0x03:
				throw new NotImplementedException();
			case 0x04:
				throw new NotImplementedException();
			case 0x05:
				throw new NotImplementedException();
			case 0x06:
				throw new NotImplementedException();
			case 0x07:
				throw new NotImplementedException();
			case 0x08:
				throw new NotImplementedException();
			case 0x09:
				throw new NotImplementedException();
			case 0x0A:
				throw new NotImplementedException();
			case 0x0B:
				throw new NotImplementedException();
			case 0x0C:
				throw new NotImplementedException();
			case 0x0D:
				throw new NotImplementedException();
			case 0x0E:
				throw new NotImplementedException();
			case 0x0F:
				throw new NotImplementedException();
			#endregion

			#region 1x
			case 0x11:
				//NOTE(Simon): LD DE, nn
				DE = ReadImmediate();
				return 3;
			case 0x12:
				//NOTE(Simon): LD (DE), A
				WriteMemory(DE, A);
				return 2;
			case 0x13:
				throw new NotImplementedException();
			case 0x14:
				throw new NotImplementedException();
			case 0x15:
				throw new NotImplementedException();
			case 0x16:
				throw new NotImplementedException();
			case 0x17:
				throw new NotImplementedException();
			case 0x18:
				throw new NotImplementedException();
			case 0x19:
				throw new NotImplementedException();
			case 0x1A:
				throw new NotImplementedException();
			case 0x1B:
				throw new NotImplementedException();
			case 0x1C:
				throw new NotImplementedException();
			case 0x1D:
				throw new NotImplementedException();
			case 0x1E:
				throw new NotImplementedException();
			case 0x1F:
				throw new NotImplementedException();
			#endregion

			#region 2x
			case 0x21:
				//NOTE(Simon): LD BC, nn
				HL = ReadImmediate();
				return 3;
			case 0x22:
				throw new NotImplementedException();
			case 0x23:
				throw new NotImplementedException();
			case 0x24:
				throw new NotImplementedException();
			case 0x25:
				throw new NotImplementedException();
			case 0x26:
				throw new NotImplementedException();
			case 0x27:
				throw new NotImplementedException();
			case 0x28:
				throw new NotImplementedException();
			case 0x29:
				throw new NotImplementedException();
			case 0x2A:
				throw new NotImplementedException();
			case 0x2B:
				throw new NotImplementedException();
			case 0x2C:
				throw new NotImplementedException();
			case 0x2D:
				throw new NotImplementedException();
			case 0x2E:
				throw new NotImplementedException();
			case 0x2F:
				throw new NotImplementedException();
			#endregion

			#region 3x
			case 0x31:
				//NOTE(Simon): LD BC, nn
				StackPointer = ReadImmediate();
				return 3;
			case 0x32:
				throw new NotImplementedException();
			case 0x33:
				throw new NotImplementedException();
			case 0x34:
				throw new NotImplementedException();
			case 0x35:
				throw new NotImplementedException();
			case 0x36:
				throw new NotImplementedException();
			case 0x37:
				throw new NotImplementedException();
			case 0x38:
				throw new NotImplementedException();
			case 0x39:
				throw new NotImplementedException();
			case 0x3A:
				throw new NotImplementedException();
			case 0x3B:
				throw new NotImplementedException();
			case 0x3C:
				throw new NotImplementedException();
			case 0x3D:
				throw new NotImplementedException();
			case 0x3E:
				throw new NotImplementedException();
			case 0x3F:
				throw new NotImplementedException();
			#endregion

			#region 4x
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			case 0x4F:
				//NOTE(Simon): LD C, A
				C = A;
				return 1;
			#endregion

			#region 5x
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			case 0x5F:
				//NOTE(Simon): LD E, A
				E = A;
				return 1;
			#endregion

			#region 6x
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			case 0x7F:
				//NOTE(Simon): LD A, A
				A = A;
				return 1;
			#endregion

			#region 8x
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			case 0x8F:
				//NOTE(Simon): ADC A
				AddWithCarryRegister(A);
				return 2;
			#endregion

			#region 9x
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			case 0x9F:
				//NOTE(Simon): SBC, A
				SubtractWithCarryRegister(A);
				return 1;
			#endregion

			#region Ax
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			case 0xAF:
				//NOTE(Simon): XOR A
				XorRegister(A);
				return 1;
			#endregion

			#region Bx
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
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			case 0xBF:
				//NOTE(Simon): CP A
				CompareRegister(A);
				return 1;
			#endregion

			#region Cx
			case 0xC0:
				throw new NotImplementedException();
			case 0xC1:
				throw new NotImplementedException();
			case 0xC2:
				throw new NotImplementedException();
			case 0xC3:
				//NOTE(Simon): JP nn
				JumpImmediate();
				return 4;
			case 0xC4:
				throw new NotImplementedException();
			case 0xC5:
				throw new NotImplementedException();
			case 0xC6:
				throw new NotImplementedException();
			case 0xC7:
				throw new NotImplementedException();
			case 0xC8:
				throw new NotImplementedException();
			case 0xC9:
				throw new NotImplementedException();
			case 0xCA:
				throw new NotImplementedException();
			case 0xCB:
				return SimulateExtendedOpcodes();
			case 0xCC:
				throw new NotImplementedException();
			case 0xCD:
				throw new NotImplementedException();
			case 0xCE:
				throw new NotImplementedException();
			case 0xCF:
				throw new NotImplementedException();
			#endregion

			#region Dx
			case 0xD0:
				throw new NotImplementedException();
			case 0xD1:
				throw new NotImplementedException();
			case 0xD2:
				throw new NotImplementedException();
			case 0xD3:
				throw new NotImplementedException();
			case 0xD4:
				throw new NotImplementedException();
			case 0xD5:
				throw new NotImplementedException();
			case 0xD6:
				throw new NotImplementedException();
			case 0xD7:
				throw new NotImplementedException();
			case 0xD8:
				throw new NotImplementedException();
			case 0xD9:
				throw new NotImplementedException();
			case 0xDA:
				throw new NotImplementedException();
			case 0xDB:
				throw new NotImplementedException();
			case 0xDC:
				throw new NotImplementedException();
			case 0xDD:
				throw new NotImplementedException();
			case 0xDE:
				throw new NotImplementedException();
			case 0xDF:
				throw new NotImplementedException();
			#endregion

			#region Ex
			case 0xE0:
				throw new NotImplementedException();
			case 0xE1:
				throw new NotImplementedException();
			case 0xE2:
				throw new NotImplementedException();
			case 0xE3:
				throw new NotImplementedException();
			case 0xE4:
				throw new NotImplementedException();
			case 0xE5:
				throw new NotImplementedException();
			case 0xE6:
				throw new NotImplementedException();
			case 0xE7:
				throw new NotImplementedException();
			case 0xE8:
				throw new NotImplementedException();
			case 0xE9:
				//NOTE(Simon): JP HL
				JumpAddress(HL);
				return 1;
			case 0xEA:
				throw new NotImplementedException();
			case 0xEB:
				throw new NotImplementedException();
			case 0xEC:
				throw new NotImplementedException();
			case 0xED:
				throw new NotImplementedException();
			case 0xEE:
				throw new NotImplementedException();
			case 0xEF:
				throw new NotImplementedException();
			#endregion

			#region Fx
			case 0xF0:
				throw new NotImplementedException();
			case 0xF1:
				throw new NotImplementedException();
			case 0xF2:
				throw new NotImplementedException();
			case 0xF3:
				throw new NotImplementedException();
			case 0xF4:
				throw new NotImplementedException();
			case 0xF5:
				throw new NotImplementedException();
			case 0xF6:
				throw new NotImplementedException();
			case 0xF7:
				throw new NotImplementedException();
			case 0xF8:
				throw new NotImplementedException();
			case 0xF9:
				throw new NotImplementedException();
			case 0xFA:
				throw new NotImplementedException();
			case 0xFB:
				throw new NotImplementedException();
			case 0xFC:
				throw new NotImplementedException();
			case 0xFD:
				throw new NotImplementedException();
			case 0xFE:
				throw new NotImplementedException();
			case 0xFF:
				throw new NotImplementedException();
			#endregion
		}

		return 0;
	}

	//NOTE(Simon): Return cycles taken
	private int SimulateExtendedOpcodes()
	{
		byte opcode = memory[PC];
		PC++;

		switch (opcode)
		{

		}

		return 0;
	}

	private void AddRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry((A & 0xF) + (value & 0xF) > 0xF ? 1 : 0);
		SetFlagCarry(A + value > 0xFF ? 1 : 0);

		A += value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void AddWithCarryRegister(byte value)
	{
		AddRegister((byte)(value + GetFlagCarry()));
	}

	private void SubtractRegister(byte value)
	{
		SetFlagSubtraction(1);
		SetFlagHalfCarry((value & 0xF) > (A & 0xF) ? 1 : 0);
		SetFlagCarry(value > A ? 1 : 0);

		A -= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void SubtractWithCarryRegister(byte value)
	{
		SubtractRegister((byte)(value + GetFlagCarry()));
	}

	private void AndRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(1);
		SetFlagCarry(0);

		A &= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void XorRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);
		SetFlagCarry(0);

		A ^= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void OrRegister(byte value)
	{
		SetFlagSubtraction(0);
		SetFlagHalfCarry(0);
		SetFlagCarry(0);

		A |= value;
		SetFlagZero(A == 0 ? 1 : 0);
	}

	private void CompareRegister(byte value)
	{
		SetFlagSubtraction(1);
		SetFlagHalfCarry((value & 0xF) > (A & 0xF) ? 1 : 0);
		SetFlagCarry(value > A ? 1 : 0);

		int result = A - value;
		SetFlagZero(result == 0 ? 1 : 0);
	}

	private void JumpAddress(ushort address)
	{
		PC = address;
	}

	private void JumpImmediate()
	{
		JumpAddress(ReadImmediate());
	}

	private void SetFlagZero(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 7, (byte)value);
	}

	private byte GetFlagZero() => (byte)((F >> 7) & 1);

	private void SetFlagSubtraction(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 6, (byte)value);
	}

	private byte GetFlagSubtraction() => (byte)((F >> 6) & 1);

	private void SetFlagHalfCarry(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 5, (byte)value);
	}
	
	private byte GetFlagHalfCarry() => (byte)((F >> 5) & 1);

	private void SetFlagCarry(int value)
	{
		Debug.Assert(value <= 1);
		Debug.Assert(value >= 0);

		F = ModifyBit(F, 4, (byte)value);
	}

	private byte GetFlagCarry() => (byte)((F >> 4) & 1);

	private static byte ModifyBit(byte original, byte position, byte value)
	{
		int mask = 1 << position;
		return (byte)((original & ~mask) |
					((value << position) & mask));
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
		return (ushort)(memory[address] << 8 | memory[address + 1]);
	}

	//NOTE(Simon): Reads uint16 at PC. PC += 2
	private ushort ReadImmediate()
	{
		ushort value = ReadMemory16(PC);
		PC += 2;
		return value;
	}

	private void SimulateScreen(int cycles)
	{
		//Update scanline register @ FF44, every 456 cycles
		//VBLANK interrupt at scanline 144
		//If scanline > 153, scanline =  0

		//Actually draw scanlines if < line 144
	}
}