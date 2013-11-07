using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Data;
using System.IO;
using System.Text;
using Settings = Heath.Dlog.CodeGen.Properties.Settings;
using Smo = Microsoft.SqlServer.Management.Smo;

namespace Heath.Dlog.CodeGen
{
    /// <summary>
    /// Represents a generic service for generating .Net code.
    /// </summary>
    internal static class CodeService
    {
        /// <summary>
        /// Gets a entity code string based on the provided tableView.
        /// </summary>
        /// <param name="tableView">The tableView generated code will be based on.</param>
        /// <param name="languageProvider">The language specific provider.</param>
        /// <param name="entityTemplate">A template to load memebers into.</param>
        /// <returns>A code string.</returns>
        public static string GenerateEntityCode(Smo.TableViewBase tableView, ILanguageProvider languageProvider, string entityTemplate)
        {
            string retval;

            if (string.IsNullOrEmpty(entityTemplate))
            {
                var type = GetEntityCodeTypeDeclaration(languageProvider, tableView);
                retval = GetCodeFromType(languageProvider, type);
            }
            else
            {
                retval = GetEntityCodeString(languageProvider, tableView, entityTemplate);
            }

            return retval;
        }

        /// <summary>
        /// Gets a data object code string based on the provided tableView.
        /// </summary>
        /// <param name="tableView">The tableView generated code will be based on.</param>
        /// <param name="languageProvider">The language specific provider.</param>
        /// <param name="dataObjectTemplate">A template to load memebers into.</param>
        /// <returns>A code string.</returns>
        public static string GetDataObjectCode(Smo.TableViewBase tableView, ILanguageProvider languageProvider, string dataObjectTemplate)
        {
            string retval;

            if (string.IsNullOrEmpty(dataObjectTemplate))
            {
                var type = GetDataObjectCodeTypeDeclaration(languageProvider, tableView);
                retval = GetCodeFromType(languageProvider, type);
            }
            else
            {
                retval = GetDataObjectCodeString(languageProvider, tableView, dataObjectTemplate);
            }

            return retval;
        }

        #region CodeDom Wrappers

        #region Entity

        /// <summary>
        /// Gets an <see cref="CodeTypeDeclaration"/> that entity code will generated from when a template is not provided.
        /// </summary>
        private static CodeTypeDeclaration GetEntityCodeTypeDeclaration(ILanguageProvider languageProvider, Smo.TableViewBase tableView)
        {
            var retval = new CodeTypeDeclaration(tableView.Name);

            foreach (Smo.Column column in tableView.Columns)
            {
                var codeMemberField = GetCodeMemberField(languageProvider, column);
                var codeMemberProperty = GetCodeMemberProperty(languageProvider, column);

                retval.Members.Add(codeMemberField);
                retval.Members.Add(codeMemberProperty);
            }

            var defaultConstructor = GetDefaultConstructor(MemberAttributes.Public);
            var parameterBasedConstructor = GetParameterBasedConstructor(languageProvider, tableView);

            retval.Members.Add(defaultConstructor);
            retval.Members.Add(parameterBasedConstructor);

            return retval;
        }

        /// <summary>
        /// Gets entity code when a template is provided.
        /// </summary>
        private static string GetEntityCodeString(ILanguageProvider languageProvider, Smo.TableViewBase tableView, string entityTemplate)
        {
            entityTemplate = entityTemplate.Replace(Settings.Default.ClassNameToken, tableView.Name);

            var fieldsString = GetFieldsString(languageProvider, tableView);
            entityTemplate = entityTemplate.Replace(Settings.Default.FieldsToken, fieldsString);

            var propertiesString = GetPropertiesString(languageProvider, tableView);
            entityTemplate = entityTemplate.Replace(Settings.Default.PropertiesToken, propertiesString);

            var constructorsString = GetConstructorsString(languageProvider, tableView);
            entityTemplate = entityTemplate.Replace(Settings.Default.ConstructorToken, constructorsString);

            return entityTemplate;
        }

        /// <summary>
        ///  Gets a code member field based on the provided column.
        /// </summary>
        private static CodeMemberField GetCodeMemberField(ILanguageProvider languageProvider, Smo.Column column)
        {
            var fieldName = languageProvider.GetFormattedInternalFieldName(column.Name);
            var typeName = languageProvider.GetTypeString(column.DataType);

            return new CodeMemberField
            {
                Name = fieldName,
                Attributes = MemberAttributes.Private,
                Type = new CodeTypeReference(typeName)
            };
        }

