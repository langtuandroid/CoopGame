using System.Collections.Generic;
using Main.Scripts.Core.Mvp;
using Main.Scripts.Core.Resources;
using Main.Scripts.Levels;
using Main.Scripts.Room;
using Main.Scripts.Skills;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.UI.Windows.HUD.ChargeInfo;
using Main.Scripts.UI.Windows.HUD.ControlsTextWindow;
using Main.Scripts.UI.Windows.HUD.HotBar;
using Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons;
using Main.Scripts.UI.Windows.HUD.PowerChargeInfo;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD
{
    public class HUDScreen : MvpMonoBehavior<HUDContract.HotBarPresenter>, HUDContract.HotBarView, UIScreen
    {
        private HotBarView hotBarView = null!;
        private ControlsTextView controlsTextView = null!;
        private ChargeInfoView chargeInfoView = null!;
        private PowerChargeInfoView powerChargeInfo = null!;
        protected override HUDContract.HotBarPresenter? presenter { get; set; }
        
        [SerializeField]
        private List<ControlsTextData> controlsTextList = new();
        [SerializeField]
        private VisualTreeAsset controlsTextLayout = null!;

        private void Awake()
        {
            var doc = GetComponent<UIDocument>();
            hotBarView = new HotBarView(doc);
            controlsTextView = new ControlsTextView(doc, controlsTextLayout);
            chargeInfoView = new ChargeInfoView(doc);
            powerChargeInfo = new PowerChargeInfoView(doc);
            SetVisibility(false);
        }

        public void Open()
        {
            if (presenter == null)
            {
                var hotBarIconDataHolder = GlobalResources.Instance.ThrowWhenNull().HotBarIconDataHolder;
                var runner = RoomManager.Instance.ThrowWhenNull().Runner;
                var levelContext = LevelContext.Instance.ThrowWhenNull();
                var skillChargeManager = levelContext.SkillChargeManager;
                var playersHolder = levelContext.PlayersHolder;
                var skillOwner = playersHolder.Get(runner.LocalPlayer)
                    .GetInterface<SkillsOwner>().ThrowWhenNull();
                var tickRate = runner.Config.Simulation.TickRate;

                presenter = new HUDPresenterImpl(
                    view: this,
                    dataHolder: hotBarIconDataHolder,
                    skillsOwner: skillOwner,
                    skillChargeManager: skillChargeManager,
                    tickRate: tickRate
                );
            }

            presenter?.OnOpen();
        }

        public void Close()
        {
            presenter?.OnClose();
        }

        public void UpdateSkillCooldown(ActiveSkillType skillType, int cooldownLeftSec)
        {
            hotBarView.SetCooldown(skillType, cooldownLeftSec);
        }

        public void OnChargeInfoChanged(int chargeLevel, int chargeProgress, int chargeProgressTarget, bool isMaxChargeLevel)
        {
            chargeInfoView.SetChargeInfo(chargeLevel, chargeProgress, chargeProgressTarget, isMaxChargeLevel);
        }

        public void OnPowerChargeInfoChanged(bool isShow, int powerChargeLevel, int powerChargeProgress)
        {
            powerChargeInfo.SetPowerChargeInfo(isShow, powerChargeLevel, powerChargeProgress);
        }

        public void Bind(ref HotBarData hotBarData)
        {
            hotBarView.Bind(ref hotBarData);
            controlsTextView.Bind(controlsTextList);
        }

        public void SetVisibility(bool isVisible)
        {
            hotBarView.SetVisibility(isVisible);
        }
    }
}