using ChecklistInspecao.Web.Models;
using Dapper;
using Npgsql;

namespace ChecklistInspecao.Web.Data;

public class LojaRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public LojaRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<LojaResumo>> ListarAsync(int[] regiaoIds, bool somenteAtivas = false)
    {
        const string sql = """
            SELECT l.id,
                   l.numero,
                   l.nome,
                   r.nome AS regiao_nome,
                   g.nome AS gerente_nome,
                   l.ativo
            FROM loja l
              JOIN regiao  r ON r.id = l.regiao_id
              JOIN gerente g ON g.id = l.gerente_id
            WHERE l.regiao_id = ANY(@RegiaoIds)
              AND (NOT @SomenteAtivas OR l.ativo)
            ORDER BY l.numero
        """;

        await using var conn = await _dataSource.OpenConnectionAsync();
        var lojas = await conn.QueryAsync<LojaResumo>(sql, new
        {
            RegiaoIds = regiaoIds,
            SomenteAtivas = somenteAtivas
        });
        return lojas.ToList();
    }

}
