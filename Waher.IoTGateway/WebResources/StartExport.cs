﻿using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.IoTGateway.WebResources.ExportFormats;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Starts data export
	/// </summary>
	public class StartExport : HttpSynchronousResource, IHttpPostMethod
	{
		internal static RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
		internal static AesCryptoServiceProvider aes = GetCryptoProvider();

		/// <summary>
		/// Starts data export
		/// </summary>
		public StartExport()
			: base("/StartExport")
		{
		}

		private static AesCryptoServiceProvider GetCryptoProvider()
		{
			AesCryptoServiceProvider Result = new AesCryptoServiceProvider()
			{
				BlockSize = 128,
				KeySize = 256,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.None
			};

			return Result;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				Gateway.AssertUserAuthenticated(Request, "Admin.Data.Backup");

				if (!Request.HasData ||
					!(Request.DecodeData() is Dictionary<string, object> RequestObj) ||
					!RequestObj.TryGetValue("TypeOfFile", out object Obj) || !(Obj is string TypeOfFile) ||
					!RequestObj.TryGetValue("Database", out Obj) || !(Obj is bool Database) ||
					!RequestObj.TryGetValue("Ledger", out Obj) || !(Obj is bool Ledger) ||
					!RequestObj.TryGetValue("WebContent", out Obj) || !(Obj is bool WebContent) ||
					!RequestObj.TryGetValue("OnlySelectedCollections", out Obj) || !(Obj is bool OnlySelectedCollections) ||
					!RequestObj.TryGetValue("selectedCollections", out Obj) || !(Obj is Array SelectedCollections) ||
					!RequestObj.TryGetValue("exportOnly", out Obj) || !(Obj is bool ExportOnly))
				{
					throw new BadRequestException();
				}

				ExportInfo ExportInfo = GetExporter(TypeOfFile, OnlySelectedCollections, SelectedCollections);
				Task T;

				lock (synchObject)
				{
					if (exporting)
					{
						Response.StatusCode = 409;
						Response.StatusMessage = "Conflict";
						Response.ContentType = "text/plain";
						T = Response.Write("Export is underway.");
					}
					else
					{
						exporting = true;
						T = null;
					}
				}

				if (!(T is null))
				{
					await T;
					return;
				}

				if (!ExportOnly)
				{
					Export.ExportType = TypeOfFile;
					Export.ExportDatabase = Database;
					Export.ExportLedger = Ledger;
					Export.ExportWebContent = WebContent;
				}

				List<string> Folders = new List<string>();

				foreach (Export.FolderCategory FolderCategory in Export.GetRegisteredFolders())
				{
					if (RequestObj.TryGetValue(FolderCategory.CategoryId, out Obj) && Obj is bool b)
					{
						if (!ExportOnly)
							await Export.SetExportFolderAsync(FolderCategory.CategoryId, b);

						if (b)
							Folders.AddRange(FolderCategory.Folders);
					}
				}

				Task _ = DoExport(ExportInfo.Exporter, Database, Ledger, WebContent, Folders.ToArray());

				Response.StatusCode = 200;
				Response.ContentType = "text/plain";
				await Response.Write(ExportInfo.LocalBackupFileName);
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

		private static bool exporting = false;
		private static readonly object synchObject = new object();

		internal class ExportInfo
		{
			public string LocalBackupFileName;
			public string LocalKeyFileName;
			public string FullBackupFileName;
			public string FullKeyFileName;
			public IExportFormat Exporter;
		}

		internal static ExportInfo GetExporter(string TypeOfFile, bool OnlySelectedCollections, Array SelectedCollections)
		{
			ExportInfo Result = new ExportInfo();
			string BasePath = Export.FullExportFolder;

			if (!Directory.Exists(BasePath))
				Directory.CreateDirectory(BasePath);

			BasePath += Path.DirectorySeparatorChar;

			Result.FullBackupFileName = BasePath + DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");

			switch (TypeOfFile)
			{
				case "XML":
					Result.FullBackupFileName = GetUniqueFileName(Result.FullBackupFileName, ".xml");
					FileStream fs = new FileStream(Result.FullBackupFileName, FileMode.Create, FileAccess.Write);
					DateTime Created = File.GetCreationTime(Result.FullBackupFileName);
					XmlWriterSettings Settings = XML.WriterSettings(true, false);
					Settings.Async = true;
					XmlWriter XmlOutput = XmlWriter.Create(fs, Settings);
					Result.LocalBackupFileName = Result.FullBackupFileName.Substring(BasePath.Length);
					Result.Exporter = new XmlExportFormat(Result.LocalBackupFileName, Created, XmlOutput, fs, OnlySelectedCollections, SelectedCollections);
					break;

				case "Binary":
					Result.FullBackupFileName = GetUniqueFileName(Result.FullBackupFileName, ".bin");
					fs = new FileStream(Result.FullBackupFileName, FileMode.Create, FileAccess.Write);
					Created = File.GetCreationTime(Result.FullBackupFileName);
					Result.LocalBackupFileName = Result.FullBackupFileName.Substring(BasePath.Length);
					Result.Exporter = new BinaryExportFormat(Result.LocalBackupFileName, Created, fs, fs, OnlySelectedCollections, SelectedCollections);
					break;

				case "Compressed":
					Result.FullBackupFileName = GetUniqueFileName(Result.FullBackupFileName, ".gz");
					fs = new FileStream(Result.FullBackupFileName, FileMode.Create, FileAccess.Write);
					Created = File.GetCreationTime(Result.FullBackupFileName);
					Result.LocalBackupFileName = Result.FullBackupFileName.Substring(BasePath.Length);
					GZipStream gz = new GZipStream(fs, CompressionLevel.Optimal, false);
					Result.Exporter = new BinaryExportFormat(Result.LocalBackupFileName, Created, gz, fs, OnlySelectedCollections, SelectedCollections);
					break;

				case "Encrypted":
					Result.FullBackupFileName = GetUniqueFileName(Result.FullBackupFileName, ".bak");
					fs = new FileStream(Result.FullBackupFileName, FileMode.Create, FileAccess.Write);
					Created = File.GetCreationTime(Result.FullBackupFileName);
					Result.LocalBackupFileName = Result.FullBackupFileName.Substring(BasePath.Length);

					byte[] Key = new byte[32];
					byte[] IV = new byte[16];

					lock (rnd)
					{
						rnd.GetBytes(Key);
						rnd.GetBytes(IV);
					}

					ICryptoTransform AesTransform = aes.CreateEncryptor(Key, IV);
					CryptoStream cs = new CryptoStream(fs, AesTransform, CryptoStreamMode.Write);

					gz = new GZipStream(cs, CompressionLevel.Optimal, false);
					Result.Exporter = new BinaryExportFormat(Result.LocalBackupFileName, Created, gz, fs, cs, 32, OnlySelectedCollections, SelectedCollections);

					string BasePath2 = Export.FullKeyExportFolder;

					if (!Directory.Exists(BasePath2))
						Directory.CreateDirectory(BasePath2);

					BasePath2 += Path.DirectorySeparatorChar;
					Result.LocalKeyFileName = Result.LocalBackupFileName.Replace(".bak", ".key");
					Result.FullKeyFileName = BasePath2 + Result.LocalKeyFileName;

					using (XmlOutput = XmlWriter.Create(Result.FullKeyFileName, XML.WriterSettings(true, false)))
					{
						XmlOutput.WriteStartDocument();
						XmlOutput.WriteStartElement("KeyAes256", Export.ExportNamepace);
						XmlOutput.WriteAttributeString("key", Convert.ToBase64String(Key));
						XmlOutput.WriteAttributeString("iv", Convert.ToBase64String(IV));
						XmlOutput.WriteEndElement();
						XmlOutput.WriteEndDocument();
					}

					long Size;

					try
					{
						using (fs = File.OpenRead(Result.FullKeyFileName))
						{
							Size = fs.Length;
						}
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						Size = 0;
					}

					Created = File.GetCreationTime(Result.FullKeyFileName);

					ExportFormat.UpdateClientsFileUpdated(Result.LocalKeyFileName, Size, Created);
					break;

				default:
					throw new NotSupportedException("Unsupported file type.");
			}

			return Result;
		}

		/// <summary>
		/// Gets a unique filename.
		/// </summary>
		/// <param name="Base">Filename base</param>
		/// <param name="Extension">Extension</param>
		/// <returns>Unique file name.</returns>
		public static string GetUniqueFileName(string Base, string Extension)
		{
			string Suffix = string.Empty;
			string s;
			int i = 1;

			while (true)
			{
				s = Base + Suffix + Extension;
				if (!File.Exists(s))
					return s;

				i++;
				Suffix = " (" + i.ToString() + ")";
			}
		}

		/// <summary>
		/// Exports gateway data
		/// </summary>
		/// <param name="Output">Export Output</param>
		/// <param name="Database">If the contents of the database is to be exported.</param>
		/// <param name="Ledger">If the contents of the ledger is to be exported.</param>
		/// <param name="WebContent">If web content is to be exported.</param>
		/// <param name="Folders">Root subfolders to export.</param>
		public static async Task DoExport(IExportFormat Output, bool Database, bool Ledger, bool WebContent, string[] Folders)
		{
			try
			{
				List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>()
				{
					new KeyValuePair<string, object>("Database", Database),
					new KeyValuePair<string, object>("Ledger", Ledger)
				};

				foreach (string Folder in Folders)
					Tags.Add(new KeyValuePair<string, object>(Folder, true));

				Log.Informational("Starting export.", Output.FileName, Tags.ToArray());

				await Output.Start();

				if (Database)
				{
					StringBuilder Temp = new StringBuilder();
					using (XmlWriter w = XmlWriter.Create(Temp, XML.WriterSettings(false, true)))
					{
						await Persistence.Database.Analyze(w, Path.Combine(Gateway.AppDataFolder, "Transforms", "DbStatXmlToHtml.xslt"), 
							Path.Combine(Gateway.RootFolder, "Data"), false, true);
					}

					await Persistence.Database.Export(Output, Output.CollectionNames);
				}

				if (Ledger && Persistence.Ledger.HasProvider)
					await Persistence.Ledger.Export(Output, Output.CollectionNames);

				if (WebContent || Folders.Length > 0)
				{
					await Output.StartFiles();
					try
					{
						string[] FileNames;
						string Folder2;

						if (WebContent)
						{
							await ExportFile(Path.Combine(Gateway.AppDataFolder, "Gateway.config"), Output);

							FileNames = Directory.GetFiles(Gateway.RootFolder, "*.*", SearchOption.TopDirectoryOnly);

							foreach (string FileName in FileNames)
								await ExportFile(FileName, Output);

							Export.FolderCategory[] ExportFolders = Export.GetRegisteredFolders();

							foreach (string Folder in Directory.GetDirectories(Gateway.RootFolder, "*.*", SearchOption.TopDirectoryOnly))
							{
								bool IsWebContent = true;

								foreach (Export.FolderCategory FolderCategory in ExportFolders)
								{
									foreach (string Folder3 in FolderCategory.Folders)
									{
										if (string.Compare(Folder3, Folder, true) == 0)
										{
											IsWebContent = false;
											break;
										}
									}

									if (!IsWebContent)
										break;
								}

								if (IsWebContent)
								{
									FileNames = Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories);

									foreach (string FileName in FileNames)
										await ExportFile(FileName, Output);
								}
							}
						}

						foreach (string Folder in Folders)
						{
							if (Directory.Exists(Folder2 = Path.Combine(Gateway.RootFolder, Folder)))
							{
								FileNames = Directory.GetFiles(Folder2, "*.*", SearchOption.AllDirectories);

								foreach (string FileName in FileNames)
									await ExportFile(FileName, Output);
							}
						}
					}
					finally
					{
						await Output.EndFiles();
					}
				}

				Log.Informational("Export successfully completed.", Output.FileName);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				string[] Tabs = ClientEvents.GetTabIDsForLocation("/Settings/Backup.md");
				await ClientEvents.PushEvent(Tabs, "BackupFailed", "{\"fileName\":\"" + CommonTypes.JsonStringEncode(Output.FileName) +
					"\", \"message\": \"" + CommonTypes.JsonStringEncode(ex.Message) + "\"}", true, "User");
			}
			finally
			{
				try
				{
					await Output.End();
					Output.Dispose();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				lock (synchObject)
				{
					exporting = false;
				}
			}
		}

		private static async Task ExportFile(string FileName, IExportFormat Output)
		{
			using (FileStream fs = File.OpenRead(FileName))
			{
				if (FileName.StartsWith(Gateway.AppDataFolder))
					FileName = FileName.Substring(Gateway.AppDataFolder.Length);

				await Output.ExportFile(FileName, fs);
			}

			await Output.UpdateClient(false);
		}

	}
}
