// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Core.Arango.Relinq.Clauses;
using Core.Arango.Relinq.Clauses.ResultOperators;
using Core.Arango.Relinq.Utilities;

namespace Core.Arango.Relinq.Parsing.Structure.IntermediateModel
{
    /// <summary>
    ///     Represents a <see cref="MethodCallExpression" /> for
    ///     <see cref="Queryable.LongCount{TSource}(System.Linq.IQueryable{TSource})" />,
    ///     <see
    ///         cref="Queryable.LongCount{TSource}(System.Linq.IQueryable{TSource},System.Linq.Expressions.Expression{System.Func{TSource,bool}})" />
    ///     ,
    ///     and for the <see cref="Array.Length" /> property of arrays.
    ///     It is generated by <see cref="ExpressionTreeParser" /> when an <see cref="Expression" /> tree is parsed.
    ///     When this node is used, it marks the beginning (i.e. the last node) of an <see cref="IExpressionNode" /> chain that
    ///     represents a query.
    /// </summary>
    internal sealed class LongCountExpressionNode : ResultOperatorExpressionNodeBase
    {
        public LongCountExpressionNode(MethodCallExpressionParseInfo parseInfo, LambdaExpression optionalPredicate)
            : base(parseInfo, optionalPredicate, null)
        {
        }

        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            foreach (var methodInfo in ReflectionUtility.EnumerableAndQueryableMethods.WhereNameMatches("LongCount"))
                yield return methodInfo;

            var arrayLongLengthMethodInfo = typeof(Array).GetRuntimeMethod("get_LongLength", new Type[0]);
            if (arrayLongLengthMethodInfo != null)
                yield return arrayLongLengthMethodInfo;
        }

        public override Expression Resolve(
            ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            // no data streams out from this node, so we cannot resolve any expressions
            throw CreateResolveNotSupportedException();
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new LongCountResultOperator();
        }
    }
}