using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SubterfugeCore.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class PriorityQueueTest
    {
        PriorityQueue<int> queue;

        [TestInitialize]
        public void setup()
        {
            this.queue = new PriorityQueue<int>();
        }

        [TestMethod]
        public void constructor()
        {
            Assert.IsNotNull(queue);
        }

        [TestMethod]
        public void enqueue()
        {
            queue.Enqueue(1);
            Assert.AreEqual(1, queue.Count);

            // Ensure the queue is prioritized.
            queue.Enqueue(6);
            Assert.AreEqual(2, queue.Count);
            Assert.AreEqual(1, queue.Peek());
        }

        [TestMethod]
        public void peek()
        {
            // Ensure the queue peeks the right item
            queue.Enqueue(4);
            Assert.AreEqual(4, queue.Peek());


            queue.Enqueue(2);
            Assert.AreEqual(2, queue.Peek());
        }

        [TestMethod]
        public void dequeue()
        {
            // Ensure the queue peeks the right item
            queue.Enqueue(4);
            int four = queue.Dequeue();
            Assert.AreEqual(0, queue.Count);
            Assert.AreEqual(4, four);


            queue.Enqueue(4);
            queue.Enqueue(2);
            int two = queue.Dequeue();
            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual(4, queue.Peek());
            Assert.AreEqual(2, two);

            // Ensure you cannot dequeue an empty queue
            queue.Dequeue();
            queue.Dequeue();
        }

        [TestMethod]
        public void equals()
        {
            Assert.IsTrue(queue.Equals(queue));
        }

        [TestCleanup]
        public void resetQueue()
        {
            queue = new PriorityQueue<int>();
        }
    }
}
