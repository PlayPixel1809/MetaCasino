using UnityEngine;
using UnityEditor;

public class SkyboxCreator : MonoBehaviour
{
    const int TEXTURE_SIZE = 1024;

    void Update()
    {
        if (Input.anyKey)
        {
            Capture();
        }
    }

    void Capture()
    {
        Cubemap cubemap = new Cubemap(TEXTURE_SIZE, TextureFormat.ARGB32, true);
        cubemap.name = "Skybox";
        Camera camera = GetComponent<Camera>();
        camera.RenderToCubemap(cubemap);

        AssetDatabase.CreateAsset(
          cubemap,
          "Assets/Textures/Skybox/Skybox.cubemap"
        );
    }
}