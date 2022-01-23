using System;
using System.Collections;
using System.Collections.Generic;

namespace LBitcoin {

    public class Node {

        byte[] Hash_ = null;

        public byte[] Hash { get { return Hash_; } }

        public void Set(byte[] value) {
            Hash_ = value;
        }
    }

    public class MerkleTree : IEnumerable {

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

        byte[] PopHash(List<byte[]> hashes) {
            if (hashes.Count == 0) throw new Exception("List empty");
            byte[] returnVal = hashes[0];
            hashes.RemoveAt(0);
            return returnVal;
        }

        public void Up() {
            currentDepth_--;
            currentIndex_ = currentIndex_ / 2;
        }

        public void Left() {
            currentDepth_++;
            currentIndex_ = 2 * currentIndex_;
        }

        public void Right() {
            currentDepth_++;
            currentIndex_ = 2 * currentIndex_ + 1;
        }

        public byte[] Root() {
            return Nodes_[0][0].Hash;
        }

        public void SetCurrentNode(byte[] value) {
            Nodes_[currentDepth_][currentIndex_].Set(value);
        }

        public Node GetCurrentNode() {
            return Nodes_[currentDepth_][currentIndex_];
        }

        public override string ToString() {
            return Byte.bytesToString(GetCurrentNode().Hash) + "   ";
        }

        public Node GetLeftNode() {
            return Nodes_[currentDepth_ + 1][currentIndex_ * 2];
        }

        public Node GetRightNode() {
            return Nodes_[currentDepth_ + 1][currentIndex_ * 2 + 1];
        }

        public bool IsLeaf() {
            return currentDepth_ == maxDepth_;
        }

        public bool RightExists() {
            return Nodes_[currentDepth_ + 1].Length > currentIndex_ * 2 + 1;
        }

        /// <summary>
        /// Fil a merkle tree with the transaction hashes provided and flags.
        /// </summary>
        public void PopulateTree(BitArray flagBits, List<byte[]> hashes) {
            while(Root() == null) {
                if(IsLeaf()) {
                    Helper.Pop(ref flagBits);
                    SetCurrentNode(PopHash(hashes));
                    Up();
                }
                else {
                    byte[] leftHash = GetLeftNode().Hash;
                    if(leftHash == null) {
                        if(Helper.Pop(ref flagBits) == false) {
                            SetCurrentNode(PopHash(hashes));
                            Up();
                        }
                        else {
                            Left();
                        }
                    }
                    else if(RightExists()) {
                        byte[] rightHash = GetRightNode().Hash;
                        if(rightHash == null) {
                            Right();
                        }
                        else {
                            SetCurrentNode(Helper.MerkleParent(leftHash, rightHash));
                            Up();
                        }
                    }
                    else {
                        SetCurrentNode(Helper.MerkleParent(leftHash, leftHash));
                        Up();
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
