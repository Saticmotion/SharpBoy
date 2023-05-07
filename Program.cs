using System.Diagnostics;
using Raylib_cs;
using System.Numerics;

namespace ChipSharp;

public class Program
{
	private static int physicalScreenWidth = 800;
	private static int physicalScreenHeight = 600;

	//NOTE(Simon): The Gameboy runs at 4MiHz (4 * 1024 * 1024)
	private static int FPS = 60;
	private static int hz = 1;
	private static int khz = hz * 1024;
	private static int mhz = khz * 1024;

	private static bool[] input = new bool[8];
	private static KeyboardKey[] inputMap = {
		KeyboardKey.KEY_RIGHT,			//Right
		KeyboardKey.KEY_LEFT,			//Left
		KeyboardKey.KEY_UP,				//Up
		KeyboardKey.KEY_DOWN,			//Down

		KeyboardKey.KEY_A,				//A
		KeyboardKey.KEY_B,				//B
		KeyboardKey.KEY_BACKSPACE,		//Select
		KeyboardKey.KEY_ENTER,			//Start
	};

	public static void Main(string[] args)
	{
		var emulator = new Emulator();
		string path = @"C:\Users\Simon\Downloads\Tetris\Tetris.gb";
		emulator.LoadProgram(ReadProgramFromDisk(path));

		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
		Raylib.InitWindow(physicalScreenWidth, physicalScreenHeight, "SharpBoy");
		Raylib.SetTargetFPS(FPS);

		var target = Raylib.LoadRenderTexture(Emulator.screenWidth, Emulator.screenHeight);
		target.texture.format = PixelFormat.PIXELFORMAT_UNCOMPRESSED_R8G8B8;
		Raylib.SetTextureWrap(target.texture, TextureWrap.TEXTURE_WRAP_CLAMP);
		Raylib.SetTextureFilter(target.texture, TextureFilter.TEXTURE_FILTER_POINT);

		float scale = Math.Min(Raylib.GetScreenWidth() / (float)Emulator.screenWidth, Raylib.GetScreenHeight() / (float)Emulator.screenHeight);

		//NOTE(Simon): We lose some precision due to integer division. But at 60fps that works out to being 80ms slower per day of runtime
		int cyclesPerFrame = 4 * mhz / 60;

		while (!Raylib.WindowShouldClose())
		{
			UpdateInput();
			emulator.Simulate(input, cyclesPerFrame);

			Raylib.BeginTextureMode(target);
			{
				unsafe
				{
					fixed (void* p = emulator.screen)
					{
						Raylib.UpdateTexture(target.texture, p);
					}
				}
			}
			Raylib.EndTextureMode();

			Raylib.BeginDrawing();
			{
				Raylib.ClearBackground(Color.BLACK);

				var sourceRect = new Rectangle(0, 0, target.texture.width, -target.texture.height);
				var destRect = new Rectangle(Raylib.GetScreenWidth() - (Emulator.screenWidth * scale) * .5f, Raylib.GetScreenHeight() - (Emulator.screenHeight * scale) * .5f, Emulator.screenWidth * scale, Emulator.screenHeight * scale);

				Raylib.DrawTexturePro(target.texture, sourceRect, destRect, new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f), 0, Color.WHITE);

			}
			Raylib.EndDrawing();
		}
		Raylib.CloseWindow();
	}

	public static void UpdateInput()
	{
		for (int i = 0; i < inputMap.Length; i++)
		{
			var key = inputMap[i];
			if (Raylib.IsKeyPressed(key))
			{
				input[i] = true;
				//TODO(Simon): Handle interrupts???
			}
		}

		for (int i = 0; i < inputMap.Length; i++)
		{
			if (Raylib.IsKeyReleased(inputMap[i]))
			{
				input[i] = false;
			}
		}
	}

	public static byte[] ReadProgramFromDisk(string path)
	{
		return File.ReadAllBytes(path);
	}
}