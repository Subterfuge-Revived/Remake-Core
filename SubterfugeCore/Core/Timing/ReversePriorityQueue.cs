//====================================================
//| Modified From                                    |
//| Visual C# Kicks - http://www.vcskicks.com/       |
//| License - http://www.vcskicks.com/license.html   |
//====================================================

using System;
using System.Collections.Generic;

namespace SubterfugeCore.Core.Timing
{
    /// <summary>
    /// Priority Queue data structure
    /// </summary>
    public class ReversePriorityQueue<T>
        where T : IComparable
    {
        protected List<T> StoredValues;

        public ReversePriorityQueue()
        {
            //Initialize the array that will hold the values
            StoredValues = new List<T>();

            //Fill the first cell in the array with an empty value
            StoredValues.Add(default(T));
        }

        public List<T> GetQueue()
        {
            return this.StoredValues;
        }

        /// <summary>
        /// Gets the number of values stored within the Priority Queue
        /// </summary>
        public virtual int Count
        {
            get { return StoredValues.Count - 1; }
        }

        /// <summary>
        /// Returns the value at the head of the Priority Queue without removing it.
        /// </summary>
        public virtual T Peek()
        {
            if (this.Count == 0)
                return default(T); //Priority Queue empty
            else
                return StoredValues[1]; //head of the queue
        }

        /// <summary>
        /// Adds a value to the Priority Queue
        /// </summary>
        public virtual void Enqueue(T value)
        {
            //Add the value to the internal array
            StoredValues.Add(value);

            //Bubble up to preserve the heap property,
            //starting at the inserted value
            this.BubbleUp(StoredValues.Count - 1);
        }

        /// <summary>
        /// Returns the minimum value inside the Priority Queue
        /// </summary>
        public virtual T Dequeue()
        {
            if (this.Count == 0)
                return default(T); //queue is empty
            else
            {
                //The smallest value in the Priority Queue is the first item in the array
                T minValue = this.StoredValues[1];

                //If there's more than one item, replace the first item in the array with the last one
                if (this.StoredValues.Count > 2)
                {
                    T lastValue = this.StoredValues[StoredValues.Count - 1];

                    //Move last node to the head
                    this.StoredValues.RemoveAt(StoredValues.Count - 1);
                    this.StoredValues[1] = lastValue;

                    //Bubble down
                    this.BubbleDown(1);
                }
                else
                {
                    //Remove the only value stored in the queue
                    StoredValues.RemoveAt(1);
                }

                return minValue;
            }
        }

        /// <summary>
        /// Restores the heap-order property between child and parent values going up towards the head
        /// </summary>
        protected virtual void BubbleUp(int startCell)
        {
            int cell = startCell;

            //Bubble up as long as the parent is greater
            while (this.IsParentBigger(cell))
            {
                //Get values of parent and child
                T parentValue = this.StoredValues[cell / 2];
                T childValue = this.StoredValues[cell];

                //Swap the values
                this.StoredValues[cell / 2] = childValue;
                this.StoredValues[cell] = parentValue;

                cell /= 2; //go up parents
            }
        }

        /// <summary>
        /// Restores the heap-order property between child and parent values going down towards the bottom
        /// </summary>
        protected virtual void BubbleDown(int startCell)
        {
            int cell = startCell;

            //Bubble down as long as either child is smaller
            while (this.IsLeftChildSmaller(cell) || this.IsRightChildSmaller(cell))
            {
                int child = this.CompareChild(cell);

                if (child == -1) //Left Child
                {
                    //Swap values
                    T parentValue = StoredValues[cell];
                    T leftChildValue = StoredValues[2 * cell];

                    StoredValues[cell] = leftChildValue;
                    StoredValues[2 * cell] = parentValue;

                    cell = 2 * cell; //move down to left child
                }
                else if (child == 1) //Right Child
                {
                    //Swap values
                    T parentValue = StoredValues[cell];
                    T rightChildValue = StoredValues[2 * cell + 1];

                    StoredValues[cell] = rightChildValue;
                    StoredValues[2 * cell + 1] = parentValue;

                    cell = 2 * cell + 1; //move down to right child
                }
            }
        }

        /// <summary>
        /// Returns if the value of a parent is greater than its child
        /// </summary>
        protected virtual bool IsParentBigger(int childCell)
        {
            if (childCell == 1)
                return false; //top of heap, no parent
            else
                return StoredValues[childCell / 2].CompareTo(StoredValues[childCell]) < 0;
            //return storedNodes[childCell / 2].Key > storedNodes[childCell].Key;
        }

        /// <summary>
        /// Returns whether the left child cell is smaller than the parent cell.
        /// Returns false if a left child does not exist.
        /// </summary>
        protected virtual bool IsLeftChildSmaller(int parentCell)
        {
            if (2 * parentCell >= StoredValues.Count)
                return false; //out of bounds
            else
                return StoredValues[2 * parentCell].CompareTo(StoredValues[parentCell]) > 0;
            //return storedNodes[2 * parentCell].Key < storedNodes[parentCell].Key;
        }

        /// <summary>
        /// Returns whether the right child cell is smaller than the parent cell.
        /// Returns false if a right child does not exist.
        /// </summary>
        protected virtual bool IsRightChildSmaller(int parentCell)
        {
            if (2 * parentCell + 1 >= StoredValues.Count)
                return false; //out of bounds
            else
                return StoredValues[2 * parentCell + 1].CompareTo(StoredValues[parentCell]) > 0;
            //return storedNodes[2 * parentCell + 1].Key < storedNodes[parentCell].Key;
        }

        /// <summary>
        /// Compares the children cells of a parent cell. -1 indicates the left child is the smaller of the two,
        /// 1 indicates the right child is the smaller of the two, 0 inidicates that neither child is smaller than the parent.
        /// </summary>
        protected virtual int CompareChild(int parentCell)
        {
            bool leftChildSmaller = this.IsLeftChildSmaller(parentCell);
            bool rightChildSmaller = this.IsRightChildSmaller(parentCell);

            if (leftChildSmaller || rightChildSmaller)
            {
                if (leftChildSmaller && rightChildSmaller)
                {
                    //Figure out which of the two is smaller
                    int leftChild = 2 * parentCell;
                    int rightChild = 2 * parentCell + 1;

                    T leftValue = this.StoredValues[leftChild];
                    T rightValue = this.StoredValues[rightChild];

                    //Compare the values of the children
                    if (leftValue.CompareTo(rightValue) > 0)
                        return -1; //left child is smaller
                    else
                        return 1; //right child is smaller
                }
                else if (leftChildSmaller)
                    return -1; //left child is smaller
                else
                    return 1; //right child smaller
            }
            else
                return 0; //both children are bigger or don't exist
        }

        public bool Remove(T item)
        {
            int index = this.StoredValues.IndexOf(item);
            if (index != -1)
            {
                T lastValue = this.StoredValues[StoredValues.Count - 1];
                if (lastValue.Equals(item))
                {
                    // No swap nessecary, just remove and exit.
                    this.StoredValues.RemoveAt(StoredValues.Count - 1);
                    return true;
                }

                // Swap the last item.
                this.StoredValues[index] = lastValue;
                this.StoredValues.RemoveAt(StoredValues.Count - 1);

                //Determine to bubble down or percolate up
                if (IsParentBigger(index))
                {
                    this.BubbleUp(index);
                    return true;
                } else if (IsLeftChildSmaller(index) || IsRightChildSmaller(index))
                {
                    this.BubbleDown(index);
                    return true;
                }
            }
            return false;
        }

    }
}
