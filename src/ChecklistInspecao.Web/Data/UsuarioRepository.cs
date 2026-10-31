using ChecklistInspecao.Web.Models;
using Dapper;
using Npgsql;

namespace ChecklistInspecao.Web.Data;

public class UsuarioRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UsuarioRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Usuario?> ObterPorLoginAsync(string login)
    {
        const string sql = """
            SELECT id, login, nome, senha_hash, perfil, ativo
                FROM usuario
            WHERE login = @Login
        """;

        await using var conn = await _dataSource.OpenConnectionAsync();
        return await conn.QuerySingleOrDefaultAsync<Usuario>(sql, new { Login = login });
    }

    public async Task<int[]> ObterIDsRegioesVisiveisAsync(int usuarioId, string perfil)
    {
        var sql = perfil == Perfis.Supervisor
            ? "SELECT regiao_id FROM usuario_regiao WHERE usuario_id = @UsuarioId"
            : "SELECT id FROM regiao";

        await using var conn = await _dataSource.OpenConnectionAsync();
        var ids = await conn.QueryAsync<int>(sql, new { UsuarioId = usuarioId });
        return ids.ToArray();
    }
}