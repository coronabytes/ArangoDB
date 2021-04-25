﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Core.Arango.Linq.Data;
using Core.Arango.Linq.Utility;
using Core.Arango.Relinq;
using Core.Arango.Relinq.Parsing.ExpressionVisitors;
using Core.Arango.Relinq.Parsing.Structure.IntermediateModel;

namespace Core.Arango.Linq.Query.Clause
{
    internal class TraversalExpressionNode : MethodCallExpressionNodeBase, IQuerySourceExpressionNode
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            LinqUtility.GetSupportedMethod(() => TraversalQueryableExtensions.Traversal<object, object>(null, "")),
            LinqUtility.GetSupportedMethod(() => TraversalQueryableExtensions.Traversal<object, object>(null, () => ""))
        };

        private readonly ResolvedExpressionCache<Expression> _resolvedAdaptedSelector;

        private readonly string identifier;

        public TraversalExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression startVertex)
            : base(parseInfo)
        {
            StartVertex = startVertex;

            identifier = Guid.NewGuid().ToString("N");

            var genericTypes = parseInfo.ParsedExpression.Type.GenericTypeArguments[0].GenericTypeArguments;
            VertextType = Expression.Constant(genericTypes[0]);
            EdgeType = Expression.Constant(genericTypes[1]);

            _resolvedAdaptedSelector = new ResolvedExpressionCache<Expression>(this);
        }

        public Expression StartVertex { get; }

        public ConstantExpression VertextType { get; }
        public ConstantExpression EdgeType { get; }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            LinqUtility.CheckNotNull("inputParameter", inputParameter);
            LinqUtility.CheckNotNull("expressionToBeResolved", expressionToBeResolved);

            var resolvedSelector = GetResolvedAdaptedSelector(clauseGenerationContext);
            var resolved = ReplacingExpressionVisitor.Replace(inputParameter,
                Expression.Parameter(resolvedSelector.Type, identifier), expressionToBeResolved);
            return resolved;
        }

        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            LinqUtility.CheckNotNull("queryModel", queryModel);

            var traversalClause = new TraversalClause(StartVertex, identifier);

            queryModel.BodyClauses.Add(traversalClause);

            clauseGenerationContext.AddContextInfo(this, traversalClause);

            //queryModel.SelectClause.Selector = GetResolvedAdaptedSelector(clauseGenerationContext);
        }

        public Expression GetResolvedAdaptedSelector(ClauseGenerationContext clauseGenerationContext)
        {
            return _resolvedAdaptedSelector.GetOrCreate(
                r =>
                {
                    var traversalDataType =
                        typeof(TraversalData<,>).MakeGenericType(VertextType.Value as Type, EdgeType.Value as Type);

                    var constr = ReflectionUtils.GetConstructors(traversalDataType).ToList()[0];
                    var newExpression =
                            Expression.Convert(
                                Expression.New(constr), traversalDataType)
                        ;

                    return r.GetResolvedExpression(newExpression, Expression.Parameter(traversalDataType, "dummy"),
                        clauseGenerationContext);
                });
        }
    }
}