namespace WCS_PG.Services.Models.request.V1
{
    public class PermissaoDto
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
    }

    public class GrupoDto
    {
        public int? Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public List<string> Permissoes { get; set; }
        public bool Ativo { get; set; }
        public string? DataCriacao { get; set; }
        public int? UsuariosVinculados { get; set; }
    }
}
