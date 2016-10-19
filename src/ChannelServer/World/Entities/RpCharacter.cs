﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Role-playing character, controlled by players.
	/// </summary>
	public class RpCharacter : PlayerCreature
	{
		/// <summary>
		/// Always returns false (RP characters don't save.)
		/// </summary>
		/// <remarks>
		/// TODO: Replace property with a list in channel client?
		/// </remarks>
		public override bool Save { get { return false; } }

		/// <summary>
		/// Reference to the creature playing this role.
		/// </summary>
		public Creature Actor { get; private set; }

		/// <summary>
		/// Creates new RP character, based on actor data.
		/// </summary>
		/// <param name="actorData">The data to base this character on.</param>
		/// <param name="name">The RP character's full name.</param>
		/// <param name="actor">
		/// Character of the player who gets control over this RP character.
		/// </param>
		/// <example>
		/// var character = new RpCharacter(AuraData.ActorDb.Find("#tarlach"), "Tarlach (FooBar)", fooBar);
		/// </example>
		public RpCharacter(ActorData actorData, string name, Creature actor)
		{
			if (actorData == null) throw new ArgumentNullException("actorData");
			if (actor == null) throw new ArgumentNullException("actor");

			if (actor.Client is DummyClient)
				throw new InvalidOperationException("Actor must be a player with a valid client.");

			var rnd = RandomProvider.Get();

			this.Actor = actor;
			this.EntityId = NPC.GetNewNpcEntityId();
			this.RaceId = actorData.RaceId;
			this.LoadDefault();

			// Name
			if (!string.IsNullOrWhiteSpace(name))
				this.Name = name;
			else
				this.Name = this.RaceData.Name;

			// State
			this.State |= CreatureStates.InstantNpc;
			this.State |= CreatureStates.EnableCommonPvp;

			// Color
			if (actorData.HasColors)
			{
				this.Color1 = actorData.Color1;
				this.Color2 = actorData.Color2;
				this.Color3 = actorData.Color3;
			}

			// Body
			this.Height = actorData.Height;
			this.Weight = actorData.Weight;
			this.Upper = actorData.Upper;
			this.Lower = actorData.Lower;
			this.EyeColor = (byte)actorData.EyeColor;
			this.EyeType = (short)actorData.EyeType;
			this.MouthType = (byte)actorData.MouthType;
			this.SkinColor = (byte)actorData.SkinColor;

			// Titles
			this.Titles.SelectedTitle = (ushort)actorData.Title;

			// Stats
			this.Age = (short)actorData.Age;
			this.Level = (short)actorData.Level;
			this.AbilityPoints = (short)actorData.AP;
			this.LifeMaxBase = actorData.Life;
			this.ManaMaxBase = actorData.Mana;
			this.StaminaMaxBase = actorData.Stamina;
			this.StrBase = actorData.Str;
			this.IntBase = actorData.Int;
			this.DexBase = actorData.Dex;
			this.WillBase = actorData.Will;
			this.LuckBase = actorData.Luck;

			// Hair and Face
			if (actorData.FaceItemId != 0)
			{
				var face = new Item(actorData.FaceItemId);
				face.Info.Color1 = (byte)actorData.SkinColor;
				this.Inventory.Add(face, Pocket.Face);
			}

			if (actorData.HairItemId != 0)
			{
				var hair = new Item(actorData.HairItemId);
				hair.Info.Color1 = actorData.HairColor;
				this.Inventory.Add(hair, Pocket.Hair);
			}

			// Items
			foreach (var itemData in actorData.Items)
			{
				var item = new Item(itemData.ItemId);
				item.Info.State = (byte)itemData.State;

				item.Info.Amount = (ushort)itemData.Amount;
				if (item.Data.StackType != StackType.Sac && item.Info.Amount < 1)
					item.Info.Amount = 1;

				if (itemData.HasColors)
				{
					item.Info.Color1 = itemData.Color1;
					item.Info.Color2 = itemData.Color2;
					item.Info.Color3 = itemData.Color3;
				}

				var pocket = (Pocket)itemData.Pocket;
				if (pocket != Pocket.None)
					this.Inventory.Add(item, pocket);
			}

			// Skills
			this.Skills.Add(SkillId.CombatMastery, SkillRank.Novice, this.RaceId);

			foreach (var skillData in actorData.Skills)
				this.Skills.Add(skillData.SkillId, skillData.Rank, this.RaceId);

			// Max stats out after skills and items were added (bonuses).
			this.Life = this.LifeMax;
			this.Mana = this.ManaMax;
			this.Stamina = this.StaminaMax;
		}
	}
}
