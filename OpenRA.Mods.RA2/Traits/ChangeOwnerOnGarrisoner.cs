﻿#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.RA2.Traits
{
	public class ChangeOwnerOnGarrisonerInfo : ChangeOwnerInfo, ITraitInfo, Requires<GarrisonableInfo>
	{
		[Desc("Speech notification played when the first actor enters this garrison.")]
		public readonly string EnterNotification = null;

		[Desc("Speech notification played when the last actor leaves this garrison.")]
		public readonly string ExitNotification = null;

		[Desc("Sound played when the first actor enters this garrison.")]
		public readonly string EnterSound = null;

		[Desc("Sound played when the last actor exits this garrison.")]
		public readonly string ExitSound = null;

		[Desc("Does the sound play under shroud or fog.")]
		public readonly bool AudibleThroughFog = false;

		[Desc("Volume the EnterSound and ExitSound played at.")]
		public readonly float SoundVolume = 1;

		public override object Create(ActorInitializer init) { return new ChangeOwnerOnGarrisoner(init.Self, this); }
	}

	public class ChangeOwnerOnGarrisoner : ChangeOwner, INotifyGarrisonerEntered, INotifyGarrisonerExited
	{
		readonly ChangeOwnerOnGarrisonerInfo info;
		readonly Garrisonable garrison;
		private readonly Player originalOwner;

		public ChangeOwnerOnGarrisoner(Actor self, ChangeOwnerOnGarrisonerInfo info)
		{
			this.info = info;
			garrison = self.Trait<Garrisonable>();
			originalOwner = self.Owner;
		}

		void INotifyGarrisonerEntered.OnGarrisonerEntered(Actor self, Actor garrisoner)
		{
			var newOwner = garrisoner.Owner;
			if (self.Owner != originalOwner || self.Owner == newOwner || self.Owner.IsAlliedWith(garrisoner.Owner))
				return;

			NeedChangeOwner(self, garrisoner, newOwner);

			if (info.AudibleThroughFog || !self.World.FogObscures(self.CenterPosition))
				Game.Sound.Play(SoundType.World, info.EnterSound, self.CenterPosition, info.SoundVolume);

			Game.Sound.PlayNotification(self.World.Map.Rules, garrisoner.Owner, "Speech", info.EnterNotification, newOwner.Faction.InternalName);
		}

		void INotifyGarrisonerExited.OnGarrisonerExited(Actor self, Actor garrisoner)
		{
			if (garrison.GarrisonerCount > 0)
				return;

			if (info.AudibleThroughFog || !self.World.FogObscures(self.CenterPosition))
				Game.Sound.Play(SoundType.World, info.ExitSound, self.CenterPosition, info.SoundVolume);

			Game.Sound.PlayNotification(self.World.Map.Rules, garrisoner.Owner, "Speech", info.ExitNotification, garrisoner.Owner.Faction.InternalName);
			NeedChangeOwner(self, garrisoner, originalOwner);
		}
	}
}
