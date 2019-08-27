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

        private class Group
        {
            public bool UseResourceName = false;
            public string[] Parts = null;
            public int[] PartIdx = null;

            public Group(string group, string pattern)
            {
                if (String.IsNullOrEmpty(group))
                {
                    UseResourceName = true;
                }
                else
                {
                    parseGroup(group, pattern);
                }
            }

            public string ToString(string resourceName)
            {
                if (UseResourceName)
                {
                    return resourceName;
                }

                int len = Parts.Length;
                if (len == 1 && Parts[0] != String.Empty)
                {
                    return Parts[0];
                }

                string[] tokens = resourceName.Split(BTSEP);
                string[] strs = new string[len];
                for (int i = 0; i < len; i++)
                {
                    if (Parts[i] == String.Empty)
                    {
                        strs[i] = tokens[PartIdx[i]];
                    }
                    else
                    {
                        strs[i] = Parts[i];
                    }
                }
                return String.Concat(strs);
            }

            private void parseGroup(string g, string p)
            {
                List<string> parts = new List<string>();
                List<int> partIdx = new List<int>();

                string[] tokens = p.Split(BTSEP);
                int len = g.Length;
                int offset = 0;
                while (offset < len)
                {
                    int i = g.IndexOf('$', offset);
                    if (i == -1)
                    {
                        i = len;
                    }
                    if (i > offset)
                    { 
                        parts.Add(g.Substring(offset, i - offset));
                        partIdx.Add(-1);
                        if (i == len)
                        {
                            break;
                        }
                    }

                    i++;
                    if (i == len)
                    {
                        throw new ArgumentException("Unexpected end of group tag.");
                    }
                    if (g[i] != '{')
                    {
                        throw new ArgumentException(String.Format("Expected character \"{{\" at pos {0}", i));
                    }
                    i++;
                    offset = i;
                    while (i != len && g[i] != '}')
                    {
                        char c = g[i];
                        if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9') && c != '_' && c != '-')
                        {
                            throw new ArgumentException(String.Format("Non alpha-numeric (a-z0-9_-) character in group tag at pos {0}", i));
                        }
                        i++;
                    }
                    if (i == len)
                    {
                        throw new ArgumentException("Unexpected end of group tag.");
                    }
                    if (i == offset)
                    {
                        throw new ArgumentException(String.Format("Empty group tag at pos {0}", i-2));
                    }
                    string tag = "$" + g.Substring(offset, i - offset);
                    int tIdx = Array.IndexOf(tokens, tag);
                    if (tIdx < 0)
                    {
                        throw new ArgumentException(String.Format("Group tag {0} not found in pattern", tag));
                    }
                    parts.Add(String.Empty);
                    partIdx.Add(tIdx);
                    offset = i + 1;
                }
                Parts = parts.ToArray();
                PartIdx = partIdx.ToArray();
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
            public Group Group;
            public List<PathParam> Params;
            public Dictionary<string, Node> Nodes;
            public Node Param;
            public Node Wild;
            public bool IsMounted;
        }

        /// <summary>
        /// Represents a match with a resource pattern,
        /// containing the handler and the params.
        /// </summary>
        public class Match
        {
            /// <summary>
            /// Registered handler.
            /// </summary>
            public IResourceHandler Handler { get; }

            /// <summary>
            /// Path parameters derived from the resource name. Null if no parameters are defined.
            /// </summary>
            public Dictionary<string, string> Params { get; }

            /// <summary>
            /// Worker group handling the resource.
            /// </summary>
            public string Group { get; }

            public Match(IResourceHandler handler, Dictionary<string, string> pathParams, string group)
            {
                Handler = handler;
                Params = pathParams;
                Group = group;
            }
        }

        private class InternalMatch
        {
            public Node Node = null;
            public Dictionary<string, string> Params = null;
        }

        /// <summary>
        /// Gets the subpattern that prefix all resources, not including the pattern of any parent Router.
        /// </summary>
        public string Pattern { get; private set; }

        /// <summary>
        /// Gets the full pattern that prefix all resources, including the pattern of any parent Router.
        /// </summary>
        public string FullPattern {
            get
            {
                return parent == null ? Pattern : MergePattern(parent.FullPattern, Pattern);
            }
        }

        private const char PMARK = '$';
        private const char PWILD = '*';
        private const char FWILD = '>';
        private const char BTSEP = '.';
        private readonly Node root;
        private Router parent;

        /// <summary>
        /// Initializes a new instance of the Router class without any prefixing pattern.
        /// </summary>
        public Router() : this("")
        {   
        }

        /// <summary>
        /// Initializes a new instance of the Router class with a prefixing pattern.
        /// </summary>
        /// <param name="pattern">Pattern to prefix all routed resources with.</param>
        public Router(string pattern)
        {
            Pattern = pattern;
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
        /// 
        /// All handling of the resource will be done in order on a single worker thread.
        /// </summary>
        /// <remarks>The resource uses its own pattern as group pattern.</remarks>
        /// <param name="subpattern">Resource pattern.</param>
        /// <param name="handler">Resource handler.</param>
        public void AddHandler(string subpattern, IResourceHandler handler)
        {
            AddHandler(subpattern, null, handler);
        }

        /// <summary>
        /// Registers a handler for the given resource pattern that belongs to a worker group.
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
        /// 
        /// All resources of the same group will be handled in order on a single worker thread.
        /// The group may contain tags, ${tagName}, where the tag name matches a parameter
        /// placeholder name in the resource pattern.
        /// </summary>
        /// <param name="subpattern">Resource subpattern.</param>
        /// <param name="group">Group pattern. Null or empty means using the resource name as group.</param>
        /// <param name="handler">Resource handler.</param>
        public void AddHandler(string subpattern, string group, IResourceHandler handler)
        {
            handler = handler ?? throw new ArgumentNullException("handler must not be null.");            

            Tuple<Node, List<PathParam>> tuple = fetch(subpattern, null);

            if (tuple.Item1.Handler != null)
            {
                throw new ArgumentException("Registration already done for pattern: " + MergePattern(Pattern, subpattern));
            }
            Node n = tuple.Item1;
            n.Group = new Group(group, subpattern);
            n.Params = tuple.Item2;
            n.Handler = handler;
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
            string spattern = MergePattern(subpattern, child.Pattern);
            if (spattern == "")
            {
                throw new InvalidOperationException("Attempting to mount to root.");
            }
            Tuple<Node, List<PathParam>> tuple = fetch(spattern, child.root);
            if (tuple.Item1 != child.root)
            {
                throw new InvalidOperationException("Attempting to mount to existing pattern: " + MergePattern(Pattern, spattern));
            }
            child.Pattern = spattern;
            child.root.IsMounted = true;
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
            int mountIdx = 0;

            for (int i = 0; i < tokenCount; i++)
            {
                if (mount != null && i == (tokenCount - 1))
                {
                    doMount = true;
                }
                if (current.IsMounted)
                {
                    mountIdx = i;
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
                                    throw new ArgumentException(String.Format("Invalid resource pattern: placeholder {0} found multiple times in pattern: {1}", t, MergePattern(Pattern, subpattern)));
                                }
                            }
                            pathParams.Add(new PathParam(name, i - mountIdx));
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
                                throw new ArgumentException(String.Format("Invalid resource pattern: attempting to mount on full wildcard pattern: {0}", MergePattern(Pattern, subpattern)));
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
        /// <returns>Returns found match, or null if there is no match.</returns>
        public Match GetHandler(string resourceName)
        {
            InternalMatch match = new InternalMatch();
            string subrname = resourceName;
            if (Pattern.Length > 0)
            {
                if (!resourceName.StartsWith(Pattern))
                {
                    return null;
                }
                if (resourceName.Length > Pattern.Length)
                {
                    if (resourceName[Pattern.Length] != '.')
                    {
                        return null;
                    }
                    subrname = resourceName.Substring(Pattern.Length + 1);
                }
                else
                {
                    subrname = "";
                }
            }

            if (subrname.Length == 0)
            {
                if (root.Handler == null)
                {
                    return null;
                }

                return new Match(root.Handler, null, root.Group.ToString(resourceName));
            }

            string[] tokens = subrname.Split(BTSEP);
            matchNode(root, tokens, 0, 0, match);

            return match.Node == null ? null : new Match(match.Node.Handler, match.Params, match.Node.Group.ToString(resourceName));            
        }

        /// <summary>
        /// Traverses through the registered handlers to see if any of them matches the predicate.
        /// </summary>
        /// <param name="predicate">Predicate to match.</param>
        /// <returns>True if a handler matching the predicate is found, otherwise false.</returns>
        public bool Contains(Predicate<IResourceHandler> predicate)
        {
            return contains(root, predicate);
        }

        private bool matchNode(Node current, string[] tokens, int tokenIdx, int mountIdx, InternalMatch nodeMatch)
        {            
            Node next = null;
            if (current.Nodes != null)
            {
                current.Nodes.TryGetValue(tokens[tokenIdx], out next);
            }
            if (current.IsMounted)
            {
                mountIdx = tokenIdx;
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
                            nodeMatch.Node = next;
                            // Check if we have path parameters for the handlers
                            if (next.Params != null) {
                                // Create a map with path parameter values
                                nodeMatch.Params = new Dictionary<string, string>(next.Params.Count);
                                foreach (PathParam pp in next.Params)
                                {
                                    nodeMatch.Params[pp.Name] = tokens[pp.Idx + mountIdx];
                                }
                            }
                            return true;
                        }
                    }
                    else
                    {
                        // Match against next node
                        if (matchNode(next, tokens, tokenIdx, mountIdx, nodeMatch))
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
                nodeMatch.Node = next;
                if (next.Params != null)
                {
                    // Create a map with path parameter values
                    nodeMatch.Params = new Dictionary<string, string>(next.Params.Count);
                    foreach (PathParam pp in next.Params)
                    {
                        nodeMatch.Params[pp.Name] = tokens[pp.Idx + mountIdx];
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Merges two pattern with a separating dot if needed.
        /// The patterns may be empty.
        /// </summary>
        /// <param name="a">Prefixing pattern.</param>
        /// <param name="b">Suffixing pattern.</param>
        /// <returns>Merged pattern.</returns>
        internal static string MergePattern(string a, string b)
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

        private bool contains(Node n, Predicate<IResourceHandler> predicate)
        {
            if (n.Wild != null && n.Wild.Handler != null && predicate(n.Wild.Handler))
            {
                return true;
            }
            if (
                n.Param != null &&
                (
                    (n.Param.Handler != null && predicate(n.Param.Handler)) ||
                    contains(n.Param, predicate)
                )
            )
            {
                return true;
            }
            if (n.Nodes != null)
            {
                foreach (KeyValuePair<string, Node> pair in n.Nodes)
                {
                    Node nn = pair.Value;
                    if (

                        (nn.Handler != null && predicate(nn.Handler)) ||
                        contains(nn, predicate)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
