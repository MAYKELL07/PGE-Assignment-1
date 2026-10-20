using UnityEngine;

namespace Emberfall
{
    // Loads the music and sound effects used by both scenes.
    public sealed class ProjectAudio : MonoBehaviour
    {
        public static ProjectAudio Instance { get; private set; }

        AudioSource musicSource;
        AudioSource effectsSource;
        AudioClip swordSlash;
        AudioClip swordDraw;
        AudioClip coinPickup;
        AudioClip footstep;
        AudioClip enemyHit;
        AudioClip chestOpen;
        AudioClip uiClick;
        AudioClip confirm;
        AudioClip error;
        AudioClip spell;
        float nextFootstepTime;
        public static ProjectAudio Create(Transform parent)
        {
            if (Instance)
            {
                return Instance;
            }

            var audioObject = new GameObject("Audio System");
            audioObject.transform.SetParent(parent);
            return audioObject.AddComponent<ProjectAudio>();
        }

        void Awake()
        {
            Instance = this;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.28f;
            effectsSource = gameObject.AddComponent<AudioSource>();
            effectsSource.playOnAwake = false;
            effectsSource.volume = 0.72f;
            musicSource.clip = Resources.Load<AudioClip>("Audio/Music/DungeonTheme");
            swordSlash = Load("SwordSlash");
            swordDraw = Load("SwordDraw");
            coinPickup = Load("CoinPickup");
            footstep = Load("Footstep");
            enemyHit = Load("EnemyHit");
            chestOpen = Load("ChestOpen");
            uiClick = Load("UIClick");
            confirm = Load("Confirm");
            error = Load("Error");
            spell = Load("Spell");
            if (musicSource.clip)
            {
                musicSource.Play();
            }
        }

        AudioClip Load(string clipName)
        {
            return Resources.Load<AudioClip>("Audio/SFX/" + clipName);
        }

        void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (!clip || !effectsSource)
            {
                return;
            }

            effectsSource.pitch = pitch;
            effectsSource.PlayOneShot(clip, volume);
            effectsSource.pitch = 1f;
        }

        public void PlaySword()
        {
            Play(swordSlash, 0.9f, Random.Range(0.94f, 1.05f));
        }

        public void PlaySwordDraw()
        {
            Play(swordDraw, 0.55f);
        }

        public void PlayCoin()
        {
            Play(coinPickup, 0.8f, Random.Range(0.96f, 1.08f));
        }

        public void PlayHit()
        {
            Play(enemyHit, 0.72f, Random.Range(0.9f, 1.06f));
        }

        public void PlaySpell()
        {
            Play(spell, 0.85f, 0.82f);
        }

        public void PlayChest()
        {
            Play(chestOpen, 0.8f);
        }

        public void PlayClick()
        {
            Play(uiClick, 0.6f);
        }

        public void PlayConfirm()
        {
            Play(confirm, 0.8f);
        }

        public void PlayError()
        {
            Play(error, 0.65f);
        }

        public void TryPlayFootstep(float speed)
        {
            if (speed < 0.1f || Time.time < nextFootstepTime)
            {
                return;
            }

            nextFootstepTime = Time.time + 0.28f;
            Play(footstep, 0.22f, Random.Range(0.9f, 1.08f));
        }
    }
}
