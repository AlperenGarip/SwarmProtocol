using System.Text;
using TMPro;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.Player;
using SwarmProtocol.Progression;

namespace SwarmProtocol.UI
{
    public class StatsOverlayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerHealth playerHealth;

        private static readonly StringBuilder _sb = new();

        private void OnEnable()
        {
            EventBus.OnGameStateChanged += OnGameStateChanged;
            Event<StatsChangedEvent>.Subscribe(OnStatsChanged);
        }

        private void OnDisable()
        {
            EventBus.OnGameStateChanged -= OnGameStateChanged;
            Event<StatsChangedEvent>.Unsubscribe(OnStatsChanged);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.LevelUp) Refresh();
        }

        private void OnStatsChanged(StatsChangedEvent _) => Refresh();

        // Called by LevelUpUI when the panel opens.
        public void Refresh()
        {
            if (statsText == null || playerStats == null) return;

            var um = UpgradeManager.Instance;

            _sb.Clear();
            _sb.AppendLine("<b>─ STATS ─</b>\n");

            float hp    = playerHealth != null ? playerHealth.CurrentHP : playerStats.MaxHP;
            float maxHp = playerStats.MaxHP;
            _sb.AppendLine(Row("HP",       $"{hp:0} / {maxHp:0}"));
            _sb.AppendLine(Row("Speed",    $"{playerStats.MoveSpeed:0.#}"));
            _sb.AppendLine(Row("Might",    $"x{playerStats.DamageMultiplier:0.##}"));
            _sb.AppendLine(Row("Fire Rate",$"x{playerStats.FireRateMultiplier:0.##}"));
            _sb.AppendLine(Row("Magnet",   $"{playerStats.MagnetRange:0.#}"));
            _sb.AppendLine(Row("Luck",     $"{playerStats.CritChance:0.#}%"));
            float curse = playerStats.Curse;
            if (curse > 0f)
                _sb.AppendLine(Row("Curse", $"{curse:0.#}"));

            if (um != null)
            {
                _sb.AppendLine();
                _sb.AppendLine("<b>─ CHARGES ─</b>\n");
                _sb.AppendLine(Row("Reroll", um.RerollCharges.ToString()));
                _sb.AppendLine(Row("Skip",   um.SkipCharges.ToString()));
                _sb.AppendLine(Row("Banish", um.BanishCharges.ToString()));
            }

            statsText.SetText(_sb);
        }

        private static string Row(string label, string value)
            => $"<color=#aaaaaa>{label,-10}</color>  <color=#ffffff><b>{value}</b></color>";
    }
}
