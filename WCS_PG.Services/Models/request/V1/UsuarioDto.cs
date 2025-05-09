namespace WCS_PG.Services.Models.request.V1
{
    public class UsuarioDto
    {
        public int? Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string? Senha { get; set; }
        public string Grupo { get; set; }
        public bool Ativo { get; set; }
        public string? DataCriacao { get; set; }
        public string? UltimoAcesso { get; set; }
    }
}
