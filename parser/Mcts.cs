using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TichuAI
{
    /// <summary>
    /// MCTS parallelized at the root level. Perform N independent searches and select the best result from them all.
    /// </summary>
    public class MultiThreadedMcts<Move> : Mcts<Move>, IPlayGenerator<Move> where Move : IComparable
    {
        private readonly int _threadCount;

        public MultiThreadedMcts(int numIterations, int simulationDepth, Random random, int threadCount) : base(numIterations, simulationDepth, random)
        {
            _threadCount = threadCount;
        }

        public override Move FindPlay(IGameState<Move> initialState)
        {
            ConcurrentBag<Move> bag = new ConcurrentBag<Move>();

            ThreadPool.GetMinThreads(out int workerThreads, out int completionThreads);
            if (workerThreads < _threadCount)
            {
                ThreadPool.SetMinThreads(_threadCount, completionThreads);
            }

            //ManualResetEventSlim mre = new ManualResetEventSlim();
            ManualResetEvent[] waitEvents = new ManualResetEvent[_threadCount];
            for (int i = 0; i < _threadCount; i++)
            {
                waitEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem( waitEventIndex => 
                {
                    var rootNode = Search(initialState);
                    bag.Add(GetMostVisitedChild(rootNode).Play);
                    waitEvents[(int)waitEventIndex].Set();
                }, i);
            }

            WaitHandle.WaitAll(waitEvents);
            return PickWinningMove(bag.ToArray());
        }

        // Selects the most-voted move by all the agents. If there's a tie, pick it randomly
        private Move PickWinningMove(Move[] moves)
        {
            List<Move> sortedMoves = new List<Move>(moves);
            sortedMoves.Sort();
            List<int> counts = new List<int>();
            List<Move> countMoves = new List<Move>();

            Move currentMove = moves[0];
            int count = 0;
            foreach (var m in sortedMoves)
            {
                if (currentMove.CompareTo(m) == 0)
                {
                    ++count;
                }
                else
                {
                    countMoves.Add(currentMove);
                    counts.Add(count);
                    currentMove = m;
                    count = 1;
                }
            }

            // Add the last run of counts
            countMoves.Add(currentMove);
            counts.Add(count);

            int maxVoteCount = counts.Max();
            var tiedWins = countMoves.Where( (move, index) =>
            {
                return counts[index] == maxVoteCount;
            });

            return tiedWins.ElementAt(_random.Next(tiedWins.Count()));
        }
    }
    public class Mcts<Move> : IPlayGenerator<Move>
    {
        /// <summary>
        /// Value of k in the Upper confidence function. The conventional default is sqrt(2).
        /// </summary>
        static readonly double uct_k = Math.Sqrt(2);
        private readonly int _numIterations;
        private readonly int _simulationDepth;
        int _exploitationCount = 0;
        int _explorationCount = 0;
        protected Random _random;

        public Mcts(int numIterations, int simulationDepth, Random random)
        {
            _numIterations = numIterations;
            _simulationDepth = simulationDepth;
            _random = random;
        }

        private SearchNode<Move> GetBestUctChild(SearchNode<Move> node, double UctK)
        {
            if (!node.IsFullyExpanded()) return null;

            double bestScore = double.MinValue;
            SearchNode<Move> bestNode = null;

            int childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                SearchNode<Move> child = node.GetChild(i);
                double exploitation = (double)child.Value / (double)child.VisitCount;
                double exploration = Math.Sqrt(Math.Log((double)node.VisitCount + 1) / (double)child.VisitCount);
                double uctScore = exploitation + UctK * exploration;
                if (exploitation > exploration)
                    _exploitationCount++;
                else
                    _explorationCount++;

                if (uctScore > bestScore)
                {
                    bestScore = uctScore;
                    bestNode = child;
                }
            }

            return bestNode;
        }

        protected SearchNode<Move> GetMostVisitedChild(SearchNode<Move> node)
        {
            int maxVisitCount = -1;
            SearchNode<Move> bestNode = null;

            int childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                SearchNode<Move> child = node.GetChild(i);
                if (child.VisitCount > maxVisitCount)
                {
                    maxVisitCount = child.VisitCount;
                    bestNode = child;
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < childCount; i++)
            {
                SearchNode<Move> child = node.GetChild(i);
                sb.Append(child.Play.ToString());
                sb.AppendLine($" Visits: {child.VisitCount} Value: {child.Value:N0}" + (child == bestNode ? " (Winner)" : ""));
            }

            Logger.Log.Write(sb.ToString());

            return bestNode;
        }

        public virtual Move FindPlay(IGameState<Move> initialState)
        {
            var root = Search(initialState);
            return GetMostVisitedChild(root).Play;
        }

        protected SearchNode<Move> Search(IGameState<Move> initialState)
        {
            SearchNode<Move> rootNode = new SearchNode<Move>(initialState, parent: null);
            
            int iterations = 0;
            while (true)
            {
                //
                // Monte-Carlo Tree Search Algorithm
                //

                // Step 1 : Select
                //          Drill down into the tree calculating UCT on all fully-expanded nodes
                SearchNode<Move> node = rootNode;
                while (!node.IsTerminal() && node.IsFullyExpanded())
                {
                    node = GetBestUctChild(node, uct_k);
                }

                // Step 2 : Expand
                //          Add a single child (if not terminal or not fully expanded)
                if (!node.IsFullyExpanded() && !node.IsTerminal())
                {
                    node = node.Expand();
                }

                IGameState<Move> state = node.State.Clone();

                // Step 3 : Simulate
                if (!node.IsTerminal())
                {
                    for (int i = 0; i < _simulationDepth; i++)
                    {
                        if (state.GameOver())
                            break;

                        Move play = state.GetRandomPlay();
                        if (play == null)
                            break;
                        
                        state.CommitPlay(play);
                    }
                }

                // Get rewards vector for all agents
                double[] rewards = state.Evaluate();

                // Keep a history of visited states for debugging / analysis
                //Console.WriteLine(state.ToString());

                // Step 4 : Back propagation
                while (node != null)
                {
                    node.Update(rewards);
                    node = node.Parent;
                }

                if (iterations > _numIterations)
                    break;

                iterations++;
            }
            //Console.WriteLine(rootNode.PrettyPrint());
            //var result = GetMostVisitedChild(rootNode).Play;

            return rootNode;
        }
    }
}