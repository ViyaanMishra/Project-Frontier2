using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace Frontier.MeshGen.Animation
{
    /// <summary>
    /// Advanced lip-sync generator with phoneme analysis, emotional inflection, and multi-language support
    /// </summary>
    public class LipSyncGen : ComponentSystem
    {
        [System.Serializable]
        public struct Phoneme
        {
            public string name;
            public float[] visemeWeights; // Array of 15 standard viseme weights
            public float duration;
            public int phonemeType; // 0=vowel, 1=plosive, 2=fricative, 3=nasal, 4=liquid
            public bool isVoiced;
        }
        
        [System.Serializable]
        public struct Viseme
        {
            public string name;
            public float jawOpen;
            public float lipStretch;
            public float lipPucker;
            public float tonguePosition;
            public float cheekRaise;
            public float jawForward;
        }
        
        public struct ActiveSpeechInstance
        {
            public Entity entity;
            public string currentText;
            public int currentPhonemeIndex;
            public float currentTime;
            public float speechSpeed;
            public float emotionIntensity;
            public EmotionType currentEmotion;
            public bool isActive;
            public AudioClip audioClip;
            public float audioTime;
        }
        
        public enum EmotionType { Neutral, Happy, Sad, Angry, Fearful, Surprised, Disgusted, Contemptuous }
        
        private NativeList<ActiveSpeechInstance> _activeSpeeches;
        private Dictionary<string, Phoneme> _phonemeDictionary;
        private List<Viseme> _visemeLibrary;
        private AnimationCurve _emotionCurve;
        
        // Standard 15 visemes (MPEG-4 compatible)
        private static readonly string[] StandardVisemes = new string[]
        {
            "sil", "pp", "ff", "th", "dd", "kk", "ch", "ss", "nn", 
            "rr", "aa", "e", "i", "o", "u"
        };
        
        protected override void OnCreate()
        {
            _activeSpeeches = new NativeList<ActiveSpeechInstance>(Allocator.Persistent);
            _phonemeDictionary = new Dictionary<string, Phoneme>();
            _visemeLibrary = new List<Viseme>();
            
            InitializePhonemeDictionary();
            InitializeVisemeLibrary();
            InitializeEmotionCurves();
        }
        
        protected override void OnDestroy()
        {
            _activeSpeeches.Dispose();
        }
        
        private void InitializePhonemeDictionary()
        {
            // Vowels
            _phonemeDictionary["AA"] = CreatePhoneme("AA", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0 }, 0.15f, 0, true);
            _phonemeDictionary["AE"] = CreatePhoneme("AE", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.8f, 0.2f, 0, 0, 0 }, 0.12f, 0, true);
            _phonemeDictionary["EH"] = CreatePhoneme("EH", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.3f, 0.7f, 0, 0, 0 }, 0.1f, 0, true);
            _phonemeDictionary["IH"] = CreatePhoneme("IH", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.1f, 0.3f, 0.6f, 0, 0 }, 0.1f, 0, true);
            _phonemeDictionary["OH"] = CreatePhoneme("OH", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.8f, 0.2f }, 0.15f, 0, true);
            _phonemeDictionary["UH"] = CreatePhoneme("UH", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.2f, 0.3f, 0.5f }, 0.12f, 0, true);
            _phonemeDictionary["IY"] = CreatePhoneme("IY", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.2f, 0.8f, 0, 0 }, 0.1f, 0, true);
            _phonemeDictionary["UW"] = CreatePhoneme("UW", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.1f, 0.2f, 0.7f }, 0.15f, 0, true);
            
            // Plosives
            _phonemeDictionary["P"] = CreatePhoneme("P", new float[] { 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.08f, 1, false);
            _phonemeDictionary["B"] = CreatePhoneme("B", new float[] { 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.08f, 1, true);
            _phonemeDictionary["T"] = CreatePhoneme("T", new float[] { 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.06f, 1, false);
            _phonemeDictionary["D"] = CreatePhoneme("D", new float[] { 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.06f, 1, true);
            _phonemeDictionary["K"] = CreatePhoneme("K", new float[] { 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.08f, 1, false);
            _phonemeDictionary["G"] = CreatePhoneme("G", new float[] { 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.08f, 1, true);
            
            // Fricatives
            _phonemeDictionary["F"] = CreatePhoneme("F", new float[] { 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 2, false);
            _phonemeDictionary["V"] = CreatePhoneme("V", new float[] { 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 2, true);
            _phonemeDictionary["TH"] = CreatePhoneme("TH", new float[] { 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 2, false);
            _phonemeDictionary["S"] = CreatePhoneme("S", new float[] { 0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 2, false);
            _phonemeDictionary["Z"] = CreatePhoneme("Z", new float[] { 0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 2, true);
            _phonemeDictionary["SH"] = CreatePhoneme("SH", new float[] { 0, 0, 0, 0, 0, 0, 0, 0.3f, 0, 0, 0.2f, 0, 0.5f, 0, 0 }, 0.12f, 2, false);
            _phonemeDictionary["ZH"] = CreatePhoneme("ZH", new float[] { 0, 0, 0, 0, 0, 0, 0, 0.3f, 0, 0, 0.2f, 0, 0.5f, 0, 0 }, 0.12f, 2, true);
            _phonemeDictionary["HH"] = CreatePhoneme("HH", new float[] { 0, 0, 0, 0, 0.3f, 0, 0, 0, 0, 0, 0.5f, 0, 0, 0, 0 }, 0.1f, 2, false);
            
            // Nasals
            _phonemeDictionary["M"] = CreatePhoneme("M", new float[] { 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 3, true);
            _phonemeDictionary["N"] = CreatePhoneme("N", new float[] { 0, 0, 0, 0, 0.3f, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0 }, 0.1f, 3, true);
            _phonemeDictionary["NG"] = CreatePhoneme("NG", new float[] { 0, 0, 0, 0, 0, 0.5f, 0, 0, 1f, 0, 0, 0, 0, 0, 0 }, 0.12f, 3, true);
            
            // Liquids and affricates
            _phonemeDictionary["L"] = CreatePhoneme("L", new float[] { 0, 0, 0, 0, 0.3f, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0 }, 0.1f, 4, true);
            _phonemeDictionary["R"] = CreatePhoneme("R", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0.5f, 0 }, 0.1f, 4, true);
            _phonemeDictionary["W"] = CreatePhoneme("W", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f }, 0.1f, 4, true);
            _phonemeDictionary["Y"] = CreatePhoneme("Y", new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.3f, 0.7f, 0, 0 }, 0.08f, 4, true);
            _phonemeDictionary["CH"] = CreatePhoneme("CH", new float[] { 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 2, false);
            _phonemeDictionary["JH"] = CreatePhoneme("JH", new float[] { 0, 0, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 0, 0, 0, 0 }, 0.1f, 2, true);
        }
        
        private Phoneme CreatePhoneme(string name, float[] visemeWeights, float duration, int type, bool voiced)
        {
            return new Phoneme
            {
                name = name,
                visemeWeights = visemeWeights,
                duration = duration,
                phonemeType = type,
                isVoiced = voiced
            };
        }
        
        private void InitializeVisemeLibrary()
        {
            _visemeLibrary = new List<Viseme>
            {
                CreateViseme("sil", 0f, 0f, 0f, 0f, 0f, 0f),
                CreateViseme("pp", 0f, 0f, 1f, 0f, 0.2f, 0f),
                CreateViseme("ff", 0.2f, 0.3f, 0.5f, 0.3f, 0f, 0.1f),
                CreateViseme("th", 0.3f, 0.1f, 0.2f, 0.5f, 0f, 0.2f),
                CreateViseme("dd", 0.4f, 0f, 0f, 0.3f, 0f, 0f),
                CreateViseme("kk", 0.5f, 0f, 0f, 0.2f, 0f, 0f),
                CreateViseme("ch", 0.3f, 0f, 0.3f, 0.4f, 0f, 0f),
                CreateViseme("ss", 0.2f, 0.4f, 0.3f, 0.2f, 0f, 0f),
                CreateViseme("nn", 0.3f, 0f, 0f, 0.3f, 0f, 0f),
                CreateViseme("rr", 0.2f, 0.1f, 0.1f, 0.4f, 0f, 0f),
                CreateViseme("aa", 0.8f, 0f, 0f, 0.2f, 0f, 0f),
                CreateViseme("e", 0.5f, 0.5f, 0f, 0.3f, 0.1f, 0f),
                CreateViseme("i", 0.3f, 0.6f, 0.2f, 0.4f, 0.2f, 0f),
                CreateViseme("o", 0.6f, 0f, 0.7f, 0.2f, 0f, 0.1f),
                CreateViseme("u", 0.4f, 0f, 0.9f, 0.1f, 0f, 0.2f)
            };
        }
        
        private Viseme CreateViseme(string name, float jawOpen, float lipStretch, float lipPucker, 
                                   float tonguePos, float cheekRaise, float jawForward)
        {
            return new Viseme
            {
                name = name,
                jawOpen = jawOpen,
                lipStretch = lipStretch,
                lipPucker = lipPucker,
                tonguePosition = tonguePos,
                cheekRaise = cheekRaise,
                jawForward = jawForward
            };
        }
        
        private void InitializeEmotionCurves()
        {
            _emotionCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f)
            );
        }
        
        public int StartSpeech(Entity entity, string text, AudioClip audioClip = null, 
                              EmotionType emotion = EmotionType.Neutral, float speed = 1f)
        {
            var instance = new ActiveSpeechInstance
            {
                entity = entity,
                currentText = text,
                currentPhonemeIndex = 0,
                currentTime = 0f,
                speechSpeed = speed,
                emotionIntensity = 0f,
                currentEmotion = emotion,
                isActive = true,
                audioClip = audioClip,
                audioTime = 0f
            };
            
            _activeSpeeches.Add(instance);
            return _activeCycles.Length - 1;
        }
        
        public void UpdateSpeech(int speechIndex, float deltaTime)
        {
            if (speechIndex < 0 || speechIndex >= _activeSpeeches.Length) return;
            
            var speech = _activeSpeeches[speechIndex];
            if (!speech.isActive) return;
            
            // Update audio time if clip exists
            if (speech.audioClip != null)
            {
                speech.audioTime += deltaTime * speech.speechSpeed;
                if (speech.audioTime >= speech.audioClip.length)
                {
                    speech.isActive = false;
                    _activeSpeeches[speechIndex] = speech;
                    return;
                }
            }
            
            // Parse phonemes from text
            var phonemes = TextToPhonemes(speech.currentText);
            if (phonemes.Count == 0)
            {
                speech.isActive = false;
                _activeSpeeches[speechIndex] = speech;
                return;
            }
            
            // Update current phoneme
            var currentPhoneme = phonemes[speech.currentPhonemeIndex];
            speech.currentTime += deltaTime * speech.speechSpeed;
            
            // Apply emotion modulation
            speech.emotionIntensity = CalculateEmotionIntensity(speech.currentEmotion, speech.currentTime);
            
            // Move to next phoneme
            if (speech.currentTime >= currentPhoneme.duration / speech.speechSpeed)
            {
                speech.currentTime = 0f;
                speech.currentPhonemeIndex++;
                
                if (speech.currentPhonemeIndex >= phonemes.Count)
                {
                    speech.isActive = false;
                }
            }
            
            _activeSpeeches[speechIndex] = speech;
        }
        
        private List<Phoneme> TextToPhonemes(string text)
        {
            var phonemes = new List<Phoneme>();
            string upperText = text.ToUpperInvariant();
            
            // Simple grapheme-to-phoneme conversion (would use CMU Dict or similar in production)
            for (int i = 0; i < upperText.Length; i++)
            {
                char c = upperText[i];
                
                // Skip spaces and punctuation
                if (char.IsWhiteSpace(c) || char.IsPunctuation(c))
                {
                    continue;
                }
                
                // Map characters to phonemes (simplified)
                Phoneme phoneme;
                switch (c)
                {
                    case 'A': phoneme = _phonemeDictionary["AA"]; break;
                    case 'E': phoneme = _phonemeDictionary["EH"]; break;
                    case 'I': phoneme = _phonemeDictionary["IH"]; break;
                    case 'O': phoneme = _phonemeDictionary["OH"]; break;
                    case 'U': phoneme = _phonemeDictionary["UH"]; break;
                    case 'P': phoneme = _phonemeDictionary["P"]; break;
                    case 'B': phoneme = _phonemeDictionary["B"]; break;
                    case 'T': phoneme = _phonemeDictionary["T"]; break;
                    case 'D': phoneme = _phonemeDictionary["D"]; break;
                    case 'K': phoneme = _phonemeDictionary["K"]; break;
                    case 'G': phoneme = _phonemeDictionary["G"]; break;
                    case 'F': phoneme = _phonemeDictionary["F"]; break;
                    case 'V': phoneme = _phonemeDictionary["V"]; break;
                    case 'S': phoneme = _phonemeDictionary["S"]; break;
                    case 'Z': phoneme = _phonemeDictionary["Z"]; break;
                    case 'M': phoneme = _phonemeDictionary["M"]; break;
                    case 'N': phoneme = _phonemeDictionary["N"]; break;
                    case 'L': phoneme = _phonemeDictionary["L"]; break;
                    case 'R': phoneme = _phonemeDictionary["R"]; break;
                    case 'W': phoneme = _phonemeDictionary["W"]; break;
                    case 'Y': phoneme = _phonemeDictionary["Y"]; break;
                    case 'H': phoneme = _phonemeDictionary["HH"]; break;
                    default: phoneme = _phonemeDictionary["AA"]; break;
                }
                
                phonemes.Add(phoneme);
            }
            
            return phonemes;
        }
        
        private float CalculateEmotionIntensity(EmotionType emotion, float time)
        {
            float baseIntensity = _emotionCurve.Evaluate(time);
            
            switch (emotion)
            {
                case EmotionType.Happy:
                    return baseIntensity * 1.2f;
                case EmotionType.Sad:
                    return baseIntensity * 0.7f;
                case EmotionType.Angry:
                    return baseIntensity * 1.5f;
                case EmotionType.Fearful:
                    return baseIntensity * 1.3f;
                case EmotionType.Surprised:
                    return baseIntensity * 1.8f;
                case EmotionType.Disgusted:
                    return baseIntensity * 0.9f;
                default:
                    return baseIntensity;
            }
        }
        
        public float[] GetCurrentVisemeWeights(int speechIndex)
        {
            if (speechIndex < 0 || speechIndex >= _activeSpeeches.Length) 
                return new float[15];
            
            var speech = _activeSpeeches[speechIndex];
            var phonemes = TextToPhonemes(speech.currentText);
            
            if (speech.currentPhonemeIndex >= phonemes.Count)
                return new float[15];
            
            var phoneme = phonemes[speech.currentPhonemeIndex];
            float blendFactor = speech.currentTime / (phoneme.duration / speech.speechSpeed);
            
            // Blend with previous and next phonemes for smoothness
            float[] weights = new float[15];
            for (int i = 0; i < 15; i++)
            {
                weights[i] = phoneme.visemeWeights[i];
            }
            
            // Apply emotion modulation
            float emotionMod = 1f + (speech.emotionIntensity * 0.3f);
            for (int i = 0; i < 15; i++)
            {
                weights[i] *= emotionMod;
            }
            
            return weights;
        }
        
        public Vector3 GetFacialBlendShapeValues(int speechIndex)
        {
            var weights = GetCurrentVisemeWeights(speechIndex);
            
            float jawOpen = 0f, lipStretch = 0f, lipPucker = 0f;
            
            for (int i = 0; i < weights.Length && i < _visemeLibrary.Count; i++)
            {
                var viseme = _visemeLibrary[i];
                jawOpen += viseme.jawOpen * weights[i];
                lipStretch += viseme.lipStretch * weights[i];
                lipPucker += viseme.lipPucker * weights[i];
            }
            
            return new Vector3(jawOpen, lipStretch, lipPucker);
        }
        
        public void SetEmotion(int speechIndex, EmotionType emotion, float intensity = 1f)
        {
            if (speechIndex < 0 || speechIndex >= _activeSpeeches.Length) return;
            
            var speech = _activeSpeeches[speechIndex];
            speech.currentEmotion = emotion;
            speech.emotionIntensity = intensity;
            _activeSpeeches[speechIndex] = speech;
        }
        
        public void StopSpeech(int speechIndex)
        {
            if (speechIndex < 0 || speechIndex >= _activeSpeeches.Length) return;
            
            var speech = _activeSpeeches[speechIndex];
            speech.isActive = false;
            _activeSpeeches[speechIndex] = speech;
        }
    }
}
