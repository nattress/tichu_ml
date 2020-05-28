using System;
using System.Text;

namespace TichuAI
{
    public class Mcts : IPlayGenerator
    {
        /// <summary>
        /// Value of k in the Upper confidence function. The conventional default is sqrt(2).
        /// </summary>
        static readonly double uct_k = Math.Sqrt(2);
        static readonly int MaxIterations = 200000;
        static readonly int SimulationDepth = 50;

        int _exploitationCount = 0;
        int _explorationCount = 0;
        private SearchNode GetBestUctChild(SearchNode node, double UctK)
        {
            if (!node.IsFullyExpanded()) return null;

            double bestScore = double.MinValue;
            SearchNode bestNode = null;

            int childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                SearchNode child = node.GetChild(i);
                double exploitation = (double)child.Value / ((double)child.VisitCount + Double.Epsilon);
                double exploration = Math.Sqrt(Math.Log((double)node.VisitCount + 1) / ((double)child.VisitCount + Double.Epsilon));
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

        private SearchNode GetMostVisitedChild(SearchNode node)
        {
            int maxVisitCount = -1;
            SearchNode bestNode = null;

            int childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                SearchNode child = node.GetChild(i);
                if (child.VisitCount > maxVisitCount)
                {
                    maxVisitCount = child.VisitCount;
                    bestNode = child;
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < childCount; i++)
            {
                SearchNode child = node.GetChild(i);
                sb.Append(child.Play.ToString());
                sb.AppendLine($" Visits: {child.VisitCount} Value: {child.Value:N0}" + (child == bestNode ? " (Winner)" : ""));
            }

            Logger.Log.Write(sb.ToString());

            return bestNode;
        }

        public Play FindPlay(IGameState initialState)
        {
            SearchNode rootNode = new SearchNode(initialState, parent: null);
            
            int iterations = 0;
            while (true)
            {
                //
                // Monte-Carlo Tree Search Algorithm
                //

                // Step 1 : Select
                //          Drill down into the tree calculating UCT on all fully-expanded nodes
                SearchNode node = rootNode;
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

                IGameState state = node.State;

                // Step 3 : Simulate
                if (!node.IsTerminal())
                {
                    for (int i = 0; i < SimulationDepth; i++)
                    {
                        if (state.GameOver())
                            break;

                        Play play = state.GetRandomPlay();
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

                if (MaxIterations > 0 && iterations > MaxIterations)
                    break;

                iterations++;
            }

            var result = GetMostVisitedChild(rootNode).Play;
            return result;
        }
    }
}