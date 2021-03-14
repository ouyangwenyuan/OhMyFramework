/*-------------------------------------------------------------------------------------------
// Copyright (C) 2020 成都，天龙互娱
//
// 模块名：Zip
// 创建日期：2020-6-24
// 创建者：qibo.li
// 模块描述：对asset的压缩和解压缩
//-------------------------------------------------------------------------------------------*/
using System;
using System.IO;
using DragonU3DSDK.AsyncTask;

namespace DragonU3DSDK.Asset.Zip
{
	/*
    public class Tool
    {
		public static void CompressFileLZMA(string inFile, string outFile)
		{
			SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();
			FileStream input = new FileStream(inFile, FileMode.Open);
			FileStream output = new FileStream(outFile, FileMode.Create);

			// Write the encoder properties
			coder.WriteCoderProperties(output);

			// Write the decompressed file size.
			output.Write(BitConverter.GetBytes(input.Length), 0, 8);

			// Encode the file.
			coder.Code(input, output, input.Length, -1, null);
			output.Flush();
			output.Close();
			input.Flush();
			input.Close();
		}

		public static void CompressStreamLZMA(byte[] stream, string outFile)
		{
			SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();
			Stream input = new MemoryStream(stream);
			FileStream output = new FileStream(outFile, FileMode.Create);

			// Write the encoder properties
			coder.WriteCoderProperties(output);

			// Write the decompressed file size.
			output.Write(BitConverter.GetBytes(input.Length), 0, 8);

			// Encode the file.
			coder.Code(input, output, input.Length, -1, null);
			output.Flush();
			output.Close();
			input.Flush();
			input.Close();
		}

		public static void DecompressFileLZMA(string inFile, string outFile)
		{
			SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
			FileStream input = new FileStream(inFile, FileMode.Open);
			FileStream output = new FileStream(outFile, FileMode.Create);

			// Read the decoder properties
			byte[] properties = new byte[5];
			input.Read(properties, 0, 5);

			// Read in the decompress file size.
			byte[] fileLengthBytes = new byte[8];
			input.Read(fileLengthBytes, 0, 8);
			long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

			// Decompress the file.
			coder.SetDecoderProperties(properties);
			coder.Code(input, output, input.Length, fileLength, null);
			output.Flush();
			output.Close();
			input.Flush();
			input.Close();
		}

		public static void DecompressStreamLZMA(byte[] stream, string outFile)
		{
			SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
			Stream input = new MemoryStream(stream);
			FileStream output = new FileStream(outFile, FileMode.Create);

			// Read the decoder properties
			byte[] properties = new byte[5];
			input.Read(properties, 0, 5);

			// Read in the decompress file size.
			byte[] fileLengthBytes = new byte[8];
			input.Read(fileLengthBytes, 0, 8);
			long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

			// Decompress the file.
			coder.SetDecoderProperties(properties);
			coder.Code(input, output, input.Length, fileLength, null);
			output.Flush();
			output.Close();
			input.Flush();
			input.Close();
		}

		public static void DecompressAB(string fileName, Action callback)
		{
			string path = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, fileName);

			ZipDecompressFileTask task = new ZipDecompressFileTask();
			task.path = path;
			task.callBack = callback;
			AsyncTaskManager.Instance.AddTask(task);

			DebugUtil.Log(string.Format("DecompressAB : {0}", fileName));
		}
	}

	public class ZipDecompressFileTask : Task
	{
		public string path;

		public Action callBack;

		public override Priority Priority { get { return Priority.Low; } }

		public override bool ThreadSafe { get { return true; } }

		public override void Execute()
		{
			string tempPath = path + ".ziptemp";
			if (File.Exists(tempPath))
			{
				File.Delete(tempPath);
			}
			File.Move(path, tempPath);

			Tool.DecompressFileLZMA(tempPath, path);

			File.Delete(tempPath);
		}

		public override void OnFinish()
		{
			if(exception == null)
            {
				DebugUtil.Log(string.Format("ZipDecompressFileTask success : {0}", path));
				callBack?.Invoke();
			}
            else
            {
				DebugUtil.LogError(string.Format("ZipDecompressFileTask error , exception : {0} ; path : {1}", exception.ToString(), path));
				
				if(Utils.IsDiskFull(exception))
                {
					EventManager.Instance.Trigger<SDKEvents.DiskFullEvent>().Trigger();
				}
			}
			callBack = null;
		}
	}
	*/
}
