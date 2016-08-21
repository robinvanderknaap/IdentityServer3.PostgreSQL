using System;

namespace IdentityServer3.PostgreSQL.Models
{
    public class StoredToken
    {
        public string Key { get; set; }
        public TokenType TokenType { get; set; }
        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public DateTime Expires { get; set; }
        public string SerializedToken { get; set; }
    }
}