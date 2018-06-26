﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skrypt.Engine;
using Skrypt.Parsing;
using Skrypt.Execution;
using Skrypt.Library;
using Skrypt.Library.SkryptClasses;
using System.Reflection;

namespace Skrypt.Analysis {
    /// <summary>
    /// The node analizer class.
    /// Analizes nodes to check for errors.
    /// </summary>
    class Analizer {
        SkryptEngine engine;

        public Analizer(SkryptEngine e) {
            engine = e;
        }
        
        public SkryptObject AnalizeExpression (Node node, ScopeContext scope) {
            Operator op = Operator.AllOperators.Find(o => o.OperationName == node.Body || o.Operation == node.Body);

            if (op != null) {
                int Members = node.SubNodes.Count;

                if (Members != op.Members) {
                    engine.throwError("Missing member of operation!",node.Token);
                }

                switch (op.OperationName) {
                    case "assign":
                        if (node.SubNodes[0].TokenType != "Identifier") {
                            engine.throwError("Can't assign non-variable", node.SubNodes[0].Token);
                        }

                        SkryptObject r = scope.Variables[node.SubNodes[0].Body] = AnalizeExpression(node.SubNodes[1], scope);
                        return r;
                }

                if (op.Members == 2) {
                    SkryptObject Left = AnalizeExpression(node.SubNodes[0], scope);
                    SkryptObject Right = AnalizeExpression(node.SubNodes[1], scope);

                    if (Left != null && Right != null) {
                        Type t1 = Left.GetType();
                        Type t2 = Right.GetType();

                        MethodInfo methodInfo1 = null;
                        MethodInfo methodInfo2 = null;
                        var Methods1 = t1.GetMethodsBySig("_" + op.OperationName, t1, t2);
                        var Methods2 = t2.GetMethodsBySig("_" + op.OperationName, t1, t2);

                        if (Methods1.Count() > 0) methodInfo1 = Methods1.ElementAt(0);
                        if (Methods2.Count() > 0) methodInfo2 = Methods2.ElementAt(0);

                        if (methodInfo1 != null) {
                            try {
                                object result = methodInfo1.Invoke(null, new object[] { Left, Right });
                                return (SkryptObject)result;
                            }
                            catch (Exception e) { }
                        }
                        else if (methodInfo2 != null) {
                            try {
                                object result = methodInfo2.Invoke(null, new object[] { Left, Right });
                                return (SkryptObject)result;
                            }
                            catch (Exception e) { }
                        }

                        engine.throwError("No such operation as " + Left.Name + " " + op.Operation + " " + Right.Name, node.SubNodes[0].Token);
                    }
                } else if (op.Members == 1) {
                    SkryptObject Left = AnalizeExpression(node.SubNodes[0], scope);

                    if (Left != null) {
                        Type t1 = Left.GetType();

                        MethodInfo methodInfo1 = null;
                        var Methods1 = t1.GetMethodsBySig("_" + op.OperationName, t1);

                        if (Methods1.Count() > 0) methodInfo1 = Methods1.ElementAt(0);

                        if (methodInfo1 != null) {
                            try {
                                object result = methodInfo1.Invoke(null, new object[] { Left });
                                return (SkryptObject)result;
                            }
                            catch (Exception e) { }
                        }

                        engine.throwError("No such operation as " + op.Operation + " " + Left.Name, node.SubNodes[0].Token);
                    }
                }
            } else if (node.SubNodes.Count == 0) {
                switch (node.TokenType) {
                    case "NumericLiteral":
                        return new Numeric { value = Double.Parse(node.Body)};
                    case "StringLiteral":
                        return new SkryptString { value = node.Body };
                }
            } else if (node.TokenType == "ArrayLiteral") {
                SkryptArray array = new SkryptArray();

                foreach (Node subNode in node.SubNodes) {
                    array.value.Add(AnalizeExpression(subNode, scope));
                }

                return array;
            }

            if (node.TokenType == "Identifier") {
                if (scope.Variables.ContainsKey(node.Body)) {
                    return scope.Variables[node.Body];
                } else {
                    engine.throwError("Variable '" + node.Body + "' does not exist in the current context!",node.Token);
                }
            }

            return null;
        }

        public void Analize (Node node) {
            ScopeContext context = new ScopeContext();

            foreach (Node subNode in node.SubNodes) {
                SkryptObject result = engine.executor.ExecuteExpression(subNode, context);
                Console.WriteLine("Result: " + result);
            }
        }
    }
}
