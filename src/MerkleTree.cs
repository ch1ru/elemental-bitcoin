using System;
using System.Collections;
using System.Collections.Generic;

namespace LBitcoin {

    class Node {

        byte[] Hash_ = null;

        public byte[] Hash { get { return Hash_; } }

        public void set(byte[] value) {
            Hash_ = value;
        }
    }

    class MerkleTree : IEnumerable {

        int total_;
        int maxDepth_;
        Node[][] Nodes_;
        int currentDepth_;
        int currentIndex_;

        /// <summary>
        /// Merkle tree for inclusion proof.
        /// </summary>
        public MerkleTree(int total) {
            total_ = total;
            maxDepth_ = (int)Math.Ceiling(Math.Log(total, 2));
            Nodes_ = new Node[maxDepth_ + 1][];
            for (int i = 0; i < maxDepth_ + 1; i++) {
                int numOfItems = (int)Math.Ceiling(total_ / Math.Pow(2, maxDepth_ - i));
                Nodes_[i] = new Node[numOfItems];
                for (int j = 0; j < numOfItems; j++) {
                    Nodes_[i][j] = new Node();
                }
            }
            //set pointer to root
            currentDepth_ = 0;
            currentIndex_ = 0;
        }

        public Node[][] Nodes { get { return Nodes_; } }

        public IEnumerator<Node> GetEnumerator() {
            foreach (Node[] level in Nodes_) {
                foreach (Node node in level) {
                    yield return node;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public Node this[int level, int index] {
            get {
                try {
                    return Nodes_[level][index];
                }
                catch(Exception e) {
                    throw new Exception(e.Message);
                }
            }
        }

        public Node[] this[int index] {
            get {
                try {
                    return Nodes_[index];
                }
                catch(Exception e) {
                    throw new Exception(e.Message);
                }
            }
        }

        byte[] popHash(List<byte[]> hashes) {
            if (hashes.Count == 0) throw new Exception("List empty");
            byte[] returnVal = hashes[0];
            hashes.RemoveAt(0);
            return returnVal;
        }

        public void up() {
            currentDepth_--;
            currentIndex_ = currentIndex_ / 2;
        }

        public void left() {
            currentDepth_++;
            currentIndex_ = 2 * currentIndex_;
        }

        public void right() {
            currentDepth_++;
            currentIndex_ = 2 * currentIndex_ + 1;
        }

        public byte[] root() {
            return Nodes_[0][0].Hash;
        }

        public void setCurrentNode(byte[] value) {
            Nodes_[currentDepth_][currentIndex_].set(value);
        }

        public Node getCurrentNode() {
            return Nodes_[currentDepth_][currentIndex_];
        }

        public override string ToString() {
            return Byte.bytesToString(getCurrentNode().Hash) + "   ";
        }

        public Node getLeftNode() {
            return Nodes_[currentDepth_ + 1][currentIndex_ * 2];
        }

        public Node getRightNode() {
            return Nodes_[currentDepth_ + 1][currentIndex_ * 2 + 1];
        }

        public bool isLeaf() {
            return currentDepth_ == maxDepth_;
        }

        public bool rightExists() {
            return Nodes_[currentDepth_ + 1].Length > currentIndex_ * 2 + 1;
        }

        /// <summary>
        /// Fil a merkle tree with the transaction hashes provided and flags.
        /// </summary>
        public void populateTree(BitArray flagBits, List<byte[]> hashes) {
            while(root() == null) {
                if(isLeaf()) {
                    Helper.pop(ref flagBits);
                    setCurrentNode(popHash(hashes));
                    up();
                }
                else {
                    byte[] leftHash = getLeftNode().Hash;
                    if(leftHash == null) {
                        if(Helper.pop(ref flagBits) == false) {
                            setCurrentNode(popHash(hashes));
                            up();
                        }
                        else {
                            left();
                        }
                    }
                    else if(rightExists()) {
                        byte[] rightHash = getRightNode().Hash;
                        if(rightHash == null) {
                            right();
                        }
                        else {
                            setCurrentNode(Helper.merkleParent(leftHash, rightHash));
                            up();
                        }
                    }
                    else {
                        setCurrentNode(Helper.merkleParent(leftHash, leftHash));
                        up();
                    }
                }
            }
            if(hashes.Count > 0) {
                throw new Exception("Not all hashes consumed");
            }
            foreach(bool bit in flagBits) {
                if(bit == true) {
                    throw new Exception("Not all flag bits consumed");
                }
            }
        }
    }
}
