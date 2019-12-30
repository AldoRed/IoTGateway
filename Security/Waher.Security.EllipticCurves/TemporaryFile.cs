﻿using System;
using System.IO;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Class managing the contents of a temporary file. When the class is disposed, the temporary file is deleted.
	/// </summary>
	internal class TemporaryFile : FileStream
	{
		/// <summary>
		/// Default buffer size, in bytes.
		/// </summary>
		public const int DefaultBufferSize = 16384;

		private string fileName;

		/// <summary>
		/// Class managing the contents of a temporary file. When the class is disposed, the temporary file is deleted.
		/// </summary>
		public TemporaryFile()
			: this(Path.GetTempFileName())
		{
		}

		/// <summary>
		/// Class managing the contents of a temporary file. When the class is disposed, the temporary file is deleted.
		/// </summary>
		/// <param name="FileName">Name of temporary file. Call <see cref="Path.GetTempFileName()"/> to get a new temporary file name.</param>
		public TemporaryFile(string FileName)
			: base(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, DefaultBufferSize, FileOptions.RandomAccess)
		{
			this.fileName = FileName;
		}

		/// <summary>
		/// Disposes of the object, and deletes the temporary file.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!(this.fileName is null))
			{
				File.Delete(this.fileName);
				this.fileName = null;
			}
		}
	}
}