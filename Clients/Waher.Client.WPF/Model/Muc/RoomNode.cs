﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Client.WPF.Dialogs.Muc;
using Waher.Events;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.MUC;
using Waher.Runtime.Settings;
using Waher.Things.DisplayableParameters;
using Waher.Content.Markdown;

namespace Waher.Client.WPF.Model.Muc
{
	/// <summary>
	/// Represents a room hosted by a Multi-User Chat service.
	/// </summary>
	public class RoomNode : TreeNode
	{
		private readonly Dictionary<string, OccupantNode> occupantByNick = new Dictionary<string, OccupantNode>();
		private readonly string roomId;
		private readonly string domain;
		private readonly string name;
		private string nickName;
		private string password;
		private bool entered;
		private bool entering = false;

		public RoomNode(TreeNode Parent, string RoomId, string Domain, string NickName, string Password, string Name, bool Entered)
			: base(Parent)
		{
			this.roomId = RoomId;
			this.domain = Domain;
			this.name = Name;
			this.nickName = NickName;
			this.password = Password;
			this.entered = Entered;

			this.SetParameters();
		}

		public override string Key => this.Jid;
		public override string Header => string.IsNullOrEmpty(this.name) ? this.Jid : this.name;
		public string RoomId => this.roomId;
		public string Domain => this.domain;
		public string NickName => this.nickName;
		public string Jid => this.roomId + "@" + this.domain;

		private void SetParameters()
		{
			List<Parameter> Parameters = new List<Parameter>()
			{
				new StringParameter("RoomID", "Room ID", this.roomId),
				new StringParameter("Domain", "Domain", this.domain)
			};

			if (!string.IsNullOrEmpty(this.name))
				Parameters.Add(new StringParameter("Name", "Name", this.name));

			if (!string.IsNullOrEmpty(this.nickName))
				Parameters.Add(new StringParameter("NickName", "Nick-Name", this.nickName));

			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};

			this.parameters = new DisplayableParameters(Parameters.ToArray());
		}

		public override string ToolTip => this.name;
		public override bool CanRecycle => true;

		public override void Recycle(MainWindow Window)
		{
			this.entered = false;
		}

		public override string TypeName
		{
			get
			{
				return "Multi-User Chat Room";
			}
		}

		public override ImageSource ImageResource
		{
			get
			{
				if (this.domain == (this.Parent as MucService)?.JID)
				{
					if (this.IsExpanded)
						return XmppAccountNode.folderYellowOpen;
					else
						return XmppAccountNode.folderYellowClosed;
				}
				else
				{
					if (this.IsExpanded)
						return XmppAccountNode.folderBlueOpen;
					else
						return XmppAccountNode.folderBlueClosed;
				}
			}
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}

		public MucService Service
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (Loop != null)
				{
					if (Loop is MucService MucService)
						return MucService;

					Loop = Loop.Parent;
				}

