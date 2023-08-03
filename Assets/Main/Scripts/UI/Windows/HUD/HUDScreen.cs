using Main.Scripts.Core.Mvp;
using Main.Scripts.Core.Resources;
using Main.Scripts.Levels;
using Main.Scripts.Room;
using Main.Scripts.Skills;
using Main.Scripts.Skills.ActiveSkills;
using Main.Scripts.UI.Windows.HUD.HotBar;
using Main.Scripts.UI.Windows.HUD.HotBar.HotBarIcons;
using Main.Scripts.Utils;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD
{
    public class HUDScreen : MvpMonoBehavior<HUDContract.HotBarPresenter>, HUDContract.HotBarView, UIScreen
    {
        private HotBarView hotBarView = default!;
        protected override HUDContract.HotBarPresenter? presenter { get; set; }

        private void Awake()
        {
            hotBarView = new HotBarView(GetComponent<UIDocument>());
            SetVisibility(false);
        }

        public void Open()
        {
            if (presenter == null)
            {
                var hotBarIconDataHolder = GlobalResources.Instance.ThrowWhenNull().HotBarIconDataHolder;
                var runner = RoomManager.Instance.ThrowWhenNull().Runner;
                var playersHolder = LevelContext.Instance.ThrowWhenNull().PlayersHolder;
                var skillOwner = playersHolder.Get(runner.LocalPlayer)
                    .GetInterface<SkillsOwner>().ThrowWhenNull();
                var tickRate = runner.Config.Simulation.TickRate;

                presenter = new HUDPresenterImpl(this, hotBarIconDataHolder, skillOwner, tickRate);
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

        public void Bind(ref HotBarData hotBarData)
        {
            hotBarView.Bind(ref hotBarData);
        }

        public void SetVisibility(bool isVisible)
        {
            hotBarView.SetVisibility(isVisible);
        }
    }
}