/// <author>
/// Benjamin Allred
/// </author>
/// <UID>
/// u1090524
/// </UID>

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Collections.Generic;

namespace PS2GradingTests
{
    /// <summary>
    ///  This is a test class for DependencyGraphTest
    /// 
    ///  These tests should help guide you on your implementation.  Warning: you can not "test" yourself
    ///  into correctness.  Tests only show incorrectness.  That being said, a large test suite will go a long
    ///  way toward ensuring correctness.
    /// 
    ///  You are strongly encouraged to write additional tests as you think about the required
    ///  functionality of yoru library.
    /// 
    ///</summary>
    [TestClass()]
    public class DependencyGraphTest
    {
        // ************************** TESTS ON EMPTY DGs ************************* //

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void ZeroSize()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Graph should be the right size
        ///</summary>
        [TestMethod()]
        public void TestSize1()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "a");
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("a", "d");
            t.AddDependency("f", "d");
            t.AddDependency("e", "c");
            t.AddDependency("r", "m");
            t.AddDependency("m", "r");
            Assert.AreEqual(8, t.Size);
        }

        /// <summary>
        ///Graph should be the right size
        ///</summary>
        [TestMethod()]
        public void TestSize2()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "a");
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("a", "d");
            t.AddDependency("f", "d");
            t.AddDependency("e", "c");
            t.AddDependency("r", "m");
            t.AddDependency("a", "d");
            t.AddDependency("a", "d");
            t.AddDependency("m", "r");
            t.AddDependency("m", "r");
            t.AddDependency("m", "r");
            t.AddDependency("e", "c");
            t.AddDependency("m", "r");
            Assert.AreEqual(8, t.Size);
        }

        /// <summary>
        ///Graph should be the right size
        ///</summary>
        [TestMethod()]
        public void TestSize3()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "a");
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("a", "d");
            t.AddDependency("f", "d");
            t.AddDependency("e", "c");
            t.AddDependency("r", "m");
            t.AddDependency("a", "d");
            t.AddDependency("a", "d");
            t.AddDependency("m", "r");
            t.AddDependency("m", "r");
            t.AddDependency("m", "r");
            t.AddDependency("e", "c");
            t.AddDependency("m", "r");
            t.RemoveDependency("m", "r");
            t.RemoveDependency("a", "b");
            t.RemoveDependency("a", "b");
            Assert.AreEqual(6, t.Size);
        }

        /// <summary>
        ///Graph should be the right size
        ///</summary>
        [TestMethod()]
        public void TestNullString()
        {
            DependencyGraph t = new DependencyGraph();
            string nullString = null;
            t.AddDependency(nullString, "a");
            t.AddDependency("a", nullString);
            t.AddDependency("a", nullString);
            t.AddDependency("f", nullString);
            t.AddDependency(nullString, "c");
            t.AddDependency(nullString, nullString);
            t.RemoveDependency("a", nullString);
            t.RemoveDependency("f", nullString);
            HashSet<string> dependents = new HashSet<string>(t.GetDependents(nullString));
            HashSet<string> dependees = new HashSet<string>(t.GetDependees(nullString));
            t.ReplaceDependees(nullString, dependents);
            t.ReplaceDependents(nullString, dependees);

            Assert.AreEqual(0, t.Size);
            Assert.AreEqual(0, t[nullString]);
            Assert.IsFalse(t.HasDependees(nullString));
            Assert.IsFalse(t.HasDependents(nullString));
        }


        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void HasNoDependees1()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.HasDependees("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void HasNoDependees2()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("b", "a");
            t.RemoveDependency("b", "a");
            Assert.IsFalse(t.HasDependees("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void HasNoDependents1()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.HasDependents("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void HasNoDependents2()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.RemoveDependency("a", "b");
            Assert.IsFalse(t.HasDependents("a"));
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyDependees()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.GetDependees("a").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyDependents()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.IsFalse(t.GetDependents("a").GetEnumerator().MoveNext());
        }

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyIndexer()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t["a"]);
        }

        /// <summary>
        ///Removing from an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void RemoveFromEmpty()
        {
            DependencyGraph t = new DependencyGraph();
            t.RemoveDependency("a", "b");
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Adding to an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void AddToEmpty()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void ReplaceEmptyDependents()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependents("a", new HashSet<string>());
            Assert.AreEqual(0, t.Size);
        }

        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void ReplaceEmptyDependees()
        {
            DependencyGraph t = new DependencyGraph();
            t.ReplaceDependees("a", new HashSet<string>());
            Assert.AreEqual(0, t.Size);
        }

        [TestMethod]
        public void TestEmptyReplaceDependees()
        {
            DependencyGraph dg = new DependencyGraph();

            dg.ReplaceDependees("b", new HashSet<string> { "a" });

            Assert.AreEqual(1, dg.Size);
            Assert.IsTrue(new HashSet<string> { "b" }.SetEquals(dg.GetDependents("a")));
        }


        [TestMethod]
        public void TestEmptyReplaceDependents()
        {
            DependencyGraph dg = new DependencyGraph();

            dg.ReplaceDependents("b", new HashSet<string> { "a" });

            Assert.AreEqual(1, dg.Size);
            Assert.IsTrue(new HashSet<string> { "b" }.SetEquals(dg.GetDependees("a")));
        }

        /**************************** SIMPLE NON-EMPTY TESTS ****************************/

        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void NonEmptySize()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            Assert.AreEqual(2, t.Size);
        }

        /// <summary>
        ///Slight variant
        ///</summary>
        [TestMethod()]
        public void AddDuplicate()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "b");
            Assert.AreEqual(1, t.Size);
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest1()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("b", "d");
            t.AddDependency("d", "d");
            Assert.IsFalse(t.HasDependees("a"));
            Assert.IsTrue(t.HasDependees("b"));
            Assert.IsTrue(t.HasDependees("c"));
            Assert.IsTrue(t.HasDependees("d"));
            Assert.IsTrue(t.HasDependents("a"));
            Assert.IsTrue(t.HasDependents("b"));
            Assert.IsFalse(t.HasDependents("c"));
            Assert.IsTrue(t.HasDependents("d"));
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest2()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "a");
            t.AddDependency("b", "b");
            t.AddDependency("c", "c");
            t.AddDependency("d", "d");
            Assert.IsTrue(t.HasDependees("a"));
            Assert.IsTrue(t.HasDependees("b"));
            Assert.IsTrue(t.HasDependees("c"));
            Assert.IsTrue(t.HasDependees("d"));
            Assert.IsTrue(t.HasDependents("a"));
            Assert.IsTrue(t.HasDependents("b"));
            Assert.IsTrue(t.HasDependents("c"));
            Assert.IsTrue(t.HasDependents("d"));
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void NonEmptyTest3()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            Assert.IsFalse(t.HasDependees("a"));
            Assert.IsTrue(t.HasDependees("b"));
            Assert.IsTrue(t.HasDependents("a"));
            Assert.IsTrue(t.HasDependees("c"));
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void ComplexGraphCount()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            HashSet<String> aDents = new HashSet<String>(t.GetDependents("a"));
            HashSet<String> bDents = new HashSet<String>(t.GetDependents("b"));
            HashSet<String> cDents = new HashSet<String>(t.GetDependents("c"));
            HashSet<String> dDents = new HashSet<String>(t.GetDependents("d"));
            HashSet<String> eDents = new HashSet<String>(t.GetDependents("e"));
            HashSet<String> aDees = new HashSet<String>(t.GetDependees("a"));
            HashSet<String> bDees = new HashSet<String>(t.GetDependees("b"));
            HashSet<String> cDees = new HashSet<String>(t.GetDependees("c"));
            HashSet<String> dDees = new HashSet<String>(t.GetDependees("d"));
            HashSet<String> eDees = new HashSet<String>(t.GetDependees("e"));
            Assert.IsTrue(aDents.Count == 2 && aDents.Contains("b") && aDents.Contains("c"));
            Assert.IsTrue(bDents.Count == 0);
            Assert.IsTrue(cDents.Count == 0);
            Assert.IsTrue(dDents.Count == 1 && dDents.Contains("c"));
            Assert.IsTrue(eDents.Count == 0);
            Assert.IsTrue(aDees.Count == 0);
            Assert.IsTrue(bDees.Count == 1 && bDees.Contains("a"));
            Assert.IsTrue(cDees.Count == 2 && cDees.Contains("a") && cDees.Contains("d"));
            Assert.IsTrue(dDees.Count == 0);
            Assert.IsTrue(dDees.Count == 0);
        }

        /// <summary>
        ///Nonempty graph should contain something
        ///</summary>
        [TestMethod()]
        public void ComplexGraphIndexer()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            Assert.AreEqual(0, t["a"]);
            Assert.AreEqual(1, t["b"]);
            Assert.AreEqual(2, t["c"]);
            Assert.AreEqual(0, t["d"]);
            Assert.AreEqual(0, t["e"]);
        }

        /// <summary>
        ///Removing from a DG 
        ///</summary>
        [TestMethod()]
        public void Remove()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.RemoveDependency("a", "b");
            Assert.AreEqual(2, t.Size);
        }

        /// <summary>
        /// Ensures GetDependents will return empty in both the bellow cases
        ///</summary>
        [TestMethod()]
        public void GetDependentsAfterRemove()
        {
            DependencyGraph t1 = new DependencyGraph();
            DependencyGraph t2 = new DependencyGraph();
            t1.AddDependency("a", "b");
            t1.RemoveDependency("a", "b");
            HashSet<string> t1Set = new HashSet<string>(t1.GetDependents("a"));
            HashSet<string> t2Set = new HashSet<string>(t2.GetDependents("a"));
            Assert.IsTrue(t1Set.SetEquals(t2Set));
        }

        /// <summary>
        /// Ensures GetDependees will return empty in both the bellow cases
        ///</summary>
        [TestMethod()]
        public void GetDependeesAfterRemove()
        {
            DependencyGraph t1 = new DependencyGraph();
            DependencyGraph t2 = new DependencyGraph();
            t1.AddDependency("b", "a");
            t1.RemoveDependency("b", "a");
            HashSet<string> t1Set = new HashSet<string>(t1.GetDependees("a"));
            HashSet<string> t2Set = new HashSet<string>(t2.GetDependees("a"));
            Assert.IsTrue(t1Set.SetEquals(t2Set));
        }

        /// <summary>
        ///Replace on a DG
        ///</summary>
        [TestMethod()]
        public void ReplaceDependents()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.ReplaceDependents("a", new HashSet<string>() { "x", "y", "z" });
            HashSet<String> aPends = new HashSet<string>(t.GetDependents("a"));
            Assert.IsTrue(aPends.SetEquals(new HashSet<string>() { "x", "y", "z" }));
        }

        /// <summary>
        ///Replace on a DG
        ///</summary>
        [TestMethod()]
        public void ReplaceDependees()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("d", "c");
            t.ReplaceDependees("c", new HashSet<string>() { "x", "y", "z" });
            HashSet<String> cDees = new HashSet<string>(t.GetDependees("c"));
            Assert.IsTrue(cDees.SetEquals(new HashSet<string>() { "x", "y", "z" }));
        }

        /// <summary>
        ///Adds a ton of dependent/dependee pairs and compares against a list
        ///</summary>
        [TestMethod()]
        public void ComplexTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "a");
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("a", "d");
            t.AddDependency("b", "a");
            t.AddDependency("b", "c");
            t.AddDependency("b", "d");
            t.AddDependency("c", "c");
            t.AddDependency("c", "d");
            t.AddDependency("c", "e");
            t.AddDependency("d", "a");
            t.AddDependency("d", "b");
            t.AddDependency("d", "c");
            t.AddDependency("d", "k");
            t.AddDependency("f", "a");
            t.AddDependency("g", "a");
            t.AddDependency("x", "g");
            HashSet<string> newDependents = new HashSet<string>() { "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            HashSet<string> newDependees = new HashSet<string>() { "a", "b", "c", "d", "e", "f", "g", "h", "i" };
            t.ReplaceDependents("d", newDependents);
            t.ReplaceDependents("f", newDependents);
            t.ReplaceDependents("g", newDependents);
            t.ReplaceDependees("g", newDependees);
            HashSet<String> aDents = new HashSet<string>() { "a", "b", "c", "d", "g" };
            HashSet<String> bDents = new HashSet<string>() { "a", "c", "d", "g" };
            HashSet<String> cDents = new HashSet<string>() { "c", "d", "e", "g"};
            HashSet<string> dDents = new HashSet<string>() { "g", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            HashSet<string> gDents = new HashSet<string>() { "g", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            HashSet<string> gDees = new HashSet<string>() { "a", "b", "c", "d", "e", "f", "g", "h", "i" };
            HashSet<string> kDees = new HashSet<string>() { "d", "f", "g" };
            HashSet<string> aDees = new HashSet<string>() { "a", "b" };
            Assert.IsTrue(aDents.SetEquals(t.GetDependents("a")));
            Assert.IsTrue(bDents.SetEquals(t.GetDependents("b")));
            Assert.IsTrue(cDents.SetEquals(t.GetDependents("c")));
            Assert.IsTrue(dDents.SetEquals(t.GetDependents("d")));
            Assert.IsTrue(gDents.SetEquals(t.GetDependents("g")));
            Assert.IsTrue(gDees.SetEquals(t.GetDependees("g")));
            Assert.IsTrue(kDees.SetEquals(t.GetDependees("k")));
            Assert.IsTrue(aDees.SetEquals(t.GetDependees("a")));
        }

        // ************************** STRESS TESTS ******************************** //
        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest1()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }



        // ********************************** ANOTHER STESS TEST ******************** //
        /// <summary>
        ///Using lots of data with replacement
        ///</summary>
        [TestMethod()]
        public void StressTest8()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Replace a bunch of dependents
            for (int i = 0; i < SIZE; i += 4)
            {
                HashSet<string> newDents = new HashSet<String>();
                for (int j = 0; j < SIZE; j += 7)
                {
                    newDents.Add(letters[j]);
                }
                t.ReplaceDependents(letters[i], newDents);

                foreach (string s in dents[i])
                {
                    dees[s[0] - 'a'].Remove(letters[i]);
                }

                foreach (string s in newDents)
                {
                    dees[s[0] - 'a'].Add(letters[i]);
                }

                dents[i] = newDents;
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        // ********************************** A THIRD STESS TEST ******************** //
        /// <summary>
        ///Using lots of data with replacement
        ///</summary>
        [TestMethod()]
        public void StressTest15()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 100;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 2; j < SIZE; j += 2)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Replace a bunch of dependees
            for (int i = 0; i < SIZE; i += 4)
            {
                HashSet<string> newDees = new HashSet<String>();
                for (int j = 0; j < SIZE; j += 7)
                {
                    newDees.Add(letters[j]);
                }
                t.ReplaceDependees(letters[i], newDees);

                foreach (string s in dees[i])
                {
                    dents[s[0] - 'a'].Remove(letters[i]);
                }

                foreach (string s in newDees)
                {
                    dents[s[0] - 'a'].Add(letters[i]);
                }

                dees[i] = newDees;
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }
    }
}
