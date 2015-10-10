﻿using Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RazorEnhanced
{
	public class Friend
	{
		[Serializable]
		public class FriendPlayer
		{
			private string m_Name;
			public string Name { get { return m_Name; } }

			private int m_Serial;
			public int Serial { get { return m_Serial; } }

			private bool m_Selected;
			internal bool Selected { get { return m_Selected; } }

			public FriendPlayer(string name, int serial, bool selected)
			{
				m_Name = name;
				m_Serial = serial;
				m_Selected = selected;
			}
		}

		internal class FriendList
		{
			private string m_Description;
			internal string Description { get { return m_Description; } }

			private bool m_AutoacceptParty;
			internal bool AutoacceptParty { get { return m_AutoacceptParty; } }

			private bool m_PreventAttack;
			internal bool PreventAttack { get { return m_PreventAttack; } }

			private bool m_IncludeParty;
			internal bool IncludeParty { get { return m_IncludeParty; } }

			private bool m_Selected;
			internal bool Selected { get { return m_Selected; } }

			public FriendList(string description, bool autoacceptparty, bool preventattack, bool includeparty, bool selected)
			{
				m_Description = description;
				m_AutoacceptParty = autoacceptparty;
				m_PreventAttack = preventattack;
				m_IncludeParty = includeparty;
				m_Selected = selected;
			}
		}

		internal static void AddLog(string addlog)
		{
			if (Engine.Running)
			{
				Engine.MainWindow.FriendLogBox.Invoke(new Action(() => Engine.MainWindow.FriendLogBox.Items.Add(addlog)));
				Engine.MainWindow.FriendLogBox.Invoke(new Action(() => Engine.MainWindow.FriendLogBox.SelectedIndex = Engine.MainWindow.FriendLogBox.Items.Count - 1));
				if (Assistant.Engine.MainWindow.FriendLogBox.Items.Count > 300)
					Assistant.Engine.MainWindow.FriendLogBox.Invoke(new Action(() => Assistant.Engine.MainWindow.FriendLogBox.Items.Clear()));
			}
		}
		internal static bool IncludeParty
		{
			get
			{
				return Assistant.Engine.MainWindow.FriendIncludePartyCheckBox.Checked;
			}
			set
			{
				Assistant.Engine.MainWindow.FriendIncludePartyCheckBox.Invoke(new Action(() => Assistant.Engine.MainWindow.FriendIncludePartyCheckBox.Checked = value));
			}
		}
		internal static bool PreventAttack
		{
			get
			{
				return Assistant.Engine.MainWindow.FriendAttackCheckBox.Checked;
			}
			set
			{
				Assistant.Engine.MainWindow.FriendAttackCheckBox.Invoke(new Action(() => Assistant.Engine.MainWindow.FriendAttackCheckBox.Checked = value));
			}
		}
		internal static bool AutoacceptParty
		{
			get
			{
				return Assistant.Engine.MainWindow.FriendPartyCheckBox.Checked;
			}
			set
			{
				Assistant.Engine.MainWindow.FriendPartyCheckBox.Invoke(new Action(() => Assistant.Engine.MainWindow.FriendPartyCheckBox.Checked = value));
			}
		}

		internal static string FriendListName
		{
			get
			{
				return (string)Assistant.Engine.MainWindow.FriendListSelect.Invoke(new Func<string>(() => Assistant.Engine.MainWindow.FriendListSelect.Text));
			}

			set
			{
				Assistant.Engine.MainWindow.FriendListSelect.Invoke(new Action(() => Assistant.Engine.MainWindow.FriendListSelect.Text = value));
			}
		}

		internal static void RefreshLists()
		{
			List<FriendList> lists;
			RazorEnhanced.Settings.Friend.ListsRead(out lists);

			if (lists.Count == 0)
			{
				Assistant.Engine.MainWindow.FriendListView.Items.Clear();
				Assistant.Engine.MainWindow.FriendAttackCheckBox.Checked = false;
				Assistant.Engine.MainWindow.FriendIncludePartyCheckBox.Checked = false;
				Assistant.Engine.MainWindow.FriendPartyCheckBox.Checked = false;
			}

			FriendList selectedList = lists.Where(l => l.Selected).FirstOrDefault();
			if (selectedList != null && selectedList.Description == Assistant.Engine.MainWindow.FriendListSelect.Text)
				return;

			Assistant.Engine.MainWindow.FriendListSelect.Items.Clear();
			foreach (FriendList l in lists)
			{
				Assistant.Engine.MainWindow.FriendListSelect.Items.Add(l.Description);

				if (l.Selected)
				{
					Assistant.Engine.MainWindow.FriendListSelect.SelectedIndex = Assistant.Engine.MainWindow.FriendListSelect.Items.IndexOf(l.Description);
					IncludeParty = l.IncludeParty;
					PreventAttack = l.PreventAttack;
					AutoacceptParty = l.AutoacceptParty;
				}
			}
		}

		internal static void RefreshPlayers()
		{
			List<FriendList> lists;
			RazorEnhanced.Settings.Friend.ListsRead(out lists);

			Assistant.Engine.MainWindow.FriendListView.Items.Clear();
			foreach (FriendList l in lists)
			{
				if (l.Selected)
				{
					List<Friend.FriendPlayer> players;
					RazorEnhanced.Settings.Friend.PlayersRead(l.Description, out players);

					foreach (FriendPlayer player in players)
					{
						ListViewItem listitem = new ListViewItem();

						listitem.Checked = player.Selected;

						listitem.SubItems.Add(player.Name);
						listitem.SubItems.Add("0x" + player.Serial.ToString("X8"));

						Assistant.Engine.MainWindow.FriendListView.Items.Add(listitem);
					}
				}
			}
		}

		internal static void AddList(string newList)
		{
			RazorEnhanced.Settings.Friend.ListInsert(newList, false, false, false);

			RazorEnhanced.Friend.RefreshLists();
			RazorEnhanced.Friend.RefreshPlayers();
		}

		internal static void RemoveList(string list)
		{
			if (RazorEnhanced.Settings.Friend.ListExists(list))
			{
				RazorEnhanced.Settings.Friend.ListDelete(list);
			}

			RazorEnhanced.Friend.RefreshLists();
			RazorEnhanced.Friend.RefreshPlayers();
		}
		internal static void AddPlayerToList(string name, int serial)
		{
			string selection = Assistant.Engine.MainWindow.FriendListSelect.Text;
			FriendPlayer player = new FriendPlayer(name, serial, true);

			if (RazorEnhanced.Settings.Friend.ListExists(selection))
			{
				if (!RazorEnhanced.Settings.Friend.PlayerExists(selection, player))
					RazorEnhanced.Settings.Friend.PlayerInsert(selection, player);
			}
			RazorEnhanced.Friend.RefreshPlayers();
		}
		internal static void UpdateSelectedPlayer(int i)
		{
			List<FriendPlayer> players;
			RazorEnhanced.Settings.Friend.PlayersRead(FriendListName, out players);

			if (players.Count != Assistant.Engine.MainWindow.FriendListView.Items.Count)
			{
				return;
			}

			ListViewItem lvi = Assistant.Engine.MainWindow.FriendListView.Items[i];
			FriendPlayer old = players[i];

			if (lvi != null && old != null)
			{
				FriendPlayer player = new Friend.FriendPlayer(old.Name, old.Serial, lvi.Checked);
				RazorEnhanced.Settings.Friend.PlayerReplace(RazorEnhanced.Friend.FriendListName, i, player);
			}
		}

		public static bool IsFriend(int serial)
		{
			List<Friend.FriendPlayer> players;
			RazorEnhanced.Settings.Friend.PlayersRead(Friend.FriendListName, out players);
			foreach (FriendPlayer player in players)        // Ricerca nella friend list normale
			{
				if (player.Selected)
					if (player.Serial == serial)
						return true;
			}

			if (Friend.IncludeParty && PacketHandlers.Party.Contains(serial))            // Ricerco nel party se attiva l'opzione
			{
				return true;
			}

			return false;
		}

		public static void ChangeList(string nomelista)
		{
			bool ListaOK = false;
			for (int i = 0; i < Assistant.Engine.MainWindow.FriendListSelect.Items.Count; i++)
			{
				if (nomelista == Assistant.Engine.MainWindow.FriendListSelect.GetItemText(Assistant.Engine.MainWindow.FriendListSelect.Items[i]))
					ListaOK = true;
			}
			if (!ListaOK)
				Misc.SendMessage("Script Error: Friend.ChangeList: Friend List: " + nomelista + " not exist");
			else
			{
				Assistant.Engine.MainWindow.FriendListSelect.Invoke(new Action(() => Assistant.Engine.MainWindow.FriendListSelect.SelectedIndex = Assistant.Engine.MainWindow.FriendListSelect.Items.IndexOf(nomelista)));  // cambio lista
			}
		}
	}
}