using System;
using System.Collections.Generic;
using System.Collections;

namespace LBitcoin {
    class Wordlist : IEnumerable {

        string[] bip39Words_;

        public string[] Words { get { return bip39Words_; } }

        public Wordlist(string language = null) {
            if(language == null) {
                language = "english";
            }
            language.ToLower();
            bip39Words_ = WordlistSource.getWordlist(language);
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

        public int[] toIndices(string[] words) {
            int[] indices = new int[words.Length];
            for(int i = 0; i < words.Length; i++) {
                indices[i] = findWordIndex(words[i], bip39Words_);
            }
            return indices;
        }

        int findWordIndex(string itemToFind, string[] collection) {
            for(int i = 0; i < collection.Length; i++) {
                if(collection[i] == itemToFind) {
                    return i;
                }
            }
            return -1;
        }
    }
}
