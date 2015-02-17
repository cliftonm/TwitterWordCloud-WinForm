using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Tweetinvi;
using Tweetinvi.Core.Enum;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Interfaces.Controllers;
using Tweetinvi.Core.Interfaces.DTO;
using Tweetinvi.Core.Interfaces.DTO.QueryDTO;
using Tweetinvi.Core.Interfaces.Models;
using Tweetinvi.Core.Interfaces.Models.Parameters;
using Tweetinvi.Core.Interfaces.oAuth;
using Tweetinvi.Core.Interfaces.Streaminvi;
using Tweetinvi.Json;
using Geo = Tweetinvi.Geo;
using SavedSearch = Tweetinvi.SavedSearch;
using Stream = Tweetinvi.Stream;

using ForceDirectedGraph;

namespace twitterWordCloud
{
	public partial class AppForm : Form
	{
		public const int MaxWords = 100;
		public const int SaturationCount = 20;
		public const int MaxTweets = 20;

		public static int CountThreshold = 0;

		// protected Dictionary<string, int> wordCount = new Dictionary<string, int>();
		protected Dictionary<string, TextNode> wordNodeMap = new Dictionary<string, TextNode>();

		// We implement a first-in, first-out list of tweets so we can easily remove old tweets, so this list doesn't grow forever.
		protected Dictionary<string, Queue<string>> wordTweetMap = new Dictionary<string, Queue<string>>();

		protected System.Windows.Forms.Timer timer;
		protected Queue<string> tweetQueue = new Queue<string>();

		protected Diagram diagram;
		protected Random rnd;

		protected List<SpotNode> newNodes = new List<SpotNode>();
		protected Node rootNode = null;
		protected int iteration = 0;
		protected int totalWordCount = 0;
		protected int paintIteration = 0;
		protected Brush brushBlack = new SolidBrush(Color.Black);
		protected Font font = new Font(FontFamily.GenericSansSerif, 12);

		protected IFilteredStream stream;

		protected List<string> skipWords = new List<string>(new string[] { "a", "an", "and", "the", "it", "them", "their", "those", "us", "you", "I", "they", "in", "on", "with", "at", "under", "over", "above", "below",
			"we", "by", "to", "that", "can", "can't", "who", "are", "only", "now", "him", "her", "from", "he", "she", "for", "every", "so", "our", "of", "yours", "all", "was", "will", "is", "having", "as", "up", "down", "out", "after", "not", "be", "my", "rt",
			"this", "or", "nor", "these", "off", "on", "his", "its", "because", "no", "amp", "ur", "me", "how", "has", "have", "into"});

		protected List<string> punctuation = new List<string>(new string[] { ".", ",", ";", "?", "!" });

		protected string mouseWord = "";
		protected TweetForm tweetForm;

		public AppForm()
		{
			InitializeComponent();
			Setup();
			TwitterAuth();
		}

		protected void TwitterAuth()
		{
			string[] keys = File.ReadAllLines("twitterauth.txt");
			TwitterCredentials.SetCredentials(keys[0], keys[1], keys[2], keys[3]);
		}

