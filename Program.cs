using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseWeChatImg
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				return;
			}
			foreach (var a in args)
			{
				if (File.Exists(a))
					ParseFile(a);
				else if (Directory.Exists(a))
				{
					var files = Directory.EnumerateFiles(a);
					foreach (var f in files)
					{
						ParseFile(f);
					}
				}
			}
		}

		private static void ParseFile(string a, string externName = ".jpg")
		{
			var isep = a.LastIndexOfAny(new char[] { '\\', '/' });
			if (isep < 0)
			{
				Console.WriteLine($"Invalid file path:{a}");
				return;
			}
			var dir = Environment.CurrentDirectory + "/imgs";
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			var file = dir + a.Substring(isep);
			file = file.Replace(".dat", externName);
			Console.WriteLine($"Convert to {file}");
			if (File.Exists(file))
			{
				Console.WriteLine($"{file} exists, skip...");
				return;
			}

			var reader = File.OpenRead(a);
			Console.WriteLine($"{a} file size:{reader.Length} bytes");

			var bs = new byte[2];
			reader.Read(bs, 0, bs.Length);
			var bresult = new byte[bs.Length];
			var jpgconst = new byte[2] { 0xFF, 0xD8 };
			for (var i = 0; i < bs.Length; i++)
			{
				bresult[i] = (byte)(jpgconst[i] ^ bs[i]);
			}
			var axor = bresult[0];
			if (bresult[0] != bresult[1])
			{
				if (externName != ".png")
				{
					var pngconst = new byte[2] { 0x89, 0x50 };
					for (var i = 0; i < bs.Length; i++)
					{
						bresult[i] = (byte)(jpgconst[i] ^ bs[i]);
					}
					if (bresult[0] == bresult[1])
					{
						ParseFile(a, ".png");
						return;
					}
					else
					{
						Console.WriteLine($"--xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r\nInvalid file {a}\r\nFile head is {Convert.ToString(bs[0], 16)}{Convert.ToString(bs[1], 16)}");
					}
				}
				else
				{
					Console.WriteLine($"==xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\r\nInvalid file {a}\r\nFile head is {Convert.ToString(bs[0], 16)}{Convert.ToString(bs[1], 16)}");
					return;
				}
			}
			reader.Position = 0;

			var writer = File.OpenWrite(file);
			var bytes = new byte[1024];
			while (reader.Position < reader.Length)
			{
				writer.WriteByte((byte)(reader.ReadByte() ^ axor));
			}
			reader.Dispose();
			writer.Dispose();
		}
	}
}
