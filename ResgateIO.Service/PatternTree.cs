using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    /// <summary>
    /// Stores patterns and efficiently retrieves pattern handler.
    /// </summary>
    internal class PatternTree
    {
        /// <summary>
        /// Represent a parameter part of the resource name.
        /// </summary>
        internal class PathParam
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
        internal class Node
        {
            public IResourceHandler Handler;
            public List<PathParam> Params;
            public Dictionary<string, Node> Nodes;
            public Node Param;
        }


        /// <summary>
        /// Represents a match with a resource pattern,
        /// containing the handler and the params.
        /// </summary>
        internal class Match
        {
            public IResourceHandler Handler = null;
            public Dictionary<string, string> Params = null;
        }

        private const char PMARK = '$';
        private const char BTSEP = '.';

        private readonly Node root = new Node();

        /// <summary>
        /// Inserts a new handler to the pattern tree.
        /// An invalid pattern, or a pattern already registered will cause an exception.
        /// </summary>
        /// <param name="pattern">Resource pattern. May contain parameter placeholders by starting with $. Eg. "example.user.$id.roles"</param>
        /// <param name="handler">Resource Handler</param>
        public void Add(string pattern, IResourceHandler handler)
        {
            Node current = root;
            Node next;
            List<PathParam> pathParams = null;

            if (pattern.Length > 0)
            {
                string[] tokens = pattern.Split(BTSEP);
                pathParams = new List<PathParam>();

                for (int i = 0; i < tokens.Length; i++)
                {
                    string t = tokens[i];
                    int lt = t.Length;
                    if (lt == 0)
                    {
                        throw new ArgumentException("Invalid resource pattern: must not contain empty part");
                    }

                    // Check for path params marker
                    if (t[0] == PMARK)
                    {
                        if (lt == 1)
                        {
                            throw new ArgumentException("Invalid resource pattern: $ must be followed by parameter name");
                        }
                        pathParams.Add(new PathParam(t.Substring(1), i));

                        // Check if the current node has any param node
                        if (current.Param == null)
                        {
                            // Create a new param node
                            current.Param = new Node();
                            next = current.Param;
                        }
                        next = current.Param;
                    }
                    else
                    {
                        // Check if the current node has any previous child nodes
                        if (current.Nodes == null)
                        {
                            // Create a new dictionary and store a new child node in it
                            current.Nodes = new Dictionary<string, Node>();
                            next = new Node();
                            current.Nodes.Add(t, next);
                        }
                        else
                        {
                            // Get the child node, or create a new if it doesn't exist
                            if (!current.Nodes.TryGetValue(t, out next))
                            {
                                next = new Node();
                                current.Nodes.Add(t, next);
                            }
                        }
                    }

                    current = next;
                }
            }

            if (current.Handler != null)
            {
                throw new InvalidOperationException("Registration already done for pattern " + pattern);
            }

            current.Params = pathParams;
            current.Handler = handler;
        }

        /// <summary>
        /// Parses the resource name and gets the registered resource handler and
        /// any path params.
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>Returns found match, or nil if there is no match.</returns>
        public Match Get(string resourceName)
        {
            Match match = new Match();

            if (resourceName.Length == 0)
            {
                if (root.Handler == null)
                {
                    return null;
                }

                match.Handler = root.Handler;
                return new Match();
            }

            string[] tokens = resourceName.Split(BTSEP);
            matchNode(root, tokens, 0, match);

            return match.Handler == null ? null : match;
        }

        private bool matchNode(Node current, string[] tokens, int pathIdx, Match match)
        {
            string token = tokens[pathIdx];
            pathIdx++;
            bool found = current.Nodes.TryGetValue(token, out Node next);
            int c = 2; // A counter to run the code below twice

            while (c > 0)
            {
                // Does the node exist
                if (found)
                {
                    // Check if it is the last token
                    if (tokens.Length == pathIdx)
                    {
                        // Check if this node has handlers
                        if (next.Handler != null)
                        {
                            match.Handler = next.Handler;
                            // Check if we have path parameters for the handlers
                            if (next.Params != null) {
                                // Create a map with path parameter values
                                match.Params = new Dictionary<string, string>(next.Params.Count);
                                foreach (PathParam pp in next.Params)
                                {
                                    match.Params[pp.Name] = tokens[pp.Idx];
                                }
                            }
                            return true;
				        }
			        }
                    else
                    {
				        // Match against next node
				        if (matchNode(next, tokens, pathIdx, match))
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

            return false;
        }
    }
}
