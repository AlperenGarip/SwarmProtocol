using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Progression.Commands;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Chest
{
    public enum JackpotType { None, Pair, Triple, FourOfAKind, FiveOfAKind }

    public struct JackpotResult
    {
        public JackpotType     Type;
        public IUpgradeCommand WinningCommand;
        public int             BonusGold;
        public int             ExtraExecutions;
        public bool            TriggerOvercharge;
        public bool            ShouldForceEvolve;
    }

    public static class JackpotProcessor
    {
        public static JackpotResult Evaluate(List<IUpgradeCommand> results, GameConfigSO config)
        {
            var freq = new Dictionary<Object, (int count, IUpgradeCommand cmd)>();

            foreach (var cmd in results)
            {
                if (cmd?.SourceItem == null) continue;
                if (freq.TryGetValue(cmd.SourceItem, out var entry))
                    freq[cmd.SourceItem] = (entry.count + 1, entry.cmd);
                else
                    freq[cmd.SourceItem] = (1, cmd);
            }

            int            bestCount = 0;
            IUpgradeCommand bestCmd  = null;
            foreach (var kv in freq)
            {
                if (kv.Value.count > bestCount)
                {
                    bestCount = kv.Value.count;
                    bestCmd   = kv.Value.cmd;
                }
            }

            if (bestCount < 2)
                return new JackpotResult { Type = JackpotType.None };

            return bestCount switch
            {
                2 => new JackpotResult
                {
                    Type           = JackpotType.Pair,
                    WinningCommand = bestCmd,
                    BonusGold      = config != null ? config.pairGoldReward : 50,
                },
                3 => new JackpotResult
                {
                    Type            = JackpotType.Triple,
                    WinningCommand  = bestCmd,
                    ExtraExecutions = config != null ? config.tripleExtraLevels : 1,
                },
                4 => new JackpotResult
                {
                    Type              = JackpotType.FourOfAKind,
                    WinningCommand    = bestCmd,
                    TriggerOvercharge = true,
                },
                _ => new JackpotResult
                {
                    Type              = JackpotType.FiveOfAKind,
                    WinningCommand    = bestCmd,
                    ShouldForceEvolve = true,
                },
            };
        }
    }
}
