using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public int speed;
    // Start is called before the first frame update
    public void MoveCameraLeft()
    {
        if (Camera.main.transform.position.x != 0)
        {
            Vector3 translation = new Vector3(-32, 0, 0);
            Camera.main.transform.Translate(translation);
        }
    }

    public void MoveCameraRight()
    {
        Vector3 translation = new Vector3(32, 0, 0);
        Camera.main.transform.Translate(translation);
    }
}
