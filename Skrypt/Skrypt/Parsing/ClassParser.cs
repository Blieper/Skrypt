﻿using System;
using System.Collections.Generic;
using Skrypt.Engine;
using Skrypt.Tokenization;

namespace Skrypt.Parsing
{
    /// <summary>
    ///     The class parser class.
    ///     Contains all methods to parse class definition code
    /// </summary>
    public class ClassParser
    {
        private readonly SkryptEngine _engine;

        public ClassParser(SkryptEngine e)
        {
            _engine = e;
        }

        static List<string> PropertyWords = new List<string> () { "static", "private", "public", "constant" };

        ParseResult TryParse(List<Token> Tokens, string Name) {
            var Properties = new List<string>();
            Node propertyNode = new Node {
                Body = "PropertyDeclaration",
                TokenType = "PropertyDeclaration",
            };

            Node markNode = propertyNode.Add(new Node {
                Body = "Properties",
                TokenType = "Properties",
            });

            List<Token> FurtherTokens = null;

            for (int i = 0; i < Tokens.Count; i++) {
                var t = Tokens[i];

                string Prop = t.Value;

                if (PropertyWords.Contains(Prop)) {
                    if (Prop == "public" && Properties.Contains("private")) {
                        _engine.ThrowError("Property can't be marked as both private and public!");
                    } else if (Prop == "private" && Properties.Contains("public")) {
                        _engine.ThrowError("Property can't be marked as both private and public!");
                    }

                    if (Properties.Contains(Prop)) {
                        _engine.ThrowError("Property is already marked as " + Prop);
                    } else {
                        markNode.Add(new Node {
                            Body = Prop,
                            TokenType = "Property",
                        });

                        Properties.Add(Prop);
                    }
                } else {
                    FurtherTokens = Tokens.GetRange(i,Tokens.Count - i);
                    break;
                }
            }

            ParseResult FurtherResult = null;

            if (FurtherTokens[0].Value == Name) {
                FurtherTokens[0].Value = "Constructor";
                FurtherTokens.Insert(0, new Token {Value = "func"});
                FurtherResult = _engine.MethodParser.Parse(FurtherTokens);
                FurtherResult.Delta--;

                if (Properties.Contains("static") || Properties.Contains("constant")) {
                    _engine.ThrowError("Constructor method cannot be marked as static or constant!", FurtherTokens[1]);
                }
            } else if (FurtherTokens[0].Value == "func") {
                FurtherResult = _engine.MethodParser.Parse(FurtherTokens);
            }
            else if (FurtherTokens[0].Value == "class") {
                FurtherResult = _engine.ClassParser.Parse(FurtherTokens);
            }
            else {
                FurtherResult = _engine.ExpressionParser.Parse(FurtherTokens);
            }

            propertyNode.Add(FurtherResult.Node);

            return new ParseResult { Node = propertyNode, Delta = FurtherResult.Delta + Properties.Count};
        }

        Node ParseContents (List<Token> tokens, string name) {
            //Node node = new Node { Body = "Block", TokenType = "ClassDeclaration" };
            //int i = 0;

            //while (i < Tokens.Count - 1) {
            //    var test = TryParse(Tokens.GetRange(i, Tokens.Count - i), Name);
            //    i += test.Delta;

            //    if (test.Node.TokenType == "MethodDeclaration")
            //    {
            //        node.AddAsFirst(test.Node);
            //        continue;
            //    }

            //    node.Add(test.Node);
            //}

            var constructorStart = -1;
            var clone = new List<Token>(tokens);

            for (int i = 0; i < clone.Count; i++) {
                var t = clone[i];

                if (t.Value == name) {
                    constructorStart = i;
                    Console.WriteLine("Found name: " + t);
                    continue;
                }

                if (constructorStart != -1) {
                    if ((t.Value == "(") && (t.Type == TokenTypes.Punctuator) && (i == (constructorStart + 1))) {
                        SkipInfo skip = _engine.ExpressionParser.SkipFromTo("(", ")", clone, i);

                        if (skip.End + 1 < clone.Count) {
                            var t2 = clone[skip.End + 1];
                            Console.WriteLine("Found args: " + t2);

                            if ((t2.Value == "{") && (t2.Type == TokenTypes.Punctuator)) {
                                skip = _engine.ExpressionParser.SkipFromTo("{", "}", clone, i);

                                Console.WriteLine("Found body: " + clone);

                                clone.RemoveAt(constructorStart);

                                clone.Insert(constructorStart, new Token {
                                    Value = "Constructor",
                                    Type = TokenTypes.Identifier,
                                    Start = clone[constructorStart].Start,
                                    End = clone[constructorStart].Start,
                                });

                                clone.Insert(constructorStart, new Token {
                                    Value = "func",
                                    Type = TokenTypes.Keyword,
                                    Start = clone[constructorStart].Start,
                                    End = clone[constructorStart].Start,
                                });

                                clone.Insert(constructorStart, new Token {
                                    Value = "static",
                                    Type = TokenTypes.Keyword,
                                    Start = clone[constructorStart].Start,
                                    End = clone[constructorStart].Start,
                                });

                                break;
                            }
                        }
                    }
                }
            }

            Console.WriteLine(ExpressionParser.TokenString(clone));

            var result = _engine.GeneralParser.Parse(clone);
            result.TokenType = "ClassDeclaration";

            return result;
        }

        public ParseResult Parse(List<Token> tokens)
        {
            var i = 0;

            var skip = _engine.ExpectType(TokenTypes.Identifier, tokens, i);
            i += skip.Delta;

            skip = _engine.ExpectValue("{", tokens, i);
            i += skip.Delta;

            List<Token> SurroundedTokens = _engine.GeneralParser.GetSurroundedTokens("{", "}", i, tokens);
            Node node = ParseContents(SurroundedTokens, tokens[1].Value);

            var result = new ParseResult { Node = node, Delta = SurroundedTokens.Count + 1 };

            var Node = result.Node;
            Node.Body = tokens[1].Value;
            i += result.Delta + 1;
             
            return new ParseResult { Node = Node, Delta = i };
        }
    }
}