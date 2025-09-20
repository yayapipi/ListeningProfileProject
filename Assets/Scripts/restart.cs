using UnityEngine;

namespace DefaultNamespace
{
    public class restart : MonoBehaviour
    {
        public void restartGame()
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}