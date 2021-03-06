﻿using System.Collections.Generic;
using System.Text;
using CodeContractNullabilityFxCopRules.Utilities;
using JetBrains.Annotations;

namespace CodeContractNullabilityFxCopRules.Test.TestDataBuilders
{
    public class MemberSourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<string> members = new List<string>();

        protected override string GetSourceCode()
        {
            var builder = new StringBuilder();
            builder.AppendLine("public class Test");
            builder.AppendLine("{");

            int index = 0;
            foreach (string member in members)
            {
                if (index > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendLine(member.Trim());
                index++;
            }

            builder.AppendLine("}");
            return builder.ToString();
        }

        [NotNull]
        public MemberSourceCodeBuilder InDefaultClass([NotNull] string memberCode)
        {
            Guard.NotNull(memberCode, "memberCode");

            members.Add(memberCode);
            return this;
        }
    }
}