using Framework;
using MikroFramework.Architecture;
using UnityEngine;

namespace Runtime.Enemies
{
    public class WormSegmentViewController : AbstractMikroController<MainGame>
    {
        public WormBoss wormBossOwner;
        private GameObject sandParticleInstance;
        private void OnTriggerEnter(Collider other)
        {
            if (!wormBossOwner.inited) {
                return;
            }

            if (!other.isTrigger)
            {
                GameObject hitObj = other.gameObject;
				
                if(hitObj.layer == LayerMask.NameToLayer("Default")
                   && sandParticleInstance != null) 
                {
                    GameObject s = wormBossOwner.sandParticlePool.Allocate();
                    s.transform.position = transform.position;
                    var recycleParticleOnDeath = s.GetComponent<RecycleParticleOnDeath>();
                    recycleParticleOnDeath.OnStopCallback = () => {
                        sandParticleInstance = null;
                        recycleParticleOnDeath.OnStopCallback = null;
                    };
                }
            }
        }
    }
}