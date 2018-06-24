namespace UnityEngine.AdventureGame
{
    public class CameraFollow : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if (SceneManager.Instance.Character != null)
            {
                transform.position = new Vector3(SceneManager.Instance.Character.transform.position.x,
                    transform.position.y, transform.position.z);
            }
        }
    }
}
