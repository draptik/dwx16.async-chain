using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncChain
{
    [TestFixture]
    public class AsyncChain
    {
        #region BoringInfrastructureStuff

        StringWriter writer = new StringWriter();

        [SetUp]
        public void SetUp()
        {
            Console.SetOut(writer);
        }

        #endregion

        /*
         * Exercise 4: Implement a manual version of the chain of responsibility by chaining method calls
         */
        [Test]
        public async Task ComposeManual()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("done");
                return Task.CompletedTask;
            };

            // TODO: Compose here the chain manually

            await Son(() => Wife(() => Husband(done)));

            Assert.That(writer.ToString(), Is.EqualTo(@"Son
Wife
Husband
done
"));
        }

        // TODO: Extend and implement this method
        public static async Task Son(Func<Task> next)
        {
            Console.WriteLine("Son");
            await next();
        }

        // TODO: Extend and implement this method
        public static async Task Wife(Func<Task> next)
        {
            Console.WriteLine("Wife");
            await next();
        }

        // TODO: Extend and implement this method
        public static async Task Husband(Func<Task> next)
        {
            Console.WriteLine("Husband");
            await next();
        }

        /*
         * Exercise 5: Implement a more generic version of the chain of responsibility pattern
         * Hints/Suggestions: Lists, Recursion, While loop?
         */
        [Test]
        public async Task ComposeGeneric()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("done");
                return Task.CompletedTask;
            };

            // TODO: Compose here the chain in a more generic way, reuse the methods Son(), Wife(), Husband()
            var tasks = new List<Func<Func<Task>, Task>>() {Son, Wife, Husband, next => done()};

            await Invoke(tasks);

            Assert.That(writer.ToString(), Is.EqualTo(@"Son
Wife
Husband
done
"));
        }

        // TODO: Extend and implement this method
        public static Task Invoke(List<Func<Func<Task>, Task>> tasks, int currentIndex = 0)
        {
            if (currentIndex == tasks.Count)
            {
                return Task.FromResult(0);
            }

            var task = tasks[currentIndex];

            // not very efficient (requires async in signature):
            //await task(async () => await Invoke(tasks, currentIndex + 1));

            // slightly better (requires async in signature):
            //await task(() => Invoke(tasks, currentIndex + 1));

            // best
            return task(() => Invoke(tasks, currentIndex + 1));
        }

        /*
         * Exercise 6: Introduce an async exception filter which catches InvalidOperationExceptions
         */
        [Test]
        public async Task ComposeGenericWithFilters()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("done");
                return Task.CompletedTask;
            };

            // TODO: Compose here the chain in a more generic way, reuse the methods Son(), Wife(), Husband() and Invoke()
            // - put EvilMethod() right before done
            // - Add filter on the top of the chain

            var funcs = new List<Func<Func<Task>, Task>>
            {
                FilterInvalidOperationException,
                Son,
                Wife,
                Husband,
                EvilMethod,
                next => done()
            };

            await Invoke(funcs);

            Assert.That(writer.ToString(), Is.EqualTo(@"FilterInvalidOperationException
Son
Wife
Husband
EvilMethod
Filtered!
"));
        }

        // TODO: Extend and implement this method
        static async Task FilterInvalidOperationException(Func<Task> next)
        {
            Console.WriteLine("FilterInvalidOperationException");

            try
            {
                await next();
            }
            catch (Exception)
            {
                Console.WriteLine("Filtered!");
            }
        }

        static async Task EvilMethod(Func<Task> next)
        {
            Console.WriteLine("EvilMethod");
            await Task.Yield();
            throw new InvalidOperationException("Boomer!");
        }

        /*
         * Exercise 7.1: Write a retrier which retries 3 times in case of an exception,
         * if after 3 retries the chain still throws an exception then it rethrows the last exception it saw
         */
        [Test]
        public void RetriesAndRethrows()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("done");
                return Task.CompletedTask;
            };

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                // TODO: Compose here the chain in a more generic way, reuse the methods Son(), Wife(), Husband() and Invoke()
                // - put ThrowsAlways() right before done
                // - Add RetryThreeTimesAndRethrowExceptionIfStillOccurs on the top of the chain
            });

            Assert.That(writer.ToString(), Is.EqualTo(@"RetryThreeTimesAndRethrowExceptionIfStillOccurs
Son
Wife
Husband
ThrowsAlways
Son
Wife
Husband
ThrowsAlways
Son
Wife
Husband
ThrowsAlways
"), writer.ToString());
        }

        /*
         * Exercise 7.2: Write a retrier which retries 3 times in case of an exception,
         * if after less than 3 retries the chain does not throw any more then the chain is successful
         */
        [Test]
        public void RetriesAndDoesNotRethrow()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("done");
                return Task.CompletedTask;
            };

            Assert.DoesNotThrowAsync(async () =>
            {
                // TODO: Compose here the chain in a more generic way, reuse the methods Son(), Wife(), Husband() and Invoke()
                // - put ThrowsTwoTimes() right before done
                // - Add RetryThreeTimesAndRethrowExceptionIfStillOccurs on the top of the chain
            });

            Assert.That(writer.ToString(), Is.EqualTo(@"RetryThreeTimesAndRethrowExceptionIfStillOccurs
Son
Wife
Husband
ThrowsTwoTimes
Son
Wife
Husband
ThrowsTwoTimes
"), writer.ToString());
        }

        // TODO: Implement this method
        public static async Task RetryThreeTimesAndRethrowExceptionIfStillOccurs(Func<Task> next)
        {
            Console.WriteLine("RetryThreeTimesAndRethrowExceptionIfStillOccurs");

        }

        public static async Task ThrowsAlways(Func<Task> next)
        {
            Console.WriteLine("ThrowsAlways");
            await Task.Yield();
            throw new InvalidOperationException();
        }

        static int invocationCounter;

        public static async Task ThrowsTwoTimes(Func<Task> next)
        {
            Console.WriteLine("ThrowsTwoTimes");
            await Task.Yield();

            invocationCounter++;
            if (invocationCounter < 2)
            {
                throw new InvalidOperationException();
            }
        }
    }
}