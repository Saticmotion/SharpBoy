namespace SharpBoy;

public class Logger
{
	private static string logFileLocation = @"%userprofile%\Desktop\SharpBoy";

	public const bool logFile = true;
	public const bool logConsole = false;

	private static StreamWriter stream;

	static Logger()
	{
		if (logFile)
		{
			string filename = $"SharpBoy_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt";
			string directory = Environment.ExpandEnvironmentVariables(logFileLocation);
			string logFilePath = Path.Join(directory, filename);
			Directory.CreateDirectory(directory);
			stream = new StreamWriter(File.OpenWrite(logFilePath));
		}
	}

	public static void Write(string text)
	{
		if (logFile)
		{
			stream.Write(text);
		}

		if (logConsole)
		{
			Console.Write(text);
		}
	}

	public static void WriteLine(string text)
	{
		if (logFile)
		{
			stream.WriteLine(text);
		}

		if (logConsole)
		{
			Console.WriteLine(text);
		}
	}
}