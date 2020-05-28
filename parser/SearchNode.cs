using System;
using System.Collections.Generic;

namespace TichuAI
{
    public class SearchNode
    {
        /// <summary>
        /// The 0-based player index who made this node's play
        /// </summary>
        int _playerNumber;
        double _value;
        int _depth;
        List<SearchNode> _children = new List<SearchNode>();
        // Possible actions from this state
        IList<Play> _plays = new List<Play>();

        public SearchNode(IGameState state, SearchNode parent)
        {
            State = state;
            Parent = parent;
            _depth = parent != null ? parent._depth + 1 : 0;
            _playerNumber = state.CurrentPlayerTurn;
        }

        /// <summary>
        /// Expand the tree by adding a single child
        /// </summary>
        public SearchNode Expand()
        {
            if (IsFullyExpanded())
                return null;

            if (_plays.Count == 0)
            {
                _plays = (List<Play>)State.GetPlays();
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
        public IGameState State { get; private set; }
        public Play Play { get; private set; }
        public SearchNode Parent { get; private set; }
        public SearchNode GetChild(int index) => _children[index];

        private SearchNode AddPlayAsChild(Play play)
        {
            // Create new child node with the same state as this node and apply the play to it
            SearchNode child = new SearchNode(State.Clone(), this);
            child.Play = play;
            child.State.CommitPlay(play);
            _children.Add(child);
            return child;
        }
    }
}