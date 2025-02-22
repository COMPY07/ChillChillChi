using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
{


    public class SoundClip
    {
        public AudioClip clip;
        public float volume;
        public bool loop;

        public SoundClip(AudioClip clip = null, float volume = 1f, bool loop = false)
        {
            this.clip = clip;
            this.loop = loop;
            this.volume = volume;
        }
        

    }
    
    
    class SoundPlay {
        private AudioSource source;

        private List<SoundClip> clips;
        
        public SoundPlay(AudioSource source) {
            clips = new List<SoundClip>();
            this.source = source;
        }

        public SoundPlay(GameObject parent) {
            this.source = parent.AddComponent<AudioSource>();
            clips = new List<SoundClip>();
        }

        public bool Next() {
            if(clips.Count <=0 || Playing()) return false;
            SoundClip clip = clips[0];
            clips.RemoveAt(0);

            source.volume = clip.volume;
            source.loop = clip.loop;
            source.clip = clip.clip;
            
            source.Play();
            return true;
        }

        public bool NextForce() {
            
            if(clips.Count <= 0) return false;
            
            Stop();
    
            SoundClip clip = clips[0];
            clips.RemoveAt(0);
    
            source.volume = clip.volume;
            source.loop = clip.loop;
            source.clip = clip.clip;
    
            source.Play();
            return true;
        }
        public bool Stop()
        {
            source.Stop();
            source.volume = 0;
            return true;
        }

        public bool Pause()
        {
            source.Pause();
            source.volume = 0;
            return true;
        }
        public bool Playing() {
            return source.isPlaying;
        }

        public bool AddSound(SoundClip clip)
        {
            if (clip == null) return false;
            clips.Add(clip);
            return true;
        }
        
        
        
    }
    
    public class SoundManager : Singleton<SoundManager> {

        private List<SoundPlay> source;
        private AudioListener listener;

        private bool stop;


        public void Awake()
        {
            base.Awake();
            if (!Init())
            {
                Debug.Log("error");
            }
        }
        public bool Init()
        {
            source = new List<SoundPlay>();
            source.Add(new SoundPlay(this.gameObject));
             //source = this.gameObject.AddComponent<AudioSource>();
            // listener = this.gameObject.AddComponent<AudioListener>();
            stop = false;
            StartCoroutine(this.NextSound());
            return source == null || listener == null;
            
        }

        public void AddSoundPlayer(int count = 1) { for(int i = 0; i < count; i++) source.Add(new SoundPlay(this.gameObject)); }

        public void SetStop(bool stop)
        {
            this.stop = stop;
            if (!this.stop) return;

            foreach (SoundPlay player in source) player.Stop();
            
        }

        public bool GetStop() { return this.stop; }
        
        
        
        public bool NextSoundAdd(int idx, SoundClip clip)
        {

            if (idx >= source.Count || clip == null) return false;

            source[idx].AddSound(clip);
            return true;
        }

        private IEnumerator NextSound() {
            while (true) { 
                if (stop) yield return new WaitForSeconds(Time.deltaTime);
                else {
                    foreach (SoundPlay player in this.source)
                    {
                        
                        player.Next();
                        yield return new WaitForSeconds(Time.deltaTime);
                    }
                }
            }
        }

        public void NextSound(int idx)
        {
            if (idx >= source.Count) return;
            source[idx].Next();
        }
        public void NextSoundForce(int idx)
        {
            if (idx >= source.Count) return;
            source[idx].NextForce();
        }

        IEnumerator SoundFadeIn(AudioSource source, AudioClip clip,
                                float increase = 0.1f)
        {
            if (source == null || clip == null) yield break;

            source.volume = 0;

            yield break;


        }

    }
}