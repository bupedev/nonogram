using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nonogram
{
    internal class AsyncSolver : Solver
    {
        internal AsyncSolver(GameState gameState) : base(gameState)
        { }

        internal async override void Solve()
        {
            base.Solve();
            await FindSolutions(new[] { Board }, GetGameStates);

        }

        private static IEnumerable<GameState> GetGameStates(GameState gameState, int row)
        {  
            GenerateLinePermutations(out List<CellState[]> permutations, gameState.RowHints[row], gameState.Width);
            foreach (CellState[] permutation in permutations)
            {
                GameState newState = gameState.Clone() as GameState;
                newState[row] = permutation;
                yield return newState;
            }
        }

        private async Task FindSolutions(IEnumerable<GameState> source, Func<GameState, int, IEnumerable<GameState>> stateSelector)
        {
            Func<GameState, int, Task> foo = null;
            foo = async (state, row) =>
            {
                if (ValidatePermutation(state))
                {
                    if (row == state.Height)
                    {
                        Solutions.Add(state);
                    }
                    else
                    { 
                        var states = stateSelector(state, row++);
                        await Task.WhenAll(states.Select(subState => foo(subState, row)));
                    }
                }
            };
            await Task.WhenAll(source.Select(subState => foo(subState, 0)));
        }
    }
}
