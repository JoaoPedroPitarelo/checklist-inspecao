using ChecklistInspecao.Web.Models;
using Dapper;
using Npgsql;

namespace ChecklistInspecao.Web.Data;

public class RegiaoRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public RegiaoRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<Opcao>> ListarAsync(int[] regiaoIds)
    {
        const string sql = """
            SELECT id, nome
            FROM regiao
                WHERE id = ANY(@RegiaoIds)
            ORDER BY nome
        """;

        await using var conn = await _dataSource.OpenConnectionAsync();
        var regioes = await conn.QueryAsync<Opcao>(sql, new { RegiaoIds = regiaoIds });
        return regioes.ToList();
    }
}
