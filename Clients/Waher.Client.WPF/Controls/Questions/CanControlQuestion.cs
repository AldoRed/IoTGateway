﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Client.WPF.Controls.Questions
{
	public class CanControlQuestion : NodeQuestion
	{
		private SortedDictionary<string, bool> parametersSorted = null;
		private ListBox parametersListBox = null;
		private ProvisioningClient client;
		private QuestionView questionView;
		private OperationRange range;
		private string parameter = null;
		private string[] parameterNames = null;
		private string[] availableParameterNames = null;
		private bool registered = false;

		public CanControlQuestion()
			: base()
		{
		}

		[DefaultValueNull]
		public string[] ParameterNames
		{
			get => this.parameterNames;
			set => this.parameterNames = value;
		}

		[DefaultValueNull]
		public string[] AvailableParameterNames
		{
			get => this.availableParameterNames;
			set => this.availableParameterNames = value;
		}

		public override string QuestionString => "Allowed to control?";

		public override void PopulateDetailsDialog(QuestionView QuestionView, ProvisioningClient ProvisioningClient)
		{
			StackPanel Details = QuestionView.Details;
			TextBlock TextBlock;
			ListBox ListBox;
			Button Button;

			this.client = ProvisioningClient;
			this.questionView = QuestionView;

			Details.Children.Add(new TextBlock()
			{
				FontSize = 18,
				FontWeight = FontWeights.Bold,
				Text = "Allowed to control?"
			});

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6)
			});

			TextBlock.Inlines.Add("Device: ");
			this.AddJidName(this.JID, ProvisioningClient, TextBlock);

			this.AddNodeInfo(Details);

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6)
			});

			TextBlock.Inlines.Add("Caller: ");
			this.AddJidName(this.RemoteJID, ProvisioningClient, TextBlock);


			Details.Children.Add(new Label()
			{
				Content = "Parameter restriction:",
				Margin = new Thickness(0, 6, 0, 0)
			});

			Details.Children.Add(ListBox = new ListBox()
			{
				MaxHeight = 150,
				SelectionMode = SelectionMode.Multiple,
				Margin = new Thickness(0, 0, 0, 6)
			});

			this.parametersListBox = ListBox;

			if (this.availableParameterNames is null)
			{
				if (!(this.parameterNames is null))
				{
					foreach (string ParameterName in this.parameterNames)
					{
						ListBox.Items.Add(new ListBoxItem()
						{
							Content = ParameterName,
							IsSelected = true,
							Tag = ParameterName
						});
					}
				}
			}
			else
			{
				foreach (string ParameterName in this.availableParameterNames)
				{
					ListBox.Items.Add(new ListBoxItem()
					{
						Content = ParameterName,
						IsSelected = (this.parameterNames is null || Array.IndexOf<string>(this.parameterNames, ParameterName) >= 0),
						Tag = ParameterName
					});
				}
			}

			StackPanel StackPanel = CanReadQuestion.AddAllClearButtons(Details, ListBox);

			if (this.availableParameterNames is null)
			{
				StackPanel.Children.Add(Button = new Button()
				{
					Margin = new Thickness(6, 6, 6, 6),
					Padding = new Thickness(20, 0, 20, 0),
					Content = "Get List",
					Tag = ListBox
				});

				Button.Click += this.GetListButton_Click;
			}

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6),
				Text = "Is the caller allowed to control your device?"
			});

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "Yes"
			});

			Button.Click += this.YesButton_Click;

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "No"
			});

			Button.Click += this.NoButton_Click;

			string s = this.RemoteJID;
			int i = s.IndexOf('@');
			if (i >= 0)
			{
				s = s.Substring(i + 1);

				Details.Children.Add(Button = new Button()
				{
					Margin = new Thickness(0, 6, 0, 6),
					Content = "Yes, to anyone from " + s
				});

				Button.Click += this.YesDomainButton_Click;

				Details.Children.Add(Button = new Button()
				{
					Margin = new Thickness(0, 6, 0, 6),
					Content = "No, to no one from " + s
				});

				Button.Click += this.NoDomainButton_Click;
			}

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "Yes, to anyone"
			});

			Button.Click += this.YesAllButton_Click;

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "No, to no one"
			});

			Button.Click += this.NoAllButton_Click;

			this.AddTokens(Details, this.client, this.YesTokenButton_Click, this.NoTokenButton_Click);
		}

		private void GetListButton_Click(object Sender, RoutedEventArgs e)
		{
			XmppClient Client = this.client.Client;

			((Button)Sender).IsEnabled = false;

			RosterItem Item = Client[this.JID];
			if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
			{
				if (!this.registered)
				{
					this.registered = true;
					this.questionView.Owner.RegisterRosterEventHandler(this.JID, this.RosterItemUpdated);
				}

				Client.RequestPresenceSubscription(this.JID);
			}
			else if (!Item.HasLastPresence || !Item.LastPresence.IsOnline)
				Client.RequestPresenceSubscription(this.JID);
			else
				this.DoRequest(Item);
		}

		public override void Dispose()
		{
			if (this.registered)
			{
				this.registered = false;
				this.questionView.Owner.UnregisterRosterEventHandler(this.JID, this.RosterItemUpdated);
			}

			base.Dispose();
		}

		private Task RosterItemUpdated(object Sender, RosterItem Item)
		{
			if ((Item.State == SubscriptionState.Both || Item.State == SubscriptionState.To) && Item.HasLastPresence && Item.LastPresence.IsOnline)
			{
				this.questionView.Owner.UnregisterRosterEventHandler(this.JID, this.RosterItemUpdated);
				this.DoRequest(Item);
			}

			return Task.CompletedTask;
		}

		private void DoRequest(RosterItem Item)
		{
			this.parametersSorted = new SortedDictionary<string, bool>();

			if (this.IsNode)
			{
				this.questionView.Owner.ControlClient.GetForm(Item.LastPresenceFullJid, "en", this.ControlFormResponse, null,
					this.GetNodeReference());
			}
			else
				this.questionView.Owner.ControlClient.GetForm(Item.LastPresenceFullJid, "en", this.ControlFormResponse, null);
		}

		private async Task ControlFormResponse(object Sender, DataFormEventArgs e)
		{
			try
			{
				if (e.Ok)
				{
					string[] Parameters;

					lock (this.parametersSorted)
					{
						foreach (Field F in e.Form.Fields)
						{
							if (!F.ReadOnly && !F.Exclude)
								this.parametersSorted[F.Var] = true;
						}

						Parameters = new string[this.parametersSorted.Count];
						this.parametersSorted.Keys.CopyTo(Parameters, 0);
					}

					this.availableParameterNames = Parameters;
					await Database.Update(this);

					MainWindow.UpdateGui(() =>
					{
						SortedDictionary<string, bool> Selected = null;
						bool AllSelected = this.parameterNames is null;

						if (!AllSelected)
						{
							Selected = new SortedDictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);

							foreach (ListBoxItem Item in this.parametersListBox.Items)
							{
								if (Item.IsSelected)
									Selected[(string)Item.Tag] = true;
							}
						}

						this.parametersListBox.Items.Clear();

						foreach (string ParameterName in this.availableParameterNames)
						{
							this.parametersListBox.Items.Add(new ListBoxItem()
							{
								Content = ParameterName,
								IsSelected = AllSelected || Selected.ContainsKey(ParameterName),
								Tag = ParameterName
							});
						}

						return Task.CompletedTask;
					});
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get control form." : e.ErrorText);
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private string[] GetParameters()
		{
			List<string> Result = new List<string>();
			bool All = true;

			foreach (ListBoxItem Item in this.parametersListBox.Items)
			{
				if (Item.IsSelected)
					Result.Add((string)Item.Tag);
				else
					All = false;
			}

			if (this.availableParameterNames is null || !All)
				return Result.ToArray();
			else
				return null;
		}

		private async Task RuleCallback(object Sender, IqResultEventArgs e)
		{
			try
			{
				if (e.Ok)
					await this.Processed(this.questionView);
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to set rule." : e.ErrorText);
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private void NoAllButton_Click(object Sender, RoutedEventArgs e)
		{
			this.range = OperationRange.All;
			this.client.CanControlResponseAll(this.Sender, this.JID, this.RemoteJID, this.Key, false, this.GetParameters(), this.GetNodeReference(), this.RuleCallback, null);
		}

		private void YesAllButton_Click(object Sender, RoutedEventArgs e)
		{
			this.range = OperationRange.All;
			this.client.CanControlResponseAll(this.Sender, this.JID, this.RemoteJID, this.Key, true, this.GetParameters(), this.GetNodeReference(), this.RuleCallback, null);
		}

		private void NoDomainButton_Click(object Sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Domain;
			this.client.CanControlResponseDomain(this.Sender, this.JID, this.RemoteJID, this.Key, false, this.GetParameters(), this.GetNodeReference(), this.RuleCallback, null);
		}

		private void YesDomainButton_Click(object Sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Domain;
			this.client.CanControlResponseDomain(this.Sender, this.JID, this.RemoteJID, this.Key, true, this.GetParameters(), this.GetNodeReference(), this.RuleCallback, null);
		}

		private void NoButton_Click(object Sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Caller;
			this.client.CanControlResponseCaller(this.Sender, this.JID, this.RemoteJID, this.Key, false, this.GetParameters(), this.GetNodeReference(), this.RuleCallback, null);
		}

		private void YesButton_Click(object Sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Caller;
			this.client.CanControlResponseCaller(this.Sender, this.JID, this.RemoteJID, this.Key, true, this.GetParameters(), this.GetNodeReference(), this.RuleCallback, null);
		}

		private void NoTokenButton_Click(object Sender, RoutedEventArgs e)
		{
			this.TokenButtonClick(Sender, e, false);
		}

		private void TokenButtonClick(object Sender, RoutedEventArgs _, bool Result)
		{
			Button Button = (Button)Sender;
			object[] P = (object[])Button.Tag;
			this.parameter = (string)P[0];
			this.range = (OperationRange)P[1];

			switch (this.range)
			{
				case OperationRange.ServiceToken:
					this.client.CanControlResponseService(this.Sender, this.JID, this.RemoteJID, this.Key, Result, this.GetParameters(), this.parameter, this.GetNodeReference(), this.RuleCallback, null);
					break;

				case OperationRange.DeviceToken:
					this.client.CanControlResponseDevice(this.Sender, this.JID, this.RemoteJID, this.Key, Result, this.GetParameters(), this.parameter, this.GetNodeReference(), this.RuleCallback, null);
					break;

				case OperationRange.UserToken:
					this.client.CanControlResponseUser(this.Sender, this.JID, this.RemoteJID, this.Key, Result, this.GetParameters(), this.parameter, this.GetNodeReference(), this.RuleCallback, null);
					break;
			}
		}

		private void YesTokenButton_Click(object Sender, RoutedEventArgs e)
		{
			this.TokenButtonClick(Sender, e, true);
		}

		public override bool IsResolvedBy(Question Question)
		{
			if (Question is CanControlQuestion CanControlQuestion)
			{
				if (this.JID != CanControlQuestion.JID)
					return false;

				switch (this.range)
				{
					case OperationRange.Caller:
						return (this.RemoteJID == CanControlQuestion.RemoteJID);

					case OperationRange.Domain:
						return (IsFriendQuestion.GetDomain(this.RemoteJID) == IsFriendQuestion.GetDomain(CanControlQuestion.RemoteJID));

					case OperationRange.All:
						return true;

					case OperationRange.ServiceToken:
						return CanReadQuestion.MatchesToken(this.parameter, CanControlQuestion.ServiceTokens);

					case OperationRange.DeviceToken:
						return CanReadQuestion.MatchesToken(this.parameter, CanControlQuestion.DeviceTokens);

					case OperationRange.UserToken:
						return CanReadQuestion.MatchesToken(this.parameter, CanControlQuestion.UserTokens);

					default:
						return false;
				}
			}
			else
				return false;
		}

	}
}
