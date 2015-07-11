using System;

namespace DotBloom
{
    /// <summary>
    /// Bloom filter
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    class BloomFilter<T>
    {
        private readonly byte[] _byteVector;
        private readonly uint _k;
        private HashFunction _secondHash;

        public delegate int HashFunction(T input);

        /// <summary>
        /// Create a Bloom Filter
        /// </summary>
        /// <param name="capacity">Size for the BloomFilter</param>
        /// <param name="k">Number of Hashing iterations</param>
        public BloomFilter(uint capacity, uint k, HashFunction hashFunction)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException("capacity", capacity, "capacity value must be superior to zero");
            }

            if (k < 1)
            {
                throw new ArgumentOutOfRangeException("k", k, "k value must be superior to zero");
            }

            if (hashFunction == null)
            {
                ResolveSecondHash();
            }
            else
            {
                _secondHash = hashFunction;
            }

            _k = k;
            _byteVector = new byte[capacity];
        }
        private void ResolveSecondHash()
        {
            if (typeof(T) == typeof(string))
            {
                _secondHash = HashString;
            }
            else if (typeof(T) == typeof(int))
            {
                _secondHash = HashInt32;
            }
            else
            {
                //TODO : Do some research to give specifics hash funcs for all primitive types or at least propose Murmur3 Hash
                throw new ArgumentNullException("hashFunction", "Please provide a hash function for your type T, when T is not a string or int.");
            }
        }

        public void Add(T item)
        {
            int primaryHash = item.GetHashCode();
            int secondaryHash = _secondHash(item);
            for (int i = 0; i < _k; i++)
            {
                int hash = this.ComputeHash(primaryHash, secondaryHash, i);
                _byteVector[hash] = 1;
            }
        }

        public bool Contains(T item)
        {
            int primaryHash = item.GetHashCode();
            int secondaryHash = _secondHash(item);
            for (int i = 0; i < _k; i++)
            {
                int hash = this.ComputeHash(primaryHash, secondaryHash, i);
                if (_byteVector[hash] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private int ComputeHash(int primaryHash, int secondaryHash, int i)
        {
            return Math.Abs((primaryHash + (i * secondaryHash)) % _byteVector.Length);
        }

        private static int HashInt32(T input)
        {
            uint? x = input as uint?;
            unchecked
            {
                x = ~x + (x << 15);
                x = x ^ (x >> 12);
                x = x + (x << 2);
                x = x ^ (x >> 4);
                x = x * 2057;
                x = x ^ (x >> 16);
                return (int)x;
            }
        }

        private static int HashString(T input)
        {
            string s = input as string;
            int hash = 0;

            for (int i = 0; i < s.Length; i++)
            {
                hash += s[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }
    }
}
