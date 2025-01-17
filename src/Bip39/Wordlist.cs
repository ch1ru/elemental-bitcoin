﻿using System;
using System.Collections.Generic;
using System.Collections;

namespace LBitcoin {

    /// <summary>
    /// The worlist is a pool of 2048 words to be chosen by random entropy.
    /// May be in English, French, Spanish, Chinese (traditional and simplified), Japanese, Portugese and Czech
    /// </summary>
    public class Wordlist : IEnumerable {

        string[] bip39Words_;

        public string[] Words { get { return bip39Words_; } }

        /// <summary>
        /// Constructor. Selects a wordlist based on language.
        /// </summary>
        public Wordlist(string language = null) {
            if(language == null) {
                language = "english";
            }
            language.ToLower();
            bip39Words_ = WordlistSource.GetWordlist(language);
        }

        public IEnumerator<string> GetEnumerator() {
            foreach (string word in bip39Words_) {
                yield return word;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public string this[int index] {
            get {
                if (index < 0 || index >= 2048)
                    throw new IndexOutOfRangeException("Index out of range");

                return bip39Words_[index];
            }
        }

        public int[] ToIndices(string[] words) {
            int[] indices = new int[words.Length];
            for(int i = 0; i < words.Length; i++) {
                indices[i] = FindWordIndex(words[i], bip39Words_);
            }
            return indices;
        }

        int FindWordIndex(string itemToFind, string[] collection) {
            for(int i = 0; i < collection.Length; i++) {
                if(collection[i] == itemToFind) {
                    return i;
                }
            }
            return -1;
        }
    }
}
