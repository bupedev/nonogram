using Nonogram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nonogram
{
    internal class ParallelSolver : Solver
    {
        internal ParallelSolver(GameState gameState) : base(gameState)
        { }

        internal override void Solve()
        {
            base.Solve();
            FindSolutions(Board, GetGameStates);
        }

        public static IEnumerable<GameState> GetGameStates(GameState gameState, int row)
        {
            GenerateLinePermutations(out List<CellState[]> permutations, gameState.RowHints[row], gameState.Width);
            foreach (CellState[] permutation in permutations)
            {
                GameState newState = gameState.Clone() as GameState;
                newState[row] = permutation;
                yield return newState;
            }
        }

        private void FindSolutions(GameState source, Func<GameState, int, IEnumerable<GameState>> stateSelector)
        {
            Action<GameState> foo = null;
            foo = (state) =>
            {
                if (Solutions.Count > 0) return;

                if (ValidatePermutation(state))
                {
                    if (state.IsFinal())
                    {
                        Solutions.Add(state);
                        return;
                    } 
                    else
                    {
                        var states = stateSelector(state, state.TargetRow);
                        state.IncrementRowTarget();
                        Parallel.ForEach(states, new ParallelOptions { MaxDegreeOfParallelism = -1 }, (subState) => foo(subState));
                    }
                }
            };
            foo(source);
        }
    }
}