				return null;
			}
		}

		private bool loadingChildren = false;

		public MultiUserChatClient MucClient
		{
			get
			{
				return this.Service?.MucClient;
			}
		}

		public override bool CanAddChildren => true;
		public override bool CanEdit => true;
		public override bool CanDelete => true;
		public override bool CustomDeleteQuestion => true;

		protected override async void LoadChildren()
		{
			try
			{
				if (!this.loadingChildren && !this.IsLoaded)
				{
					Mouse.OverrideCursor = Cursors.Wait;
					this.loadingChildren = true;

					if (!await this.AssertEntered(true))
					{
						this.loadingChildren = false;
						MainWindow.MouseDefault();
						return;
					}

					this.MucClient?.GetOccupants(this.roomId, this.domain, null, null, (sender, e) =>
					{
						this.loadingChildren = false;
						MainWindow.MouseDefault();

						if (e.Ok)
						{
							SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

							this.Service.NodesRemoved(this.children.Values, this);

							lock (this.occupantByNick)
							{
								foreach (KeyValuePair<string, OccupantNode> P in this.occupantByNick)
									Children[P.Key] = P.Value;
							}

							foreach (MucOccupant Occupant in e.Occupants)
							{
								lock (this.occupantByNick)
								{
									if (!this.occupantByNick.TryGetValue(Occupant.NickName, out OccupantNode Node))
									{
										Node = new OccupantNode(this, Occupant.RoomId, Occupant.Domain, Occupant.NickName,
											Occupant.Affiliation, Occupant.Role, Occupant.Jid);

										this.occupantByNick[Occupant.NickName] = Node;
										Children[Occupant.Jid] = Node;
									}
								}
							}

							this.children = new SortedDictionary<string, TreeNode>(Children);
							this.OnUpdated();
							this.Service.NodesAdded(Children.Values, this);
						}
						else
							MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get occupants." : e.ErrorText);

						return Task.CompletedTask;

					}, null);
				}

				base.LoadChildren();
			}
			catch (Exception ex)
			{
				this.loadingChildren = false;
				MainWindow.ErrorBox(ex.Message);
			}
		}

		protected override void UnloadChildren()
		{
			base.UnloadChildren();

			if (this.IsLoaded)
			{
				if (this.children != null)
					this.Service?.NodesRemoved(this.children.Values, this);

				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};

				this.OnUpdated();
			}
		}

		internal async void EnterIfNotAlready(bool ForwardPresence)
		{
			try
			{
				await this.AssertEntered(ForwardPresence);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task<bool> AssertEntered(bool ForwardPresence)
		{
			if (!this.entered && !this.entering)
			{
				this.entering = true;

				try
				{
					UserPresenceEventArgs e;
					EnterRoomForm Form = null;

					if (string.IsNullOrEmpty(this.nickName))
						e = null;
					else
						e = await this.MucClient.EnterRoomAsync(this.roomId, this.domain, this.nickName, this.password);

					while (!(e?.Ok ?? false))
					{
						TaskCompletionSource<bool> InputReceived = new TaskCompletionSource<bool>();

						if (Form is null)
						{
							Form = new EnterRoomForm(this.roomId, this.domain)
							{
								Owner = MainWindow.currentInstance
							};

							Form.NickName.Text = this.nickName;
							Form.Password.Password = this.password;
						}

						MainWindow.UpdateGui(() =>
						{
							bool? Result = Form.ShowDialog();
							InputReceived.TrySetResult(Result.HasValue && Result.Value);
						});

						if (!await InputReceived.Task)
							return false;

						e = await this.MucClient.EnterRoomAsync(this.roomId, this.domain, Form.NickName.Text, Form.Password.Password);
						if (!e.Ok)
							MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to enter room." : e.ErrorText);
					}

					if (!(Form is null))
					{
						string Prefix = this.MucClient.Client.BareJID + "." + this.roomId + "@" + this.domain;
						bool Updated = false;

						if (this.nickName != Form.NickName.Text)
						{
							this.nickName = Form.NickName.Text;
							await RuntimeSettings.SetAsync(Prefix + ".Nick", this.nickName);
							Updated = true;
						}

						if (this.password != Form.Password.Password)
						{
							this.password = Form.Password.Password;
							await RuntimeSettings.SetAsync(Prefix + ".Pwd", this.password);
							Updated = true;
						}

						if (Updated)
							this.OnUpdated();
					}

					if (ForwardPresence)
						await this.Service.MucClient_OccupantPresence(this, e);

					this.entered = true;
				}
				finally
				{
					this.entering = false;
				}
			}

			return true;
		}

		public override async void Edit()
		{
			try
			{
				if (!await this.AssertEntered(true))
					return;

				DataForm ConfigurationForm = await this.MucClient.GetRoomConfigurationAsync(this.roomId, this.domain, (sender, e) =>
				{
					if (e.Ok)
						this.OnUpdated();

					return Task.CompletedTask;
				}, null);

				MainWindow.currentInstance.ShowDataForm(ConfigurationForm);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		public override void Delete(TreeNode Parent, EventHandler OnDeleted)
		{
			DestroyRoomForm Form = new DestroyRoomForm(this.Header)
			{
				Owner = MainWindow.currentInstance
			};

			bool? Result = Form.ShowDialog();
			if (!Result.HasValue || !Result.Value)
				return;

			this.MucClient.DestroyRoom(this.roomId, this.domain, Form.Reason.Text, Form.AlternativeRoomJid.Text, (sender, e) =>
			{
				if (e.Ok)
					base.Delete(Parent, OnDeleted);
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to destroy room." : e.ErrorText);

				return Task.CompletedTask;
			}, null);
		}

		public override void Add()
		{
			SendRoomInvitationForm Form = new SendRoomInvitationForm()
			{
				Owner = MainWindow.currentInstance
			};

			bool? Result = Form.ShowDialog();
			if (!Result.HasValue || !Result.Value)
				return;

			this.MucClient.Invite(this.roomId, this.domain, Form.BareJid.Text, Form.Reason.Text);

			MainWindow.ShowStatus("Invitation sent.");
		}

		public override bool CanChat => true;

		public override void SendChatMessage(string Message, MarkdownDocument Markdown)
		{
			if (Markdown is null)
				this.MucClient.SendGroupChatMessage(this.roomId, this.domain, Message);
			else
				this.MucClient.SendCustomGroupChatMessage(this.roomId, this.domain, XmppContact.MultiFormatMessage(Markdown));
		}

		public override bool RemoveChild(TreeNode Node)
		{
			if (Node is OccupantNode OccupantNode)
			{
				lock (this.occupantByNick)
				{
					this.occupantByNick.Remove(OccupantNode.NickName);
				}
			}

			return base.RemoveChild(Node);
		}

		private OccupantNode AddOccupantNode(string RoomId, string Domain, string NickName, Affiliation? Affiliation, Role? Role, string Jid)
		{
			OccupantNode Node = new OccupantNode(this, RoomId, Domain, NickName, Affiliation, Role, Jid);

			lock (this.occupantByNick)
			{
				this.occupantByNick[NickName] = Node;
			}

			if (this.IsLoaded)
			{
				if (this.children is null)
					this.children = new SortedDictionary<string, TreeNode>() { { Node.Key, Node } };
				else
				{
					lock (this.children)
					{
						this.children[Node.Key] = Node;
					}
				}

				MainWindow.UpdateGui(() =>
				{
					this.Service?.Account?.View?.NodeAdded(this, Node);

					if (!(Node.Children is null))
					{
						foreach (TreeNode Node2 in Node.Children)
							this.Service?.Account?.View?.NodeAdded(Node, Node2);
					}

					this.OnUpdated();
				});
			}

			return Node;
		}

		public OccupantNode GetOccupantNode(string NickName, Affiliation? Affiliation, Role? Role, string Jid)
		{
			OccupantNode Result;

			lock (this.occupantByNick)
			{
				if (!this.occupantByNick.TryGetValue(NickName, out Result))
					Result = null;
			}

			if (!(Result is null))
			{
				bool Changed = false;

				if (Affiliation.HasValue && Affiliation != Result.Affiliation)
				{
					Result.Affiliation = Affiliation;
					Changed = true;
				}

				if (Role.HasValue && Role != Result.Role)
				{
					Result.Role = Role;
					Changed = true;
				}

				if (!string.IsNullOrEmpty(Jid) && Jid != Result.Jid)
				{
					Result.Jid = Jid;
					Changed = true;
				}

				if (Changed)
					Result.OnUpdated();

				return Result;
			}

			Result = this.AddOccupantNode(this.roomId, this.domain, NickName, Affiliation, Role, Jid);

			return Result;
		}
	}
}
