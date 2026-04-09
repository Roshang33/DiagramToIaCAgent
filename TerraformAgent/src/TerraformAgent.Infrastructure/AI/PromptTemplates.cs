namespace TerraformAgent.Infrastructure.AI;

public static class PromptTemplates
{
    public const string TerraformGeneration = """
You are a Terraform infrastructure expert.

Use the provided reference code snippets and user requirements
to generate Terraform code.

User request:
{{$input}}

Reference snippets:
{{$context}}

Generate complete Terraform code.
Do not explain anything.
Return only Terraform code.
""";
}