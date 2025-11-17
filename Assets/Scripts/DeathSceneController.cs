using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;


using UnityEngine.InputSystem;


public class DeathSceneController : MonoBehaviour
{
    [Header("UI")]
    public Image flash;
    public Image bloodOverlay;
    public TextMeshProUGUI restartText;

    [Header("Actors")]
    public GameObject slender;
    public Animator slenderAnim;
    public Camera mainCamera;

    Texture2D staticTex;
    Sprite staticSprite;

    AudioSource staticSource;
    AudioSource slenderAudio;

    Renderer[] slenderRenderers;

    bool canRestart = false;

    const float BLACK_HOLD = 2.2f;
    const float STATIC_TIME = 1.1f;
    const float GRAB_TIME = 0.8f;

    uint noiseState = 987654;


    // ------------------------------------------------------
    // Initialization
    // ------------------------------------------------------
    void Awake()
    {
        if (flash) flash.color = Color.black;
        if (bloodOverlay) bloodOverlay.color = new Color(1, 1, 1, 0);
        if (restartText) restartText.gameObject.SetActive(false);

        if (slender)
        {
            slender.SetActive(false);
            slenderRenderers = slender.GetComponentsInChildren<Renderer>(true);
            slenderAudio = slender.GetComponent<AudioSource>();
        }

        staticSource = gameObject.AddComponent<AudioSource>();
        staticSource.playOnAwake = false;
        staticSource.loop = true;
        staticSource.spatialBlend = 0f;
        staticSource.volume = 0.45f;

        staticTex = new Texture2D(512, 288, TextureFormat.RGB24, false);
        staticTex.filterMode = FilterMode.Point;
        staticSprite = Sprite.Create(staticTex, new Rect(0, 0, staticTex.width, staticTex.height), new Vector2(0.5f, 0.5f));
    }

    void Start() => StartCoroutine(RunDeathSequence());


