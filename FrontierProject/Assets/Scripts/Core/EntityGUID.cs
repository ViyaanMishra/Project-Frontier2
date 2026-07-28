using System;
using System.Runtime.CompilerServices;

namespace Frontier.Core
{
    /// <summary>
    /// 128-bit stable GUID struct with versioning support.
    /// Provides deterministic entity identification across saves and sessions.
    /// </summary>
    public readonly struct EntityGUID : IEquatable<EntityGUID>
    {
        // 128 bits split into four 32-bit components for efficient comparison
        private readonly uint _partA;
        private readonly uint _partB;
        private readonly uint _partC;
        private readonly uint _partD;
        
        // Version byte for migration support
        private readonly byte _version;
        
        // Reserved flags (4 bits): transient, dirty, reserved1, reserved2
        private readonly byte _flags;

        public byte Version => _version;
        public bool IsTransient => (_flags & 0x01) != 0;
        public bool IsDirty => (_flags & 0x02) != 0;

        // Special GUIDs
        public static readonly EntityGUID Empty = new EntityGUID(0, 0, 0, 0);
        public static readonly EntityGUID Invalid = new EntityGUID(uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue);

        public EntityGUID(uint a, uint b, uint c, uint d, byte version = 1, byte flags = 0)
        {
            _partA = a;
            _partB = b;
            _partC = c;
            _partD = d;
            _version = version;
            _flags = flags;
        }

        public EntityGUID(byte[] data, int offset = 0)
        {
            if (data == null || data.Length < offset + 18)
            {
                this = Empty;
                return;
            }

            _partA = BitConverter.ToUInt32(data, offset);
            _partB = BitConverter.ToUInt32(data, offset + 4);
            _partC = BitConverter.ToUInt32(data, offset + 8);
            _partD = BitConverter.ToUInt32(data, offset + 12);
            _version = data[offset + 16];
            _flags = data[offset + 17];
        }

        /// <summary>
        /// Generate a new random GUID (non-deterministic).
        /// Use only for runtime entities that don't need save persistence.
        /// </summary>
        public static EntityGUID NewRandom(byte version = 1)
        {
            var random = new Random();
            return new EntityGUID(
                (uint)random.Next(),
                (uint)random.Next(),
                (uint)random.Next(),
                (uint)random.Next(),
                version
            );
        }

        /// <summary>
        /// Generate a deterministic GUID from seed values.
        /// Essential for procedural generation consistency.
        /// </summary>
        public static EntityGUID FromSeed(long seed, ushort entityType, ushort instanceId, byte version = 1)
        {
            unchecked
            {
                uint a = (uint)(seed >> 32) ^ (uint)entityType;
                uint b = (uint)seed ^ (uint)instanceId;
                uint c = (uint)((seed * 0x9E3779B9) >> 32);
                uint d = (uint)((seed * 0x7F4A7C15) >> 32);
                return new EntityGUID(a, b, c, d, version);
            }
        }

        /// <summary>
        /// Create a child GUID from parent (for hierarchical entities).
        /// </summary>
        public EntityGUID CreateChild(ushort childIndex, byte version = 1)
        {
            return new EntityGUID(
                _partA,
                _partB,
                _partC ^ (uint)childIndex,
                _partD ^ 0xC411DC45,
                version
            );
        }

        /// <summary>
        /// Serialize to byte array (18 bytes: 16 for GUID + 1 version + 1 flags).
        /// </summary>
        public void WriteToBytes(byte[] buffer, int offset = 0)
        {
            if (buffer == null || buffer.Length < offset + 18)
                throw new ArgumentException("Buffer too small");

            BitConverter.GetBytes(_partA).CopyTo(buffer, offset);
            BitConverter.GetBytes(_partB).CopyTo(buffer, offset + 4);
            BitConverter.GetBytes(_partC).CopyTo(buffer, offset + 8);
            BitConverter.GetBytes(_partD).CopyTo(buffer, offset + 12);
            buffer[offset + 16] = _version;
            buffer[offset + 17] = _flags;
        }

        /// <summary>
        /// Mark as transient (not saved).
        /// </summary>
        public EntityGUID WithTransientFlag()
        {
            return new EntityGUID(_partA, _partB, _partC, _partD, _version, (byte)(_flags | 0x01));
        }

        /// <summary>
        /// Mark as dirty (needs serialization).
        /// </summary>
        public EntityGUID WithDirtyFlag()
        {
            return new EntityGUID(_partA, _partB, _partC, _partD, _version, (byte)(_flags | 0x02));
        }

        public bool Equals(EntityGUID other)
        {
            return _partA == other._partA &&
                   _partB == other._partB &&
                   _partC == other._partC &&
                   _partD == other._partD &&
                   _version == other._version;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityGUID other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _partA.GetHashCode();
                hash = hash * 31 + _partB.GetHashCode();
                hash = hash * 31 + _partC.GetHashCode();
                hash = hash * 31 + _partD.GetHashCode();
                hash = hash * 31 + _version.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(EntityGUID left, EntityGUID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityGUID left, EntityGUID right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{_partA:X8}-{_partB:X4}-{_partC:X4}-{_partD:X4}:v{_version}";
        }

        /// <summary>
        /// Fast comparison without version check (for spatial queries).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FastEquals(in EntityGUID other)
        {
            return _partA == other._partA &&
                   _partB == other._partB &&
                   _partC == other._partC &&
                   _partD == other._partD;
        }
    }

    /// <summary>
    /// GUID generator with sequential ID allocation for deterministic ordering.
    /// </summary>
    public class GUIDGenerator
    {
        private ulong _sequenceCounter;
        private readonly long _baseSeed;
        private readonly byte _version;

        public GUIDGenerator(long baseSeed, byte version = 1)
        {
            _baseSeed = baseSeed;
            _version = version;
            _sequenceCounter = 0;
        }

        public EntityGUID Next(ushort entityType)
        {
            unchecked
            {
                uint seqLow = (uint)_sequenceCounter;
                
                var guid = EntityGUID.FromSeed(_baseSeed, entityType, (ushort)seqLow, _version);
                _sequenceCounter++;
                return guid;
            }
        }

        public void Reset(ulong counter = 0)
        {
            _sequenceCounter = counter;
        }
    }
}
