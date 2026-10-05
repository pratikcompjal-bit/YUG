using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TempleLens : MonoBehaviour
{
    Material lens;
    void Awake()
    {
        var shader = Resources.Load<Shader>("TempleLens");
        if (shader != null && shader.isSupported) lens = new Material(shader);
        GetComponent<Camera>().allowHDR = true;
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (lens == null) Graphics.Blit(source, destination);
        else Graphics.Blit(source, destination, lens);
    }
    void OnDestroy() { if (lens != null) Destroy(lens); }
}
