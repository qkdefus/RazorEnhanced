using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Assistant
{
	internal enum LockType : byte
	{
		Up = 0,
		Down = 1,
		Locked = 2,
	}

	internal enum MsgLevel
	{
		Debug = 0,
		Info = 0,
		Warning = 1,
		Error = 2,
		Force = 3,
	}

	internal class Skill
	{
		internal static int Count = 55;

		private LockType m_Lock;
		private ushort m_Value;
		private ushort m_Base;
		private ushort m_Cap;
		private short m_Delta;
		private int m_Idx;

		internal Skill(int idx)
		{
			m_Idx = idx;
		}

		internal int Index { get { return m_Idx; } }

		internal LockType Lock
		{
			get { return m_Lock; }
			set { m_Lock = value; }
		}

		internal ushort FixedValue
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		internal ushort FixedBase
		{
			get { return m_Base; }
			set
			{
				m_Delta += (short)(value - m_Base);
				m_Base = value;
			}
		}

		internal ushort FixedCap
		{
			get { return m_Cap; }
			set { m_Cap = value; }
		}

		internal double Value
		{
			get { return m_Value / 10.0; }
			set { m_Value = (ushort)(value * 10.0); }
		}

		internal double Base
		{
			get { return m_Base / 10.0; }
			set { m_Base = (ushort)(value * 10.0); }
		}

		internal double Cap
		{
			get { return m_Cap / 10.0; }
			set { m_Cap = (ushort)(value * 10.0); }
		}

		internal double Delta
		{
			get { return m_Delta / 10.0; }
			set { m_Delta = (short)(value * 10); }
		}
	}

	internal enum SkillName
	{
		Alchemy = 0,
		Anatomy = 1,
		AnimalLore = 2,
		ItemID = 3,
		ArmsLore = 4,
		Parry = 5,
		Begging = 6,
		Blacksmith = 7,
		Fletching = 8,
		Peacemaking = 9,
		Camping = 10,
		Carpentry = 11,
		Cartography = 12,
		Cooking = 13,
		DetectHidden = 14,
		Discordance = 15,
		EvalInt = 16,
		Healing = 17,
		Fishing = 18,
		Forensics = 19,
		Herding = 20,
		Hiding = 21,
		Provocation = 22,
		Inscribe = 23,
		Lockpicking = 24,
		Magery = 25,
		MagicResist = 26,
		Tactics = 27,
		Snooping = 28,
		Musicianship = 29,
		Poisoning = 30,
		Archery = 31,
		SpiritSpeak = 32,
		Stealing = 33,
		Tailoring = 34,
		AnimalTaming = 35,
		TasteID = 36,
		Tinkering = 37,
		Tracking = 38,
		Veterinary = 39,
		Swords = 40,
		Macing = 41,
		Fencing = 42,
		Wrestling = 43,
		Lumberjacking = 44,
		Mining = 45,
		Meditation = 46,
		Stealth = 47,
		RemoveTrap = 48,
		Necromancy = 49,
		Focus = 50,
		Chivalry = 51,
		Bushido = 52,
		Ninjitsu = 53,
		SpellWeaving = 54,
	}

	internal class PlayerData : Mobile
	{
		internal class MoveEntry
		{
			//public byte Seq;
			internal Direction Dir;
			//public int x;
			//public int y;
			internal Point3D Position;
			internal bool IsStep;

			internal bool FilterAck;
		}

		internal int VisRange = 18;
		internal int MultiVisRange { get { return VisRange + 5; } }

		private int m_MaxWeight = -1;

		private short m_FireResist, m_ColdResist, m_PoisonResist, m_EnergyResist, m_Luck;
		private ushort m_DamageMin, m_DamageMax;

		private ushort m_Str, m_Dex, m_Int;
		private LockType m_StrLock, m_DexLock, m_IntLock;
		private uint m_Gold;
		private ushort m_Weight;
		private Skill[] m_Skills;
		private ushort m_AR;
		private ushort m_StatCap;
		private byte m_Followers;
		private byte m_FollowersMax;
		private int m_Tithe;
		private sbyte m_LocalLight;
		private byte m_GlobalLight;
		private ushort m_Features;
		private byte m_Season;
		private int[] m_MapPatches = new int[10];

		private bool m_SkillsSent;
		//private Item m_Holding;
		//private ushort m_HoldAmt;
		private ConcurrentDictionary<byte, MoveEntry> m_MoveInfo;
		private Timer m_CriminalTime;
		private DateTime m_CriminalStart = DateTime.MinValue;
		private byte m_WalkSeq;

		internal static int FastWalkKey = 0;

		internal List<BuffIcon> m_Buffs = new List<BuffIcon>();
		internal List<BuffIcon> Buffs { get { return m_Buffs; } }

		internal override void SaveState(BinaryWriter writer)
		{
			base.SaveState(writer);

			writer.Write(m_Str);
			writer.Write(m_Dex);
			writer.Write(m_Int);
			writer.Write(m_StamMax);
			writer.Write(m_Stam);
			writer.Write(m_ManaMax);
			writer.Write(m_Mana);
			writer.Write((byte)m_StrLock);
			writer.Write((byte)m_DexLock);
			writer.Write((byte)m_IntLock);
			writer.Write(m_Gold);
			writer.Write(m_Weight);

			writer.Write((byte)Skill.Count);
			for (int i = 0; i < Skill.Count; i++)
			{
				writer.Write(m_Skills[i].FixedBase);
				writer.Write(m_Skills[i].FixedCap);
				writer.Write(m_Skills[i].FixedValue);
				writer.Write((byte)m_Skills[i].Lock);
			}

			writer.Write(m_AR);
			writer.Write(m_StatCap);
			writer.Write(m_Followers);
			writer.Write(m_FollowersMax);
			writer.Write(m_Tithe);

			writer.Write(m_LocalLight);
			writer.Write(m_GlobalLight);
			writer.Write(m_Features);
			writer.Write(m_Season);

			writer.Write((byte)m_MapPatches.Length);
			for (int i = 0; i < m_MapPatches.Length; i++)
				writer.Write((int)m_MapPatches[i]);
		}

		internal PlayerData(BinaryReader reader, int version)
			: base(reader, version)
		{
			int c;
			m_Str = reader.ReadUInt16();
			m_Dex = reader.ReadUInt16();
			m_Int = reader.ReadUInt16();
			m_StamMax = reader.ReadUInt16();
			m_Stam = reader.ReadUInt16();
			m_ManaMax = reader.ReadUInt16();
			m_Mana = reader.ReadUInt16();
			m_StrLock = (LockType)reader.ReadByte();
			m_DexLock = (LockType)reader.ReadByte();
			m_IntLock = (LockType)reader.ReadByte();
			m_Gold = reader.ReadUInt32();
			m_Weight = reader.ReadUInt16();

			m_MoveInfo = new ConcurrentDictionary<byte, MoveEntry>();

			if (version >= 4)
			{
				Skill.Count = c = reader.ReadByte();
			}
			else if (version == 3)
			{
				long skillStart = reader.BaseStream.Position;
				c = 0;
				reader.BaseStream.Seek(7 * 49, SeekOrigin.Current);
				for (int i = 48; i < 60; i++)
				{
					ushort Base, Cap, Val;
					byte Lock;

					Base = reader.ReadUInt16();
					Cap = reader.ReadUInt16();
					Val = reader.ReadUInt16();
					Lock = reader.ReadByte();

					if (Base > 2000 || Cap > 2000 || Val > 2000 || Lock > 2)
					{
						c = i;
						break;
					}
				}

				if (c == 0)
					c = 52;
				else if (c > 54)
					c = 54;

				Skill.Count = c;

				reader.BaseStream.Seek(skillStart, SeekOrigin.Begin);
			}
			else
			{
				Skill.Count = c = 52;
			}

			m_Skills = new Skill[c];
			for (int i = 0; i < c; i++)
			{
				m_Skills[i] = new Skill(i);
				m_Skills[i].FixedBase = reader.ReadUInt16();
				m_Skills[i].FixedCap = reader.ReadUInt16();
				m_Skills[i].FixedValue = reader.ReadUInt16();
				m_Skills[i].Lock = (LockType)reader.ReadByte();
			}

			m_AR = reader.ReadUInt16();
			m_StatCap = reader.ReadUInt16();
			m_Followers = reader.ReadByte();
			m_FollowersMax = reader.ReadByte();
			m_Tithe = reader.ReadInt32();

			m_LocalLight = reader.ReadSByte();
			m_GlobalLight = reader.ReadByte();
			m_Features = reader.ReadUInt16();
			m_Season = reader.ReadByte();

			if (version >= 4)
				c = reader.ReadByte();
			else
				c = 8;
			m_MapPatches = new int[c];
			for (int i = 0; i < c; i++)
				m_MapPatches[i] = reader.ReadInt32();
		}

		internal PlayerData(Serial serial)
			: base(serial)
		{
			m_MoveInfo = new ConcurrentDictionary<byte, MoveEntry>();
			m_Skills = new Skill[Skill.Count];
			for (int i = 0; i < m_Skills.Length; i++)
				m_Skills[i] = new Skill(i);
		}

		internal ushort Str
		{
			get { return m_Str; }
			set { m_Str = value; }
		}

		internal ushort Dex
		{
			get { return m_Dex; }
			set { m_Dex = value; }
		}

		internal ushort Int
		{
			get { return m_Int; }
			set { m_Int = value; }
		}

		internal uint Gold
		{
			get { return m_Gold; }
			set { m_Gold = value; }
		}

		internal ushort Weight
		{
			get { return m_Weight; }
			set { m_Weight = value; }
		}

		internal ushort MaxWeight
		{
			get
			{
				if (m_MaxWeight == -1)
					return (ushort)((m_Str * 3.5) + 40);
				else
					return (ushort)m_MaxWeight;
			}
			set
			{
				m_MaxWeight = value;
			}
		}

		internal short FireResistance
		{
			get { return m_FireResist; }
			set { m_FireResist = value; }
		}

		internal short ColdResistance
		{
			get { return m_ColdResist; }
			set { m_ColdResist = value; }
		}

		internal short PoisonResistance
		{
			get { return m_PoisonResist; }
			set { m_PoisonResist = value; }
		}

		internal short EnergyResistance
		{
			get { return m_EnergyResist; }
			set { m_EnergyResist = value; }
		}

		internal short Luck
		{
			get { return m_Luck; }
			set { m_Luck = value; }
		}

		internal ushort DamageMin
		{
			get { return m_DamageMin; }
			set { m_DamageMin = value; }
		}

		internal ushort DamageMax
		{
			get { return m_DamageMax; }
			set { m_DamageMax = value; }
		}

		internal LockType StrLock
		{
			get { return m_StrLock; }
			set { m_StrLock = value; }
		}

		internal LockType DexLock
		{
			get { return m_DexLock; }
			set { m_DexLock = value; }
		}

		internal LockType IntLock
		{
			get { return m_IntLock; }
			set { m_IntLock = value; }
		}

		internal ushort StatCap
		{
			get { return m_StatCap; }
			set { m_StatCap = value; }
		}

		internal ushort AR
		{
			get { return m_AR; }
			set { m_AR = value; }
		}

		internal byte Followers
		{
			get { return m_Followers; }
			set { m_Followers = value; }
		}

		internal byte FollowersMax
		{
			get { return m_FollowersMax; }
			set { m_FollowersMax = value; }
		}

		internal int Tithe
		{
			get { return m_Tithe; }
			set { m_Tithe = value; }
		}

		internal Skill[] Skills { get { return m_Skills; } }

		internal bool SkillsSent
		{
			get { return m_SkillsSent; }
			set { m_SkillsSent = value; }
		}

		internal byte WalkSequence { get { return m_WalkSeq; } }

		internal int CriminalTime
		{
			get
			{
				if (m_CriminalStart != DateTime.MinValue)
				{
					int sec = (int)(DateTime.Now - m_CriminalStart).TotalSeconds;
					if (sec > 300)
					{
						if (m_CriminalTime != null)
							m_CriminalTime.Stop();
						m_CriminalStart = DateTime.MinValue;
						return 0;
					}
					else
					{
						return sec;
					}
				}
				else
				{
					return 0;
				}
			}
		}

		internal void Resync()
		{
			m_OutstandingMoves = m_WalkSeq = 0;
			m_MoveInfo.Clear();
		}

		private int m_OutstandingMoves = 0;

		internal int OutstandingMoveReqs { get { return m_OutstandingMoves; } }

		internal MoveEntry GetMoveEntry(byte seq)
		{
			return m_MoveInfo[seq];
		}

		private static Timer m_OpenDoorReq = Timer.DelayedCallback(TimeSpan.FromSeconds(0.005), new TimerCallback(OpenDoor));
		private static void OpenDoor()
		{
			if (World.Player != null)
				ClientCommunication.SendToServer(new OpenDoorMacro());
		}

		private Serial m_LastDoor = Serial.Zero;
		private DateTime m_LastDoorTime = DateTime.MinValue;

		internal void MoveReq(Direction dir, byte seq)
		{
			m_OutstandingMoves++;
			FastWalkKey++;

			MoveEntry e = new MoveEntry();

			if (!m_MoveInfo.ContainsKey(seq))
				m_MoveInfo.TryAdd(seq, e);
			else
				m_MoveInfo[seq] = e;

			e.IsStep = (dir & Direction.Mask) == (Direction & Direction.Mask);
			e.Dir = dir;

			ProcessMove(dir); // shouldnt this be in MoveAck?!?

			e.Position = Position;

			if (Body != 0x03DB && !IsGhost && ((int)(e.Dir & Direction.Mask)) % 2 == 0 && RazorEnhanced.Settings.General.ReadBool("AutoOpenDoors"))
			{
				int x = Position.X, y = Position.Y;
				Utility.Offset(e.Dir, ref x, ref y);

				int z = CalcZ;

				foreach (Item i in World.Items.Values)
				{
					if (i.Position.X == x && i.Position.Y == y && i.IsDoor && i.Position.Z - 15 <= z && i.Position.Z + 15 >= z && (m_LastDoor != i.Serial || m_LastDoorTime + TimeSpan.FromSeconds(1) < DateTime.Now))
					{
						m_LastDoor = i.Serial;
						m_LastDoorTime = DateTime.Now;
						m_OpenDoorReq.Start();
						break;
					}
				}
			}

			e.FilterAck = false;

			m_WalkSeq = (byte)(seq >= 255 ? 1 : seq + 1);
		}

		internal void ProcessMove(Direction dir)
		{
			if ((dir & Direction.Mask) == (this.Direction & Direction.Mask))
			{
				int x = Position.X, y = Position.Y;

				Utility.Offset(dir & Direction.Mask, ref x, ref y);

				int newZ = Position.Z;
				try { newZ = Assistant.Map.ZTop(Map, x, y, newZ); }
				catch { }
				Position = new Point3D(x, y, newZ);
			}
			Direction = dir;
		}

		internal bool HasWalkEntry(byte seq)
		{
			return m_MoveInfo[seq] != null;
		}

		internal void MoveRej(byte seq, Direction dir, Point3D pos)
		{
			m_OutstandingMoves--;

			Direction = dir;
			Position = pos;
			Resync();
		}

		internal bool MoveAck(byte seq)
		{
			m_OutstandingMoves--;

			MoveEntry e;
			m_MoveInfo.TryGetValue(seq, out e);
			if (e != null)
			{
				if (e.IsStep && !IsGhost)
					StealthSteps.OnMove();

				return !e.FilterAck;
			}
			else
			{
				return true;
			}
		}

		private static bool m_ExternZ = false;
		internal static bool ExternalZ { get { return m_ExternZ; } set { m_ExternZ = value; } }

		//private sbyte m_CalcZ = 0;
		internal int CalcZ
		{
			get
			{
				if (!m_ExternZ || !ClientCommunication.IsCalibrated())
					return Assistant.Map.ZTop(Map, Position.X, Position.Y, Position.Z);
				else
					return Position.Z;
			}
		}

		internal override Point3D Position
		{
			get
			{
				if (m_ExternZ && ClientCommunication.IsCalibrated())
				{
					Point3D p = new Point3D(base.Position);
					p.Z = ClientCommunication.GetZ(p.X, p.Y, p.Z);
					return p;
				}
				else
				{
					return base.Position;
				}
			}
			set
			{
				base.Position = value;

				if (Engine.MainWindow != null && Engine.MainWindow.MapWindow != null)
					Engine.MainWindow.MapWindow.PlayerMoved();
			}
		}

		internal override void OnPositionChanging(Point3D newPos)
		{
			List<Mobile> mobiles = new List<Mobile>(World.Mobiles.Values);

			foreach (Mobile m in mobiles)
			{
				if (m != this)
				{
					if (!Utility.InRange(m.Position, newPos, VisRange))
						m.Remove();
					else
						Targeting.CheckLastTargetRange(m);
				}
			}


			List<Item> items = new List<Item>(World.Items.Values);
			foreach (Item item in items)
			{
				if (item.Deleted || item.Container != null)
					continue;

				int dist = Utility.Distance(item.GetWorldPosition(), newPos);
				if (item != DragDropManager.Holding && (dist > MultiVisRange || (!item.IsMulti && dist > VisRange)))
					item.Remove();
			}

			base.OnPositionChanging(newPos);
		}

		internal override void OnMapChange(byte old, byte cur)
		{
			List<Mobile> list = new List<Mobile>(World.Mobiles.Values);
			for (int i = 0; i < list.Count; i++)
			{
				Mobile m = (Mobile)list[i];
				if (m != this && m.Map != cur)
					m.Remove();
			}

			World.Items.Clear();
			//Counter.Reset();
			for (int i = 0; i < Contains.Count; i++)
			{
				Item item = (Item)Contains[i];
				World.AddItem(item);
				item.Contains.Clear();
			}

			if (RazorEnhanced.Settings.General.ReadBool("AutoSearch") && Backpack != null)
				PlayerData.DoubleClick(Backpack);

			ClientCommunication.PostMapChange(cur);

			if (Engine.MainWindow != null && Engine.MainWindow.MapWindow != null)
				Engine.MainWindow.MapWindow.PlayerMoved();
		}

		/*public override void OnMapChange( byte old, byte cur )
		{
			World.Mobiles.Clear();
			World.Items.Clear();
			Counter.Reset();

			Contains.Clear();
			
			World.AddMobile( this );

			ClientCommunication.PostMapChange( cur );
		}*/

		protected override void OnNotoChange(byte old, byte cur)
		{
			if ((old == 3 || old == 4) && (cur != 3 && cur != 4))
			{
				// grey is turning off
				// SendMessage( "You are no longer a criminal." );
				if (m_CriminalTime != null)
					m_CriminalTime.Stop();
				m_CriminalStart = DateTime.MinValue;
			}
			else if ((cur == 3 || cur == 4) && (old != 3 && old != 4 && old != 0))
			{
				// grey is turning on
				ResetCriminalTimer();
			}
		}

		internal void ResetCriminalTimer()
		{
			if (m_CriminalStart == DateTime.MinValue || DateTime.Now - m_CriminalStart >= TimeSpan.FromSeconds(1))
			{
				m_CriminalStart = DateTime.Now;
				if (m_CriminalTime == null)
					m_CriminalTime = new CriminalTimer(this);
				m_CriminalTime.Start();
			}
		}

		private class CriminalTimer : Timer
		{
			private PlayerData m_Player;
			internal CriminalTimer(PlayerData player)
				: base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
			{
				m_Player = player;
			}

			protected override void OnTick()
			{
			}
		}

		internal void SendMessage(MsgLevel lvl, LocString loc, params object[] args)
		{
			SendMessage(lvl, Language.Format(loc, args));
		}

		internal void SendMessage(MsgLevel lvl, LocString loc)
		{
			SendMessage(lvl, Language.GetString(loc));
		}

		internal void SendMessage(LocString loc, params object[] args)
		{
			SendMessage(MsgLevel.Info, Language.Format(loc, args));
		}

		internal void SendMessage(LocString loc)
		{
			SendMessage(MsgLevel.Info, Language.GetString(loc));
		}

		/*internal void SendMessage( int hue, LocString loc, params object[] args )
		{
			SendMessage( hue, Language.Format( loc, args ) );
		}*/

		internal void SendMessage(MsgLevel lvl, string format, params object[] args)
		{
			SendMessage(lvl, String.Format(format, args));
		}

		internal void SendMessage(string format, params object[] args)
		{
			SendMessage(MsgLevel.Info, String.Format(format, args));
		}

		internal void SendMessage(string text)
		{
			SendMessage(MsgLevel.Info, text);
		}

		internal void SendMessage(MsgLevel lvl, string text)
		{
			if (lvl >= (MsgLevel)RazorEnhanced.Settings.General.ReadInt("MessageLevel") && text.Length > 0)
			{
				int hue;
				switch (lvl)
				{
					case MsgLevel.Error:
					case MsgLevel.Warning:
						hue = RazorEnhanced.Settings.General.ReadInt("WarningColor");
						break;

					default:
						hue = RazorEnhanced.Settings.General.ReadInt("SysColor");
						break;
				}

				ClientCommunication.SendToClient(new UnicodeMessage(0xFFFFFFFF, -1, MessageType.Regular, hue, 3, Language.CliLocName, "System", text));

				PacketHandlers.SysMessages.Add(text.ToLower());

				if (PacketHandlers.SysMessages.Count >= 25)
					PacketHandlers.SysMessages.RemoveRange(0, 10);
			}
		}

		internal uint CurrentGumpS, CurrentGumpI;
		internal bool HasGump;
		internal List<string> CurrentGumpStrings = new List<string>();
		internal Queue<RazorEnhanced.Journal.JournalEntry> Journal = new Queue<RazorEnhanced.Journal.JournalEntry>();
		internal uint CurrentMenuS;
		internal ushort CurrentMenuI;
		internal bool HasMenu;

		private ushort m_SpeechHue;
		internal ushort SpeechHue { get { return m_SpeechHue; } set { m_SpeechHue = value; } }

		internal sbyte LocalLightLevel { get { return m_LocalLight; } set { m_LocalLight = value; } }
		internal byte GlobalLightLevel { get { return m_GlobalLight; } set { m_GlobalLight = value; } }
		internal byte Season { get { return m_Season; } set { m_Season = value; } }
		internal ushort Features { get { return m_Features; } set { m_Features = value; } }
		internal int[] MapPatches { get { return m_MapPatches; } set { m_MapPatches = value; } }

		private int m_LastSkill = -1;
		internal int LastSkill { get { return m_LastSkill; } set { m_LastSkill = value; } }

		private Serial m_LastObj = Serial.Zero;
		internal Serial LastObject { get { return m_LastObj; } }

		private int m_LastSpell = -1;
		internal int LastSpell { get { return m_LastSpell; } set { m_LastSpell = value; } }

		//private UOEntity m_LastCtxM = null;
		//public UOEntity LastContextMenu { get { return m_LastCtxM; } set { m_LastCtxM = value; } }

		internal static bool DoubleClick(object clicked)
		{
			return DoubleClick(clicked, true);
		}

		internal static bool DoubleClick(object clicked, bool silent)
		{
			Serial s;
			if (clicked is Mobile)
				s = ((Mobile)clicked).Serial.Value;
			else if (clicked is Item)
				s = ((Item)clicked).Serial.Value;
			else if (clicked is Serial)
				s = ((Serial)clicked).Value;
			else
				s = Serial.Zero;

			if (s != Serial.Zero)
			{
				Item free = null, pack = World.Player.Backpack;
				if (s.IsItem && pack != null && RazorEnhanced.Settings.General.ReadBool("PotionEquip"))
				{
					Item i = World.FindItem(s);
					if (i != null && i.IsPotion && i.ItemID != 3853) // dont unequip for exploison potions
					{
						// dont worry about uneqipping RuneBooks or SpellBooks
						Item left = World.Player.GetItemOnLayer(Layer.LeftHand);
						Item right = World.Player.GetItemOnLayer(Layer.RightHand);

						if (left != null && (right != null || left.IsTwoHanded))
							free = left;
						else if (right != null && right.IsTwoHanded)
							free = right;

						if (free != null)
						{
							if (DragDropManager.HasDragFor(free.Serial))
								free = null;
							else
								DragDropManager.DragDrop(free, pack);
						}
					}
				}

				ActionQueue.DoubleClick(silent, s);

				if (free != null)
					DragDropManager.DragDrop(free, World.Player, free.Layer, true);

				if (s.IsItem)
					World.Player.m_LastObj = s;
			}

			return false;
		}
	}
}
