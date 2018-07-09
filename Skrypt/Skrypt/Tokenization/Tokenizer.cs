﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using Skrypt.Engine;

namespace Skrypt.Tokenization
{
    public enum TokenTypes : byte
    {
        None,
        NumericLiteral,
        Identifier,
        Keyword,
        BooleanLiteral,
        NullLiteral,
        EndOfExpression,
        Punctuator,
        StringLiteral,
        HasValue = NumericLiteral | Identifier | BooleanLiteral | NullLiteral | StringLiteral
    }

    /// <summary>
    ///     The main Tokenization class.
    ///     Contains all methods for tokenization.
    /// </summary>
    public class Tokenizer
    {
        private readonly SkryptEngine _engine;
        private readonly List<TokenRule> _tokenRules = new List<TokenRule>();

        public Tokenizer(SkryptEngine e)
        {
            _engine = e;
        }

        public void AddRule(Regex pattern, TokenTypes type)
        {
            _tokenRules.Add(new TokenRule
            {
                Pattern = pattern,
                Type = type
            });
        }

        /// <summary>
        ///     Tokenizes the given input string according to the token rules given to this object.
        /// </summary>
        /// <returns>
        ///     A list of tokens.
        /// </returns>
        public List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();

            var index = 0;
            var originalInput = input;

            while (index < originalInput.Length)
            {
                Match foundMatch = null;
                TokenRule foundRule = null;

                // Check input string for all token rules
                foreach (var rule in _tokenRules)
                {
                    var match = rule.Pattern.Match(input);

                    // Only permit match if it's found at the start of the string
                    if (match.Index == 0 && match.Success)
                    {
                        foundMatch = match;
                        foundRule = rule;
                    }
                }

                // No match was found; this means we encountered an unexpected token.
                if (foundMatch == null)
                    _engine.ThrowError("Unexpected token '" + originalInput[index] + "' found",
                        new Token {Start = index});

                var token = new Token
                {
                    Value = foundMatch.Value,
                    Type = foundRule.Type,
                    Start = index + foundMatch.Index,
                    End = index + foundMatch.Index + foundMatch.Value.Length - 1
                };

                // Ignore token if it's type equals null
                if (foundRule.Type != TokenTypes.None) tokens.Add(token);

                // Increase current index and cut away part of the string that got matched so we don't repeat it again.
                index += foundMatch.Value.Length;
                input = originalInput.Substring(index);
            }

            return tokens;
        }
    }
}