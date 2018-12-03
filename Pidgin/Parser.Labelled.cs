using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pidgin
{
    public partial class Parser<TToken, T>
    {
        /// <summary>
        /// Creates a parser equivalent to the current parser, with a custom label.
        /// The label will be reported in an error message if the parser fails, instead of the default error message.
        /// <seealso cref="ParseError{TToken}.Expected"/>
        /// <seealso cref="Expected{TToken}.Label"/>
        /// </summary>
        /// <param name="label">The custom label to apply to the current parser</param>
        /// <returns>A parser equivalent to the current parser, with a custom label</returns>
        public Parser<TToken, T> Labelled(string label)
        {
            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }
            return WithExpected(ImmutableSortedSet.Create(new Expected<TToken>(label)));
        }
            
        internal Parser<TToken, T> WithExpected(ImmutableSortedSet<Expected<TToken>> expected)
            => new WithExpectedParser(this, expected);

        private sealed class WithExpectedParser : Parser<TToken, T>
        {
            private readonly Parser<TToken, T> _parser;
            private readonly ImmutableSortedSet<Expected<TToken>> _expected;

            public WithExpectedParser(Parser<TToken, T> parser, ImmutableSortedSet<Expected<TToken>> expected)
            {
                _parser = parser;
                _expected = expected;
            }

            internal override InternalResult<T> Parse(ref ParseState<TToken> state)
            {
                var result = _parser.Parse(ref state);
                if (!result.Success)
                {
                    state.Error = state.Error.WithExpected(_expected);
                }
                return result;
            }
        }
    }
}