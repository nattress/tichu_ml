using System;
using System.Collections.Generic;
using System.Text;

namespace TichuAI
{
    public class SearchNode<Move>
    {
        /// <summary>
        /// The 0-based player index who made this node's play
        /// </summary>
        int _playerNumber;
        double _value;
        int _depth;
        List<SearchNode<Move>> _children = new List<SearchNode<Move>>();
        // Possible actions from this state
        IList<Move> _plays = new List<Move>();

        public SearchNode(IGameState<Move> state, SearchNode<Move> parent)
        {
            State = state;
            Parent = parent;
            _depth = parent != null ? parent._depth + 1 : 0;
            _playerNumber = state.CurrentPlayerTurn;
        }

        /// <summary>
        /// Expand the tree by adding a single child
        /// </summary>
        public SearchNode<Move> Expand()
        {
            if (IsFullyExpanded())
                return null;

            if (_plays.Count == 0)
            {
                _plays = (List<Move>)State.GetPlays();
                _plays.Shuffle();
            }

            return AddPlayAsChild(_plays[_children.Count]);
        }

        public void Update(double[] rewards)
        {
            _value += rewards[_playerNumber];
            VisitCount++;
        }

        public bool IsFullyExpanded() => _children.Count > 0 && _children.Count == _plays.Count;
        public bool IsTerminal() => State.GameOver();
        public double Value => _value;
        public int Depth => _depth;
        public int ChildCount => _children.Count;
        public int VisitCount { get; private set; }
        public IGameState<Move> State { get; private set; }
        public Move Play { get; private set; }
        public SearchNode<Move> Parent { get; private set; }
        public SearchNode<Move> GetChild(int index) => _children[index];

        private SearchNode<Move> AddPlayAsChild(Move play)
        {
            // Create new child node with the same state as this node and apply the play to it
            SearchNode<Move> child = new SearchNode<Move>(State.Clone(), this);
            child.Play = play;
            child.State.CommitPlay(play);
            _children.Add(child);
            return child;
        }

        private string PrettyPrint(string indent, bool lastChild)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(indent);
            
            if (lastChild)
            {
                sb.Append("`-");
                indent += "  ";
            }
            else
            {
                sb.Append("|-");
                indent += "| ";
            }

            sb.AppendLine($"[{_playerNumber}]{Play?.ToString()} Visits: {VisitCount} Value: {Value:N0} Depth: {Depth} Expanded: {IsFullyExpanded()} IsTerminal: {IsTerminal()}");

            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                bool childIsLast = i == _children.Count - 1;
                sb.Append(_children[i].PrettyPrint(indent, childIsLast));
            }
            
            return sb.ToString();
        }

        public string PrettyPrint()
        {
            return PrettyPrint("", true);
        }
        
        public override string ToString()
        {
            return $"Visits: {VisitCount} Value: {Value:N0} Depth: {Depth} Expanded: {IsFullyExpanded()} Terminal: {IsTerminal()}";
        }
    }
}