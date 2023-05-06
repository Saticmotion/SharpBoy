using System.Buffers.Binary;
using System.Diagnostics;

namespace ChipSharp;

public class Emulator
{
	public static int screenWidth = 160;
	public static int screenHeight = 144;

	private byte[] memory = new byte[0x10000];

	private byte[] backbuffer = new byte[256 * 256];

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

		return realCycles;
	}

	//NOTE(Simon): Return cycles taken
	private int SimulateNextOpcode()
	{
		byte opcode = memory[PC];
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
				return 1;
			case 0x89:
				//NOTE(Simon): ADC C
				AddWithCarryRegister(C);
				return 1;
			case 0x8A:
				//NOTE(Simon): ADC D
				AddWithCarryRegister(D);
				return 1;
			case 0x8B:
				//NOTE(Simon): ADC E
				AddWithCarryRegister(E);
				return 1;
			case 0x8C:
				//NOTE(Simon): ADC H
				AddWithCarryRegister(H);
				return 1;
			case 0x8D:
				//NOTE(Simon): ADC L
				AddWithCarryRegister(L);
				return 1;
			case 0x8E:
				throw new NotImplementedException();
			case 0x8F:
				//NOTE(Simon): ADC A
				AddWithCarryRegister(A);
				return 1;
			#endregion

			#region 9x
			case 0x90:
				throw new NotImplementedException();
			case 0x91:
				throw new NotImplementedException();
			case 0x92:
				throw new NotImplementedException();
			case 0x93:
				throw new NotImplementedException();
			case 0x94:
				throw new NotImplementedException();
			case 0x95:
				throw new NotImplementedException();
			case 0x96:
				throw new NotImplementedException();
			case 0x97:
				throw new NotImplementedException();
			case 0x98:
				throw new NotImplementedException();
			case 0x99:
				throw new NotImplementedException();
			case 0x9A:
				throw new NotImplementedException();
			case 0x9B:
				throw new NotImplementedException();
			case 0x9C:
				throw new NotImplementedException();
			case 0x9D:
				throw new NotImplementedException();
			case 0x9E:
				throw new NotImplementedException();
			case 0x9F:
				throw new NotImplementedException();
			#endregion

			#region Ax
			case 0xA0:
				throw new NotImplementedException();
			case 0xA1:
				throw new NotImplementedException();
			case 0xA2:
				throw new NotImplementedException();
			case 0xA3:
				throw new NotImplementedException();
			case 0xA4:
				throw new NotImplementedException();
			case 0xA5:
				throw new NotImplementedException();
			case 0xA6:
				throw new NotImplementedException();
			case 0xA7:
				throw new NotImplementedException();
			case 0xA8:
				throw new NotImplementedException();
			case 0xA9:
				throw new NotImplementedException();
			case 0xAA:
				throw new NotImplementedException();
			case 0xAB:
				throw new NotImplementedException();
			case 0xAC:
				throw new NotImplementedException();
			case 0xAD:
				throw new NotImplementedException();
			case 0xAE:
				throw new NotImplementedException();
			case 0xAF:
				throw new NotImplementedException();
			#endregion

			#region Bx
			case 0xB0:
				throw new NotImplementedException();
			case 0xB1:
				throw new NotImplementedException();
			case 0xB2:
				throw new NotImplementedException();
			case 0xB3:
				throw new NotImplementedException();
			case 0xB4:
				throw new NotImplementedException();
			case 0xB5:
				throw new NotImplementedException();
			case 0xB6:
				throw new NotImplementedException();
			case 0xB7:
				throw new NotImplementedException();
			case 0xB8:
				throw new NotImplementedException();
			case 0xB9:
				throw new NotImplementedException();
			case 0xBA:
				throw new NotImplementedException();
			case 0xBB:
				throw new NotImplementedException();
			case 0xBC:
				throw new NotImplementedException();
			case 0xBD:
				throw new NotImplementedException();
			case 0xBE:
				throw new NotImplementedException();
			case 0xBF:
				throw new NotImplementedException();
			#endregion

			#region Cx
			case 0xC0:
				throw new NotImplementedException();
			case 0xC1:
				throw new NotImplementedException();
			case 0xC2:
				throw new NotImplementedException();
			case 0xC3:
				throw new NotImplementedException();
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
				throw new NotImplementedException();
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
		bool halfCarry = (A & 0xF) + (value & 0xF) > 0xF;
		SetFlagHalfCarry(halfCarry ? (byte)1 : (byte)0);

		bool carry = A + value > 0xFF;
		SetFlagCarry(carry ? (byte)1 : (byte)0);

		byte result = (byte)(A + value);

		bool zero = result == 0;
		SetFlagZero(zero ? (byte)1 : (byte)0);
	}

	private void AddWithCarryRegister(byte value)
	{
		AddRegister((byte)(value + GetFlagCarry()));
	}

	private void SetFlagZero(byte value)
	{
		Debug.Assert(value <= 1);

		F = ModifyBit(F, 7, value);
	}

	private byte GetFlagZero() => (byte)((F >> 7) & 1);

	private void SetFlagSubtraction(byte value)
	{
		Debug.Assert(value <= 1);

		F = ModifyBit(F, 6, value);
	}

	private byte GetFlagSubtraction() => (byte)((F >> 6) & 1);

	private void SetFlagHalfCarry(byte value)
	{
		Debug.Assert(value <= 1);

		F = ModifyBit(F, 5, value);
	}
	
	private byte GetFlagHalfCarry() => (byte)((F >> 5) & 1);

	private void SetFlagCarry(byte value)
	{
		Debug.Assert(value <= 1);

		F = ModifyBit(F, 4, value);
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