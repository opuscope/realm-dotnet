﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Realms.Schema;

namespace Realms
{
    internal class WhereClauseVisitor : ExpressionVisitor
    {
        private readonly RealmObjectBase.Metadata _metadata;

        private WhereClause _whereClause;

        public WhereClauseVisitor(RealmObjectBase.Metadata metadata)
        {
            _metadata = metadata;
            _whereClause = new WhereClause();
        }

        public WhereClause VisitWhere(LambdaExpression whereClause)
        {
            _whereClause.Expression = Extract(whereClause.Body);
            var json = JsonConvert.SerializeObject(_whereClause, formatting: Formatting.Indented);
            return _whereClause;
        }

        private ExpressionNode Extract(Expression node)
        {
            var realmLinqExpression = Visit(node) as RealmLinqExpression;
            return realmLinqExpression.ExpressionNode;
        }

        protected override Expression VisitBinary(BinaryExpression be)
        {
            ExpressionNode returnNode;
            if (be.NodeType == ExpressionType.AndAlso)
            {
                var andNode = new AndNode();
                andNode.Left = Extract(be.Left);
                andNode.Right = Extract(be.Right);
                returnNode = andNode;
            }
            else if (be.NodeType == ExpressionType.OrElse)
            {
                var orNode = new OrNode();
                orNode.Left = Extract(be.Left);
                orNode.Right = Extract(be.Right);
                returnNode = orNode;
            }
            else
            {
                ComparisonNode comparisonNode;
                switch (be.NodeType)
                {
                    case ExpressionType.Equal:
                        comparisonNode = new EqualityNode();
                        break;
                    case ExpressionType.NotEqual:
                        comparisonNode = new NotEqualNode();
                        break;
                    case ExpressionType.LessThan:
                        comparisonNode = new LtNode();
                        break;
                    case ExpressionType.LessThanOrEqual:
                        comparisonNode = new LteNode();
                        break;
                    case ExpressionType.GreaterThan:
                        comparisonNode = new GtNode();
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        comparisonNode = new GteNode();
                        break;
                    default:
                        throw new NotSupportedException($"The binary operator '{be.NodeType}' is not supported");
                }

                if (be.Left is MemberExpression me)
                {
                    if (me.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
                    {
                        var leftName = GetColumnName(me, me.NodeType);

                        comparisonNode.Left.Value = leftName;
                        comparisonNode.Left.Kind = "property";
                        comparisonNode.Left.Type = GetKind(me.Type);
                    }
                    else
                    {
                        throw new NotSupportedException(me + " is null or not a supported type.");
                    }
                }

                if (be.Left is ConstantExpression ce)
                {
                    comparisonNode.Left.Value = ce.Value;
                    comparisonNode.Left.Kind = "constant";
                    comparisonNode.Left.Type = GetKind(ce.Value.GetType());
                }

                if (be.Right is MemberExpression mo)
                {
                    if (mo.Expression != null && mo.Expression.NodeType == ExpressionType.Parameter)
                    {
                        var leftName = GetColumnName(mo, mo.NodeType);

                        comparisonNode.Right.Value = leftName;
                        comparisonNode.Right.Kind = "property";
                        comparisonNode.Right.Type = GetKind(mo.Type);
                    }
                    else
                    {
                        throw new NotSupportedException(mo + " is null or not a supported type.");
                    }
                }

                if (be.Right is ConstantExpression co)
                {
                    comparisonNode.Right.Value = co.Value;
                    comparisonNode.Right.Kind = "constant";
                    comparisonNode.Right.Type = GetKind(co.Value.GetType());
                }

                returnNode = comparisonNode;
            }

            return RealmLinqExpression.Create(returnNode);
        }

        private static string GetKind(object valueType)
        {
            // TODO: Possible with switch statment in our current .NET version?
            if (valueType == typeof(float))
            {
                return "float";
            }
            else if (valueType == typeof(long))
            {
                return "long";
            }
            else if (valueType == typeof(double))
            {
                return "double";
            }
            else
            {
                throw new NotSupportedException(valueType + "is not a supported type.");
            }
        }

        private string GetColumnName(MemberExpression memberExpression, ExpressionType? parentType = null)
        {
            var name = memberExpression?.Member.GetMappedOrOriginalName();

            if (parentType.HasValue)
            {
                if (name == null ||
                    memberExpression.Expression.NodeType != ExpressionType.Parameter ||
                    !(memberExpression.Member is PropertyInfo) ||
                    !_metadata.Schema.TryFindProperty(name, out var property) ||
                    property.Type.HasFlag(PropertyType.Array) ||
                    property.Type.HasFlag(PropertyType.Set))
                {
                    throw new NotSupportedException($"The {parentType} operator must be a direct access to a persisted property in Realm.\nUnable to process '{memberExpression}'.");
                }
            }

            return name;
        }
    }
}
