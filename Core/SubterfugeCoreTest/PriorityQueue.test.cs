using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SubterfugeCore.Core.Timing;
/*
namespace SubterfugeCoreTest
{
    [TestClass]
    public class PriorityQueueTest
    {
        PriorityQueue<int> _queue;

        [TestInitialize]
        public void Setup()
        {
            this._queue = new PriorityQueue<int>();
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(_queue);
        }

        [TestMethod]
        public void Enqueue()
        {
            _queue.Enqueue(1);
            Assert.AreEqual(1, _queue.Count);

            // Ensure the queue is prioritized.
            _queue.Enqueue(6);
            Assert.AreEqual(2, _queue.Count);
            Assert.AreEqual(1, _queue.Peek());
        }

        [TestMethod]
        public void Peek()
        {
            // Ensure the queue peeks the right item
            _queue.Enqueue(4);
            Assert.AreEqual(4, _queue.Peek());


            _queue.Enqueue(2);
            Assert.AreEqual(2, _queue.Peek());
        }

        [TestMethod]
        public void Dequeue()
        {
            // Ensure the queue peeks the right item
            _queue.Enqueue(4);
            int four = _queue.Dequeue();
            Assert.AreEqual(0, _queue.Count);
            Assert.AreEqual(4, four);


            _queue.Enqueue(4);
            _queue.Enqueue(2);
            int two = _queue.Dequeue();
            Assert.AreEqual(1, _queue.Count);
            Assert.AreEqual(4, _queue.Peek());
            Assert.AreEqual(2, two);

            // Ensure you cannot dequeue an empty queue
            _queue.Dequeue();
            _queue.Dequeue();
        }

        [TestMethod]
        public void Equals()
        {
            Assert.IsTrue(_queue.Equals(_queue));
        }

        [TestCleanup]
        public void ResetQueue()
        {
            _queue = new PriorityQueue<int>();
        }
    }
}
*/