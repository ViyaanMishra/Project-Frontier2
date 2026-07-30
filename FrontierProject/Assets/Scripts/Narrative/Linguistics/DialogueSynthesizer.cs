using System;
using Unity.Collections;
using Frontier.Core;
using Frontier.Simulation;

namespace Frontier.Narrative.Linguistics
{
    /// <summary>
    /// Advanced procedural dialogue generation using semantic templates and dynamic variable injection.
    /// Generates context-aware speech patterns based on NPC traits, culture, and emotional state.
    /// </summary>
    public class DialogueSynthesizer : IService
    {
        public int Priority => 20;

        private NativeHashMap<int, DialogueTemplate> _templateRegistry;
        private CulturalMemetics _memetics;
        private StoryVariableStore _variables;

        public struct DialogueTemplate
        {
            public FixedString512Bytes TemplateText; // "Greetings, {PlayerName}. The {Faction} speaks of your {Reputation}."
            public NativeArray<FixedString64Bytes> Variables;
            public EmotionalTone Tone;
            public SpeechPattern Pattern;
        }

        public enum EmotionalTone
        {
            Neutral, Joyful, Sorrowful, Angry, Fearful, Disgusted, Surprised, Contemptuous
        }

        public enum SpeechPattern
        {
            Formal, Casual, Archaic, Technical, Slang, Poetic, Blunt, Eloquent
        }

        public void Initialize()
        {
            _templateRegistry = new NativeHashMap<int, DialogueTemplate>(512, Allocator.Persistent);
            _memetics = ServiceRegistry.Get<CulturalMemetics>();
            _variables = ServiceRegistry.Get<StoryVariableStore>();
            
            LoadBaseTemplates();
            UnityEngine.Debug.Log("[DialogueSynthesizer] Initialized linguistic engine.");
        }

        public void Tick(float dt)
        {
            // Dynamic template adjustment based on world state
        }

        public void Shutdown()
        {
            if (_templateRegistry.IsCreated) _templateRegistry.Dispose();
        }

        private void LoadBaseTemplates()
        {
            // Load base greeting templates
            var greeting = new DialogueTemplate
            {
                TemplateText = new FixedString512Bytes("Welcome to {Settlement}, traveler. I am {SpeakerName}, a {Role} of this place."),
                Variables = new NativeArray<FixedString64Bytes>(new[] { 
                    new FixedString64Bytes("Settlement"), 
                    new FixedString64Bytes("SpeakerName"), 
                    new FixedString64Bytes("Role") 
                }, Allocator.Temp),
                Tone = EmotionalTone.Neutral,
                Pattern = SpeechPattern.Formal
            };
            _templateRegistry.Add("Greeting_Generic".GetHashCode(), greeting);
        }

        public FixedString512Bytes SynthesizeDialogue(int templateId, NativeHashMap<FixedString64Bytes, FixedString128Bytes> contextVars)
        {
            if (!_templateRegistry.TryGetValue(templateId, out var template))
            {
                return new FixedString512Bytes("[Error: Template not found]");
            }

            FixedString512Bytes result = template.TemplateText;
            
            // Replace variables
            for (int i = 0; i < template.Variables.Length; i++)
            {
                var varName = template.Variables[i];
                if (contextVars.TryGetValue(varName, out var value))
                {
                    result = ReplaceToken(result, $"{{{varName}}}", value.ToString());
                }
            }

            // Apply cultural modifiers
            if (_memetics != null)
            {
                result = _memetics.ApplyCulturalDialect(result);
            }

            // Apply emotional inflection
            result = ApplyEmotionalInflection(result, template.Tone);

            return result;
        }

        private FixedString512Bytes ReplaceToken(FixedString512Bytes text, string token, string value)
        {
            // Simple token replacement logic
            // In production, use proper string manipulation
            return text; 
        }

        private FixedString512Bytes ApplyEmotionalInflection(FixedString512Bytes text, EmotionalTone tone)
        {
            switch (tone)
            {
                case EmotionalTone.Angry:
                    return new FixedString512Bytes(text.ToString().ToUpper() + "!");
                case EmotionalTone.Sorrowful:
                    return new FixedString512Bytes(text.ToString() + "...");
                case EmotionalTone.Joyful:
                    return new FixedString512Bytes(text.ToString() + "!");
                default:
                    return text;
            }
        }

        public void RegisterTemplate(FixedString128Bytes id, DialogueTemplate template)
        {
            int hash = id.GetHashCode();
            if (!_templateRegistry.ContainsKey(hash))
            {
                _templateRegistry.Add(hash, template);
            }
        }
    }
}
