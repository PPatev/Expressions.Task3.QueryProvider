using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        readonly StringBuilder _resultStringBuilder;
        List<string> queries = new List<string>();

        public ExpressionToFtsRequestTranslator()
        {
            _resultStringBuilder = new StringBuilder();
        }

        public string Translate(Expression exp)
        {
            Visit(exp);

            return _resultStringBuilder.ToString();
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }

            if (node.Method.DeclaringType == typeof(String))
            {
                var member = node.Object;
                Visit(member);
                var constant = node.Arguments[0];
                _resultStringBuilder.Append("(");

                switch (node.Method.Name)
                {
                    case "Equals":
                        Visit(constant);
                        break;
                    case "StartsWith":
                        Visit(constant);
                        _resultStringBuilder.Append("*");
                        break;
                    case "EndsWith":
                        _resultStringBuilder.Append("*");
                        Visit(constant);
                        break;
                    case "Contains":
                        _resultStringBuilder.Append("*");
                        Visit(constant);
                        _resultStringBuilder.Append("*");
                        break;
                    default:
                        throw new NotSupportedException();
                }

                _resultStringBuilder.Append(")");

                return node;
            }
           
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    if (node.Left.NodeType == ExpressionType.Constant && node.Right.NodeType == ExpressionType.MemberAccess)
                    {
                        Visit(node.Right);
                        _resultStringBuilder.Append("(");
                        Visit(node.Left);
                        _resultStringBuilder.Append(")");
                    }
                    else if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                    {
                        Visit(node.Left);
                        _resultStringBuilder.Append("(");
                        Visit(node.Right);
                        _resultStringBuilder.Append(")");
                    }
                    else
                    {
                        throw new NotSupportedException($"Left operand of type - {node.Left.NodeType} and Right operand of type - {node.Right.NodeType} is not supported.");
                    }
                    
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    var left = node.Left;
                    var right = node.Right;
                    Visit(left);

                    var firstQuery = _resultStringBuilder.ToString();
                    queries.Add(firstQuery);
                    _resultStringBuilder.Remove(0, firstQuery.Length);

                    Visit(right);
                    var secondQuery = _resultStringBuilder.ToString();
                    queries.Add(secondQuery);
                    _resultStringBuilder.Remove(0, secondQuery.Length);

                    break;
                
                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            };

            return node;
        }
        
        protected override Expression VisitMember(MemberExpression node)
        {
            _resultStringBuilder.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _resultStringBuilder.Append(node.Value);

            return node;
        }

        #endregion
    }
}