    // ------------------------------------------------------
    // Restart input
    // ------------------------------------------------------
    void Update()
    {
        if (canRestart && Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ------------------------------------------------------
    // Main death logic
    // ------------------------------------------------------
    IEnumerator RunDeathSequence()
    {
        yield return new WaitForSeconds(BLACK_HOLD);

        PlayStaticVisual();
        StartWhiteNoise();
        yield return new WaitForSeconds(STATIC_TIME);

        StopWhiteNoise();
        ResetFlashBlack();
        yield return new WaitForSeconds(0.15f);

        SetupSlenderModel();

        StartCoroutine(PlayCameraGrabAndShake(GRAB_TIME));
        StartCoroutine(PlayBloodEffect());

        yield return new WaitForSeconds(0.4f);
        ShowRestartPrompt();

        canRestart = true;
    }


    // ------------------------------------------------------
    // Static TV effect
    // ------------------------------------------------------
    void PlayStaticVisual()
    {
        if (!flash) return;

        flash.sprite = staticSprite;
        flash.color = Color.white;

        StartCoroutine(AnimateStaticTexture(STATIC_TIME));
    }

    void ResetFlashBlack()
    {
        if (flash)
        {
            flash.sprite = null;
            flash.color = Color.black;
            flash.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    void StartWhiteNoise()
    {
        staticSource.clip = AudioClip.Create("Static", 44100, 1, 44100, true, OnNoiseRead);
        staticSource.Play();
    }

    void StopWhiteNoise()
    {
        staticSource.Stop();
    }

    float GenerateNoise()
    {
        noiseState ^= noiseState << 13;
        noiseState ^= noiseState >> 17;
        noiseState ^= noiseState << 5;

        return ((noiseState & 0xFFFFFF) / (float)0xFFFFFF) * 2f - 1f;
    }

    void OnNoiseRead(float[] data)
    {
        for (int i = 0; i < data.Length; i++)
            data[i] = GenerateNoise() * 0.35f;
    }

    IEnumerator AnimateStaticTexture(float duration)
    {
        if (!flash) yield break;

        float t = 0f;
        int w = staticTex.width;
        int h = staticTex.height;
        int offset = 0;

        while (t < duration)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float v = Random.value;
                    staticTex.SetPixel(x, (y + offset) % h, new Color(v, v, v));
                }
            }

            offset = (offset + 3) % h;
            staticTex.Apply(false, false);

            flash.rectTransform.anchoredPosition =
                new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));

            t += Time.deltaTime;
            yield return null;
        }
    }


    // ------------------------------------------------------
    // Slenderman setup
    // ------------------------------------------------------
    void SetupSlenderModel()
    {
        if (!slender || !mainCamera) return;

        Vector3 pos = mainCamera.transform.position + mainCamera.transform.forward * 4.5f;

        slender.transform.position = pos;
        slender.transform.rotation =
            Quaternion.LookRotation(-mainCamera.transform.forward, Vector3.up);
        slender.transform.localScale = Vector3.one * 2.8f;

        slender.SetActive(true);

        if (slenderAnim) slenderAnim.enabled = false;

        if (slenderRenderers != null)
        {
            foreach (var r in slenderRenderers)
            {
                foreach (var m in r.materials)
                {
                    m.SetColor("_Color", Color.black);
                    if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", Color.black);
                    if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0f);
                    if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
                }
            }
        }
    }

    // ------------------------------------------------------
    // Camera grab + violent shake
    // ------------------------------------------------------
    IEnumerator PlayCameraGrabAndShake(float duration)
    {
        if (!mainCamera || !slender) yield break;

        Transform cam = mainCamera.transform;
        Transform sl = slender.transform;

        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;

        Vector3 targetPos = sl.position - sl.forward * 1.8f + sl.up * 0.8f;
        Quaternion lookRot = Quaternion.LookRotation(sl.position - startPos);

        float elapsed = 0f;
        float grabTime = 0.2f;

        // Grab phase
        while (elapsed < grabTime)
        {
            float t = Mathf.Pow(elapsed / grabTime, 3f);
            cam.position = Vector3.Lerp(startPos, targetPos, t);
            cam.rotation = Quaternion.Slerp(startRot, lookRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 heldPos = targetPos;
        Quaternion heldRot = Quaternion.LookRotation(sl.position - heldPos);

        // Shake phase
        while (elapsed < duration)
        {
            float shake = Mathf.Lerp(1f, 0.8f, (elapsed - grabTime) / (duration - grabTime));

            Vector3 posShake = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.1f, 0.1f)
            ) * shake;

            Vector3 rotShake = new Vector3(
                Random.Range(-25f, 25f),
                Random.Range(-30f, 30f),
                Random.Range(-50f, 50f)
            ) * shake;

            cam.position = heldPos + posShake;
            cam.rotation = heldRot * Quaternion.Euler(rotShake);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.position = heldPos;
        cam.rotation = heldRot;
    }


    // ------------------------------------------------------
    // Blood overlay effect
    // ------------------------------------------------------
    IEnumerator PlayBloodEffect()
    {
        if (!bloodOverlay) yield break;

        yield return new WaitForSeconds(0.3f);

        // Fade in
        for (float a = 0; a < 1f; a += Time.deltaTime * 8f)
        {
            bloodOverlay.color = new Color(1, 1, 1, a);
            yield return null;
        }

        bloodOverlay.color = Color.white;

        // Shake
        float t = 0f;
        while (t < 0.5f)
        {
            bloodOverlay.rectTransform.anchoredPosition =
                new Vector2(Random.Range(-8f, 8f), Random.Range(-8f, 8f));

            t += Time.deltaTime;
            yield return null;
        }

        bloodOverlay.rectTransform.anchoredPosition = Vector2.zero;
    }


    // ------------------------------------------------------
    // Restart prompt
    // ------------------------------------------------------
    void ShowRestartPrompt()
    {
        if (!restartText) return;

        restartText.text = "PRESS SPACE TO RESTART";
        restartText.alpha = 0f;
        restartText.gameObject.SetActive(true);

        StartCoroutine(BlinkText(restartText));
    }

    IEnumerator BlinkText(TextMeshProUGUI t)
    {
        while (true)
        {
            t.alpha = 1f; yield return new WaitForSeconds(0.55f);
            t.alpha = 0.25f; yield return new WaitForSeconds(0.55f);
        }
    }
}
