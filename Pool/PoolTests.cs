
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GMTK
{
    namespace Tests
    {
        public class Test
        {
            public string data;
        }

        [TestClass]
        public class PoolTests
        {
            [TestMethod]
            public void YouCanAcquireAnElementFromThePool()
            {
                Pool<Test> pool = new Pool<Test>(1);
                var test = pool.Acquire();
                Assert.IsNotNull(test);
            }

            [TestMethod]
            public void YouCanGetTheCountOfAcquiredElements()
            {
                Pool<Test> pool = new Pool<Test>(10);
                var test1 = pool.Acquire();
                Assert.AreEqual(pool.Count, 1);
                var test2 = pool.Acquire();
                Assert.AreEqual(pool.Count, 2);
                pool.Release(ref test1);
                Assert.AreEqual(pool.Count, 1);
                pool.Release(ref test2);
                Assert.AreEqual(pool.Count, 0);
            }

            [TestMethod]
            public void YouCanReleaseAnAcquiredElement()
            {
                Pool<Test> pool = new Pool<Test>(1);
                var test = pool.Acquire();
                Assert.IsNotNull(test);
                pool.Release(ref test);
                Assert.IsNull(test);
            }

            [TestMethod]
            public void PoolCantReleaseNull()
            {
                Pool<Test> pool = new Pool<Test>(1);
                var test = pool.Acquire();
                Assert.IsNotNull(test);
                test = null;
                Assert.ThrowsException<Pool<Test>.PoolException>(() =>
                {
                    pool.Release(ref test);
                });
            }

            [TestMethod]
            public void YouCanAugmentDataInAcquiredAndReleasedElements()
            {
                Pool<Test>.PoolElementMutation acquire = (ref Test element) => { element.data = "SET!"; };

                Pool<Test> pool = new Pool<Test>(1);
                pool.OnAcquired += acquire;
                pool.OnReleased += (ref Test element) => { element.data = "UNSET!"; };

                var test = pool.Acquire();
                Assert.IsNotNull(test);
                Assert.IsNotNull(test.data);
                Assert.AreEqual(test.data, "SET!");

                pool.Release(ref test);
                Assert.IsNull(test);

                pool.OnAcquired -= acquire;
                var result = pool.Acquire();
                Assert.AreEqual(result.data, "UNSET!");
            }

            [TestMethod]
            public void YouCanUseRangeForeachWithThePool()
            {
                Random random = new Random();
                int capacity = random.Next(3, 100);
                Pool<Test> pool = new Pool<Test>(101);
                pool.OnAcquired += (ref Test element) => { element.data = $"{random.Next(10000)}"; };

                HashSet<string> data = new HashSet<string>();
                for (int i = 0; i < capacity; i++)
                {
                    var element = pool.Acquire();
                    data.Add(element.data);
                }

                int check = 0;
                foreach (var acquired in pool)
                {
                    Assert.IsTrue(data.Contains(acquired.data));
                    check++;
                }

                Assert.AreEqual(check, capacity);
            }

            [TestMethod]
            public void PoolCantHaveZeroCapacity()
            {
                Assert.ThrowsException<Pool<Test>.PoolException>(() =>
                {
                    var _ = new GMTK.Pool<Test>(0);
                });
            }

            [TestMethod]
            public void PoolWillThrowErrorIfExertedOverCapacity()
            {
                Pool<Test> pool = new GMTK.Pool<Test>(1);
                var _ = pool.Acquire();
                Assert.ThrowsException<Pool<Test>.PoolException>(() =>
                {
                    var _ = pool.Acquire();
                });
            }

            [TestMethod]
            public void PoolWillThrowErrorIfPollutedFromOutside()
            {
                Pool<Test> pool = new GMTK.Pool<Test>(100);
                var test = new Test();

                Assert.ThrowsException<Pool<Test>.PoolException>(() =>
                {
                    pool.Release(ref test);
                });
            }

            [TestMethod]
            public void PoolWillThrowErrorIfReleasingOutsideElement()
            {
                Pool<Test> pool = new Pool<Test>(1);
                var test = pool.Acquire();
                Assert.IsNotNull(test);
                var pollution = new Test();

                Assert.ThrowsException<Pool<Test>.PoolException>(() =>
                {
                    pool.Release(ref pollution);
                });
            }
        }
    }
}
