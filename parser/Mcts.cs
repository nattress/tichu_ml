using System;
using System.Text;

namespace TichuAI
{
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

        public Mcts(int numIterations, int simulationDepth)
        {
            _numIterations = numIterations;
            _simulationDepth = simulationDepth;
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
                // double exploitation = (double)child.Value / ((double)child.VisitCount + Double.Epsilon);
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

        private SearchNode<Move> GetMostVisitedChild(SearchNode<Move> node)
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

        public Move FindPlay(IGameState<Move> initialState)
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
            var result = GetMostVisitedChild(rootNode).Play;
            return result;
        }
    }
}