		protected void Setup()
		{

			diagram = new Diagram(this);
			rnd = new Random();
			bool overrun = false;

			diagram.Arrange();

			timer = new System.Windows.Forms.Timer();
			timer.Interval = 1000 / 20;		// 20 times a second, in milliseconds.
			timer.Tick += (sender, args) => pnlCloud.Invalidate(true);

			pnlCloud.Paint += (sender, args) =>
			{
				Graphics gr = args.Graphics;
				gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

				++paintIteration;
				// ReduceCounts();

				if (!overrun)
				{
					overrun = true;
					int maxTweets = 20;

					// We assume here that we can parse the data faster than the incoming stream hands it to us.
					// But we put in a safety check to handle only 20 tweets.
					while (tweetQueue.Count > 0 && (--maxTweets > 0))
					{
						string tweet;

						lock (this)
						{
							tweet = tweetQueue.Dequeue();
						}

						SynchronousUpdate(tweet);
					}

					// gr.Clear(Color.White);
					diagram.Iterate(Diagram.DEFAULT_DAMPING, Diagram.DEFAULT_SPRING_LENGTH, Diagram.DEFAULT_MAX_ITERATIONS);
					diagram.Draw(gr, Rectangle.FromLTRB(12, 24, pnlCloud.Width - 12, pnlCloud.Height - 36));

					// gr.DrawString(paintIteration.ToString() + "  " + diagram.Nodes.Count.ToString() + "  " + tweetQueue.Count.ToString() + "  " + diagram.layout.Count.ToString(), font, brushBlack, new Point(3, 3));

					overrun = false;
				}
				else
				{
					gr.DrawString("overrun", font, brushBlack, new Point(3, 3));
				}
			};

			pnlCloud.MouseMove += OnMouseMove;
			pnlCloud.MouseLeave += (sender, args) =>
				{
					if (tweetForm != null)
					{
						tweetForm.Close();
						tweetForm=null;
						mouseWord=String.Empty;
					}
				};

			timer.Start();

			Node node = new SpotNode(Color.Black);
			rootNode = node;
			diagram.AddNode(node);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			// redraw on resize
			Invalidate();
		}

		/// <summary>
		/// Display tweets for the word the user is hovering over.
		/// If a tweet popup is currently displayed, move popup window until the mouse is over a different word.
		/// </summary>
		protected void OnMouseMove(object sender, MouseEventArgs args)
		{
			var hits = wordNodeMap.Where(w => w.Value.Region.Contains(args.Location));
			Point windowPos = PointToScreen(args.Location);
			windowPos.Offset(50, 70);

			if (hits.Count() > 0)
			{
				string word = hits.First().Key;
				TextNode node = hits.First().Value;

				if (mouseWord == word)
				{
					tweetForm.Location = windowPos;
				}
				else
				{
					if (tweetForm == null)
					{
						tweetForm = new TweetForm();
						tweetForm.Location = windowPos;
						tweetForm.Show();
						tweetForm.TopMost = true;
					}

					// We have a new word.
					tweetForm.tbTweets.Clear();
					ShowTweets(word);
					mouseWord = word;
				}
			}
			else
			{
				// Just move the window.
				if (tweetForm != null)
				{
					tweetForm.Location = windowPos;
					tweetForm.TopMost = true;
				}
			}
		}

		/// <summary>
		/// Display the tweets in the textbox.
		/// </summary>
		protected void ShowTweets(string word)
		{
			foreach (string tweet in wordTweetMap[word])
			{
				tweetForm.tbTweets.AppendText(tweet + "\r\n");
			}
		}

		protected void UpdateFdg(string text)
		{
			lock (this)
			{
				tweetQueue.Enqueue(text);
			}
		}

		protected void SynchronousUpdate(string tweet)
		{
			string[] words = tweet.Split(' ');

			++iteration;
			ReduceCounts();

			foreach (string w in words)
			{
				string word = w.StripPunctuation();
				string lcword = word.ToLower();
				TextNode node;

				if (!EliminateWord(lcword))
				{
					if (!wordNodeMap.TryGetValue(lcword, out node))
					{
						++totalWordCount;
						PointF p = rootNode.Location;
						RemoveAStaleWord();
						TextNode n = new TextNode(word, p);
						rootNode.AddChild(n);
						wordNodeMap[lcword] = n;
						wordTweetMap[lcword] = new Queue<string>(new string[] { tweet });
					}
					else
					{
						wordNodeMap[lcword].IncrementCount();
						Queue<string> tweets = wordTweetMap[lcword];

						// Throw away the oldest tweet if we have more than 20 associated with this word.
						if (tweets.Count > MaxTweets)
						{
							tweets.Dequeue();
						}

						tweets.Enqueue(tweet);
					}
				}
			}
		}

