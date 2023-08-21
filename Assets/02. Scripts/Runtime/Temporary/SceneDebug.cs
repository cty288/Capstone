using UnityEngine;

namespace Runtime.Temporary
{
    public class SceneDebug : MonoBehaviour
    {
        public TestEnity boss;
        public TestEnity player;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                boss.TakeDamage(10);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                player.TakeDamage(10);
            }
        }
    }
}
