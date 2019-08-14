using System;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    /// <summary>
    /// Stores patterns and efficiently retrieves pattern handler.
    /// </summary>
    public class Router
    {
        /// <summary>
        /// Represent a parameter part of the resource name.
        /// </summary>
        private class PathParam
        {
            public string Name { get; }
            public int Idx { get;  }

            public PathParam(string name, int idx)
            {
                Name = name;
                Idx = idx;
            }
        }

        /// <summary>
        /// Represents one part of the path, and has references
        /// to the next nodes, including wildcards.
        /// Only one Handler may exist per node.
        /// </summary>
        private class Node
        {
            public IResourceHandler Handler;
            public List<PathParam> Params;
            public Dictionary<string, Node> Nodes;
            public Node Param;
            public Node Wild;
        }


        /// <summary>
        /// Represents a match with a resource pattern,
        /// containing the handler and the params.
        /// </summary>
        public class Match
        {
            public IResourceHandler Handler = null;
            public Dictionary<string, string> Params = null;
        }

        /// <summary>
        /// Gets the subpattern that prefix all resources, not including the pattern of any parent Router.
        /// </summary>
        public string Pattern { get { return pattern; } }

        /// <summary>
        /// Gets the full pattern that prefix all resources, including the pattern of any parent Router.
        /// </summary>
        public string FullPattern {
            get
            {
                return parent == null ? pattern : mergePattern(parent.FullPattern, pattern);
            }
        }

        private const char PMARK = '$';
        private const char PWILD = '*';
        private const char FWILD = '>';
        private const char BTSEP = '.';

        private string pattern;
        private readonly int pathLength;
        private readonly Node root;
        private Router parent;

        public Router() : this("")
        {   
        }

        public Router(string pattern)
        {
            this.pattern = pattern;
            pathLength = pattern.Split(BTSEP).Length;
            root = new Node();
        }

        /// <summary>
        /// Registers a handler for the given resource pattern.
        ///
        /// A pattern may contain placeholders that acts as wildcards, and will be
        /// parsed and stored in the request.PathParams dictionary.
        /// A placeholder is a resource name part starting with a dollar ($) character:
        ///     s.AddHandler("user.$id", handler) // Will match "user.10", "user.foo", etc.
        /// An anonymous placeholder is a resource name part using an asterisk (*) character:
        ///     s.AddHandler("user.*", handler)   // Will match "user.10", "user.foo", etc.
        /// A full wildcard can be used as last part using a greather than (>) character:
        ///     s.AddHandler("data.>", handler)   // Will match "data.foo", "data.foo.bar", etc.
        ///
        /// If the pattern is already registered, or if there are conflicts among
        /// the handlers, an exception will be thrown.
        /// </summary>
        /// <param name="pattern">Resource pattern.</param>
        /// <param name="handler">Resource handler.</param>
        public void AddHandler(string subpattern, IResourceHandler handler)
        {
            Tuple<Node, List<PathParam>> tuple = fetch(subpattern, null);

            if (tuple.Item1.Handler != null)
            {
                throw new InvalidOperationException("Registration already done for pattern: " + mergePattern(pattern, subpattern));
            }
            tuple.Item1.Params = tuple.Item2;
            tuple.Item1.Handler = handler;
        }

        /// <summary>
        /// Attatches another Router at the pattern set on the child Router.
        /// </summary>
        /// <param name="child">Router that should be mounted.</param>
        public void Mount(Router child)
        {
            Mount("", child);
        }

        /// <summary>
        /// Attatches another Router at a given pattern.
        /// When mounting, any pattern set on the child Router will be suffixed to the subpattern.
        /// </summary>
        /// <param name="subpattern">Subpattern to mount to.</param>
        /// <param name="child">Router that should be mounted to the subpattern.</param>
        public void Mount(string subpattern, Router child)
        {
            if (child.parent != null)
            {
                throw new InvalidOperationException("Router is already mounted.");
            }
            string spattern = mergePattern(subpattern, child.pattern);
            if (spattern == "")
            {
                throw new InvalidOperationException("Attempting to mount to root.");
            }
            Tuple<Node, List<PathParam>> tuple = fetch(spattern, child.root);
            if (tuple.Item1 != child.root)
            {
                throw new InvalidOperationException("Attempting to mount to existing pattern: " + mergePattern(pattern, spattern));
            }
            child.pattern = spattern;
            child.parent = this;
        }

        private Tuple<Node, List<PathParam>> fetch(string subpattern, Node mount)
        {
            string[] tokens = subpattern.Split(BTSEP);
            Node current = root;
            Node next;
            List<PathParam> pathParams = new List<PathParam>();
            bool doMount = false;
            int tokenCount = tokens.Length;

            for (int i = 0; i < tokenCount; i++)
            {
                if (mount != null && i == (tokenCount - 1))
                {
                    doMount = true;
                }

                string t = tokens[i];
                int tokenLength = t.Length;
                if (tokenLength == 0)
                {
                    throw new ArgumentException("Invalid resource pattern: must not contain empty part.");
                }

                switch (t[0])
                {

                    // Check for path params marker
                    case PMARK:
                    case PWILD:
                        if (tokenLength == 1)
                        {
                            throw new ArgumentException("Invalid resource pattern: $ must be followed by parameter name.");
                        }
                        if (t[0] == PMARK)
                        {
                            string name = t.Substring(1);
                            foreach (PathParam p in pathParams)
                            {
                                if (p.Name == name)
                                {
                                    throw new ArgumentException(String.Format("Invalid resource pattern: placeholder {0} found multiple times in pattern: {1}", t, mergePattern(pattern, subpattern)));
                                }
                            }
                            pathParams.Add(new PathParam(name, i));
                        }
                        // Check if the current node has any param node
                        if (current.Param == null)
                        {
                            if (doMount)
                            {
                                current.Param = mount;
                            }
                            else
                            {
                                current.Param = new Node();
                            }
                        }
                        next = current.Param;
                        break;

                    case FWILD:
                        // Validate the full wildcard is last
                        if (tokenLength > 1 || i < (tokenCount - 1))
                        {
                            throw new ArgumentException("Invalid resource pattern: > (full wildcard) must be the last token.");
                        }
                        if (current.Wild == null)
                        {
                            if (doMount)
                            {
                                throw new ArgumentException(String.Format("Invalid resource pattern: attempting to mount on full wildcard pattern: {0}", mergePattern(pattern, subpattern)));
                            }
                            current.Wild = new Node();
                        }
                        next = current.Wild;
                        break;

                    default:
                        // Check if the current node has any previous child nodes
                        if (current.Nodes == null)
                        {
                            // Create a new dictionary and store a new child node in it
                            current.Nodes = new Dictionary<string, Node>();
                            if (doMount)
                            {
                                next = mount;
                            }
                            else
                            {
                                next = new Node();
                            }
                            current.Nodes.Add(t, next);
                        }
                        else
                        {
                            // Get the child node, or create a new if it doesn't exist
                            if (!current.Nodes.TryGetValue(t, out next))
                            {
                                if (doMount)
                                {
                                    next = mount;
                                }
                                else
                                {
                                    next = new Node();
                                }
                                current.Nodes.Add(t, next);
                            }
                        }
                        break;
                }

                current = next;
            }

            return new Tuple<Node, List<PathParam>>(current, pathParams);
        }

        /// <summary>
        /// Parses the resource name and gets the registered resource handler and
        /// any path params.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>Returns found match, or nil if there is no match.</returns>
        public Match GetHandler(string resourceName)
        {
            Match match = new Match();

            if (pattern.Length > 0)
            {
                if (!resourceName.StartsWith(pattern))
                {
                    return null;
                }
                if (resourceName.Length > pattern.Length)
                {
                    if (resourceName[pattern.Length] != '.')
                    {
                        return null;
                    }
                    resourceName = resourceName.Substring(pattern.Length + 1);
                }
                else
                {
                    resourceName = "";
                }
            }

            if (resourceName.Length == 0)
            {
                if (root.Handler == null)
                {
                    return null;
                }

                match.Handler = root.Handler;
                return match;
            }

            string[] tokens = resourceName.Split(BTSEP);
            matchNode(root, tokens, 0, match);

            return match.Handler == null ? null : match;
        }

        private bool matchNode(Node current, string[] tokens, int tokenIdx, Match nodeMatch)
        {            
            Node next = null;
            if (current.Nodes != null)
            {
                current.Nodes.TryGetValue(tokens[tokenIdx], out next);
            }
            tokenIdx++;
            int c = 2; // A counter to run the code below twice

            while (c > 0)
            {
                // Does the node exist
                if (next != null)
                {
                    // Check if it is the last token
                    if (tokens.Length == tokenIdx)
                    {
                        // Check if this node has handlers
                        if (next.Handler != null)
                        {
                            nodeMatch.Handler = next.Handler;
                            // Check if we have path parameters for the handlers
                            if (next.Params != null) {
                                // Create a map with path parameter values
                                nodeMatch.Params = new Dictionary<string, string>(next.Params.Count);
                                foreach (PathParam pp in next.Params)
                                {
                                    nodeMatch.Params[pp.Name] = tokens[pp.Idx];
                                }
                            }
                            return true;
				        }
			        }
                    else
                    {
				        // Match against next node
				        if (matchNode(next, tokens, tokenIdx, nodeMatch))
                        {
                            return true;
                        }
			        }
		        }

                // To avoid repeating code above, set node to test to l.param
                // and run it all again.
                next = current.Param;
                c--;
            }

            // Check full wild card
            if (current.Wild != null)
            {
                next = current.Wild;
                nodeMatch.Handler = next.Handler;
                if (next.Params != null)
                {
                    // Create a map with path parameter values
                    nodeMatch.Params = new Dictionary<string, string>(next.Params.Count);
                    foreach (PathParam pp in next.Params)
                    {
                        nodeMatch.Params[pp.Name] = tokens[pp.Idx];
                    }
                }
            }

            return false;
        }

        private string mergePattern(string a, string b)
        {
            if (a == "")
            {
                return b;
            }
            if (b == "")
            {
                return a;
            }
            return a + "." + b;
        }
    }
}
