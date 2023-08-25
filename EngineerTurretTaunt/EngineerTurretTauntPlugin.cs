using System;
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
	[BepInPlugin("com.kanggamming.EngineerTurretTaunt", "Engineer Turret Taunt", "0.1.0")]
	[R2APISubmoduleDependency("NetworkingAPI")]
    public class EngineerTurretTauntPlugin : BaseUnityPlugin
    {
		public EngineerTurretTauntPlugin()
			: base()
		{
			allAlliesEmote = Config.Bind("General", "All allies emote", false, "Whether all of your allies will taunt with you. (Does not make other players or their allies taunt)");
		}

		public void Awake()
		{
			_playAnimHook = new Hook((PlayAnimationOrig)CustomEmotesAPI.PlayAnimation, (PlayAnimationHandler)OnPlayAnimation);

			ModSettingsManager.SetModDescription("A mod that makes your turrets taunt/emote with you. Need I say more?");
			ModSettingsManager.AddOption(new CheckBoxOption(allAlliesEmote));
		}

		private IDetour _playAnimHook;
		public static ConfigEntry<bool> allAlliesEmote;

		public void OnDestroy()
		{
			_playAnimHook.Undo();
		}

		public void Update()
		{

		}

		private bool OwnershipCheck(CharacterMaster minion, CharacterMaster owner)
		{
			return minion.minionOwnership.ownerMaster == owner;
		}

		private void OnPlayAnimation(PlayAnimationOrig orig, string animationName, int pos)
		{
			orig(animationName, pos);
			CharacterBody playerBody = NetworkUser.readOnlyLocalPlayersList[0].master?.GetBody();
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(playerBody.teamComponent.teamIndex);
			
			List<CharacterBody> engiTurrets = new List<CharacterBody>();
			for (int i = 0; i < teamMembers.Count; i++)
			{
				if (teamMembers[i].body.baseNameToken == "ENGITURRET_BODY_NAME"
					|| (allAlliesEmote.Value && !teamMembers[i].body.master.playerCharacterMasterController)
					&& OwnershipCheck(teamMembers[i].body.master, playerBody.master))
					engiTurrets.Add(teamMembers[i].body);
			}

			if (engiTurrets.Count < 1)
				return;

			for (int i = 0; i < engiTurrets.Count; i++)
			{
				new SyncAnimationToServer(engiTurrets[i].netId, animationName, pos).Send(NetworkDestination.Server);
			}
		}
    }
}
