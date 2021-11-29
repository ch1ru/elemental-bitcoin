using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LBitcoin {
    class Mnemonic : IEnumerable {

        string Mnemonic_;
        byte[] Entropy_;
        string[] Words_;
        Wordlist Wordlist_;
        public int[] Indices_;
        byte[] Seed_;

        public string[] Words { get { return Words_; } }

        public byte[] Seed { get { return Seed_; } }

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

        /*Create a mnemonic from a pre-defined set of words*/
        public Mnemonic(string mnemonic, string password = "", Wordlist wordlist = null) {
            if(mnemonic == null) {
                throw new Exception("Mnemonic is empty");
            }
            Mnemonic_ = mnemonic.Trim();

            if(wordlist == null) {
                //auto detect or default to english
            }
            var words = mnemonic.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            
            if(!correctWordCount(words.Length)) {
                throw new Exception("Number of words are insufficient");
            }

            Wordlist_ = wordlist;
            Words_ = words;
            Indices_ = wordlist.toIndices(words);
            //string salt = "mnemonic" + password;
            string salt = password;
            Seed_ = this.toSeed(salt);
        }

        /*Create a mnemonic with pre-defined entropy*/
        public Mnemonic(Wordlist wordlist, byte[] entropy, string password = "") {

            Wordlist_ = wordlist;
            Entropy_ = entropy;

            BitArray entropyBitsReversed = new BitArray(entropy);
            BitArray checksumBits = checksum(entropy);
            BitArray entropyBits = Helper.reverseWordBits(entropyBitsReversed);

            entropyBits = Helper.join(entropyBits, checksumBits);
            int numOfWords = entropyBits.Length / 11;

            BitArray singleWord = new BitArray(11);
            string[] words = new string[numOfWords];

            for(int i = 0; i < numOfWords; i++) {
                for (int j = 0; j < 11; j++) { 
                    singleWord[j] = Helper.pop(ref entropyBits);
                }
                words[i] = fetchWordByIndex(singleWord);
            }

            foreach (string word in words) Mnemonic_ += word + " ";
            Mnemonic_.Trim();
            Words_ = words;
            Indices_ = wordlist.toIndices(words);
            string salt = password; //after demo, change to "mnemonic + password"
            Seed_ = this.toSeed(salt);
        }

        /*Create a new mnemonic phrase*/
        public Mnemonic(Wordlist wordlist, int wordCount = 12, string password = "") {

            Wordlist_ = wordlist;

            /*create entropy based on number of words*/
            BitArray entropy = createEntropy(wordCount);
            string[] words = new string[wordCount];

            for(int i = 0; i < wordCount; i++) {
                BitArray word = new BitArray(11);
                for(int j = 0; j < 11; j++) {
                    word[j] = Helper.pop(ref entropy);
                }
                words[i] = fetchWordByIndex(word);
            }
            
            foreach (string word in words) Mnemonic_ += word + " ";
            Mnemonic_.Trim();
            Words_ = words;
            Indices_ = wordlist.toIndices(words);
            string salt = "mnemonic" + password;
            Seed_ = this.toSeed(salt);
        }

        /*Creates bytes of entropy depending on number of words, default is 12*/
        BitArray createEntropy(int mnemonicLength = 12) {

            BitArray entropy = null;

            switch(mnemonicLength) {
                case 12:
                    Entropy_ = csrng.randomEntropy(16);
                    entropy = Helper.join(new BitArray(Entropy_), checksum(Entropy_));
                    break;
                case 15:
                    Entropy_ = csrng.randomEntropy(20);
                    entropy = Helper.join(new BitArray(Entropy_), checksum(Entropy_)); ;
                    break;
                case 18:
                    Entropy_ = csrng.randomEntropy(24);
                    entropy = Helper.join(new BitArray(Entropy_), checksum(Entropy_));
                    break;
                case 21:
                    Entropy_ = csrng.randomEntropy(28);
                    entropy = Helper.join(new BitArray(Entropy_), checksum(Entropy_));
                    break;
                case 24:
                    Entropy_ = csrng.randomEntropy(32);
                    entropy = Helper.join(new BitArray(Entropy_), checksum(Entropy_));
                    break;
            }

            return entropy;
        }

        BitArray checksum(byte[] data) {
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

        bool correctWordCount(int count) {
            if(count != 12 || count != 15 || count != 18 || count != 21 || count != 24) {
                return false;
            }
            return true;
        }

        string fetchWordByIndex(int index) {
            string[] bip39Words = Wordlist_.Words;
            return bip39Words[index];
        }

        string fetchWordByIndex(BitArray bitArray) {
            int index = Helper.getIntFromBitArray(bitArray);
            return fetchWordByIndex(index);
        }

        byte[] toSeed(string salt = "Bitcoin seed") { //CHANGE
            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            return Hash.HMACSHA512Encode(Entropy_, saltBytes);
        }
    }
}
