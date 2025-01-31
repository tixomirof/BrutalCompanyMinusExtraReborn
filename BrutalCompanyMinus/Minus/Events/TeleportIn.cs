﻿using BrutalCompanyMinus;
using BrutalCompanyMinus.Minus.MonoBehaviours;
using FacilityMeltdown.API;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;
using System.Threading;

namespace BrutalCompanyMinus.Minus.Events
{
    internal class TeleportIn : MEvent
    {
        public override string Name() => nameof(TeleportIn);

        public static TeleportIn Instance;

        private float startTime;
        private bool teleportStarted;

        public override void Initalize()
        {
            Instance = this;

            Weight = 2;
            Descriptions = new List<string>() { "Forced Entry", "Hope you find your way back...", "Hope you're not in a trapped room...", "Imagine getting stuck..." };
            ColorHex = "#800000";
            Type = EventType.VeryBad;

        }

        public override void Execute()
        {
            startTime = Time.fixedTime;

            this.teleportStarted = false;
            TimeOfDay.Instance.onTimeSync.AddListener(teleportTime);
        }

        public override void OnShipLeave()
        {
            TimeOfDay.Instance.onTimeSync.RemoveListener(teleportTime);
            teleportStarted = false;
            startTime = 0.0f;
        }

        public void teleportTime()
        {
            if (!teleportStarted && (Time.fixedTime - startTime > 19 ))
            {
                Random random = new Random();
                teleportStarted = true;
  
                foreach (GameObject player in StartOfRound.Instance.allPlayerObjects)
                {
                    PlayerControllerB component = player.GetComponent<PlayerControllerB>();
                    if (component != null)
                    {
                        if (component.isPlayerDead) continue; // Ignore anyone stupid enough to die before the teleport
                                                              // Rest in pieces - SoftDiamond
                                                              // Thank you Jacon for the code help here

                        Vector3 position = RoundManager.Instance.insideAINodes[random.Next(RoundManager.Instance.insideAINodes.Length)].transform.position;
                        Net.Instance.TeleportPlayerServerRPC(player, position);
                        component.isInElevator = false;
                        component.isInHangarShipRoom = false;
                        component.isInsideFactory = true;
                        component.averageVelocity = 0f;
                        component.velocityLastFrame = Vector3.zero;
                        component.TeleportPlayer(position);
                        component.beamOutParticle.Play();
                    }
                }
            }
        }
    }
}
