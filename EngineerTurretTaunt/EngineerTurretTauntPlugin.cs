﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using MonoMod.RuntimeDetour;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using UnityEngine;
using MonoMod.Utils;
using BepInEx.Configuration;
using EmotesAPI;
using System.Collections.ObjectModel;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;

namespace EngineerTurretTaunt
{
	[BepInDependency("com.bepis.r2api")]
	[BepInDependency("com.weliveinasociety.CustomEmotesAPI")]
	[BepInPlugin("com.kanggamming.EngineerTurretTaunt", "Engineer Turret Taunt", "0.2.1")]
	[R2APISubmoduleDependency("NetworkingAPI")]
    public class EngineerTurretTauntPlugin : BaseUnityPlugin
    {
		public EngineerTurretTauntPlugin()
			: base()
		{
			allAlliesEmote = Config.Bind("General", "All allies emote", false, "Whether all of your allies will taunt with you. (Does not make other players or their allies taunt)");
			stopEmotingSolo = Config.Bind("Keybinds", "Stop emoting (yourself)", KeyboardShortcut.Empty, "Keybind to make only yourself stop emoting.");
			joinEmoteSolo = Config.Bind("Keybinds", "Join emote (yourself)", KeyboardShortcut.Empty, "Keybind to join a Join Spot without making your allies join with you. (alternatively you can just press Sync With Nearest Emote in the original mod)");
			stopEmotingAllies = Config.Bind("Keybinds", "Stop emoting (allies only)", KeyboardShortcut.Empty, "Keybind to make only your allies stop emoting.");
			joinEmoteAllies = Config.Bind("Keybinds", "Join emote (allies only)", KeyboardShortcut.Empty, "Keybind to make only your allies join a Join Spot or sync with you. (the join spot can be buggy... for reasons I don't know)");
		}

		public void Awake()
		{
			_playAnimHook = new Hook((PlayAnimationOrig)CustomEmotesAPI.PlayAnimation, (PlayAnimationHandler)OnPlayAnimation);

			ModSettingsManager.SetModDescription("A mod that makes your turrets taunt/emote with you. Need I say more?");
			ModSettingsManager.AddOption(new CheckBoxOption(allAlliesEmote));
			ModSettingsManager.AddOption(new KeyBindOption(stopEmotingSolo));
			ModSettingsManager.AddOption(new KeyBindOption(stopEmotingAllies));
			ModSettingsManager.AddOption(new KeyBindOption(joinEmoteSolo));
			ModSettingsManager.AddOption(new KeyBindOption(joinEmoteAllies));
		}

		private IDetour _playAnimHook;
		public static ConfigEntry<bool> allAlliesEmote;
		public static ConfigEntry<KeyboardShortcut> stopEmotingSolo;
		public static ConfigEntry<KeyboardShortcut> joinEmoteSolo;
		public static ConfigEntry<KeyboardShortcut> stopEmotingAllies;
		public static ConfigEntry<KeyboardShortcut> joinEmoteAllies;

		public void Update()
		{
			if (Run.instance != null)
			{
				CharacterBody soloTarget = NetworkUser.readOnlyLocalPlayersList[0].master.GetBody();
				if (stopEmotingSolo.Value.IsDown())
				{
					SendAnimation(soloTarget, "none");
				}
				if (joinEmoteSolo.Value.IsDown())
				{
					JoinAnimation(soloTarget);
				}
				if (stopEmotingAllies.Value.IsDown())
				{
					var targets = GetAllyTargets(soloTarget, allAlliesEmote.Value);
					for (int i = 0; i < targets.Count; i++)
					{
						SendAnimation(targets[i], "none");
					}
				}
				if (joinEmoteAllies.Value.IsDown())
				{
					var targets = GetAllyTargets(soloTarget, allAlliesEmote.Value);
					for (int i = 0; i < targets.Count; i++)
					{
						JoinAnimation(targets[i]);
					}
				}
			}
		}

		private void SendAnimation(CharacterBody body, string animationName, int pos = -2)
		{
			new SyncAnimationToServer(body.netId, animationName, pos).Send(NetworkDestination.Server);
		}

		private void JoinAnimation(CharacterBody body)
		{
			BoneMapper mapper = GetBoneMapper(body);
			if (mapper.currentEmoteSpot != null)
				mapper.JoinEmoteSpot();
		}

		private BoneMapper GetBoneMapper(CharacterBody body)
		{
			List<BoneMapper> mappers = (List<BoneMapper>)typeof(BoneMapper).GetField("allMappers").GetValue(null);
			return mappers.Find(x => x.mapperBody == body);
		}

		private List<CharacterBody> GetAllyTargets(CharacterBody playerBody, bool turretsOnly)
		{
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(playerBody.teamComponent.teamIndex);
			
			List<CharacterBody> targetMembers = new List<CharacterBody>();
			for (int i = 0; i < teamMembers.Count; i++)
			{
				if ((teamMembers[i].body.baseNameToken == "ENGITURRET_BODY_NAME"
					|| (turretsOnly && !teamMembers[i].body.master.playerCharacterMasterController))
					&& OwnershipCheck(teamMembers[i].body.master, playerBody.master))
					targetMembers.Add(teamMembers[i].body);
			}

			return targetMembers;
		}

		private bool OwnershipCheck(CharacterMaster minion, CharacterMaster owner)
		{
			if (owner != null)
				Logger.LogInfo($"{minion.GetBody().GetDisplayName()}'s owner is {owner.GetBody().GetDisplayName()}");
			else
				Logger.LogInfo($"{minion.GetBody().GetDisplayName()} doesnt have an owner");
			return minion.minionOwnership.ownerMaster == owner;
		}

		private void OnPlayAnimation(PlayAnimationOrig orig, string animationName, int pos)
		{
			orig(animationName, pos);
			CharacterBody playerBody = NetworkUser.readOnlyLocalPlayersList[0].master.GetBody();
			List<CharacterBody> engiTurrets = GetAllyTargets(playerBody, allAlliesEmote.Value);

			if (engiTurrets.Count < 1)
				return;

			for (int i = 0; i < engiTurrets.Count; i++)
			{
				new SyncAnimationToServer(engiTurrets[i].netId, animationName, pos).Send(NetworkDestination.Server);
			}
		}
    }
}
