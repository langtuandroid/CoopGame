using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Core.GameLogic;
using Main.Scripts.Core.GameLogic.Phases;
using UnityEngine;

namespace Main.Scripts.Skills.Charge
{
    public class SkillChargeManager : GameLoopEntityNetworked
    {
        private const int MAX_CHARGE_LEVEL = 5;

        [SerializeField]
        [Min(1)]
        private int decreaseProgressFrequencyTicks = 12;
        [SerializeField]
        [Min(0)]
        private int decreaseProgressValue;

        [SerializeField]
        [Min(1)]
        private int progressForLevel2 = 1;
        [SerializeField]
        [Min(1)]
        private int progressForLevel3 = 1;
        [SerializeField]
        [Min(1)]
        private int progressForLevel4 = 1;
        [SerializeField]
        [Min(1)]
        private int progressForLevel5 = 1;

        private List<Listener> listeners = new();
        private int addChargeValue;

        private GameLoopPhase[] gameLoopPhases =
        {
            GameLoopPhase.SkillChargeUpdate,
            GameLoopPhase.VisualStateUpdatePhase,
        };
        private GameLoopPhase[] proxyGameLoopPhases =
        {
            GameLoopPhase.VisualStateUpdatePhase,
            GameLoopPhase.SyncChargeValue
        };

        [Networked]
        private int chargeLevel { get; set; } = 1;
        [Networked]
        private int chargeProgress { get; set; }

        public int ChargeLevel => chargeLevel;
        public int ChargeProgress => chargeProgress;
        public bool IsMaxChargeLevel => chargeLevel == MAX_CHARGE_LEVEL;

        public override void OnGameLoopPhase(GameLoopPhase phase)
        {
            switch (phase)
            {
                case GameLoopPhase.SkillChargeUpdate:
                    UpdateCharge();
                    break;
                case GameLoopPhase.VisualStateUpdatePhase:
                    VisualStateUpdatePhase();
                    break;
                case GameLoopPhase.SyncChargeValue:
                    SyncChargeValue();
                    break;
            }
        }

        private void UpdateCharge()
        {
            if (IsMaxChargeLevel)
            {
                addChargeValue = 0;
                return;
            }

            var decreaseValue = Runner.Tick % decreaseProgressFrequencyTicks == 0 ? decreaseProgressValue : 0;
            chargeProgress = Math.Max(0, chargeProgress + addChargeValue - decreaseValue);
            addChargeValue = 0;

            var progressForNextLevel = GetProgressForNextLevel();

            if (chargeProgress >= progressForNextLevel)
            {
                chargeLevel++;
                if (IsMaxChargeLevel)
                {
                    chargeProgress = 0;
                }
                else
                {
                    chargeProgress -= progressForNextLevel;
                }
            }
        }

        public int GetProgressForNextLevel()
        {
            if (IsMaxChargeLevel)
            {
                return 0;
            }

            return chargeLevel switch
            {
                1 => progressForLevel2,
                2 => progressForLevel3,
                3 => progressForLevel4,
                4 => progressForLevel5,
                _ => throw new Exception(
                    $"Doesn't have value of charge progress need for ChargeLevel {chargeLevel + 1}")
            };
        }

        private void VisualStateUpdatePhase()
        {
            foreach (var listener in listeners)
            {
                listener?.OnChargeInfoChanged();
            }
        }

        private void SyncChargeValue()
        {
            RPC_AddCharge(addChargeValue);
            addChargeValue = 0;
        }

        public void AddListener(Listener listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(Listener listener)
        {
            listeners.Remove(listener);
        }

        public override IEnumerable<GameLoopPhase> GetSubscribePhases()
        {
            return HasStateAuthority ? gameLoopPhases : proxyGameLoopPhases;
        }

        public void AddCharge(int chargeValue)
        {
            addChargeValue += chargeValue;
        }

        [Rpc(RpcSources.Proxies, RpcTargets.StateAuthority)]
        private void RPC_AddCharge(int chargeValue)
        {
            addChargeValue += chargeValue;
        }

        public interface Listener
        {
            void OnChargeInfoChanged();
        }
    }
}