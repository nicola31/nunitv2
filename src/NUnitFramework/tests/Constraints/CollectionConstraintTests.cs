// *****************************************************
// Copyright 2007, Charlie Poole
// Licensed under the NUnit License, see license.txt
// *****************************************************

using System;
using System.Collections;

namespace NUnit.Framework.Constraints.Tests
{
    [TestFixture]
    public class AllItemsTests : NUnit.Framework.Tests.MessageChecker
    {
        [Test]
        public void AllItemsAreNotNull()
        {
            object[] c = new object[] { 1, "hello", 3, Environment.OSVersion };
            Assert.That(c, new AllItemsConstraint(Is.Not.Null));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void AllItemsAreNotNullFails()
        {
            object[] c = new object[] { 1, "hello", null, 3 };
            expectedMessage = Msgs.Pfx_Expected + "all items not null" + Environment.NewLine +
                Msgs.Pfx_Actual + "< 1, \"hello\", null, 3 >" + Environment.NewLine;
            Assert.That(c, new AllItemsConstraint(Is.Not.Null));
        }

        [Test]
        public void AllItemsAreInRange()
        {
            int[] c = new int[] { 12, 27, 19, 32, 45, 99, 26 };
            Assert.That(c, new AllItemsConstraint(Is.GreaterThan(10) & Is.LessThan(100)));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void AllItemsAreInRangeFailureMessage()
        {
            int[] c = new int[] { 12, 27, 19, 32, 107, 99, 26 };
            expectedMessage = 
                Msgs.Pfx_Expected + "all items greater than 10 and less than 100" + Environment.NewLine +
                Msgs.Pfx_Actual   + "< 12, 27, 19, 32, 107, 99, 26 >" + Environment.NewLine;
            Assert.That(c, new AllItemsConstraint(Is.GreaterThan(10) & Is.LessThan(100)));
        }

        [Test]
        public void AllItemsAreInstancesOfType()
        {
            object[] c = new object[] { 'a', 'b', 'c' };
            Assert.That(c, Is.All.InstanceOfType(typeof(char)));
        }

        [Test, ExpectedException(typeof(AssertionException))]
        public void AllItemsAreInstancesOfTypeFailureMessage()
        {
            object[] c = new object[] { 'a', "b", 'c' };
            expectedMessage = 
                Msgs.Pfx_Expected + "all items instance of <System.Char>" + Environment.NewLine +
                Msgs.Pfx_Actual   + "< 'a', \"b\", 'c' >" + Environment.NewLine;
            Assert.That(c, new AllItemsConstraint(Is.InstanceOfType(typeof(char))));
        }
    }

    [TestFixture]
    public class CollectionContainsTests
    {
        [Test]
        public void CanTestContentsOfArray()
        {
            object item = "xyz";
            object[] c = new object[] { 123, item, "abc" };
            Assert.That(c, new CollectionContainsConstraint(item));
        }

        [Test]
        public void CanTestContentsOfArrayList()
        {
            object item = "xyz";
            ArrayList list = new ArrayList( new object[] { 123, item, "abc" } );
            Assert.That(list, new CollectionContainsConstraint(item));
        }

        [Test]
        public void CanTestContentsOfSortedList()
        {
            object item = "xyz";
            SortedList list = new SortedList();
            list.Add("a", 123);
            list.Add("b", item);
            list.Add("c", "abc");
            Assert.That(list.Values, new CollectionContainsConstraint(item));
            Assert.That(list.Keys, new CollectionContainsConstraint("b"));
        }
    }
}