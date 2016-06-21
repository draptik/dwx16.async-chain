using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace AsyncChain
{
    [TestFixture]
    public class SyncChain
    {
        #region BoringInfrastructureStuff

        StringWriter writer = new StringWriter();

        [SetUp]
        public void SetUp()
        {
            writer = new StringWriter();
            Console.SetOut(writer);
        }

        #endregion

        /*
         * Exercise 1: Implement a manual version of the chain of responsibility by chaining method calls
         */
        [Test]
        public void ComposeManual()
        {
            Action done = () =>
            {
                Console.WriteLine("done");
            };

            // TODO: Compose here the chain manually

            Son(() => Wife(() => Husband(done)));

            Assert.That(writer.ToString(), Is.EqualTo(@"Son
Wife
Husband
done
"));
        }

        // TODO: Extend and implement this method
        public static void Son(Action action)
        {
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            action.Invoke();
        }

        // TODO: Extend and implement this method
        public static void Wife(Action action)
        {
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            action.Invoke();
        }

        // TODO: Extend and implement this method
        public static void Husband(Action action)
        {
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            action.Invoke();
        }

        /*
         * Exercise 2: Implement a more generic version of the chain of responsibility pattern
         * Hints/Suggestions: Lists, Recursion, While loop?
         */
        [Test]
        public void ComposeGeneric()
        {
            Action done = () =>
            {
                Console.WriteLine("done");
            };

            // TODO: Compose here the chain in a more generic way, reuse the methods Son(), Wife(), Husband()
            var actions = new List<Action<Action>> {Son, Wife, Husband, next => done()};
            Invoke(actions, 0);

            Assert.That(writer.ToString(), Is.EqualTo(@"Son
Wife
Husband
done
"));
        }

        // TODO: Extend and implement this method
        public static void Invoke(List<Action<Action>> actions, int currentIndex)
        {
            if (currentIndex == actions.Count)
            {
                return;
            }

            var action = actions[currentIndex];
            action(() => Invoke(actions, currentIndex + 1));
        }

        /*
         * Exercise 3: Introduce an exception filter which catches InvalidOperationExceptions
         */
        [Test]
        public void ComposeGenericWithFilters()
        {
            Action done = () =>
            {
                Console.WriteLine("done");
            };

            // TODO: Compose here the chain in a more generic way, reuse the methods Son(), Wife(), Husband() and Invoke()
            // - put EvilMethod() right before done
            // - Add filter on the top of the chain

            var actions = new List<Action<Action>> { FilterInvalidOperationException, Son, Wife, Husband, EvilMethod };
            Invoke(actions, 0);


            Assert.That(writer.ToString(), Is.EqualTo(@"FilterInvalidOperationException
Son
Wife
Husband
EvilMethod
Filtered!
"));
        }

        // TODO: Extend and implement this method,
        static void FilterInvalidOperationException(Action next)
        {
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            
            try
            {
                next();
            }
            catch (Exception)
            {
                // TODO: Move this line where appropriate
                Console.WriteLine("Filtered!");
            }
        }

        static void EvilMethod(Action next)
        {
            Console.WriteLine(MethodBase.GetCurrentMethod().Name);
            throw new InvalidOperationException("Boomer!");
        }

    }
}