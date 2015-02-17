// A Force-Directed Diagram Layout Algorithm
// Bradley Smith - 2010/07/01

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ForceDirectedGraph
{

	/// <summary>
	/// Represents a simple diagram consisting of nodes and connections, implementing a 
	/// force-directed algorithm for automatically arranging the nodes.
	/// </summary>
	public class Diagram
	{

		private const double ATTRACTION_CONSTANT = 0.1;		// spring constant
		private const double REPULSION_CONSTANT = 10000;	// charge constant

		public const double DEFAULT_DAMPING = 0.5;
		public const int DEFAULT_SPRING_LENGTH = 100;
		public const int DEFAULT_MAX_ITERATIONS = 500;

		private Form form;
		private Random rnd;

		private List<Node> nodes;
		public List<NodeLayoutInfo> layout;

		/// <summary>
		/// Gets a read-only collection of the nodes in this Diagram.
		/// </summary>
		public IList<Node> Nodes
		{
			get
			{
				return nodes.AsReadOnly();
			}
		}

		/// <summary>
		/// Initialises a new instance of the Diagram class.
		/// </summary>
		public Diagram(Form form)
		{
			nodes = new List<Node>();
			layout = new List<NodeLayoutInfo>();
			this.form = form;
		}

		/// <summary>
		/// Adds the specified Node to this Diagram.
		/// </summary>
		/// <param name="node">The Node to add to the diagram.</param>
		/// <returns>True if the node was added, false if the node is already on this Diagram.</returns>
		public bool AddNode(Node node)
		{
			bool ret = false;

			if (node == null) throw new ArgumentNullException("node");

			if (!nodes.Contains(node))
			{
				// add node, associate with diagram, then add all connected nodes
				nodes.Add(node);
				node.Diagram = this;
				layout.Add(new NodeLayoutInfo(node, new Vector(), node.Location));

				foreach (Node child in node.Connections)
				{
					AddNode(child);
				}

				ret = true;
			}

			return ret;
		}

		/// <summary>
		/// Removes the specified node from the diagram. Any connected nodes will remain on the diagram.
		/// </summary>
		/// <param name="node">The node to remove from the diagram.</param>
		/// <returns>True if the node belonged to the diagram.</returns>
		public bool RemoveNode(Node node)
		{
			bool ret;

			// Disconnect this node from other nodes.

			foreach (Node other in nodes)
			{
				if ((other != node) && other.Connections.Contains(node))
				{
					other.Disconnect(node);
				}
			}

			ret = nodes.Remove(node);
			layout.Remove(layout.Single(n => n.Node == node));

			return ret;
		}

		/// <summary>
		/// Runs the force-directed layout algorithm on this Diagram, using the default parameters.
		/// </summary>
		public void Arrange()
		{
			Arrange(DEFAULT_DAMPING, DEFAULT_SPRING_LENGTH, DEFAULT_MAX_ITERATIONS, true);
		}

		/// <summary>
		/// Runs the force-directed layout algorithm on this Diagram, offering the option of a random or deterministic layout.
		/// </summary>
		/// <param name="deterministic">Whether to use a random or deterministic layout.</param>
		public void Arrange(bool deterministic)
		{
			Arrange(DEFAULT_DAMPING, DEFAULT_SPRING_LENGTH, DEFAULT_MAX_ITERATIONS, deterministic);
		}

		/// <summary>
		/// Runs the force-directed layout algorithm on this Diagram, using the specified parameters.
		/// </summary>
		/// <param name="damping">Value between 0 and 1 that slows the motion of the nodes during layout.</param>
		/// <param name="springLength">Value in pixels representing the length of the imaginary springs that run along the connectors.</param>
		/// <param name="maxIterations">Maximum number of iterations before the algorithm terminates.</param>
		/// <param name="deterministic">Whether to use a random or deterministic layout.</param>
		public void Arrange(double damping, int springLength, int maxIterations, bool deterministic)
		{
			// random starting positions can be made deterministic by seeding System.Random with a constant
			rnd = deterministic ? new Random(0) : new Random();

			// copy nodes into an array of metadata and randomise initial coordinates for each node

			for (int i = 0; i < nodes.Count; i++)
			{
				layout.Add(new NodeLayoutInfo(nodes[i], new Vector(), Point.Empty));
				// Points must vary slightly in order to introduce a variation in the bearing angle for each point.
				// Otherwise, with the bearing angles all the same, the forces will end up all being the same.
				layout[i].Node.Location = new Point(0, 0);
			}
		}

		public void Iterate(double damping, int springLength, int maxIterations)
		{
			for (int i = 0; i < layout.Count; i++)
			{
				NodeLayoutInfo current = layout[i];

				// express the node's current position as a vector, relative to the origin
				Vector currentPosition = new Vector(CalcDistance(Point.Empty, current.Node.Location), GetBearingAngle(Point.Empty, current.Node.Location));
				Vector netForce = new Vector(0, 0);

				// determine repulsion between nodes
				foreach (Node other in nodes)
				{
					if (other != current.Node)
					{
						netForce += CalcRepulsionForce(current.Node, other);
					}
				}

				// determine attraction caused by connections
				foreach (Node child in current.Node.Connections)
				{
					netForce += CalcAttractionForce(current.Node, child, springLength);
				}

				foreach (Node parent in nodes)
				{
					if (parent.Connections.Contains(current.Node))
					{
						netForce += CalcAttractionForce(current.Node, parent, springLength);
					}
				}

				// apply net force to node velocity
				current.Velocity = (current.Velocity + netForce) * damping;

				if (current.Velocity.Magnitude > 5)
				{
					current.Velocity.Magnitude = 5;
				}

				// apply velocity to node position
				current.NextPosition = (currentPosition + current.Velocity).ToPointF();
			}

			// move nodes to resultant positions (and calculate total displacement)
			for (int i = 0; i < layout.Count; i++)
			{
				NodeLayoutInfo current = layout[i];

				// totalDisplacement += CalcDistance(current.Node.Location, current.NextPosition);
				current.Node.Location = current.NextPosition;
			}

			// center the diagram around the origin
			Rectangle logicalBounds = GetDiagramBounds();
			Point midPoint = new Point(logicalBounds.X + (logicalBounds.Width / 2), logicalBounds.Y + (logicalBounds.Height / 2));

			foreach (Node node in nodes)
			{
				node.Location -= (Size)midPoint;
			}
		}

		/// <summary>
		/// Calculates the attraction force between two connected nodes, using the specified spring length.
		/// </summary>
		/// <param name="x">The node that the force is acting on.</param>
		/// <param name="y">The node creating the force.</param>
		/// <param name="springLength">The length of the spring, in pixels.</param>
		/// <returns>A Vector representing the attraction force.</returns>
		private Vector CalcAttractionForce(Node x, Node y, double springLength)
		{
			double proximity = Math.Max(CalcDistance(x.Location, y.Location), 1);

			// Hooke's Law: F = -kx
			double force = ATTRACTION_CONSTANT * Math.Max(proximity - springLength, 0);
			double angle = GetBearingAngle(x.Location, y.Location);

			return new Vector(force, angle);
		}

		/// <summary>
		/// Calculates the distance between two points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point.</param>
		/// <returns>The pixel distance between the two points.</returns>
		public static double CalcDistance(PointF a, PointF b)
		{
			double xDist = (a.X - b.X);
			double yDist = (a.Y - b.Y);

			return Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));
		}

		/// <summary>
		/// Calculates the repulsion force between any two nodes in the diagram space.
		/// </summary>
		/// <param name="x">The node that the force is acting on.</param>
		/// <param name="y">The node creating the force.</param>
		/// <returns>A Vector representing the repulsion force.</returns>
		private Vector CalcRepulsionForce(Node x, Node y)
		{
			double proximity = Math.Max(CalcDistance(x.Location, y.Location), 1);

			// Coulomb's Law: F = k(Qq/r^2)
			double force = -(REPULSION_CONSTANT / Math.Pow(proximity, 2));
			double angle = GetBearingAngle(x.Location, y.Location);

			return new Vector(force, angle);
		}

		/// <summary>
		/// Removes all nodes and connections from the diagram.
		/// </summary>
		public void Clear()
		{
			nodes.Clear();
			layout.Clear();
		}

		/// <summary>
		/// Determines whether the diagram contains the specified node.
		/// </summary>
		/// <param name="node">The node to test.</param>
		/// <returns>True if the diagram contains the node.</returns>
		public bool ContainsNode(Node node)
		{
			return nodes.Contains(node);
		}

		/// <summary>
		/// Draws the diagram using GDI+, centering and scaling within the specified bounds.
		/// </summary>
		/// <param name="graphics">GDI+ Graphics surface.</param>
		/// <param name="bounds">Bounds in which to draw the diagram.</param>
		public void Draw(Graphics graphics, Rectangle bounds)
		{
			// center the diagram around the origin
			Rectangle logicalBounds = GetDiagramBounds();
			Point midPoint = new Point(logicalBounds.X + (logicalBounds.Width / 2), logicalBounds.Y + (logicalBounds.Height / 2));

			foreach (Node node in nodes)
			{
				node.Location -= (Size)midPoint;
			}

			PointF center = new Point(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));

			// determine the scaling factor
			double scale = 1;
			if (logicalBounds.Width > logicalBounds.Height)
			{
				if (logicalBounds.Width != 0)
				{
					scale = (double)Math.Min(bounds.Width, bounds.Height) / (double)logicalBounds.Width;
				}
			}
			else
			{
				if (logicalBounds.Height != 0)
				{
					scale = (double)Math.Min(bounds.Width, bounds.Height) / (double)logicalBounds.Height;
				}
			}

			/*
			// draw all of the connectors first
			foreach (Node node in nodes)
			{
				PointF source = ScalePoint(node.Location, scale);

				// connectors
				foreach (Node other in node.Connections)
				{
					PointF destination = ScalePoint(other.Location, scale);
					node.DrawConnector(graphics, center + new SizeF(source), center + new SizeF(destination), other);
				}
			}
				*/

			// then draw all of the nodes
			foreach (Node node in nodes)
			{
				PointF destination = ScalePoint(node.Location, scale);

				Size nodeSize = node.Size;
				RectangleF nodeBounds = new RectangleF(center.X + destination.X - (nodeSize.Width / 2), center.Y + destination.Y - (nodeSize.Height / 2), nodeSize.Width, nodeSize.Height);
				node.DrawNode(graphics, nodeBounds);
			}
		}

		/// <summary>
		/// Calculates the bearing angle from one point to another.
		/// </summary>
		/// <param name="start">The node that the angle is measured from.</param>
		/// <param name="end">The node that creates the angle.</param>
		/// <returns>The bearing angle, in degrees.</returns>
		private double GetBearingAngle(PointF start, PointF end)
		{
			double angle = 0;

			if (start != end)
			{
				angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
				angle = angle * 360 / (2.0 * 3.1415);
			}
			else
			{
				angle = rnd.Next(360);
			}

			return angle;
		}

		/// <summary>
		/// Determines the logical bounds of the diagram. This is used to center and scale the diagram when drawing.
		/// </summary>
		/// <returns>A System.Drawing.Rectangle that fits exactly around every node in the diagram.</returns>
		private Rectangle GetDiagramBounds()
		{
			int minX = Int32.MaxValue, minY = Int32.MaxValue;
			int maxX = Int32.MinValue, maxY = Int32.MinValue;
			foreach (Node node in nodes)
			{
				if (node.X < minX) minX = (int)node.X;
				if (node.X > maxX) maxX = (int)node.X;
				if (node.Y < minY) minY = (int)node.Y;
				if (node.Y > maxY) maxY = (int)node.Y;
			}

			return Rectangle.FromLTRB(minX, minY, maxX, maxY);
		}

		/// <summary>
		/// Applies a scaling factor to the specified point, used for zooming.
		/// </summary>
		/// <param name="point">The coordinates to scale.</param>
		/// <param name="scale">The scaling factor.</param>
		/// <returns>A System.Drawing.Point representing the scaled coordinates.</returns>
		private PointF ScalePoint(PointF point, double scale)
		{
			return new PointF(((float)(point.X * scale)), ((float)(point.Y * scale)));
		}

		/// <summary>
		/// Private inner class used to track the node's position and velocity during simulation.
		/// </summary>
		public class NodeLayoutInfo
		{

			public Node Node;			// reference to the node in the simulation
			public Vector Velocity;		// the node's current velocity, expressed in vector form
			public PointF NextPosition;	// the node's position after the next iteration

			/// <summary>
			/// Initialises a new instance of the Diagram.NodeLayoutInfo class, using the specified parameters.
			/// </summary>
			/// <param name="node"></param>
			/// <param name="velocity"></param>
			/// <param name="nextPosition"></param>
			public NodeLayoutInfo(Node node, Vector velocity, PointF nextPosition)
			{
				Node = node;
				Velocity = velocity;
				NextPosition = nextPosition;
			}
		}
	}
}