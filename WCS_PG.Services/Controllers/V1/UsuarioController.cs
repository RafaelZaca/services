using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WCS_PG.Data;
using WCS_PG.Data.Models;
using WCS_PG.Services.Models.request.V1;

namespace WCS_PG.Services.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger;
        private readonly WCSContext _context;

        public UsuarioController(ILogger<UsuarioController> logger, WCSContext context)
        {
            _logger = logger;
            _context = context;
        }


        [HttpGet]
        [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UsuarioDto>>> GetUsuariosAsync()
        {
            try
            {
                var usuarios = await _context.Users
                    .Include(u => u.Group)
                    .Select(u => new UsuarioDto
                    {
                        Id = u.Id,
                        Nome = u.Name,
                        Email = u.Email,
                        Login = u.Username,
                        Grupo = u.Group.Name,
                        Ativo = u.IsActive,
                        DataCriacao = u.CreatedAt.ToString("o"),
                        UltimoAcesso = u.LastLogin == null ? "" : ((DateTime)u.LastLogin).ToString("o")
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("grupos")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public ActionResult<List<string>> GetGrupos()
        {
            var grupos = _context.Groups.Where(_ => _.IsActive).ToList();
            return Ok(grupos.Select(_ => _.Name));
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Users
                    .Include(u => u.Group)
                    .Where(u => u.Id == id)
                    .Select(u => new UsuarioDto
                    {
                        Id = u.Id,
                        Nome = u.Name,
                        Email = u.Email,
                        Login = u.Username,
                        Grupo = u.Group.Name,
                        Ativo = u.IsActive,
                        DataCriacao = u.CreatedAt.ToString("o"),
                        UltimoAcesso = u.LastLogin == null ? "" : ((DateTime)u.LastLogin).ToString("o")
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return NotFound($"Usuário com ID {id} não encontrado");
                }

                return Ok(usuario);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
}

        [HttpPost]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioDto>> CriarUsuarioAsync([FromBody] UsuarioDto usuario)
        {
            try { 
            // Verificar se já existe usuário com o mesmo login
            User? _user = await _context.Users.Where(srch => srch.Username == usuario.Login).FirstOrDefaultAsync();
            if (_user != null)
            {
                return BadRequest("Já existe um usuário com este login, tente novamente");
            }

            // Verificar se o grupo existe
            var _grupo = await _context.Groups.Where(g => g.Name == usuario.Grupo).FirstOrDefaultAsync();
            if (_grupo == null)
            {
                return BadRequest("O grupo informado não existe");
            }

            // Criar o novo usuário
            User _newUser = new User
            {
                CreatedAt = DateTime.Now,
                Email = usuario.Email,
                Username = usuario.Login,
                Name = usuario.Nome,
                PasswordHash = usuario.Senha!, 
                GroupId = _grupo.Id,
                IsActive = usuario.Ativo
            };

            await _context.Users.AddAsync(_newUser);
            await _context.SaveChangesAsync();

            // Criar o DTO de resposta
            var novoUsuarioDto = new UsuarioDto
            {
                Id = _newUser.Id,
                Nome = _newUser.Name,
                Email = _newUser.Email,
                Login = _newUser.Username,
                Grupo = _grupo.Name,
                Ativo = _newUser.IsActive,
                DataCriacao = _newUser.CreatedAt.ToString("o"),
                UltimoAcesso = _newUser.LastLogin?.ToString("o")
            };

            _logger.LogInformation($"Usuário criado: {usuario.Nome}");
            return CreatedAtAction(nameof(GetUsuario), new { id = _newUser.Id }, novoUsuarioDto);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDto>> AtualizarUsuarioAsync(int id, [FromBody] UsuarioDto usuario)
        {
            try { 
            if (id != usuario.Id)
            {
                return BadRequest("ID da URL não corresponde ao ID no corpo da requisição");
            }

            var _user = await _context.Users.FindAsync(id);
            if (_user == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado");
            }

            // Verificar se o login está sendo alterado e se já existe outro usuário com esse login
            if (_user.Username != usuario.Login)
            {
                var usuarioExistente = await _context.Users.Where(u => u.Username == usuario.Login).FirstOrDefaultAsync();
                if (usuarioExistente != null)
                {
                    return BadRequest("Já existe um usuário com este login");
                }
            }

            // Verificar se o grupo existe
            var grupo = await _context.Groups.Where(g => g.Name == usuario.Grupo).FirstOrDefaultAsync();
            if (grupo == null)
            {
                return BadRequest("O grupo informado não existe");
            }

            // Atualizar os dados do usuário
            _user.Name = usuario.Nome;
            _user.Email = usuario.Email;
            _user.Username = usuario.Login;
            _user.IsActive = usuario.Ativo;

            // Atualizar senha apenas se fornecida
            if (!string.IsNullOrEmpty(usuario.Senha))
            {
                _user.PasswordHash = usuario.Senha;
            }

            await _context.SaveChangesAsync();

            // Criar o DTO de resposta
            var usuarioAtualizado = new UsuarioDto
            {
                Id = _user.Id,
                Nome = _user.Name,
                Email = _user.Email,
                Login = _user.Username,
                Grupo = grupo.Name,
                Ativo = _user.IsActive,
                DataCriacao = _user.CreatedAt.ToString("o"),
                UltimoAcesso = _user.LastLogin?.ToString("o")
            };

            _logger.LogInformation($"Usuário atualizado: {usuario.Nome}");
            return Ok(usuarioAtualizado);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ExcluirUsuarioAsync(int id)
        {
            try { 
            var usuario = await _context.Users.FindAsync(id);
            if (usuario == null)
            {
                return NotFound($"Usuário com ID {id} não encontrado");
            }
            _context.Users.Remove(usuario);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Usuário excluído: {id}");
            return NoContent();

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}/permissoes")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<string>>> GetPermissoesUsuario(int id)
        {
            try
            {
                var usuario = await _context.Users
                    .Include(u => u.Group)
                    .ThenInclude(g => g.Permissions)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return NotFound($"Usuário com ID {id} não encontrado");
                }

                var permissoes = usuario.Group.Permissions.Select(p => p.Id).ToList();
                return Ok(permissoes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar permissões do usuário com ID {id}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