		/// <summary>
		/// Remove the stalest 1 hit count word from the list -- this is the word that has not been updated the longest.
		/// We do this only when the word count exceends MaxWords
		/// </summary>
		protected void RemoveAStaleWord()
		{
			if (wordNodeMap.Count > MaxWords)
			{
				// TODO: Might be more efficient to maintain a sorted list to begin with!
				DateTime now = DateTime.Now;
				KeyValuePair<string, TextNode> tnode = wordNodeMap.Where(w => w.Value.Count==1).OrderByDescending(w => (now - w.Value.UpdatedOn).TotalMilliseconds).First();
				// Do not call RemoveNode, as this results in a stack overflow because the property setter has this side effect.
				tnode.Value.Diagram = null;					// THIS REMOVES THE NODE FROM THE DIAGRAM.  
				wordNodeMap.Remove(tnode.Key);
				wordTweetMap.Remove(tnode.Key);
			}
		}

		/// <summary>
		/// Prevent saturation by decrementing all word counts every 20 tweets.
		/// </summary>
		protected void ReduceCounts()
		{
			// Every 20 iterations (the default for SaturationCount), decrement the word count on all non-1 count words.
			// This allows us to eventually replace old words no longer comning up in new tweets.
			if (iteration % SaturationCount == 0)
			{
				iteration = 0;
				wordNodeMap.Where(wc => wc.Value.Count > 1).Select(wc => wc.Key).ForEach(w => wordNodeMap[w].DecrementCount());
			}
		}

		/// <summary>
		/// Return true if the word should be eliminated.
		/// The word should be in lowercase!
		/// </summary>
		protected bool EliminateWord(string word)
		{
			bool ret = false;
			int n;

			if (int.TryParse(word, out n))
			{
				ret = true;
			}
			else if (word.StartsWith("#"))
			{
				ret = true;
			}
			else if (word.StartsWith("http"))
			{
				ret = true;
			}
			else
			{
				ret = skipWords.Contains(word);
			}

			return ret;
		}

		/// <summary>
		/// User wants to start a stream.
		/// </summary>
		protected void OnGo(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(tbKeyword.Text))
			{
				MessageBox.Show("Please enter a keyword.", "Keyword Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				RestartStream(tbKeyword.Text);
			}
		}

		/// <summary>
		/// If a stream hasn't been started, just start it.
		/// If a stream has been started, shut it down, and when it's stopped, start the new stream.
		/// </summary>
		protected void RestartStream(string keyword)
		{
			if (stream != null)
			{
				Clear();
				stream.StreamStopped += (sender, args) => StartStream(keyword);
				stream.StopStream();
			}
			else
			{
				StartStream(keyword);
			}
		}

		/// <summary>
		/// Start a stream, filtering ony the keyword and only English language tweets.
		/// </summary>
		protected void StartStream(string keyword)
		{
			stream = Stream.CreateFilteredStream();
			stream.AddTrack(keyword);
			stream.MatchingTweetReceived += (sender, args) =>
			{
				if (args.Tweet.Language == Language.English)
				{
					UpdateFdg(args.Tweet.Text);
				}
			};

			stream.StartStreamMatchingAllConditionsAsync();
		}

		/// <summary>
		/// User wants to stop the stream.
		/// </summary>
		protected void OnStop(object sender, EventArgs e)
		{
			if (stream != null)
			{
				stream.StreamStopped += (s, args) => stream = null;
				stream.StopStream();
			}
		}

		/// <summary>
		/// Clear the word cloud.
		/// </summary>
		protected void Clear()
		{
			wordNodeMap.ForEach(kvp => kvp.Value.Diagram = null);
			wordNodeMap.Clear();
			wordTweetMap.Clear();
		}

		/// <summary>
		/// Update the word hit filter.
		/// </summary>
		protected void OnMinCountChanged(object sender, EventArgs e)
		{
			CountThreshold = (int)nudMinCount.Value;
		}
	}
}
