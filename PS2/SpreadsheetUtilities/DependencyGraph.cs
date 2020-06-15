/// <author>
/// Benjamin Allred
/// </author>
/// <UID>
/// u1090524
/// </UID>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, HashSet<string>> orderedPairs;
        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            orderedPairs = new Dictionary<string, HashSet<string>>();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            //evaluates size based on how many strings are in each HashSet
            get
            {
                int count = 0;
                foreach (HashSet<string> dependeeSet in orderedPairs.Values)
                {
                    count += dependeeSet.Count;
                }
                return count;
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get
            {
                if (s == null)
                    return 0;
                int numDependees = 0;
                foreach (HashSet<string> dependeeSet in orderedPairs.Values)
                {
                    if (dependeeSet.Contains(s))
                        numDependees++;
                }
                return numDependees;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            if (s == null)
                return false;
            if (orderedPairs.ContainsKey(s))
            {
                if (orderedPairs[s].Count != 0)
                    return true;
                return false;
            }
            return false;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (s == null)
                return false;
            foreach (string dependee in orderedPairs.Keys)
            {
                if (orderedPairs[dependee].Contains(s))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (s == null)
                yield break;
            if (orderedPairs.ContainsKey(s))
            {
                foreach (string dependent in orderedPairs[s])
                {
                    yield return dependent;
                }
            }
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (s == null)
                yield break;
            foreach (string dependee in orderedPairs.Keys)
            {
                if (orderedPairs[dependee].Contains(s))
                    yield return dependee;
            }
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            if (s == null || t == null)
                return;
            if (!orderedPairs.ContainsKey(s))
            {
                orderedPairs.Add(s, new HashSet<string>());
                orderedPairs[s].Add(t);
            }
            else
            {
                orderedPairs[s].Add(t);
            }
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (s == null || t == null)
                return;
            if (!orderedPairs.ContainsKey(s))
                return;
            else
                orderedPairs[s].Remove(t);
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (s == null)
                return;
            //remove all values associated with key s
            orderedPairs.Remove(s);
            //adds all the new dependents to a new HashSet
            orderedPairs.Add(s, new HashSet<string>(newDependents));
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            if (s == null)
                return;
            //remove every instance where s is a dependent
            foreach (string dependee in orderedPairs.Keys)
            {
                orderedPairs[dependee].Remove(s);
            }

            //add all new instances where s is a dependent
            foreach (string newDependee in newDependees)
            {
                AddDependency(newDependee, s);
            }
        }

    }

}