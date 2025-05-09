using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrupoController : ControllerBase
    {
        private readonly ILogger<GrupoController> _logger;
        private readonly WCSContext _context;

        public GrupoController(ILogger<GrupoController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GrupoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GrupoDto>>> GetGrupos()
        {
            try
            {
                var grupos = await _context.Groups
                    .Select(g => new GrupoDto
                    {
                        Id = g.Id,
                        Nome = g.Name,
                        Descricao = g.Description,
                        Permissoes = g.Permissions.Select(p => p.Id).ToList(),
                        Ativo = g.IsActive,
                        DataCriacao = g.CreatedAt.ToString("o"),
                        UsuariosVinculados = g.Users.Count
                    })
                    .ToListAsync();

                return Ok(grupos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar grupos");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("permissoes")]
        [ProducesResponseType(typeof(List<PermissaoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PermissaoDto>>> GetPermissoes()
        {
            try
            {
                var permissoes = await _context.Permissions
                    .Select(p => new PermissaoDto
                    {
                        Id = p.Id,
                        Nome = p.Name,
                        Descricao = p.Description
                    })
                    .ToListAsync();

                return Ok(permissoes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar permissões");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GrupoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoDto>> GetGrupo(int id)
        {
            try
            {
                var grupo = await _context.Groups
                    .Include(g => g.Permissions)
                    .Include(g => g.Users)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (grupo == null)
                {
                    return NotFound($"Grupo com ID {id} não encontrado");
                }

                var grupoDto = new GrupoDto
                {
                    Id = grupo.Id,
                    Nome = grupo.Name,
                    Descricao = grupo.Description,
                    Permissoes = grupo.Permissions.Select(p => p.Id).ToList(),
                    Ativo = grupo.IsActive,
                    DataCriacao = grupo.CreatedAt.ToString("o"),
                    UsuariosVinculados = grupo.Users.Count
                };

                return Ok(grupoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar grupo com ID {id}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(GrupoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GrupoDto>> CriarGrupo([FromBody] GrupoDto grupo)
        {
            try
            {
                // Verificar se já existe grupo com mesmo nome
                var grupoExistente = await _context.Groups.FirstOrDefaultAsync(g => g.Name == grupo.Nome);
                if (grupoExistente != null)
                {
                    return BadRequest("Já existe um grupo com este nome");
                }

                // Validar permissões
                if (grupo.Permissoes != null && grupo.Permissoes.Any())
                {
                    foreach (var permissaoId in grupo.Permissoes)
                    {
                        var permissao = await _context.Permissions.FindAsync(permissaoId);
                        if (permissao == null)
                        {
                            return BadRequest($"Permissão com ID {permissaoId} não encontrada");
                        }
                    }
                }

                // Criar novo grupo
                var novoGrupo = new Group
                {
                    Name = grupo.Nome,
                    Description = grupo.Descricao,
                    IsActive = grupo.Ativo,
                    CreatedAt = DateTime.Now
                };

                await _context.Groups.AddAsync(novoGrupo);
                await _context.SaveChangesAsync();

                // Adicionar permissões
                if (grupo.Permissoes != null && grupo.Permissoes.Any())
                {
                    foreach (var permissaoId in grupo.Permissoes)
                    {
                        var groupPermission = new GroupPermission
                        {
                            GroupId = novoGrupo.Id,
                            PermissionId = permissaoId,
                            CreatedAt = DateTime.Now
                        };
                        await _context.GroupPermissions.AddAsync(groupPermission);
                    }
                    await _context.SaveChangesAsync();
                }

                // Recarregar o grupo com permissões
                var grupoComPermissoes = await _context.Groups
                    .Include(g => g.Permissions)
                    .FirstOrDefaultAsync(g => g.Id == novoGrupo.Id);

                // Criar DTO de resposta
                var novoGrupoDto = new GrupoDto
                {
                    Id = grupoComPermissoes.Id,
                    Nome = grupoComPermissoes.Name,
                    Descricao = grupoComPermissoes.Description,
                    Permissoes = grupoComPermissoes.Permissions.Select(p => p.Id).ToList(),
                    Ativo = grupoComPermissoes.IsActive,
                    DataCriacao = grupoComPermissoes.CreatedAt.ToString("o"),
                    UsuariosVinculados = 0
                };

                _logger.LogInformation($"Grupo criado: {grupo.Nome}");
                return CreatedAtAction(nameof(GetGrupo), new { id = novoGrupo.Id }, novoGrupoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar grupo");
                return StatusCode(500, "Erro interno ao processar requisição");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GrupoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoDto>> AtualizarGrupo(int id, [FromBody] GrupoDto grupo)
        {
            try
            {
                if (id != grupo.Id)
                {
                    return BadRequest("ID da URL não corresponde ao ID no corpo da requisição");
                }

                var grupoExistente = await _context.Groups
                    .Include(g => g.Permissions)
                    .Include(g => g.Users)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (grupoExistente == null)
                {
                    return NotFound($"Grupo com ID {id} não encontrado");
                }

                // Verificar nome duplicado
                if (grupoExistente.Name != grupo.Nome)
                {
                    var grupoDuplicado = await _context.Groups
                        .FirstOrDefaultAsync(g => g.Name == grupo.Nome && g.Id != id);

                    if (grupoDuplicado != null)
                    {
                        return BadRequest("Já existe um grupo com este nome");
                    }
                }

                // Validar permissões
                if (grupo.Permissoes != null && grupo.Permissoes.Any())
                {
                    foreach (var permissaoId in grupo.Permissoes)
                    {
                        var permissao = await _context.Permissions.FindAsync(permissaoId);
                        if (permissao == null)
                        {
                            return BadRequest($"Permissão com ID {permissaoId} não encontrada");
                        }
                    }
                }

                // Atualizar grupo
                grupoExistente.Name = grupo.Nome;
                grupoExistente.Description = grupo.Descricao;
                grupoExistente.IsActive = grupo.Ativo;

                // Remover todas as permissões antigas
                var permissoesAnteriores = await _context.GroupPermissions
                    .Where(gp => gp.GroupId == id)
                    .ToListAsync();

                _context.GroupPermissions.RemoveRange(permissoesAnteriores);

                // Adicionar novas permissões
                if (grupo.Permissoes != null && grupo.Permissoes.Any())
                {
                    foreach (var permissaoId in grupo.Permissoes)
                    {
                        var groupPermission = new GroupPermission
                        {
                            GroupId = id,
                            PermissionId = permissaoId,
                            CreatedAt = DateTime.Now
                        };
                        await _context.GroupPermissions.AddAsync(groupPermission);
                    }
                }

                await _context.SaveChangesAsync();

                // Recarregar o grupo com permissões atualizadas
                var grupoAtualizado = await _context.Groups
                    .Include(g => g.Permissions)
                    .Include(g => g.Users)
                    .FirstOrDefaultAsync(g => g.Id == id);

                // Criar DTO de resposta
                var grupoDto = new GrupoDto
                {
                    Id = grupoAtualizado.Id,
                    Nome = grupoAtualizado.Name,
                    Descricao = grupoAtualizado.Description,
                    Permissoes = grupoAtualizado.Permissions.Select(p => p.Id).ToList(),
                    Ativo = grupoAtualizado.IsActive,
                    DataCriacao = grupoAtualizado.CreatedAt.ToString("o"),
                    UsuariosVinculados = grupoAtualizado.Users.Count
                };

                _logger.LogInformation($"Grupo atualizado: {grupo.Nome}");
                return Ok(grupoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar grupo com ID {id}");
                return StatusCode(500, "Erro interno ao processar requisição");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ExcluirGrupo(int id)
        {
            try
            {
                var grupo = await _context.Groups
                    .Include(g => g.Users)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (grupo == null)
                {
                    return NotFound($"Grupo com ID {id} não encontrado");
                }

                // Verificar se há usuários vinculados
                if (grupo.Users.Any())
                {
                    return BadRequest("Não é possível excluir o grupo pois existem usuários vinculados a ele");
                }

                // Remover permissões do grupo
                var permissoes = await _context.GroupPermissions
                    .Where(gp => gp.GroupId == id)
                    .ToListAsync();

                _context.GroupPermissions.RemoveRange(permissoes);

                // Remover o grupo
                _context.Groups.Remove(grupo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Grupo excluído: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao excluir grupo com ID {id}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
