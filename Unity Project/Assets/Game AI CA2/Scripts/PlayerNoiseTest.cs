using UnityEngine;

public class PlayerNoiseTest : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("PlayerNoiseTest requires an AudioSource.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (audioSource && audioSource.clip)
            {
                audioSource.Play();
            }
        }
    }
}
