using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace LBitcoin {

    /// <summary>
    /// Bitcoin mnemonic phrase for human-readable backup of the master key.
    /// For more info see <see href="https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki">bip39</see>.
    /// </summary>
    public class Mnemonic : IEnumerable {

        string Mnemonic_;
        byte[] Entropy_;
        string[] Words_;
        Wordlist Wordlist_;
        public int[] Indices_;

        public static readonly int PBKDF2_ITERATOR = 2048;

        public string[] Words { get { return Words_; } }

        public IEnumerator<string> GetEnumerator() {
            foreach (string word in Words_) {
                yield return word;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public string this[int index] {
            get {
                if (index < 0 || index >= Words_.Length)
                    throw new IndexOutOfRangeException("Index out of range");

                return Words_[index];
            }
        }

        public override string ToString() {
            return Mnemonic_;
        }

        /// <summary>
        /// Constructor. Creates a mnemonic from a pre-defined set of words.
        /// </summary>
        /// <param name="mnemonic">Mnemonic phrase as string.</param>
        /// <param name="password">Mnemonic optional passphrase.</param>
        /// <param name="wordlist">Type of wordlist.</param>
        public Mnemonic(string mnemonic, string password = "", Wordlist wordlist = null) {
            if(mnemonic == null) {
                throw new Exception("Mnemonic is empty");
            }
            Mnemonic_ = mnemonic.Trim();

            
            var words = mnemonic.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (wordlist == null) {
                wordlist = DetectWordlist(words);
            }

            if (!CorrectWordCount(words.Length)) {
                throw new Exception("Number of words are insufficient, word count: " + words.Length);
            }

            if (!ValidateWordlist(words, wordlist)) {
                throw new Exception("Wordlist is invalid");
            }

            Wordlist_ = wordlist;
            Words_ = words;
            Indices_ = wordlist.ToIndices(words);
            Entropy_ = WordsToEntropy(words);
        }

        /// <summary>
        /// Constructor. Creates a mnemonic with pre-defined entropy.
        /// </summary>
        /// <param name="wordlist">Type of wordlist used.</param>
        /// <param name="entropy">Random entropy to seed the mnemonic.</param>
        /// <param name="password">Optional passphrase.</param>
        public Mnemonic(Wordlist wordlist, byte[] entropy, string password = "") {

            Wordlist_ = wordlist;
            Entropy_ = entropy;

            BitArray entropyBitsReversed = new BitArray(entropy);
            BitArray checksumBits = Checksum(entropy);
            BitArray entropyBits = Helper.reverseWordBits(entropyBitsReversed);

            entropyBits = Helper.join(entropyBits, checksumBits);
            int numOfWords = entropyBits.Length / 11;

            BitArray singleWord = new BitArray(11);
            string[] words = new string[numOfWords];

            for(int i = 0; i < numOfWords; i++) {
                for (int j = 0; j < 11; j++) { 
                    singleWord[j] = Helper.Pop(ref entropyBits);
                }
                words[i] = FetchWordByIndex(singleWord);
            }

            foreach (string word in words) Mnemonic_ += word + " ";
            Mnemonic_ = Mnemonic_.Trim();
            Words_ = words;
            Indices_ = wordlist.ToIndices(words);
        }

        /// <summary>
        /// Constructor. Create a new mnemonic phrase.
        /// </summary>
        /// <param name="wordlist">Type of wordlist used.</param>
        /// <param name="wordCount">Number of words (12/15/18/21/24).</param>
        /// <param name="password">Optional passphrase.</param>
        public Mnemonic(Wordlist wordlist, int wordCount = 12, string password = "") {

            Wordlist_ = wordlist;

            /*create entropy based on number of words*/
            BitArray entropy = CreateEntropy(wordCount);
            string[] words = new string[wordCount];

            for(int i = 0; i < wordCount; i++) {
                BitArray word = new BitArray(11);
                for(int j = 0; j < 11; j++) {
                    word[j] = Helper.Pop(ref entropy);
                }
                words[i] = FetchWordByIndex(word);
            }
            
            foreach (string word in words) Mnemonic_ += word + " ";
            Mnemonic_ = Mnemonic_.Trim();
            Words_ = words;
            Indices_ = wordlist.ToIndices(words);
        }

        /// <summary>
        /// Creates bytes of entropy depending on number of words, default is 12.
        /// </summary>
        BitArray CreateEntropy(int mnemonicLength = 12) {

            BitArray entropy = null;

            switch(mnemonicLength) {
                case 12:
                    Entropy_ = Csrng.RandomEntropy(16);
                    entropy = Helper.join(new BitArray(Entropy_), Checksum(Entropy_));
                    break;
                case 15:
                    Entropy_ = Csrng.RandomEntropy(20);
                    entropy = Helper.join(new BitArray(Entropy_), Checksum(Entropy_)); ;
                    break;
                case 18:
                    Entropy_ = Csrng.RandomEntropy(24);
                    entropy = Helper.join(new BitArray(Entropy_), Checksum(Entropy_));
                    break;
                case 21:
                    Entropy_ = Csrng.RandomEntropy(28);
                    entropy = Helper.join(new BitArray(Entropy_), Checksum(Entropy_));
                    break;
                case 24:
                    Entropy_ = Csrng.RandomEntropy(32);
                    entropy = Helper.join(new BitArray(Entropy_), Checksum(Entropy_));
                    break;
            }

            return entropy;
        }

        byte[] WordsToEntropy(string[] words) {
            BitArray b = null;
            foreach(string word in words) {
                int index = FetchIndexByWord(word);
                if(index < 0) {
                    throw new Exception("Word not found");
                }
                b = Helper.join(b, new BitArray(new int[] { index }));
            }

            /*remove checksum*/
            int len = (12 * 11) - 4;
            if (Words_.Length == 15) {
                len = (12 * 11) - 5;
            } else if (Words_.Length == 18) {
                len = (12 * 11) - 6;
            } else if (Words_.Length == 21) {
                len = (12 * 11) - 7;
            } else if (Words_.Length == 24) {
                len = (12 * 11) - 8;
            }


            bool[] bits = new bool[len];
            for(int i = 0; i < len; i++) {
                if(b[i]) {
                    bits[i] = true;
                }
            }
            
            return Helper.bitArrayToBytes(new BitArray(bits));
        }

        BitArray Checksum(byte[] data) {
            byte[] h256 = Hash.sha256(data);
            int numOfBits = 4;

            switch(data.Length) {
                case 16:
                    numOfBits = 4;
                    break;
                case 20:
                    numOfBits = 5;
                    break;
                case 24:
                    numOfBits = 6;
                    break;
                case 28:
                    numOfBits = 7;
                    break;
                case 32:
                    numOfBits = 8;
                    break;
            }

            BitArray hashBitsFull = Helper.reverseWordBits(new BitArray(h256));
            BitArray checksum = new BitArray(numOfBits);

            for(int i = 0; i < numOfBits; i++) {
                checksum[i] = hashBitsFull[i];
            }

            return checksum;
        }

        bool CorrectWordCount(int count) {
            if(count != 12 && count != 15 && count != 18 && count != 21 && count != 24) {
                return false;
            }
            return true;
        }

        string FetchWordByIndex(int index) {
            string[] bip39Words = Wordlist_.Words;
            return bip39Words[index];
        }

        string FetchWordByIndex(BitArray bitArray) {
            int index = Helper.getIntFromBitArray(bitArray);
            return FetchWordByIndex(index);
        }

        int FetchIndexByWord(string word) {
            for(int i = 0; i < Wordlist_.Words.Length; i++) {
                if(Wordlist_[i] == word) {
                    return i;
                }
            }
            return -1;
        }

        Wordlist DetectWordlist(string[] mnemonic) {
            /*if the wordlist is Chinese or Japanese, only 1 match is needed*/
            /*3 matches needed for other languages*/
            /*There is no way to distinguish between Chinese Simplified and Chinese Traditional, default
             * to simplified*/

            WordlistSource.CreateWordlist();
            
            int matches = 0;
            foreach (KeyValuePair<string, string> item in WordlistSource.wordlists_) {
                foreach(string word in mnemonic) {
                    if (item.Value.Contains(word)) {
                        matches++;

                        if (item.Key == "chinese_simplified" || item.Key == "japanese") {
                            return new Wordlist(item.Key);
                        } 
                        else if (matches >= 3) {
                            return new Wordlist(item.Key);
                        }
                    }
                }
                matches = 0; //reset match count for each wordlist
            }
            return new Wordlist();
        }

        bool ValidateWordlist(string[] words, Wordlist wList) {
            //really inefficient linear search!!
            foreach(string inputWord in words) {
                bool found = false;
                foreach(string bip39Word in wList.Words) {
                    if(inputWord == bip39Word) {
                        found = true;
                    }
                }
                if (!found) return false;
            }
            return true;
        }

        /// <summary>
        /// Performs PBKDF2 key stretching function.
        /// </summary>
        /// <param name="salt">A passphrase for the key.</param>
        /// <returns>The seed as byte array.</returns>
        public byte[] ToSeed(string passphrase = "") {
            passphrase = passphrase ?? "";
            byte[] salt = Encoding.UTF8.GetBytes("mnemonic".Normalize(NormalizationForm.FormKD) + passphrase.Normalize(NormalizationForm.FormKD));
            byte[] bytes = Encoding.UTF8.GetBytes(Mnemonic_.Normalize(NormalizationForm.FormKD));
            Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(bytes, salt, PBKDF2_ITERATOR, HashAlgorithmName.SHA512);
            return derive.GetBytes(64);
        }
    }
}