        /// <summary>
        /// Gets a field string generated from all the columns in the tableView.
        /// </summary>
        private static string GetFieldsString(ILanguageProvider languageProvider, Smo.TableViewBase tableView)
        {
            var builder = new StringBuilder();

            foreach (Smo.Column column in tableView.Columns)
            {
                var codeMemberField = GetCodeMemberField(languageProvider, column);
                var codeFromMember = GetCodeFromMember(languageProvider, codeMemberField);

                builder.Append(codeFromMember);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets a code member property based on the provided column.
        /// </summary>
        private static CodeMemberProperty GetCodeMemberProperty(ILanguageProvider languageProvider, Smo.Column column)
        {
            var typeName = languageProvider.GetTypeString(column.DataType);
            var fieldName = languageProvider.GetFormattedInternalFieldName(column.Name);
            var retval = new CodeMemberProperty
            {
                Name = column.Name,
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Type = new CodeTypeReference(typeName),
                HasGet = true,
                HasSet = true
            };

            retval.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
            retval.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodePropertySetValueReferenceExpression()));

            return retval;
        }

        /// <summary>
        /// Gets a property string generated from all the columns in the tableView.
        /// </summary>
        private static string GetPropertiesString(ILanguageProvider languageProvider, Smo.TableViewBase tableView)
        {
            var builder = new StringBuilder();

            foreach (Smo.Column column in tableView.Columns)
            {
                var codeMemberProperty = GetCodeMemberProperty(languageProvider, column);
                var codeFromMember = GetCodeFromMember(languageProvider, codeMemberProperty);

                builder.Append(codeFromMember);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets a default constructor object.
        /// </summary>
        private static CodeConstructor GetDefaultConstructor(MemberAttributes attributes)
        {
            return new CodeConstructor { Attributes = attributes };
        }

        /// <summary>
        /// Gets a parameter based constructor object.
        /// </summary>
        private static CodeConstructor GetParameterBasedConstructor(ILanguageProvider languageProvider, Smo.TableViewBase tableView)
        {
            var retval = new CodeConstructor { Attributes = MemberAttributes.Public };

            foreach (Smo.Column column in tableView.Columns)
            {
                var typeName = languageProvider.GetTypeString(column.DataType);
                var fieldName = languageProvider.GetFormattedInternalFieldName(column.Name);
                var methodParameterName = languageProvider.GetFormattedParamenterName(column.Name);

                retval.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeName), methodParameterName));
                retval.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeArgumentReferenceExpression(methodParameterName)));
            }

            return retval;
        }

        /// <summary>
        /// Gets a default and parameter based constructor string.
        /// </summary>
        private static string GetConstructorsString(ILanguageProvider languageProvider, Smo.TableViewBase tableView)
        {
            var builder = new StringBuilder();

            var defaultConstructor = GetDefaultConstructor(MemberAttributes.Public);
            builder.Append(languageProvider.GetFormattedConstructor(GetCodeFromMember(languageProvider, defaultConstructor), tableView.Name));

            var parameterBasedConstructor = GetParameterBasedConstructor(languageProvider, tableView);
            builder.Append(languageProvider.GetFormattedConstructor(GetCodeFromMember(languageProvider, parameterBasedConstructor), tableView.Name));

            return builder.ToString();
        }

        #endregion

        #region Data Object

        /// <summary>
        /// Gets a <see cref="CodeTypeDeclaration"/> that data object code will be generated from when a template is not provided.
        /// </summary>
        private static CodeTypeDeclaration GetDataObjectCodeTypeDeclaration(ILanguageProvider languageProvider, Smo.TableViewBase tableView)
        {
            var retval = new CodeTypeDeclaration(string.Format("{0}Data", tableView.Name));

            var defaultConstructor = GetDefaultConstructor(MemberAttributes.Private);
            retval.Members.Add(defaultConstructor);

            var insertCodeMemberMethod = GetModifyCodeMemberMethod(tableView, "Insert", CrudMethodType.Create);
            retval.Members.Add(insertCodeMemberMethod);

            var readCodeMemberMethod = GetReadCodeMemberMethod(languageProvider, tableView);
            retval.Members.Add(readCodeMemberMethod);

            var updateCodeMemberMethod = GetModifyCodeMemberMethod(tableView, "Update", CrudMethodType.Update);
            retval.Members.Add(updateCodeMemberMethod);

            var deleteCodeMemberMethod = GetModifyCodeMemberMethod(tableView, "Delete", CrudMethodType.Delete);
            retval.Members.Add(deleteCodeMemberMethod);

            return retval;
        }

        /// <summary>
        /// Gets data object code when a template is provided.
        /// </summary>
        private static string GetDataObjectCodeString(ILanguageProvider languageProvider, Smo.TableViewBase tableView, string dataObjectTemplate)
        {
            var retval = dataObjectTemplate.Replace(Settings.Default.ClassNameToken, string.Format("{0}Data", tableView.Name));

            var defaultConstructorconstructor = GetDefaultConstructor(MemberAttributes.Private);
            retval = retval.Replace(Settings.Default.ConstructorToken, languageProvider.GetFormattedConstructor(GetCodeFromMember(languageProvider, defaultConstructorconstructor), tableView.Name));

            var insertCodeMemberMethod = GetModifyCodeMemberMethod(tableView, "Insert", CrudMethodType.Create);
            retval = retval.Replace(Settings.Default.CreateMethodToken, GetCodeFromMember(languageProvider, insertCodeMemberMethod));

            var readCodeMemberMethod = GetReadCodeMemberMethod(languageProvider, tableView);
            retval = retval.Replace(Settings.Default.ReadMethodToken, GetCodeFromMember(languageProvider, readCodeMemberMethod));

            var updateCodeMemberMethod = GetModifyCodeMemberMethod(tableView, "Update", CrudMethodType.Update);
            retval = retval.Replace(Settings.Default.UpdateMethodToken, GetCodeFromMember(languageProvider, updateCodeMemberMethod));

            var deleteCodeMemberMethod = GetModifyCodeMemberMethod(tableView, "Delete", CrudMethodType.Delete);
            retval = retval.Replace(Settings.Default.DeleteMethodToken, GetCodeFromMember(languageProvider, deleteCodeMemberMethod));

            return retval;
        }

        /// <summary>
        /// Gets a Create, Update, or Delete method determined by the method type enum.
        /// </summary>
        private static CodeMemberMethod GetModifyCodeMemberMethod(Smo.TableViewBase tableView, string methodPrefix, CrudMethodType methodType)
        {
            var retval = new CodeMemberMethod();

            var methodName = string.Format("{0}{1}", methodPrefix, tableView.Name);
            var entityInstanceName = GetFormattedObjectInstanceName(tableView.Name);

            retval.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            retval.Name = methodName;
            retval.Parameters.Add(new CodeParameterDeclarationExpression(tableView.Name, entityInstanceName));

            var tryCatchStatement = new CodeTryCatchFinallyStatement();

            tryCatchStatement.TryStatements.Add(new CodeVariableDeclarationStatement("Database", "database", new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("DatabaseFactory"), "CreateDatabase", new CodeExpression[0])));
            tryCatchStatement.TryStatements.Add(new CodeVariableDeclarationStatement("DbCommand", "command", new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("database"), "GetStoredProcCommand", new CodeExpression[] { new CodePrimitiveExpression(methodName) })));

            switch (methodType)
            {
                case CrudMethodType.Create:
                    AddStoredProcParameters(tableView, tryCatchStatement.TryStatements, entityInstanceName, c => !c.Identity);
                    break;

                case CrudMethodType.Update:
                    AddStoredProcParameters(tableView, tryCatchStatement.TryStatements, entityInstanceName, c => true);
                    break;

                case CrudMethodType.Delete:
                    AddStoredProcParameters(tableView, tryCatchStatement.TryStatements, entityInstanceName, c => c.Identity);
                    break;
            }

            tryCatchStatement.TryStatements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("database"), "ExecuteNonQuery", new CodeExpression[] { new CodeVariableReferenceExpression("command") }));
            tryCatchStatement.CatchClauses.Add(GetCatchClause());

            retval.Statements.Add(tryCatchStatement);

            return retval;
        }

        /// <summary>
        /// Gets a "GetAll" method for the data object.
        /// </summary>
        private static CodeMemberMethod GetReadCodeMemberMethod(ILanguageProvider languageProvider, Smo.TableViewBase tableView)
        {
            var retval = new CodeMemberMethod();

            var methodName = string.Format("GetAll{0}", tableView.Name);

            retval.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            retval.Name = methodName;
            retval.ReturnType = new CodeTypeReference("IEnumerable", new[] { new CodeTypeReference(tableView.Name) });
            retval.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("IEnumerable", new[] { new CodeTypeReference(tableView.Name) }), "retval", new CodeObjectCreateExpression(new CodeTypeReference("List", new[] { new CodeTypeReference(tableView.Name) }))));

            var tryCatchStatement = new CodeTryCatchFinallyStatement();

            tryCatchStatement.TryStatements.Add(new CodeVariableDeclarationStatement("Database", "database", new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("DatabaseFactory"), "CreateDatabase", new CodeExpression[0])));
            tryCatchStatement.TryStatements.Add(new CodeVariableDeclarationStatement("DbCommand", "command", new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("database"), "GetStoredProcCommand", new CodeExpression[] { new CodePrimitiveExpression(methodName) })));
            tryCatchStatement.TryStatements.Add(new CodeVariableDeclarationStatement("IDataReader", "reader", new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("database"), "ExecuteReader", new CodeExpression[] { new CodeVariableReferenceExpression("command") })));

            var loopStatement = new CodeIterationStatement(new CodeSnippetStatement(""), new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("reader"), "Read")), new CodeSnippetStatement(""), new CodeStatement[0]);

            var entityInstanceName = GetFormattedObjectInstanceName(tableView.Name);

            loopStatement.Statements.Add(new CodeVariableDeclarationStatement(tableView.Name, entityInstanceName, new CodeObjectCreateExpression(tableView.Name, new CodeExpression[0])));

            foreach (Smo.Column column in tableView.Columns)
                loopStatement.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(entityInstanceName), column.Name), new CodeCastExpression(languageProvider.GetTypeString(column.DataType), new CodeIndexerExpression(new CodeVariableReferenceExpression("reader"), new CodeExpression[] { new CodePrimitiveExpression(column.Name) }))));

            loopStatement.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("retval"), "Add", new CodeExpression[] { new CodeVariableReferenceExpression(entityInstanceName) }));

            tryCatchStatement.TryStatements.Add(loopStatement);

            var catchClause = GetCatchClause();
            tryCatchStatement.CatchClauses.Add(catchClause);

            retval.Statements.Add(tryCatchStatement);
            retval.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("retval")));

            return retval;
        }

        /// <summary>
        /// Adds a stored proc parameter statement for each column in the tableView. Filtered by the provided predicate.
        /// </summary>
        private static void AddStoredProcParameters(Smo.TableViewBase tableView, CodeStatementCollection statements, string entityInstanceName, Func<Smo.Column, bool> columnFilter)
        {
            foreach (Smo.Column column in tableView.Columns)
            {
                if (columnFilter(column))
                {
                    var parameterName = string.Format("@{0}", column.Name);
                    statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("database"), "AddInParameter", new CodeExpression[] { new CodeVariableReferenceExpression("command"), new CodePrimitiveExpression(parameterName), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(DbType)), GetDbTypeString(column.DataType)), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(entityInstanceName), column.Name) }));
                }
            }
        }

        /// <summary>
        /// Gets a catch clause.
        /// </summary>
        private static CodeCatchClause GetCatchClause()
        {
            var retval = new CodeCatchClause("exception", new CodeTypeReference(typeof(Exception)));

            retval.Statements.Add(new CodeConditionStatement(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("ExceptionPolicy"), "HandleException", new CodeExpression[] { new CodeVariableReferenceExpression("exception"), new CodePrimitiveExpression("Global Exception Policy") }), new CodeStatement[] { new CodeThrowExceptionStatement() }));

            return retval;
        }

        /// <summary>
        /// Gets a string representation of the provided data type.
        /// </summary>
        private static string GetDbTypeString(Smo.DataType dataType)
        {
            switch (dataType.SqlDataType)
            {
                case Smo.SqlDataType.Bit:
                    return "Binary";

                case Smo.SqlDataType.SmallDateTime:
                    return "DateTime";

                case Smo.SqlDataType.Int:
                    return "Int32";

                case Smo.SqlDataType.NVarChar:
                    return "String";

                case Smo.SqlDataType.VarChar:
                    return "String";

                default:
                    return "String";
            }
        }

        /// <summary>
        /// Camel cases the provided string.
        /// </summary>
        private static string GetFormattedObjectInstanceName(string original)
        {
            return string.Concat(original.Substring(0, 1).ToLower(), original.Substring(1, original.Length - 1));
        }

        #endregion

        /// <summary>
        /// Gets a code string generated from the code type.
        /// </summary>
        private static String GetCodeFromType(ILanguageProvider languageProvider, CodeTypeDeclaration type)
        {
            var provider = languageProvider.GetCodeDomProvider();
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var options = new CodeGeneratorOptions { BracingStyle = "C" };

            provider.GenerateCodeFromType(type, writer, options);

            return builder.ToString();
        }

        /// <summary>
        /// Gets a code string generated from the code member.
        /// </summary>
        private static String GetCodeFromMember(ILanguageProvider languageProvider, CodeTypeMember member)
        {
            var provider = languageProvider.GetCodeDomProvider();
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);
            var options = new CodeGeneratorOptions { BracingStyle = "C" };

            provider.GenerateCodeFromMember(member, writer, options);

            return builder.ToString();
        }

        #endregion

    }
}