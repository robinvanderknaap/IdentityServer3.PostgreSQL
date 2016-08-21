namespace IdentityServer3.PostgreSQL.SchemaGenerator
{
    public enum SchemaCreationOptions
    {
        CreateWhenNotExists = 0,
        AlwaysRecreate = 1,
        NeverCreate = 2
    }
}
