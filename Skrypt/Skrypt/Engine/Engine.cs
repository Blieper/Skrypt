﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Tokenization;
using System.Text.RegularExpressions;
using Skrypt.Parsing;
using System.Diagnostics; // Using this so we can check how fast everything is happening

namespace Skrypt.Engine {
    class SkryptEngine {
        Tokenizer tokenizer = new Tokenizer();
        List<Token> Tokens;
        List<SkryptClass> Classes = new List<SkryptClass>();
        SkryptClass ProgramClass;

        public SkryptEngine() {
            // Tokens that are found using a token rule with type defined as 'null' won't get added to the token list.
            // This means you can ignore certain characters, like whitespace in this case, that way.
            tokenizer.AddRule(
                new Regex(@"\s"),
                null
            );

            tokenizer.AddRule(
                new Regex(@"\d+(\.\d+)?"),
                "NumericLiteral"
            );

            tokenizer.AddRule(
                new Regex(@"[_a-zA-Z]+[_a-zA-Z0-9]*"),
                "Identifier"
            );

            tokenizer.AddRule(
                new Regex("class"),
                "Keyword"
            );

            tokenizer.AddRule(
                new Regex("true|false"),
                "BooleanLiteral"
            );

            tokenizer.AddRule(
                new Regex(@"(&&)|(\|\|)|(\|\|\|)|(==)|(!=)|(>=)|(<=)|(<<)|(>>)|(>>>)|(\+\+)|(--)|[~=;:<>+\-*/%^&|!\[\]\(\)\.\,{}]"),
                "Punctuator"
            );

            tokenizer.AddRule(
                new Regex(@""".*?(?<!\\)"""),
                "StringLiteral"
            );

            // Multi line comment
            tokenizer.AddRule(
                new Regex(@"\/\*(.|\n)*\*\/"),
                null
            );

            // Single line comment
            tokenizer.AddRule(
                new Regex(@"\/\/.*\n"),
                null
            );
        }

        public SkryptClass Parse (string code) {

            Tokens = tokenizer.Tokenize(code);      
            if (Tokens == null) { return null; }

            TokenProcessor.ProcessTokens(Tokens);

            int index = 0;
            ProgramClass = ClassParser.Parse(Tokens, ref index);

            Console.WriteLine(ProgramClass);

            return ProgramClass;
        }
    }
}
