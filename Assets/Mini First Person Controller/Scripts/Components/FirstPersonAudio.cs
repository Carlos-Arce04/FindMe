using System.Linq;
using UnityEngine;

public class FirstPersonAudio : MonoBehaviour
{
    public FirstPersonMovement character;
    public GroundCheck groundCheck;

    // <<< TU PARTE EMPIEZA AQUÍ >>>
    [Header("AI Sound Emitter")]
    [Tooltip("Referencia a tu script que emite sonidos para la IA.")]
    public PlayerSoundEmitter soundEmitter; // Arrastra tu script aquí en el Inspector

    [Tooltip("Alcance del sonido al caminar para que el monstruo lo oiga.")]
    public float walkSoundRange = 5f;

    [Tooltip("Alcance del sonido al correr.")]
    public float runSoundRange = 15f;

    [Tooltip("Alcance del sonido al caminar agachado (muy bajo).")]
    public float crouchSoundRange = 1.5f;

    [Tooltip("Alcance del sonido al saltar.")]
    public float jumpSoundRange = 8f;

    [Tooltip("Alcance del sonido al aterrizar.")]
    public float landSoundRange = 10f;
    // <<< TU PARTE TERMINA AQUÍ >>>


    [Header("Step")]
    public AudioSource stepAudio;
    public AudioSource runningAudio;
    [Tooltip("Minimum velocity for moving audio to play")]
    public float velocityThreshold = .01f;
    Vector2 lastCharacterPosition;
    Vector2 CurrentCharacterPosition => new Vector2(character.transform.position.x, character.transform.position.z);

    [Header("Landing")]
    public AudioSource landingAudio;
    public AudioClip[] landingSFX;

    [Header("Jump")]
    public Jump jump;
    public AudioSource jumpAudio;
    public AudioClip[] jumpSFX;

    [Header("Crouch")]
    public Crouch crouch;
    public AudioSource crouchStartAudio, crouchedAudio, crouchEndAudio;
    public AudioClip[] crouchStartSFX, crouchEndSFX;

    AudioSource[] MovingAudios => new AudioSource[] { stepAudio, runningAudio, crouchedAudio };


    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>();
        groundCheck = (transform.parent ?? transform).GetComponentInChildren<GroundCheck>();
        stepAudio = GetOrCreateAudioSource("Step Audio");
        runningAudio = GetOrCreateAudioSource("Running Audio");
        landingAudio = GetOrCreateAudioSource("Landing Audio");
        jump = GetComponentInParent<Jump>();
        if (jump) jumpAudio = GetOrCreateAudioSource("Jump audio");
        crouch = GetComponentInParent<Crouch>();
        if (crouch)
        {
            crouchStartAudio = GetOrCreateAudioSource("Crouch Start Audio");
            crouchedAudio = GetOrCreateAudioSource("Crouched Audio"); // Corregí un error que había en el código original aquí
            crouchEndAudio = GetOrCreateAudioSource("Crouch End Audio"); // Corregí un error que había en el código original aquí
        }

        // <<< TU PARTE >>>
        // Intenta encontrar tu emisor de sonido automáticamente
        soundEmitter = GetComponentInParent<PlayerSoundEmitter>();
    }

    void OnEnable() => SubscribeToEvents();
    void OnDisable() => UnsubscribeToEvents();

    void FixedUpdate()
    {
        float velocity = Vector3.Distance(CurrentCharacterPosition, lastCharacterPosition);
        if (velocity >= velocityThreshold && groundCheck && groundCheck.isGrounded)
        {
            if (crouch && crouch.IsCrouched)
            {
                SetPlayingMovingAudio(crouchedAudio);
               
            }
            else if (character.IsRunning)
            {
                SetPlayingMovingAudio(runningAudio);
                // <<< TU PARTE >>>
                soundEmitter?.EmitSound(runSoundRange); // Emitir sonido de correr
            }
            else
            {
                SetPlayingMovingAudio(stepAudio);
                // <<< TU PARTE >>>
                soundEmitter?.EmitSound(walkSoundRange); // Emitir sonido de caminar
            }
        }
        else
        {
            SetPlayingMovingAudio(null);
        }
        lastCharacterPosition = CurrentCharacterPosition;
    }

    void SetPlayingMovingAudio(AudioSource audioToPlay)
    {
        foreach (var audio in MovingAudios.Where(audio => audio != audioToPlay && audio != null))
        {
            audio.Pause();
        }
        if (audioToPlay && !audioToPlay.isPlaying)
        {
            audioToPlay.Play();
        }
    }

    #region Play instant-related audios.
    void PlayLandingAudio()
    {
        PlayRandomClip(landingAudio, landingSFX);
        // <<< TU PARTE >>>
        soundEmitter?.EmitSound(landSoundRange); // Emitir sonido de aterrizaje
    }
    void PlayJumpAudio()
    {
        PlayRandomClip(jumpAudio, jumpSFX);
        // <<< TU PARTE >>>
        soundEmitter?.EmitSound(jumpSoundRange); // Emitir sonido de salto
    }
    void PlayCrouchStartAudio() => PlayRandomClip(crouchStartAudio, crouchStartSFX);
    void PlayCrouchEndAudio() => PlayRandomClip(crouchEndAudio, crouchEndSFX);
    #endregion

    // ... (El resto del script se queda igual) ...
    #region Subscribe/unsubscribe to events.
    void SubscribeToEvents()
    {
        groundCheck.Grounded += PlayLandingAudio;
        if (jump) jump.Jumped += PlayJumpAudio;
        if (crouch)
        {
            crouch.CrouchStart += PlayCrouchStartAudio;
            crouch.CrouchEnd += PlayCrouchEndAudio;
        }
    }
    void UnsubscribeToEvents()
    {
        groundCheck.Grounded -= PlayLandingAudio;
        if (jump) jump.Jumped -= PlayJumpAudio;
        if (crouch)
        {
            crouch.CrouchStart -= PlayCrouchStartAudio;
            crouch.CrouchEnd -= PlayCrouchEndAudio;
        }
    }
    #endregion
    #region Utility.
    AudioSource GetOrCreateAudioSource(string name)
    {
        AudioSource result = System.Array.Find(GetComponentsInChildren<AudioSource>(), a => a.name == name);
        if (result) return result;
        result = new GameObject(name).AddComponent<AudioSource>();
        result.spatialBlend = 1;
        result.playOnAwake = false;
        result.transform.SetParent(transform, false);
        return result;
    }
    static void PlayRandomClip(AudioSource audio, AudioClip[] clips)
    {
        if (!audio || clips.Length <= 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clips.Length > 1)
            while (clip == audio.clip)
                clip = clips[Random.Range(0, clips.Length)];
        audio.clip = clip;
        audio.Play();
    }
    #endregion 
}