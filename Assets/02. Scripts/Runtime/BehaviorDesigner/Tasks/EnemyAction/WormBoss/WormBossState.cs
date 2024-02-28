using BehaviorDesigner.Runtime;

namespace _02._Scripts.Runtime.Enemies.ViewControllers.Instances.WormBoss
{
    public enum WormBossState
    {
        None,
        AcidBombs,
        LaserBeam,
        RapidFire,
        ArcMissiles,
        MoveMissiles,
        FallAttack,
    }
    
    [System.Serializable]
    public class SharedWormBossState : SharedVariable<WormBossState>
    {
        public static implicit operator SharedWormBossState(WormBossState value)
        {
            return new SharedWormBossState { Value = value };
        }
    }
}