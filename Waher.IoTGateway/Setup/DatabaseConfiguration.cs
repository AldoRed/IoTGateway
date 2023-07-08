﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.IoTGateway.Setup.Databases;
using Waher.IoTGateway.Setup.Databases.Sniffing;
using Waher.Content.Json;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Database Configuration
	/// </summary>
	public class DatabaseConfiguration : SystemMultiStepConfiguration
	{
		private static DatabaseConfiguration instance = null;
		private static string[] repairedCollections = null;

		private HttpResource selectDatabase = null;
		private HttpResource testDatabase = null;

		private IDatabasePlugin databasePlugin = null;
		private string databasePluginName = null;
		private DatabaseSettings databasePluginSettings = null;

		private SniffableDatabase sniffableDatabase = null;
		private SniffableLedger sniffableLedger = null;

		/// <summary>
		/// Full name of database plugin class.
		/// </summary>
		[DefaultValueNull]
		public string DatabasePluginName
		{
			get => this.databasePluginName;
			set => this.databasePluginName = value;
		}

		/// <summary>
		/// Settings for database plugin.
		/// </summary>
		[DefaultValueNull]
		public DatabaseSettings DatabasePluginSettings
		{
			get => this.databasePluginSettings;
			set => this.databasePluginSettings = value;
		}

		/// <summary>
		/// Current database plugin, if defined, null otherwise.
		/// </summary>
		public IDatabasePlugin DatabasePlugin
		{
			get
			{
				if (this.databasePlugin is null)
				{
					if (this.databasePluginName is null)
						return null;

					Type T = Types.GetType(this.databasePluginName);
					if (!(T is null))
						this.databasePlugin = Types.Instantiate(T) as IDatabasePlugin;
				}

				return this.databasePlugin;
			}
		}

		/// <summary>
		/// Makes database activity sniffable.
		/// </summary>
		public SniffableDatabase SniffableDatabase
		{
			get
			{
				if (sniffableDatabase is null)
					sniffableDatabase = new SniffableDatabase();

				return sniffableDatabase;
			}
		}

		/// <summary>
		/// Makes ledger activity sniffable.
		/// </summary>
		public SniffableLedger SniffableLedger
		{
			get
			{
				if (sniffableLedger is null)
					sniffableLedger = new SniffableLedger();

				return sniffableLedger;
			}
		}

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static DatabaseConfiguration Instance => instance;

		/// <summary>
		/// Collections repaired during startup.
		/// </summary>
		public static string[] RepairedCollections
		{
			get => repairedCollections;
			internal set => repairedCollections = value;
		}

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Database.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 0;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 1, "Database");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override async Task ConfigureSystem()
		{
			IDatabasePlugin Plugin = this.DatabasePlugin;

			if (!(Plugin is null))
				await Plugin.ConfigureSettings(this.databasePluginSettings);
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as DatabaseConfiguration;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.selectDatabase = WebServer.Register("/Settings/SelectDatabase", null, this.SelectDatabase, true, false, true);
			this.testDatabase = WebServer.Register("/Settings/TestDatabase", null, this.TestDatabase, true, false, true);

			return base.InitSetup(WebServer);
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.selectDatabase);
			WebServer.Unregister(this.testDatabase);

			return base.UnregisterSetup(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Data.Database";

		private async Task SelectDatabase(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is string PluginName))
				throw new BadRequestException();

			Type PluginType = Types.GetType(PluginName);
			if (PluginType is null)
				throw new NotFoundException("Database plugin not found: " + PluginName);

			if (!(Types.Instantiate(PluginType) is IDatabasePlugin Plugin))
				throw new BadRequestException();

			if (this.databasePluginName != PluginName)
			{
				this.databasePlugin = Plugin;
				this.databasePluginName = PluginName;
				this.databasePluginSettings = Plugin.CreateNewSettings();

				if (string.IsNullOrEmpty(Plugin.SettingsResource))
					this.Step = 1;
				else
				{
					this.Step = 0;
					this.Complete = false;
				}
			}

			Response.ContentType = JsonCodec.DefaultContentType;

			string Html = string.Empty;
			bool HasSettings = false;
			string ResourceName = Plugin?.SettingsResource;
			if (!string.IsNullOrEmpty(ResourceName))
			{
				if (ResourceName.StartsWith("/"))
					ResourceName = ResourceName.Substring(1);

				ResourceName = ResourceName.Replace('/', Path.DirectorySeparatorChar);
				ResourceName = Path.Combine(Gateway.RootFolder, ResourceName);
				if (File.Exists(ResourceName))
				{
					string Markdown = await Resources.ReadAllTextAsync(ResourceName);
					MarkdownSettings Settings = new MarkdownSettings()
					{
						Variables = Request.Session
					};
					MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings);

					Html = await Doc.GenerateHTML();
					Html = HtmlDocument.GetBody(Html);
					HasSettings = true;
				}
			}

			await Response.Write(JSON.Encode(new Dictionary<string, object>()
			{
				{ "html", Html },
				{ "isDone", this.Step >= 1 },
				{ "hasSettings", HasSettings },
				{ "restart", Database.Locked }
			}, false));
		}

		private async Task TestDatabase(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is Dictionary<string, object> Form))
				throw new BadRequestException();

			if (!Form.TryGetValue("save", out Obj) ||
				!(Obj is bool Save) ||
				!Form.TryGetValue("Plugin", out Obj) ||
				!(Obj is string PluginName) ||
				this.databasePluginName != PluginName ||
				this.databasePlugin is null ||
				this.databasePluginSettings is null)
			{
				throw new BadRequestException();
			}

			await this.databasePlugin.Test(Form, Save, this.databasePluginSettings);

			if (Save)
			{
				this.Step = 1;
				await Gateway.InternalDatabase.Update(this);
			}

			Response.ContentType = "text/plain";

			if (Database.Locked)
				await Response.Write("2");
			else
				await Response.Write("1");

			await Response.SendResponse();
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			InternalDatabase Plugin = new InternalDatabase();

			this.databasePlugin = Plugin;
			this.databasePluginName = Plugin.GetType().FullName;
			this.databasePluginSettings = Plugin.CreateNewSettings();
			this.Step = 1;

			return Task.FromResult(true);
		}

	}
}
