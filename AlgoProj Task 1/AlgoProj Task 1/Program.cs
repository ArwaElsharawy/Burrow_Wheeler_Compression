using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Project
{

    internal class Program
    {
        public class PriorityQueue<T> // O(log C)
        {
            private (int, T)[] heap;
            private int count;

            public PriorityQueue(int capacity)
            {
                heap = new (int, T)[capacity];
                count = 0;
            }

            public int Count => count;

            private int Parent(int i)
            {
                return (i - 1) / 2;
            }

            private int LeftChild(int i)
            {
                return 2 * i + 1;
            }

            private int RightChild(int i)
            {
                return 2 * i + 2;
            }

            private void Swap(int i, int j) //O(1)
            {  //var 
                (int, T) temp = heap[i];
                heap[i] = heap[j];
                heap[j] = temp;
            }

            private void SiftUp(int i) //O(log C)
            {
                while (i > 0 && heap[i].Item1 < heap[Parent(i)].Item1)
                {
                    Swap(i, Parent(i)); //o(1)
                    i = Parent(i);
                }
            }

            private void SiftDown(int i) //O(log(c))
            {
                int minIndex = i;
                int left = LeftChild(i); //o(1) 
                if (left < count && heap[left].Item1 < heap[minIndex].Item1) //freq 
                    minIndex = left;
                int right = RightChild(i);
                if (right < count && heap[right].Item1 < heap[minIndex].Item1)
                    minIndex = right;
                if (i != minIndex)
                {
                    Swap(i, minIndex);
                    SiftDown(minIndex);
                }
            }

            public void Enqueue(int priority, T item) // O(log C) due to SiftUp
            {
                heap[count] = (priority, item);
                SiftUp(count); //O(log c)
                count++;
            }

            public (int, T) Dequeue() //O(log c )because SiftDown
            {
                var minItem = heap[0];
                heap[0] = heap[count - 1];
                count--;
                SiftDown(0);
                return minItem;
            }
        }

        public static Dictionary<int, int> freq = new Dictionary<int, int>();
        public static Dictionary<int, List<int>> codes = new Dictionary<int, List<int>>();

        public class Node
        {
            public Node left, right, parent;
            public int freq;
            public int symbol;
        }

        public static void CalculateFrequencies(List<int> encrypted) //O(N)
        {
            ////Console.WriteLine("time of calc freq");
            foreach (int i in encrypted) //O(N)
            {
                if (freq.ContainsKey(i)) //O(1)
                {
                    freq[i]++;
                }
                else
                {
                    freq[i] = 1;
                }
            }
        }
        public static Node BuildTree(Dictionary<int, int> freq) //O(C log C)
        {
            PriorityQueue<Node> priorityQueue = new PriorityQueue<Node>(freq.Count);

            // Enqueue all nodes with their frequencies
            foreach (KeyValuePair<int, int> nodeinfreq in freq)   //O(C)
            {
                Node node = new Node();
                node.symbol = nodeinfreq.Key;
                node.freq = nodeinfreq.Value;
                priorityQueue.Enqueue(node.freq, node);  //enqueue only O(log c)
            }

            while (priorityQueue.Count > 1)  //O(C log C)
            {
                //O(3 log c)
                Node X = priorityQueue.Dequeue().Item2;
                Node Y = priorityQueue.Dequeue().Item2;
                Node z = new Node();
                z.left = X;
                z.right = Y;
                z.freq = X.freq + Y.freq;
                priorityQueue.Enqueue(z.freq, z);
            }

            // Return the root of the Huffman tree
            return priorityQueue.Dequeue().Item2; //o(logc)
        }

        public static void GeneratePath(Node node, List<int> code) //O(C)
        {

            if (node == null)
                return;

            if (node.left == null && node.right == null)
            {
                codes[node.symbol] = new List<int>(code);
                //codes the dic that contains the symbol and it's path
                //code the path (0's and 1's ) to reach this node 
                return;
            }
            //0 to the code list when traversing the left child and a 1 when traversing the right child.

            code.Add(0);
            GeneratePath(node.left, code);
            code.RemoveAt(code.Count - 1);

            code.Add(1);
            GeneratePath(node.right, code);
            // because i append before recurse , what if it's a leaf node ? there will be one bit extra so remove it 
            code.RemoveAt(code.Count - 1);


        }

        static List<int> Compress(List<int> data) //O(N)
        {
            ////Console.WriteLine("time of huffman com");
            Stopwatch swm = new Stopwatch();
            swm.Start();
            List<int> compressed = new List<int>();
            foreach (int symbol in data)
            {
                // foreach symbol return path (0's , 1's) 
                //codes the dic that contains the symbol and it's path

                compressed.AddRange(codes[symbol]);
            }

            swm.Stop();
            //////Console.WriteLine(swm.ElapsedMilliseconds);
            return compressed;
        }

        static List<int> Decompress(string compressed, Node root)
        // worst O(log C* m )  m :size of compressed file 
        {
            List<int> decompressed = new List<int>();
            Node current = root;
            foreach (int bit in compressed) //O(m)
            {
                if (bit == '0')
                {
                    current = current.left;
                }
                else if (bit == '1')
                {
                    current = current.right;
                }

                if (current.left == null && current.right == null)
                {
                    decompressed.Add(current.symbol);
                    current = root;
                }
            }
            return decompressed;
        }

        //----------------------------------------------------------------------------------------------------------------------------------


        static Dictionary<int, int> sd = new Dictionary<int, int>();
        static (char[], int[]) BWT(string path)  //nlog(n)
        {
            string text = File.ReadAllText(path); //o(n)
            int[] startindices = new int[text.Length];
            int simi = text.Length - 1;
            char firstChar = ' ';
            for (int i = 0; i < text.Length; i++)   //O(N)
            {
                startindices[i] = i;
                sd.Add(i, i); //o(1)
                if (i == 0)
                {
                    firstChar = text[0];
                    continue;
                }
                if (text[i] == firstChar)
                {
                    // If any character is different from the first one, the text is not identical
                    simi--;

                }

            }
            if (simi == 0)
            {
                ////Console.WriteLine("identiacl");

                return (text.ToArray(), startindices);  //o(n)
            }
            else
            {
                char[] copyText = text.ToCharArray(); //o(n)
                merge_sort(copyText, startindices, 0, text.Length - 1); //nlog(n)

                char[] lastColumn = new char[copyText.Length];
                int globalIdx = sd[0];

                // 3 loops O(N) 
                for (int i = 0; i < globalIdx; i++)
                {
                    lastColumn[i] = (copyText[startindices[i] - 1]);
                }
                lastColumn[globalIdx] = (copyText[copyText.Length - 1]);
                for (int i = globalIdx + 1; i < text.Length; i++)
                {
                    lastColumn[i] = (copyText[startindices[i] - 1]);
                }
                return (lastColumn, startindices);
            }
        }

        static void merge_sort(char[] copyText, int[] sindices, int start, int end) //O(Nlog(N)) ,worst O(N^2)
        {
            if (start < end)
            {
                int mid = (start + end) / 2;
                merge_sort(copyText, sindices, start, mid);
                merge_sort(copyText, sindices, mid + 1, end);
                merge(copyText, sindices, start, mid, end);  //O(N)   , worst O(N^2)
            }
        }
        static void merge(char[] copyText, int[] indices, int start, int mid, int end) //O(N)
        {
            int[] tempArray = new int[end - start + 1];
            int leftIndex = start;
            int rightIndex = mid + 1;
            int tempIndex = 0;
            int a = 0;
            while (leftIndex <= mid && rightIndex <= end) //o(1/2N)
            {
                int i = indices[leftIndex];
                int j = indices[rightIndex];

                int k = 0;
                while (k < copyText.Length)//o(N) 
                {
                    if (copyText[(i + k) % copyText.Length] == copyText[(j + k) % copyText.Length]) //O(N)
                    {
                        k++;
                    }
                    else if (copyText[(i + k) % copyText.Length] < copyText[(j + k) % copyText.Length])// if this case done o(1)
                    {
                        tempArray[tempIndex] = i;
                        sd[tempArray[tempIndex]] = a;
                        a++;
                        tempIndex++;
                        leftIndex++;


                        break;
                    }
                    else// if this case done o(1)
                    {
                        tempArray[tempIndex] = j;
                        sd[tempArray[tempIndex]] = a;
                        a++;
                        rightIndex++;
                        tempIndex++;
                        break;
                    }
                }
            }

            while (leftIndex <= mid) //(1/2n)
            {
                tempArray[tempIndex] = indices[leftIndex];
                sd[tempArray[tempIndex]] = a;
                a++;
                tempIndex++;
                leftIndex++;
            }

            while (rightIndex <= end) //(1/2n)
            {
                tempArray[tempIndex] = indices[rightIndex];
                sd[tempArray[tempIndex]] = a;
                a++;
                tempIndex++;
                rightIndex++;
            }

            for (int i = 0; i < tempIndex; i++) //O(N)
            {
                indices[start + i] = tempArray[i];
            }
        }

        static List<int> createNext(List<int> indices, Dictionary<int, int> dic)
        {
            List<int> next = new List<int>();
            for (int i = 0; i < indices.Count; i++)
            {
                int value = indices[i];
                if ((value + 1) % indices.Count == 0)
                {
                    value = 0;
                }
                else
                {
                    value += 1;
                }
                next.Add(sd[value]);
            }
            return next;
        }
        static char[] merge_sortT(char[] arr) //nlog(n)
        {
            if (arr.Length <= 1)
            {
                return arr;
            }
            int mid = arr.Length / 2;
            char[] leftArray = new char[mid];
            char[] rightArray = new char[arr.Length - mid];

            Array.Copy(arr, 0, leftArray, 0, mid); //(1/2N)
            Array.Copy(arr, mid, rightArray, 0, arr.Length - mid);//(1/2N)

            char[] leftResult = merge_sortT(leftArray);
            char[] rightResult = merge_sortT(rightArray);
            char[] sortedArray = mergeT(leftResult, rightResult); //o(n)

            return sortedArray;
        }

        static char[] mergeT(char[] leftArray, char[] rightArray) //o(n)
        {
            char[] sortedArray = new char[leftArray.Length + rightArray.Length];
            int ileft = 0, iright = 0, index = 0;
            while (ileft < leftArray.Length && iright < rightArray.Length)
            {
                if (leftArray[ileft] <= rightArray[iright])
                {
                    sortedArray[index] = leftArray[ileft];
                    ileft++;
                }
                else
                {
                    sortedArray[index] = rightArray[iright];
                    iright++;
                }
                index++;
            }
            while (ileft < leftArray.Length)
            {
                sortedArray[index] = leftArray[ileft];
                ileft++;
                index++;
            }
            while (iright < rightArray.Length)
            {
                sortedArray[index] = rightArray[iright];
                iright++;
                index++;
            }

            return sortedArray;
        }
        static int[] GetNext(char[] sortedT, char[] t)  //o(n)
        {
            // give size to queue/////
            Dictionary<char, Queue<int>> charIndicesT = new Dictionary<char, Queue<int>>();

            int[] next = new int[t.Length];

            for (int i = 0; i < t.Length; i++) // o(N)
            {
                char c = t[i];
                if (!charIndicesT.ContainsKey(c))  //O(1)
                {
                    charIndicesT[c] = new Queue<int>();
                }
                charIndicesT[c].Enqueue(i);   //O(1)
            }

            //foreach (var kvp in charIndicesT)
            //{
            //    //Console.Write($"Queue for character '{kvp.Key}': ");
            //    foreach (int ndex in kvp.Value)
            //    {
            //        //Console.Write($"{ndex} ");
            //    }
            //    ////Console.WriteLine();
            //}
            //////Console.WriteLine("------------------------------");
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            int index = 0;
            foreach (char c in sortedT)
            {
                next[index] = charIndicesT[c].Dequeue();
                index++;
            }
            //sw.Stop();
            //////Console.WriteLine(sw.ElapsedMilliseconds);
            return next;
        }
        static char[] BWI(int[] next, char[] t, int index) //O(N)
        {
            char[] reconstructed = new char[next.Length];
            int idx = index;

            for (int i = 0; i < next.Length; i++) //o(N)
            {
                reconstructed[i] = t[idx];
                idx = next[idx];

            }
            return reconstructed;
        }

        //static List<char> BWI(List<int> next, List<char> t, int index)
        //{
        //    List<char> reconstructed = new List<char>();
        //    int idx = index;
        //    for (int i = 0; i < next.Count; i++)
        //    {
        //        idx = next[idx];
        //        reconstructed.Add(t[idx]);
        //        ////Console.Write(t[idx]);
        //    }
        //    //////Console.WriteLine();
        //    return reconstructed;
        //}

        //----------------------------------------------------------------------------------------------------------------------------------
        static List<int> encrypt(List<char> txt)  //O(N)
        {
            List<char> alphabet = new List<char>();
            List<int> final = new List<int>();
            // o (1)

            for (int i = 0; i < 256; i++)
            {
                alphabet.Add((char)(i));
            }
            foreach (char c in txt)  //o(n)
            {
                // all are O(1)
                final.Add(alphabet.IndexOf(c)); // 256
                alphabet.Remove(c); //256
                alphabet.Insert(0, c); //256
            }
            return final;
        }

        static List<char> Dec(List<int> txt) //O(N)
        {
            List<char> alpha = new List<char>();
            for (int i = 0; i < 256; i++)
            {
                alpha.Add((char)i);
            }

            List<char> PT = new List<char>();

            foreach (int num in txt) //O(N)
            {
                //all of them are O(1)
                PT.Add(alpha[num]);
                char temp = alpha[num];
                alpha.RemoveAt(num);
                alpha.Insert(0, temp);
            }
            return PT;
        }


        //----------------------------------------------------------------------------------------------------------------------------------


        public static void WriteIntToBinaryFile(int num, BinaryWriter writer)
        {
            try
            {
                if (num == 0)
                {
                    writer.Write((byte)0);
                }
                else
                {
                    while (num != 0)
                    {
                        byte b = (byte)(num & 0x7F);
                        num >>= 7;
                        if (num != 0)
                        {
                            b |= 0x80;
                        }
                        writer.Write(b);
                    }
                }
            }
            catch (IOException ex)
            {
                ////Console.WriteLine("Error writing to binary file: " + ex.Message);
            }
        }
        public static int ReadIntFromBinaryFile(BinaryReader reader)
        {
            int result = 0;
            int shift = 0;

            byte b;
            do
            {
                b = reader.ReadByte();  // Read a byte
                result |= (b & 0x7F) << shift;  // Extract 7 bits and merge
                shift += 7;  // Increment the shift
            } while ((b & 0x80) != 0);  // Check if the high bit is set, indicating continuation

            return result;
        }

        //-----------------------------------------------------------
        //----------- replaced with tostring -----------------------
        static List<int> BitsToDecimal(List<int> bits)
        {
            List<int> decimalValues = new List<int>();

            int extraZeros = 0; // Count of extra zeros appended

            // Append zeros to make the count of bits divisible by 8
            while (bits.Count % 8 != 0)
            {
                bits.Add(0);
                extraZeros++;
            }

            for (int i = 0; i < bits.Count; i += 8)
            {
                int decimalValue = 0;

                // Convert each group of 8 bits into a decimal value
                for (int j = 0; j < 8; j++)
                {
                    decimalValue <<= 1; // Left shift the value by one position
                    decimalValue |= bits[i + j]; // Set the least significant bit
                }

                decimalValues.Add(decimalValue);
            }

            decimalValues.Insert(0, extraZeros); // Insert extra zeros count at the beginning
            return decimalValues;
        }

        static List<int> DecimalToBits(List<int> decimalValues)
        {
            List<int> bits = new List<int>();

            // Get the count of extra zeros appended
            int extraZeros = decimalValues[0];
            decimalValues.RemoveAt(0);

            foreach (int decimalValue in decimalValues)
            {
                for (int i = 7; i >= 0; i--)
                {
                    bits.Add((decimalValue >> i) & 1); // Extract each bit from the decimal value
                }
            }

            // Remove the extra zeros
            bits.RemoveRange(bits.Count - extraZeros, extraZeros);

            return bits;
        }
        //--------------------------------------------------------------


        //-ve
        //public static void WriteIntToBinaryFile(int num, BinaryWriter writer)
        //{
        //    try
        //    {
        //        // Adjust the number to handle negative values
        //        uint unsignedNum = (uint)(num < 0 ? (-num << 1) | 1 : num << 1);

        //        if (unsignedNum == 0)
        //        {
        //            writer.Write((byte)0);
        //        }
        //        else
        //        {
        //            while (unsignedNum != 0)
        //            {
        //                byte b = (byte)(unsignedNum & 0x7F);
        //                unsignedNum >>= 7;
        //                if (unsignedNum != 0)
        //                {
        //                    b |= 0x80;
        //                }
        //                writer.Write(b);
        //            }
        //        }
        //    }
        //    catch (IOException ex)
        //    {
        //        ////Console.WriteLine("Error writing to binary file: " + ex.Message);
        //    }
        //}

        //public static int ReadIntFromBinaryFile(BinaryReader reader)
        //{
        //    int result = 0;
        //    int shift = 0;
        //    byte b;
        //    do
        //    {
        //        b = reader.ReadByte();
        //        result |= (b & 0x7F) << shift;
        //        shift += 7;
        //    } while ((b & 0x80) != 0);

        //    // Check if the sign bit is set
        //    if ((result & 1) != 0)
        //    {
        //        result = -(result >> 1); // Convert back to negative
        //    }
        //    else
        //    {
        //        result >>= 1; // Positive number, just shift
        //    }
        //    return result;
        //}


        public static void PrintTree(Node root, int level)
        {
            if (root != null)
            {
                PrintTree(root.right, level + 1);
                for (int i = 0; i < level; i++)
                    //Console.Write("   ");
                    ////Console.WriteLine(root.symbol);
                    PrintTree(root.left, level + 1);
            }
        }
        //------------------------------------------------------------------------------------------------------

        static void WriteBinaryStringToFile(string binaryString, string fileName)
        {
            // Calculate padding length
            int paddingLength = binaryString.Length % 8 == 0 ? 0 : 8 - binaryString.Length % 8;

            // Prepend the binary string with the number of padding bits
            string header = Convert.ToString(paddingLength, 2).PadLeft(8, '0');
            binaryString = header + binaryString;

            // Append padding if needed
            for (int i = 0; i < paddingLength; i++)
            {
                binaryString += '0';
            }

            // Convert binary string to byte array
            byte[] byteArray = new byte[binaryString.Length / 8];
            for (int i = 0; i < byteArray.Length; i++)
            {
                string byteString = binaryString.Substring(i * 8, 8);
                byteArray[i] = Convert.ToByte(byteString, 2);
            }

            // Write byte array to binary file
            File.WriteAllBytes(fileName, byteArray);
        }

        static string ReadBinaryFileToString(string fileName)
        {
            // Read bytes from binary file
            byte[] byteArray = File.ReadAllBytes(fileName);

            // Create a StringBuilder to store the binary string
            StringBuilder binaryStringBuilder = new StringBuilder(byteArray.Length * 8);

            // Convert byte array to binary string
            foreach (byte b in byteArray)
            {
                // Use bitwise operations to convert byte to binary string
                binaryStringBuilder.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            // Extract the header containing the number of padding bits
            string binaryString = binaryStringBuilder.ToString();
            string header = binaryString.Substring(0, 8);
            int paddingLength = Convert.ToInt32(header, 2);

            // Remove the header and padding bits
            binaryString = binaryString.Substring(8, binaryString.Length - 8 - paddingLength);
            return binaryString;
        }

        //-----------------------------------------------------------------
        static string treeString = "";
        static string codeString = "";
        static void constructTree(Node root)
        {
            if (root.left == null && root.right == null)
            {
                treeString += '1';
                string temp = Convert.ToString(root.symbol, 2);
                temp = temp.PadLeft(7, '0');
                codeString += temp;
                ////Console.Write("shifted: ");
                //////Console.WriteLine(temp);
                return;
            }
            treeString += '0';
            constructTree(root.right);
            constructTree(root.left);
        }

        static void reconstructTree(int i, int j, string treestr, byte[] values, Node root)
        {
            if (i >= treestr.Length || values.Length == 0)
            {
                return;
            }
            char tree = treestr[i];
            if (root.right == null)
            {
                if (tree == '1')
                {
                    Node newnode = new Node();
                    root.right = newnode;
                    newnode.parent = root;
                    newnode.symbol = values[j];
                    reconstructTree(i + 1, j + 1, treestr, values, root);
                }
                else
                {
                    Node newnode = new Node();
                    root.right = newnode;
                    newnode.parent = root;
                    reconstructTree(i + 1, j, treestr, values, newnode);
                }

            }
            else if (root.left == null)
            {
                if (tree == '1')
                {
                    Node newnode = new Node();
                    root.left = newnode;
                    newnode.parent = root;
                    newnode.symbol = values[j];
                    reconstructTree(i + 1, j + 1, treestr, values, root);
                    return;
                }
                else
                {
                    Node newnode = new Node();
                    root.left = newnode;
                    newnode.parent = root;
                    reconstructTree(i + 1, j, treestr, values, newnode);
                }
            }
            else
            {
                reconstructTree(i, j, treestr, values, root.parent);
            }

        }

        static (Node, int, string) readFile(string wholeString)
        {
            byte rowBits = Convert.ToByte(wholeString.Substring(0, 8), 2);

            int rowNum = Convert.ToInt32(wholeString.Substring(8, rowBits), 2);

            byte symbolsNum = 0;
            byte treeSize = Convert.ToByte(wholeString.Substring(8 + rowBits, 8), 2);
            string tree = "";
            for (int i = 8 + rowBits + 8; i < (8 + rowBits + 8) + treeSize; i++)
            {
                tree += wholeString[i];
                if (wholeString[i] == '1')
                {
                    symbolsNum++;
                }
            }
            byte[] symbols = new byte[symbolsNum];
            byte index = 0;
            for (int i = (8 + rowBits + 8) + treeSize; i < ((8 + rowBits + 8) + treeSize) + (symbolsNum * 7); i += 7)
            {
                symbols[index] = Convert.ToByte(wholeString.Substring(i, 7), 2);
                ////Console.WriteLine(symbols[index]);
                index++;
            }
            string decompressed = wholeString.Substring((((8 + rowBits + 8) + treeSize) + (symbolsNum * 7)), wholeString.Length - (((8 + rowBits + 8) + treeSize) + (symbolsNum * 7)));
            string symbolsStr = "";
            foreach (var com in symbols)
            {
                //compressedStr += com;
                symbolsStr = symbolsStr + string.Join("", com);
            }
            Node root = new Node();
            reconstructTree(1, 0, tree, symbols, root);
            PrintTree(root, 0);
            return (root, rowNum, decompressed);
            ////Console.WriteLine(treeSize);
            ////Console.WriteLine(tree);
            ////Console.WriteLine(symbolsNum);
            ////Console.WriteLine(decompressed);
        }

        static string rowNumber(int row)
        {
            string toStore = "";
            string binary = Convert.ToString(row, 2);
            ////Console.WriteLine("row binary " + binary);
            string bytes = Convert.ToString(binary.Length, 2).PadLeft(8, '0');
            ////Console.WriteLine("row binary length  " + binary.Length);

            ////Console.WriteLine("row binary length binary " + bytes);
            toStore = toStore + bytes + binary;
            return toStore;
        }



        static void Main(string[] args)
        {

            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            Stopwatch sw3 = new Stopwatch();
            sw1.Start();
            string testFilePath = @"D:\Semster 6\Algo\Project\[2] Burrow-Wheeler Compression\Test Files\Large Cases\Medium\test";
            string file = @"D:\Semster 6\Algo\Project\[2] Burrow-Wheeler Compression\Test Files\Large Cases\Medium\chromosome11-human.txt";
            (char[] t, int[] i) = BWT(file);
            List<int> encrypted = encrypt(t.ToList());


            CalculateFrequencies(encrypted);
            Node root = BuildTree(freq);

            PrintTree(root, 0);

            GeneratePath(root, new List<int>());
            List<int> compressed = Compress(encrypted);
            constructTree(root);
            string compressedStr = string.Join("", compressed);
            string stringToStore = rowNumber(sd[0]);
            stringToStore = stringToStore + Convert.ToString(treeString.Length, 2).PadLeft(8, '0') + treeString + codeString + compressedStr;
            WriteBinaryStringToFile(stringToStore, testFilePath);
            sw1.Stop();
            sw2.Start();
            string decompressed = ReadBinaryFileToString(testFilePath);
            sw2.Stop();
            sw3.Start();
            (Node reconstructedRoot, int reconstructedRowNum, string reconstructedDecompressed) = readFile(decompressed);
            List<int> finalDecompression = Decompress(reconstructedDecompressed, reconstructedRoot);
            List<char> x = Dec(finalDecompression);
            char[] sorted = merge_sortT(x.ToArray());
            int[] nextarwa = GetNext(sorted, x.ToArray());
            char[] reconstructed = BWI(nextarwa, sorted, reconstructedRowNum);
            string reconstructedString = new string(reconstructed.ToArray());
            File.WriteAllText(@"D:\Semster 6\Algo\Project\[2] Burrow-Wheeler Compression\Test Files\Large Cases\Medium\output.txt", reconstructedString);
            sw3.Stop();
            Console.WriteLine("SW 1: (from beginning of main to after writing in binary file): " + sw1.ElapsedMilliseconds);
            Console.WriteLine("SW 2: (Just the time needed to read the binary file): " + sw2.ElapsedMilliseconds);
            Console.WriteLine("SW 3: (From after reading binary file to the end of main): " + sw3.ElapsedMilliseconds);
            //------------------------


            //FileInfo originalfileInfo = new FileInfo(file);
            //long originalfileSizeInBytes = originalfileInfo.Length;
            //string compressedfilename = filename;
            //FileInfo compressedfileInfo = new FileInfo(compressedfilename);
            //long compressedfileSizeInBytes = compressedfileInfo.Length; // Use compressed file length here

            //double compressionRatio = 1.0 - ((double)compressedfileSizeInBytes / originalfileSizeInBytes);
            //double compressionRatioPercentage = compressionRatio * 100;
            //// Calculate the compression ratio

            //////Console.WriteLine("Compression Ratio: " + compressionRatio);
            //////Console.WriteLine("Compression Ratio Percentage: " + compressionRatioPercentage + "%");

          
        }
    }
}